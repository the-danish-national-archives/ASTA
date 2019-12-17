/*
Version: 8.0
Encoding: UTF-8 without byte order mark
Note: The working directory must contain the data file (sas7bdat)
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

* Escape reserved xml characters;
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



/*nyt*/
proc sql;
create table mylib.odsOut as
select a.*, b.Fmt
from mylib.odsOut a left join mylib.varinfo b on upcase(a.Variable)=upcase(b.Name);
quit;

/*nyt*/

proc sort data=mylib.odsOut;by num;run;
* Create output with variable name and format;
data mylib.varNames(keep=varNameFormat);
set mylib.odsOut;
format Format $200.;
*If format is missing, map the generic type to format;
if lowcase(type) eq 'num' then do;
if format='' then format=Fmt;
end;
else do;
if lowcase(type) EQ 'char' then do;
type='$';
Format=cats(type,len,'.');
end;
end;

varNameFormat=cat(strip(Variable),' ',strip(lowcase(Format)));
run;
* Write output to file;
%let name=%str({2}_VARIABEL.txt);
%let outfile=&astaDir&name;
data _null_;
set mylib.varNames;
file "&outfile" encoding='utf-8' dsd dlm='09'x lrecl=2000000;
put(_all_)(+0);
run;

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
