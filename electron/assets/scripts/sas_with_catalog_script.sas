/*
Version: 9.0
Encoding: UTF-8 without byte order mark
Note: The working directory must contain the data file (sas7bdat) and catalog file (sas7bcat)
NB: The data and catalog files must have the same name
NB: The link between variable and format must be stored in the data file
NB: The formats in the catalog file must represent code list(s) only
NB: The values in the catalog file must be explicitly specified (ranges are invalid)
*/

 * Set the working directory and data file name;
				  %let astaDir=%str({3}{0});
                                  %let outDir=%str({1}{0});
                                  %let inputSas=%str({2});
                                  libname mylib "&outDir";

								  
Proc printto log= "mylib&inputSas..Log";
run;


                                  * Set options;
                                  options locale=da_DK replace=yes;
                                  options nofmterr;

                                  /* Escape reserved xml characters;
                                  %macro clean(varName);
                                  &varname=strip(&varname);
                                  label=tranwrd(&varName,'&','&amp;');
                                  label=tranwrd(&varName,'<','<');
                                  label=tranwrd(&varName,'>','>');
                                  label=tranwrd(&varName,"'",'&apos;');
                                  label=tranwrd(&varName,'"','&quot;');
                                  %mend clean;*/

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
								  
 /**/



  %let felter=;
  proc sql noprint;
    select variable into :felter separated by ';'
    from mylib.odsOut
;
  quit;
data mylib.varinfo(keep=name fmt);
 set mylib.&inputSas end=eof;
 length felt $ 32;
 length Name $ 32;
 length Fmt $ 20;
 retain a1-a25000 0 b1-b25000 0;
 array bred{25000} a1-a25000 (0);
 array deci{25000} b1-b25000 (0);
 
 count=0;
 do until(felt=' ');
   count+1;
   felt=scan("&felter", count,';');
   if felt~=' ' then do;
     val=trim(left(vvaluex(felt)));
	 tjekExp=indexc(val,'E');
     w=length(val);
	 if tjekExp>0 then do;
       w=substr(val,tjekexp+1,length(val)-tjekExp)+1;
	 end;
     if w>bred{count} then bred{count}=w;
     tmp=indexc(val,'.');
     if tmp>0 then do;
	 	if tmp>1 then do;
       val1=substr(val,1,tmp-1);
	   w2=length(val1);
	   w3=w-w2-1;
	   if w3>deci{count} and tjekExp=0 then do;
         deci{count}=w3;
	   end;
     end;
   end;
 end;
end;

 if eof then do; 
 count=0;
 do until(Name=' ');
   count+1;
   Name=scan("&felter", count,';');
   Fmt=compress('f'||bred{count}||'.'||deci{count});
   if deci{count}=0 then Fmt=compress('f'||bred{count}||'.');
   if Name ne ' ' then output;
 end;
    
 end;
run;


/**/


								  
								  * Get code list items;
								  proc catalog catalog=mylib.&inputSas;
                                  contents out=mylib.odsFmtOut;
                                  run;
                                  * Get ready for merge;
                                  data mylib.odsFmtOut(keep=FmtNam CodeList);
                                    set mylib.odsFmtOut;
                                    FmtNam=upcase(cat(strip(name),'.'));
									if type='FORMATC' then FmtNam=upcase(cat('$',strip(name),'.'));
                                    CodeList=1;
                                  run;
								  * Merge to get code-list vars flagged;
                                  proc sql;
                                    create table mylib.odsOut as
                                    select a.*, b.Codelist
                                    from mylib.odsOut a left join mylib.odsFmtOut b on upcase(a.Format)=b.FmtNam;
                                  quit;
								  
/*nyt*/
proc sql;
create table mylib.odsOut as
select a.*, b.Fmt
from mylib.odsOut a left join mylib.varinfo b on upcase(a.Variable)=upcase(b.Name);
quit;
/*nyt*/				
proc sort data=mylib.odsout;by num;run;

 
                                  * Create output with variable name, type and format;
                                  data mylib.varNames(keep=varNameFormat);
                                  set mylib.odsOut;
                                  format Format $char200.;
				  format VarType $char200.;
                                  *If format is missing, or we have a UserFormat, map the generic type to type;
                                  if lowcase(type) eq 'num' then do;
				    if codelist ne 1 then do;
				      Vartype=format;
                                      if (prxmatch('/f\d*\./',lowcase(strip(Format)))=1) or
				         (prxmatch('/\d*\./',lowcase(strip(Format)))=1) or format=''
					then vartype=Fmt;
				    end;
				  end;
                                  else do;
                                    if lowcase(type) EQ 'char' then do; 
                                      VarType=cats('$',len,'.');
				    end;
                                  end;
				
				  if CodeList=1 then do;
				    VarType=Fmt;
                                    Format=lowcase(Format);
				  end;
				  else do;
				    Format='';
				  end;
                                  varNameFormat=cat(strip(Variable),' ',strip(lowcase(VarType)),' ',strip(lowcase(Format)));
                                  run;
								  
                                  * Write output to file;
                                  %let name=%str({2}_VARIABEL.txt);
                                  %let outfile=&astaDir&name;
                                  data _null_;
                                  set mylib.varNames;
                                  file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
                                  put(_all_)(+0);
                                  run;
								  
								  proc datasets lib=mylib;delete odsFmtOut;quit;

                                  * CREATE VARIABELBESKRIVELSE;
                                  data mylib.varLabels(keep=varLabels);
                                  length Label $6400;
                                  set mylib.odsOut;
                                  *%clean(Label);
                                  if Label eq '' then Label='n.a.';
                                  length varLabels $7200;
                                  varLabels=cat(strip(Variable)," '",strip(Label),"'");
                                  run;
                                  * Write output to file;
                                  %let name=%str({2}_VARIABELBESKRIVELSE.txt);
                                  %let outfile=&astaDir&name;
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
                                  *%clean(Label);
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
                                  %let name=%str({2}_KODELISTE.txt);
                                  %let outfile=&astaDir&name;
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
                                  delete odsOut varNames varLabels valLabels varinfo;
                                  run;

  
                                  data _null_;
                                  call symput('datafile', "mylib.&inputSas");
                                  filename csv "&astaDir&inputSas..csv" encoding='utf-8';
                                  %let outFile=csv;
                                  proc export data=&datafile outfile=&outFile dbms=dlm replace;
                                  delimiter=';';
                                  putnames=yes;
                                  run;

    


%Put NOTICE: SAS IS FINISHED RUNNING THIS SCRIPT;
