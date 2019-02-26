/*
Version: 1.0
Encoding: UTF-8 without byte order mark
Note: The working directory must contain the data file (sas7bdat)
*/

* Set the working directory and data file name;
%let outDir=%str(G:\SAS-uden katalog\);
%let inputSas=%str(sas_23765_short);
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
* Create output with variable name and format;
data mylib.varNames(keep=varNameFormat);
set mylib.odsOut;
format Format $char200.;
*If format is missing, map the generic type to format;
if lowcase(type) eq 'num' then type='';
else
do;
if lowcase(type) EQ 'char' then type='$';
end;
if Format eq '' then Format=cats(type,len,'.');
* If present in format, remove "char" and "best";
if prxmatch('/\$char\d+\./',lowcase(strip(Format)))>0 then Format=cats(type,len,'.');
if prxmatch('/best\d*\./',lowcase(strip(Format)))>0 then Format=cats(type,len,'.');
varNameFormat=cat(strip(Variable),' ',strip(lowcase(Format)));
run;
* Write output to file;
%let name=%str(sas_23765_short_VARIABEL.txt);
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
%let name=%str(sas_23765_short_VARIABELBESKRIVELSE.txt);
%let outfile=&outDir&name;
data _null_;
set mylib.varLabels;
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
*/
