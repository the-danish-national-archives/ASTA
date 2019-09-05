/*
Version: 6.0
Encoding: UTF-8 without byte order mark
Note: The working directory must contain the data file (dta)
*/

* Set the working directory and data file name
global fileDir "{1}"
global astaDir "{3}"
global inputStata $fileDir{0}{2}
global inputStata_ "{2}"

* Set options
set scrollbufsize 2048000
set linesize 255
set more off

* Set end of line character(s)
if regexm(c(os),"Windows")==1 {
global osEOL="\W"
}
else if regexm(c(os),"Mac")==1 {
global osEOL="\M"
}
else if regexm(c(os),"Unix")==1 {
global osEOL="\U"
}

* Escape reserved xml characters
capture program drop clean
program define clean
*filefilter $fileDir{0}temp.txt $fileDir{0}tempA.txt, from ("\038d") to ("&amp;") replace
*filefilter $fileDir{0}tempA.txt $fileDir{0}tempB.txt, from ("\060d") to ("<") replace
*filefilter $fileDir{0}tempB.txt $fileDir{0}tempC.txt, from ("\062d") to (">") replace
*filefilter $fileDir{0}tempC.txt $fileDir{0}tempD.txt, from ("\039d") to ("&apos;") replace
filefilter $fileDir{0}temp.txt $fileDir{0}tempE.txt, from ("") to ("") replace
end

* CREATE VARIABEL
use $inputStata, replace
foreach var of varlist _all {
label var `var\' ""
}
cls
describe _all
translate @Results $fileDir{0}temp.txt, replace
clean
* Import file and extract code list name (value label)
* Note: Variable name and format can be truncated in the input
* Note: Use the format statement to get the full length of variable name and format
import delimited v1 using $fileDir{0}tempE.txt, clear ///
delimiter("\t") stringcols(_all) encoding("utf-8")
replace v1=regexr(v1,"\* $","")
drop if v1=="" | regexm(v1,"^.+[ ].+[ ]%.+[ ]*")==0
replace v1=trim(stritrim(v1))
split v1, gen(var)
capture confirm string variable var4
if _rc {
gen var4=""
}
keep var4
rename var4 var3
gen v1=_n
save $fileDir{0}valLabelName, replace
* Make new dataset
use $inputStata, replace
cls
format _all
translate @Results $fileDir{0}temp.txt, replace
clean
* Import file and extract variable name and format
import delimited v1 v2 using $fileDir{0}tempE.txt, clear ///
delimiter("%") stringcols(_all) encoding("utf-8")
replace v1=trim(v1)
drop if v2==""
replace v2="%"+trim(v2)
gen var1=v1
gen var2=v2
drop v1 v2
gen v1=_n
order v1 var1 var2
save $fileDir{0}varNameFormat, replace
* Create output with variable name, format and codelist ref.
merge 1:1 v1 using $fileDir{0}valLabelName, nogenerate
drop v1
replace var3=var3+"." if var3!=""
gen var=var1+" "+var2+" "+var3
drop var1 var2 var3
* Write output to file
export delimited using $fileDir{0}tempB.txt, delimiter(",") novarnames nolabel replace
filefilter $fileDir{0}tempB.txt $astaDir{0}{2}_VARIABEL.txt, from(",$osEOL") to ("") replace

* CREATE VARIABELBESKRIVELSE
use $inputStata, replace
* Strip all value labels
_strip_labels _all
cls
describe _all
translate @Results $fileDir{0}temp.txt, replace
clean
* Import file and extract variable name and description
import delimited v1 using $fileDir{0}tempE.txt, clear ///
delimiter("\t") stringcols(_all) encoding("utf-8")
* Handle broken line in description
replace v1=v1+" ...NB! LABEL TRUNCATED BY INVALID LINE FEED..." if regexm(v1[_n+1],"^[ ][ ][ ]+")==0
replace v1=trim(stritrim(v1))
drop if v1=="" | regexm(v1,"^.+[ ].+[ ]%.+[ ]*")==0
split v1, gen(var) limit(4)
replace var4="" if var4!="*"
gen v0=length(var1+var2+var3+var4)+4
gen varLabel=substr(v1,v0,.)
replace varLabel=trim(varLabel)
keep varLabel
gen v1=_n
* Create output with variable name and description
merge 1:1 v1 using $fileDir{0}varNameFormat, nogenerate
* If label is empty, use default value
replace varLabel="n.a." if varLabel==""
gen v0=var1+" '"+varLabel+"'"
keep v0
* Write output to file
export delimited using $astaDir{0}{2}_VARIABELBESKRIVELSE.txt, delimiter(tab) novarnames nolabel replace

* CREATE KODELISTE
use $inputStata, replace
cls
label list
translate @Results $fileDir{0}temp.txt, replace
clean
* Import file and extract variable name, value, and value label
import delimited v1 using $fileDir{0}tempE.txt, clear ///
delimiter("\t") stringcols(_all) encoding("utf-8")
* Handle broken line in label
replace v1=v1+" ...NB! LABEL TRUNCATED BY INVALID LINE FEED..." if regexm(v1[_n+1],"^[ ][ ][ ]+")==0
drop if regexm(v1,"^[ ]+.*:$")==0 & regexm(v1,"^[ ]+([0-9]+|\.[a-z])[ ].*$")==0
replace v1=trim(stritrim(v1))
drop if v1==""
drop if regexm(v1,"2[ ]+\.[ ]+translate")>0 | regexm(v1,"1[ ]+\.[ ]+label list")>0
* If value label(s) exist, create output with code list(s)
if _N!=0 {
split v1, gen(var) limit(1)
gen v0=length(var1)+2
gen onlyLabel=substr(v1,v0,.)
replace var1=regexr(var1,":$","")
gen valLabel=var1 if onlyLabel==""
replace var1="" if onlyLabel==""
replace var1="'"+var1+"'" if var1!=""
replace onlyLabel="'"+onlyLabel+"'" if onlyLabel!=""
replace valLabel=var1+" "+onlyLabel if valLabel==""
keep valLabel
* Write output to file
export delimited using $astaDir{0}{2}_KODELIST.txt, delimiter(tab) novarnames nolabel replace
}
else {
cls
display in red "Note: No value labels found"
}

* Delete temporary files on disk
erase $fileDir{0}temp.txt
*erase $fileDir{0}tempA.txt
erase $fileDir{0}tempB.txt
*erase $fileDir{0}tempC.txt
*erase $fileDir{0}tempD.txt
erase $fileDir{0}tempE.txt
erase $fileDir{0}valLabelName.dta
erase $fileDir{0}varNameFormat.dta

/*
CONVERT DATA TO DELIMITED TEXT FILE
Note: Write variable names on the first line
Note: Write text qualifier only if data contain the delimiter
Note: Escape a double quote in string data with a double quote and double quoting of the whole string
Note: Null values in numeric variables are not respresented at all
Note: Special codes for missing values are written as lowercase letters with a preceding dot (.a-.z)
*/
use $inputStata, clear
export delimited using $astaDir{0}{2}.csv, delimiter(";") nolabel replace

