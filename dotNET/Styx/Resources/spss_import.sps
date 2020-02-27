* Encoding: UTF-8.

***********************************.
* Version: 3,0
* Importscript.
* TFL august 2019.
***********************************.

* dan underscripts.

* 1. dan getdatascript.
* 2. dan setvarlabelsscript.
* 3. dan setvaluelabelsscript.
* 4. dan setmissingscript.
* 5. kør scripts.
* 6. slet evt. scripts.

* 0 HANDLES.
file handle dataDir /name '{1}'.
file handle datafil /name 'dataDir\{2}.csv'.
file handle brugerkode /name 'dataDir\{2}_BRUGERKODE.txt'.
file handle kodeliste /name 'dataDir\{2}_KODELISTE.txt'.
file handle variabel /name 'dataDir\{2}_VARIABEL.txt'.
file handle varbesk /name 'dataDir\{2}_VARIABELBESKRIVELSE.txt'.

* 1. dan getscript.
* hent var liste.

 get data
                      /type=txt
                      /file='variabel'
                      /arrangement=delimited
                      /delimiters=' '
                      /variables=varname A5000 varfmt A64 varvallst A64.
execute.

do if $casenum=1.
compute varname=concat('GET DATA  /TYPE=TXT', ' /FILE="datafil"',
                                       ' /DELCASE=LINE /DELIMITERS=";" /ARRANGEMENT=DELIMITED /FIRSTCASE=2 /IMPORTCASE=ALL /VARIABLES=',varname).
end if.
execute.

COMPUTE id=$CASENUM.
 FORMAT id (F8.0).
EXECUTE.

sort cases id (D).
execute.

compute varfmt = 'AUTO'.
do if $casenum=1.
compute varfmt=concat(varfmt,'.').
end if.
execute.

sort cases id (A).
execute.

compute varname=concat(varname,' ',varfmt).

write outfile 'dataDir\getdatascript.sps' / varname.
execute.


* 2. danvarlabelscript.
 get data
                      /type=txt
                      /file='varbesk'
                      /arrangement=delimited
                      /delimiters=' '
                      /QUALIFIER="'"
                      /variables=varname A564 varlabel A500.
execute.


do if $casenum=1.
compute varname=concat('VARIABLE LABELS ',varname).
end if.
execute.

COMPUTE id=$CASENUM.
 FORMAT id (F8.0).
EXECUTE.

sort cases id (D).
execute.

compute varname=concat(varname," '",varlabel,"'").

do if $casenum=1.
compute varname=concat(varname,'.').
end if.
execute.

sort cases id (A).
execute.

write outfile 'dataDir\setvarlabelsscript.sps' / varname.
execute.


* 3. dan valuelabelscript.
 get data
                      /type=txt
                      /file='kodeliste'
                      /arrangement=delimited
                      /delimiters=' '
                      /QUALIFIER="'"
                      /variables=varname A564 varlabel A500.
execute.

do if $casenum=1.
compute varname=concat('VALUE LABELS ',varname).
end if.
execute.

do if varlabel<>''.
compute varlabel=concat("'",varlabel,"'").
compute varname=concat("'",varname,"'").
end if.
execute.

COMPUTE id=$CASENUM.
 FORMAT id (F8.0).
EXECUTE.

sort cases id (D).
execute.

do if $casenum=1.
compute varlabel=concat(varlabel,'.').
end if.
execute.

do if $casenum>1 and lag(varlabel)=''.
compute varlabel=concat(varlabel,' / ').
end if.
execute.

sort cases id (A).
execute.

compute varname=concat(varname,' ',varlabel).

write outfile 'dataDir\setvaluelabelsscript.sps' / varname.
execute.


* 4. dan missingscript.
 get data
                      /type=txt
                      /file='brugerkode'
                      /arrangement=delimited
                      /delimiters=' '
                      /QUALIFIER="'"
                      /variables=varname A64 var1 A50  var2 A50  var3 A50  var4 A50  var5 A50 var6 A50 var7 A50 var8 A50 var9 A50 var10 A50 .
execute.

do if var2<>''.
compute var1=concat(var1,',',var2).
end if.
execute.
do if var3<>''.
compute var1=concat(var1,',',var3).
end if.
execute.
do if var4<>''.
compute var1=concat(var1,',',var4).
end if.
execute.
do if var5<>''.
compute var1=concat(var1,',',var5).
end if.
execute.
do if var6<>''.
compute var1=concat(var1,',',var6).
end if.
execute.
do if var7<>''.
compute var1=concat(var1,',',var7).
end if.
execute.
do if var8<>''.
compute var1=concat(var1,',',var8).
end if.
execute.
do if var9<>''.
compute var1=concat(var1,',',var9).
end if.
execute.
do if var10<>''.
compute var1=concat(var1,',',var10).
end if.
execute.

compute varname=concat('MISSING VALUES ',varname,' (',var1,').').
execute.

write outfile 'dataDir\setmissingscript.sps' / varname.
execute.


* 5. kør scripts.

insert file= 'dataDir\getdatascript.sps'.
insert file= 'dataDir\setvarlabelsscript.sps'.
insert file= 'dataDir\setvaluelabelsscript.sps'.
insert file= 'dataDir\setmissingscript.sps'.
execute.


* 6. Gem .sav fil.

SAVE OUTFILE='dataDir\spss_statistiktfil.sav'
  /COMPRESSED.
