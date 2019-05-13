sas_syntax_content_with_catalog : /*
                                  Version: 1.0
                                  Encoding: UTF-8 without byte order mark
                                  Note: The working directory must contain the data file (sas7bdat) and catalog file (sas7bcat)
                                  NB: The data and catalog files must have the same name
                                  NB: The link between variable and format must be stored in the data file
                                  NB: The formats in the catalog file must represent code list(s) only
                                  NB: The values in the catalog file must be explicitly specified (ranges are invalid)
                                  */

                                  * Set the working directory and data file name;
                                  %let outDir=%str(C:\Users\lokalAdmin\Desktop\TestSuite\testtroels\nyeskript_0519\medcat\);
                                  %let inputSas=%str(sas_23765_shortdecbc1);
                                  libname mylib "&outDir";

                                  * Set options;
                                  options locale=da_DK replace=yes;
                                  options nofmterr;

                                  * Escape reserved xml characters;
                                  %macro clean(varName);
                                  &varname=strip(&varname);
                                  label=tranwrd(&varName,'&','&amp;');
                                  label=tranwrd(&varName,'<','<');
                                  label=tranwrd(&varName,'>','>');
                                  label=tranwrd(&varName,"'",'&apos;');
                                  label=tranwrd(&varName,'"','&quot;');
                                  %mend clean;

                                  * CREATE VARIABEL;
                                  ods listing;
                                  ods trace on /listing;
                                  * Ods output <ouput-object>=<sas data set>;
                                  ods output variables=mylib.odsOut;
                                  proc contents data=mylib.&inputSas;
                                  run;
                                  ods trace off;
                                  ods _all_ close;
                                  * If format column is missing in output, add column;
                                  data mylib.odsOut;
                                  length Format $200;
                                  set mylib.odsOut;
                                  id=open('mylib.odsOut');
                                  if varnum(id, 'Format')=0 then Format='';
                                  rc=close(id);
                                  drop rc id;
                                  run;
								  * Get code list items;
								  proc catalog catalog=mylib.&inputSas;
                                  contents out=mylib.odsFmtOut;
                                  run;
                                  * Get ready for merge;
                                  data mylib.odsFmtOut(keep=FmtNam CodeList);
                                    set mylib.odsFmtOut;
                                    FmtNam=upcase(cat(strip(desc),'f.'));
                                    CodeList=1;
                                  run;
								  * Merge to get code-list vars flagged;
                                  proc sql;
                                    create table mylib.odsOut as
                                    select a.*, b.Codelist
                                    from mylib.odsOut a left join mylib.odsFmtOut b on a.Format=b.FmtNam;
                                  quit;
								  * Sort according to original order;
                                  proc sort data=mylib.odsOut;by num;run;
                                  * Create output with variable name, type and format;
                                  data mylib.varNames(keep=varNameFormat);
                                  set mylib.odsOut;
                                  format Format $char200.;
								  format VarType $char200.;
                                  *If format is missing, or we have a UserFormat, map the generic type to type;
                                  if lowcase(type) eq 'num' then type='f';
                                  else
                                  do;
                                  if lowcase(type) EQ 'char' then type='$';
                                  end;
                                  if Format eq '' or CodeList=1 then do;
                                    VarType=cats(type,len,'.');
									if CodeList=1 then Format=lowcase(Format);
								  end;
                                  else do;
								    if indexc('char',lowcase(Format))>0 then Format=cats(type,len,'.');
								    if prxmatch('/\f\d+\./',lowcase(strip(Format)))>0 then Format=cats(type,len,'.');
                                    if prxmatch('/best\d*\./',lowcase(strip(Format)))>0 then Format=cats(type,len,'.');
                                    VarType=Format;
									Format='';
								  end;								  
                                  if Format eq '' then Format=cats(type,len,'.');
                                  * If present in format, remove "char" and "best";

                                  varNameFormat=cat(strip(Variable),' ',strip(lowcase(VarType)),' ',strip(lowcase(Format)));
                                  run;
                                  * Write output to file;
                                  %let name=%str(sas_23765_shortdecbc1_VARIABEL.txt);
                                  %let outfile=&outDir&name;
                                  data _null_;
                                  set mylib.varNames;
                                  file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
                                  put(_all_)(+0);
                                  run;

                                  * CREATE VARIABELBESKRIVELSE;
                                  data mylib.varLabels(keep=varLabels);
                                  length Label $6400;
                                  set mylib.odsOut;
                                  %clean(Label);
                                  if Label eq '' then Label='n.a.';
                                  length varLabels $7200;
                                  varLabels=cat(strip(Variable)," '",strip(Label),"'");
                                  run;
                                  * Write output to file;
                                  %let name=%str(sas_23765_shortdecbc1_VARIABELBESKRIVELSE.txt);
                                  %let outfile=&outDir&name;
                                  data _null_;
                                  set mylib.varLabels;
                                  file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
                                  put(_all_)(+0);
                                  run;

                                  * CREATE KODELISTE;
                                  %let name=%str(valLabels);
                                  %let outfile=&outDir&name;
                                  * Get content from the catalog file;
                                  proc format out="&outfile" fmtlib library=mylib.&inputSas;
                                  run;
                                  data mylib.valLabels(keep=Fmtname valLabels);
                                  length Label $32767;
                                  set mylib.valLabels;
                                  %clean(Label);
                                  * If label is empty, use default value;
                                  if Label eq '' then Label='n.a.';
                                  Fmtname=strip(lowcase(Fmtname));
                                  if lag(Fmtname)=Fmtname then Fmtname='';
                                  * Remove preceding dot in front of special values (.A-.Z);
                                  if prxmatch('/\.[a-z]/',lowcase(strip(Start)))>0 then Start=substr(strip(Start),2,1);
                                  length valLabels $32767;
                                  valLabels=cat("'",strip(Start),"'"," '",strip(Label),"'");
                                  run;
                                  * Create output with code list(s);
                                  %let name=%str(sas_23765_shortdecbc1_KODELISTE.txt);
                                  %let outfile=&outDir&name;
                                  data _null_;
                                  set mylib.valLabels;
                                  file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
                                  put(Fmtname)(+0);
                                  put(valLabels)(+0);
                                  run;
                                  data mylib.valLabels;
                                  infile "&outfile" encoding='utf-8' delimiter='09'x missover dsd lrecl=32767;
                                  informat var1 $32767.;
                                  format var1 $32767.;
                                  input var1 $; var1=strip(var1);
                                  run;
                                  data mylib.valLabels;
                                  set mylib.valLabels;
                                  if var1 eq '' then delete;
                                  run;
                                  * Write output to file;
                                  data _null_;
                                  set mylib.valLabels;
                                  file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
                                  put(_all_)(+0);
                                  run;

                                  * Delete temporary files on disk;
                                  proc datasets library=mylib;
                                  delete odsOut varNames varLabels valLabels;
                                  run;

                                  /*
                                  CONVERT DATA TO DELIMITED TEXT FILE (DEFAULT METHOD)
                                  Note: Write variable names on the first line
                                  Note: Write text qualifier only if data contain the delimiter
                                  Note: Escape a double quote in string data with a double quote and double quoting of the whole string
                                  Note: Null values in numeric variables are not respresented at all
                                  Note: Special codes for missing values are written as uppercase letters (A-Z)
                                  NB: Max lrecl in output=32767
                                  */
                                  data _null_;
                                  call symput('datafile', "mylib.&inputSas");
                                  filename csv "&outDir\&inputSas..csv" encoding='utf-8';
                                  %let outFile=csv;
                                  proc export data=&datafile outfile=&outFile dbms=dlm replace;
                                  delimiter=';';
                                  putnames=yes;
                                  run;

                                  /*
                                  CONVERT DATA TO DELIMITED TEXT FILE (ALTERNATIVE METHOD)
                                  Note: Write text qualifier only if data contain the delimiter
                                  Note: Escape a double quote in string data with a double quote and double quoting of the whole string
                                  Note: Null values in numeric variables are not respresented at all
                                  Note: Special codes for missing values are written as uppercase letters (A-Z)
                                  NB: Does not write variable names on the first line
                                  data _null_;
                                  file csv dsd dlm=';' lrecl=2000000;
                                  set mylib.&inputName;
                                  put(_all_)(+0);
                                  run;
 
