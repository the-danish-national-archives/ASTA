﻿* Encoding: UTF-8.
 */
                      Version: 11,0
                      Encoding: UTF-8 with byte order mark
                      Note: The working directory must contain the data file (sav)
                      */

                      * Set the working directory and data file name.
file handle astaDir
/name '{3}'.
file handle dataDir
/name '{1}'.
file handle inputSpss
/name 'dataDir{0}{2}.sav'.
file handle OutputSpss
/name 'dataDir{0}{2}_output.sav'.

* Set options.
set unicode on decimal dot olang=english.

* Escape reserved xml characters.
*define !clean(var=!tokens(1))
*compute !var=ltrim(rtrim(!var)).
*compute !var=replace(!var, '&', '&amp;').
*compute !var=replace(!var, '<', '<').
*compute !var=replace(!var, '>', '>').
*compute !var=replace(!var, "'", '&apos;').
*compute !var=replace(!var, '"', '&quot;').
*!enddefine.

* Create dictionary file.
define !dictionary(subtype=!tokens(1))
get file 'outputSpss'.
oms
/select tables
/if subtypes=[!subtype]
/destination format=sav outfile='dataDir{0}dictionary.sav' viewer=no.
display dictionary.
omsend.
!enddefine.

* Create dummy variable with metadata.
get file 'inputSpss'.
compute absoluteDum=1.
alter type absoluteDum(f1).
variable labels absoluteDum 'Nothing'.
value labels absoluteDum 1 'Nothing'.
missing values absoluteDum(1).
save outfile 'outputSpss'.

* Create file with all code lists.
* NB: Dummy decimals are added to all code values, if a code value has decimals somewhere.
get file 'outputSpss'.
!dictionary subtype='variable values'.
get file 'dataDir{0}dictionary.sav'
/keep var1 var2 label
/rename var1=varName var2=val label=valLabel.
alter type varName(a64) val valLabel(a32767).
sort cases by varName val valLabel.
*!clean var=val.
*!clean var=valLabel.
alter type val valLabel(amin).
save outfile 'dataDir{0}code_lists.sav'.

* Map variable to code list.
get file 'dataDir{0}code_lists.sav'.
string compare(a32767).
compute casenum=$casenum.
sort cases by varName.
compute rep=1.
* Count codes.
if lag(varName) eq varName rep=1+lag(rep).
sort cases by varName(d) rep(d).
compute codeCount=rep.
* Store maximum code count in variable.
if lag(varName) eq varName codeCount=lag(codeCount).
sort cases by varName val valLabel.
* Concatenate code and value.
compute compare=concat(val, valLabel).
compute lengthCompare=length(compare).
do if lag(varName) eq varName.
do if lag(compare)=''.
compute compare=''.
else.
compute lengthCompare=length(compare)+length(lag(compare)).
do if lengthCompare le 32767.
* Concatenate all codes and values into one string.
compute compare=concat(lag(compare), compare).
else.
compute compare=''.
end if.
end if.
end if.
execute.
* Select string to compare.
select if rep eq codeCount and compare ne ''.
execute.
* Link the all code list to variables.
sort cases by compare.
string varName_(a64).
compute varName_=varName.
* if lag(compare) eq compare varName_=lag(varName_).
alter type varName_(a66).
sort cases by varName.
save outfile 'dataDir{0}variable_to_code_list.sav'
/keep varName varName_.
execute.


* Remove redundant code lists.
match files file 'dataDir{0}code_lists.sav'
/table='dataDir{0}variable_to_code_list.sav'
/by varName.
save outfile 'dataDir{0}all_code_lists.sav'.

* CREATE VARIABEL.
!dictionary subtype='variable information'.
get file 'dataDir{0}dictionary.sav'
/keep var1 writeformat
/rename var1=varName writeformat=varFormat.
alter type varName(a64) varFormat(a64).
compute varFormat=lower(varFormat).
compute casenum=$casenum.
sort cases by varName.
save outfile='dataDir{0}variable.sav'.
* Link variable to code list (if link exists).
get file 'dataDir{0}code_lists.sav'.
compute rep=1.
* Keep all variable names.
if lag(varName)=varName rep=1+lag(rep).
select if rep eq 1.
sort cases by varName.
save outfile='dataDir{0}variable_has_code_list.sav'
/keep varName.
* Add code list reference to variable list.
match files file 'dataDir{0}variable.sav' /in=master
/file='dataDir{0}variable_has_code_list.sav' /in=codeVar
/file='dataDir{0}variable_to_code_list.sav' /in=codeListName
/by varName.
string type(a1) variable(a32767).
alter type varFormat(a66) varName_(a66).
* If variable has a code list, make sure to use it.
compute type=char.substr(varFormat, 1, 1).
*if codeVar eq 1 varFormat=concat(varName_, '.').
*if codeVar eq 1 and type eq 'a' varFormat=concat('$', varFormat).
if codeVar eq 1 varName_=concat(ltrim(rtrim(varName_)), '.').
if codeVar eq 1 and type eq 'a' varName_=concat('$', varName_).
*compute variable=concat(ltrim(rtrim(varName)), ' ', ltrim(rtrim(varFormat))).
        compute variable=concat(ltrim(rtrim(varName)), ' ', ltrim(rtrim(varFormat)), ' ', varName_).
sort cases by casenum.
select if varName ne 'absoluteDum'.
alter type variable(amin).
delete variables casenum varName varName_ type varFormat master codeVar codeListName.
* Note: Trailing spaces included in output lines.
write outfile 'astaDir{0}{2}_VARIABEL.txt' /variable.
execute.

* CREATE VARIABELBESKRIVELSE.
!dictionary subtype='variable information'.
get file 'dataDir{0}dictionary.sav'
/keep var1 label
/rename var1=varName label=varLabel.
alter type varName(a64) varLabel(a32767).
* If label is empty, use default value.
if varLabel='<none>' varLabel='n.a.'.
*!clean var=varLabel.
alter type varLabel(amin).
string varDescription(a32767).
compute varDescription=concat(ltrim(rtrim(varName)), concat(" '", ltrim(rtrim(varLabel)), "'")).
select if varName ne 'absoluteDum'.
alter type varDescription(amin).
* Note: Trailing spaces included in output lines.
write outfile 'astaDir{0}{2}_VARIABELBESKRIVELSE.txt' /varDescription.
execute.

* CREATE KODELISTE.
get file 'dataDir{0}all_code_lists.sav'.
sort cases by varName.
get file  'dataDir{0}variable.sav'.
sort cases by varName.
match files file='dataDir{0}all_code_lists.sav'
/table='dataDir{0}variable.sav'
/by varName.
execute.
save outfile 'dataDir{0}all_codes_and_formats.sav'.
get file  'dataDir{0}all_codes_and_formats.sav'.
string tmp(a100) varRef codeList(a32767).
compute tmp=val.
alter type tmp (f).
alter type tmp (amin).
if tmp='' codeList=concat('"', "'", ltrim(rtrim(val)), "'", concat(" '", ltrim(rtrim(valLabel)), "'"), '"').
if tmp ne '' codeList=concat('"', "'", ltrim(rtrim(tmp)), "'", concat(" '", ltrim(rtrim(valLabel)), "'"), '"').
* compute codeList=concat('"', "'", ltrim(rtrim(tmp)), "'", concat(" '", ltrim(rtrim(valLabel)), "'"), '"').
* compute codeList=concat(ltrim(rtrim(tmp)), concat(" '", ltrim(rtrim(valLabel)), "'")).
if lag(varName) ne varName varRef=concat('"', ltrim(rtrim(varName)), '"').
select if varName ne 'absoluteDum'.
alter type codeList varRef(amin).
write outfile 'dataDir{0}temp.txt' /1 varRef /2 codeList.
get data
/type=txt
/file='dataDir{0}temp.txt'
/arrangement=delimited
/delimiters=';'
/qualifier='"'
/variables=codeList A.
select if codeList ne ''.
alter type codeList(amin).
* Note: Trailing spaces included in output lines.
write outfile 'astaDir{0}{2}_KODELISTE.txt' /codeList.
execute.

* CREATE BRUGERKODE.
!dictionary subtype='variable information'.
get file 'dataDir{0}dictionary.sav'
/keep var1 missingvalues
/rename var1=varName missingvalues=missing.
alter type varName(a64) missing(amin).
sort cases by varName.
save outfile 'dataDir{0}user_defined_missing_values.sav'.
* Remove missing values from redundant code lists.
match files file 'dataDir{0}user_defined_missing_values.sav'
/file='dataDir{0}variable_to_code_list.sav'
/by varName.
compute keep=1.
if varName ne varName_ keep=0.
select if keep=1.
* Format user-defined missing values.
select if char.length(missing) gt 0.
compute missing=replace(missing, '"', '').
compute missing=replace(missing, ',', '').
*!clean var=missing.
alter type missing(amin).
string varMissing(a32767).
compute varMissing=concat(ltrim(rtrim(varName)), " '", replace(ltrim(rtrim(missing)), ' ', "' '"), "'").
select if varName ne 'absoluteDum'.
alter type varMissing(amin).
* Note: Trailing spaces included in output lines.
write outfile 'astaDir{0}{2}_BRUGERKODE.txt' /varMissing.
execute.

* Delete temporary content (in file and on disk).
get file 'outputSpss'.
save outfile 'outputSpss'
/drop absoluteDum.
*get file 'inputSpss'.
*save outfile 'inputSpss'.
* /drop absoluteDum.
erase file='dataDir{0}dictionary.sav'.
erase file='dataDir{0}code_lists.sav'.
erase file='dataDir{0}variable_to_code_list.sav'.
erase file='dataDir{0}all_code_lists.sav'.
erase file='dataDir{0}all_codes_and_formats.sav'.
erase file='dataDir{0}variable.sav'.
erase file='dataDir{0}variable_has_code_list.sav'.
erase file='dataDir{0}user_defined_missing_values.sav'.
erase file='dataDir{0}temp.txt'.
execute.

*/
CONVERT DATA TO DELIMITED TEXT FILE
Note: Write variable names on the first line
Note: Write text qualifier only if data contain the delimiter
Note: Escape a double quote in string data with a double quote and double quoting of the whole string
Note: Null values in numeric variables are presented as space
Note: User-defined missing values are presented "as is" (either number or string value)
*/

define !export(outfile=!tokens(1))
get file !QUOTE(!CONCAT(dataDir, '{0}', !outfile, '.sav')).
save translate outfile=!QUOTE(!CONCAT(astaDir, '{0}', !outfile, '.csv'))
/textoptions delimiter=';' qualifier='"' decimal=dot format=variable
/type=csv
/encoding='utf8'
/map
/replace
/fieldnames
/cells=values.
!enddefine.
!export outfile={2}.

