using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Security.Cryptography;


//Missing values

//Der finde 3 typer missing values
//1 tomt felt
//2 BRUGERKODE
//3 Special koder A-Z .a - .z

//Hvis der er defineret værdier under BRUGERKODER, så indskrives disse i researchIndex og "specialNumeric" sættes til FALSE

//Hvis der IKKE er værdier under BRUGERKODER, så gennemløbes samtlige felter hvor er tilknyttet kodelister 
//og hvis kodeliste indeholder specialkoder (A-Z .a - .z), så læses datafilen række for række og der 
//tjekkes om feltet (kolonnen) er en specialkode. De opsamlede koder registreres i researchIndes (og "specialNumeric" sættes til True)



namespace Athena_II_REM
{
    public partial class Form1 : Form
    {
        List<string> SQLReservedWords = new List<string>();
        
        DataConverter ConvertTo1007 = new DataConverter();//Benyttes konvertering af statestik datatyper til XML datatyper
        public EBNF_system EBNF_Sys;//Placeholder til al nødvendig info vedr. afleveringen
       
        //Midlertidig liste som benytter til opslag
        public List<EBNF_brugerkoder> brugerKodeListe;


        string pathToFDfolder = "";
        string pathTo1007folder = "";
        string pathToSchemas = @"C:\Dropbox\SA Forskningsdata\Schemas";

        int allTextGlobalPos = 0;
        string allText = "";

        int tableNum1007 = 0;//Global 1007 tabelnr

        bool contextDoc_readyToTest = true;
        bool indices_readyToTest = true;
        bool metadata_readyToTest = true;
        bool data_readyToTest = true;


        bool indices_OK = false;
        bool contextDoc_mappeindhold_OK = false;
        bool contextDoc_xml_OK = false;

        bool dataFolderStructure_OK = false;
        bool metadata_OK = false;
        bool data_parse_OK = false;
        bool data_typer_OK = false;

        public Form1()
        {
            InitializeComponent();

            toolStripStatusLabel1.Text = "Status:";
            richTextBoxMetadata.WordWrap = false;

            //Indlæs liste med reserverede SQL 99 ord
            string fileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ReservedWord.txt";
            foreach (string line in File.ReadLines(fileName))
            {
                if (line.Trim() != "") 
                SQLReservedWords.Add(line.Trim().ToUpper());
            }


            String[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1)
            {
                //MessageBox.Show(arguments.Length.ToString());
                MessageBox.Show(arguments[1]);
                if (Directory.Exists(arguments[1]) == true)
                {
                    Parse(arguments[1]);
                }
            }
        }


       


        public Boolean isINT(string value)
        {
            int tempint;

            value = value.Replace('.', ',');

            //int.TryParse(value, out tempint);
            return (int.TryParse(value, out tempint));
        }

        private void LOG_Clear()
        {
            richTextBoxLOG.Clear();
        }


        //private void LOG(string txt)
        //{
        //    richTextBoxLOG.AppendText(txt + "\r\n");
        //}


        private void LOG(string txt, bool bold)
        {
            richTextBoxLOG.AppendText(txt + "\n");

            if (txt.Length > 0)
            {
                richTextBoxLOG.Select(richTextBoxLOG.Text.Length - (txt.Length + 1), txt.Length);
                if (bold == true)
                {
                    richTextBoxLOG.SelectionFont = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                }
               // richTextBoxLOG.SelectionColor = Color.Black;
            }

            richTextBoxLOG.ScrollToCaret();
        }


        /// <summary>
        /// LOG
        /// </summary>
        private void LOG(string txt, Color color, bool bold, float size)
        {
            richTextBoxLOG.AppendText(txt + "\n");

            if (txt.Length > 0)
            {
                richTextBoxLOG.Select(richTextBoxLOG.Text.Length - (txt.Length + 1), txt.Length);
                if (bold == true)
                {
                    richTextBoxLOG.SelectionFont = new Font("Microsoft Sans Serif", size, FontStyle.Bold);
                }
                richTextBoxLOG.SelectionColor = color;
            }

            richTextBoxLOG.ScrollToCaret();
        }


        /// <summary>
        /// Benyttes til at opbygge AVID udfra FD.xxxx mappen
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }


        /// <summary>
        /// Find linienummer for bestemt udtryk fx TABELBESKRIVELSE
        /// </summary>
        /// <param name="udtryk"></param>
        /// <returns></returns>
        private int GetLinePos(string udtryk)
        {
            int lnr = 0;
            foreach (string line in richTextBoxMetadata.Lines)
            {
                if (line.Trim() == udtryk)
                {
                    return (lnr);
                }
                lnr++;
            }
            return(-1);
        }


        /// <summary>
        /// Returnerer SYSTEMNAVN (egentlig oprindeligt filformat fx SPSS)
        /// </summary>
        /// <returns></returns>
        private string getSYSTEMNAVN()
        {
            int lnr = GetLinePos("SYSTEMNAVN");

            //Her antages at der ikke kan være dbl. linieskift !!!
            return (richTextBoxMetadata.Lines[lnr + 1]);
        }


        /// <summary>
        /// Returnerer . eller ,
        /// </summary>
        /// <returns></returns>
        private string getDECIMALTEGN()
        {
            int lnr = GetLinePos("DECIMALTEGN");

            //Her antages at der ikke kan være dbl. linieskift !!!
            return (richTextBoxMetadata.Lines[lnr + 1]);
        }


        private string  getREFERENCETABEL()
        {
            //!!! mangler !!!!
            //Skal laves multibel (liste / klasse) eftersom der kan være flere ...
            int lnr = GetLinePos("REFERENCE");
         
            string line = richTextBoxMetadata.Lines[lnr + 1];
            //while (line.Trim() != "")
            //{
            //    //Opdel linie i de elementer som referencen består af

            //    lnr++;
            //    line = richTextBox1.Lines[lnr + 1];
            //}

            //!!!! lige nu returneres bare en uparset linie !!!!!
            return (line);
        }


        /// <summary>
        ///  Tilgår sektion TABELNAVN og returnerer tabelnavn
        /// </summary>
        /// <returns></returns>
        private string getTABELNAVN()
        {
            int lnr = GetLinePos("DATAFILNAVN");
            
            //Her antages at der ikke kan være dbl. linieskift !!!
            return (richTextBoxMetadata.Lines[lnr + 1]);
        }


        /// <summary>
        /// Tilgår sektion TABELBESKRIVELSE og returnerer beskrivelse 
        /// </summary>
        /// <returns></returns>
        private string getTABELBESKRIVELSE()
        {
            int lnr = GetLinePos("DATAFILBESKRIVELSE");
            //Her antages at der ikke kan være dbl. linieskift !!!

            return (richTextBoxMetadata.Lines[lnr + 1]);
        }


        /// <summary>
        /// Parser sektion NØGLEVARIABEL og returnerer liste
        /// </summary>
        /// <returns></returns>
        private List<string> getNØGLEVARIABEL()
        {
            List<string> PNList = new List<string>();
            int lnr = GetLinePos("NØGLEVARIABEL");

            string[] pkValues = richTextBoxMetadata.Lines[lnr + 1].Trim().Split(' ');//Split på mellemrum
            
            foreach (string PN in pkValues)
            {
                if (PN.Trim() != "")
                {
                    PNList.Add(PN);
                    //MessageBox.Show(PN);
                }
            }

            return (PNList);
        }


        private Dictionary<string, string> getVARIABELBESKRIVELSER()
        {
            Dictionary<string, string> varDick = new Dictionary<string,string>();

            string beskrivelse = "";

            int lnr = GetLinePos("VARIABELBESKRIVELSE");
            string line = richTextBoxMetadata.Lines[lnr + 1];
            while (line.Trim() != "")
            {
                string[] items = line.Split(' ');
                
                string variable = items[0].Trim();
            
                //alle delelementer pånær det første
                beskrivelse = line.Substring(items[0].Length, line.Length - items[0].Length);

                //Trim enderne
                beskrivelse = beskrivelse.TrimStart();
                beskrivelse = beskrivelse.TrimEnd();

                //Fjern evt foranstillet '
                if (beskrivelse[0] == '\'')
                {
                    beskrivelse = beskrivelse.Remove(0, 1);
                }

                //Fjern evt bagvedstillet '
                if (beskrivelse[beskrivelse.Length - 1] == '\'')
                {
                    beskrivelse = beskrivelse.Remove(beskrivelse.Length - 1, 1);
                }


                string decodedBeskrivelse = System.Net.WebUtility.HtmlDecode(beskrivelse);
                // if (beskrivelse.Contains('&')) MessageBox.Show(encodedXml);

                varDick.Add(variable, decodedBeskrivelse);    

             
                
                lnr++;
                line = richTextBoxMetadata.Lines[lnr + 1];
            }

            return (varDick);
        }


        /// <summary>
        /// Parse sektion VARIABELBESKRIVELSE og returner hvis fundet
        /// </summary>
        /// <param name="variabel"></param>
        /// <returns></returns>
        //private string getVARIABELBESKRIVELSE(string variabel)
        //{
        //    string result = "";
            
        //    int lnr = GetLinePos("VARIABELBESKRIVELSE");
        //    string line = richTextBoxMetadata.Lines[lnr + 1];
        //    while (line.Trim() != "")
        //    {
        //        string[] items = line.Split(' ');
        //        if (items[0].Trim() == variabel)
        //        {
        //            //alle delelementer pånær det første
        //            result = line.Substring(items[0].Length, line.Length - items[0].Length);

        //            //Trim enderne
        //            result = result.TrimStart();
        //            result = result.TrimEnd();
                    
        //            //Fjern evt foranstillet '
        //            if (result[0] == '\'')
        //            {
        //                result = result.Remove(0, 1);
        //            }

        //            //Fjern evt bagvedstillet '
        //            if (result[result.Length-1] == '\'')
        //            {
        //                result = result.Remove(result.Length - 1, 1);
        //            }
        //            return (result);
        //        }
        //        lnr++;
        //        line = richTextBoxMetadata.Lines[lnr + 1];
        //    }
        //    return (result);
        //}


        //public Boolean isINT(string value)
        //{
        //    int tempint;
            
        //    string newvalue = "";
        //    for (int i = value.Length - 1; i > 0; i--)
        //    {
        //        if (value[i] == '.')
        //        {
        //            newvalue = value.Substring(0, i);
        //            break;
        //        }
        //    }
        //    return(int.TryParse(newvalue, out tempint));
        //}


        //public Boolean isDEC(string value)
        //{
        //    decimal tempDec;
        //    //decimal.TryParse(value, out tempDec);
        //    return (decimal.TryParse(value, out tempDec));
        //}


        /// <summary>
        /// Genererer liste med oplysninger om alle variabler tilhørende en given tabel
        /// </summary>
        /// <returns></returns>
        private List<EBNF_variabel> getVARIABEL_LISTE(string systemNavn)
        {

            Dictionary<string,string> variabelDictonairy = new Dictionary<string,string>();
            variabelDictonairy = getVARIABELBESKRIVELSER();//Hent liste over beskrivelse

            List<EBNF_variabel> vlist = new List<EBNF_variabel>(); 

            int lnr = GetLinePos("VARIABEL");

            //For hver variabel ...
            string line = richTextBoxMetadata.Lines[lnr + 1];
            while (line.Trim() != "")//terminer ved første tomme linie
            {
                this.Text = lnr.ToString();
                Application.DoEvents();
                
                EBNF_variabel v = new EBNF_variabel();
                v.specialKoder = new List<string>();//Forbered liste som evt. kommer til at indeholde specialkoder fx '.u' 'U'
                string[] items = line.Split(' ');

                v.variabelNavn = items[0].Trim();
                v.datatypeOrg = items[1].Trim();

                //MessageBox.Show(v.variabelNavn);

                //Generer 1007 datatype som skal benyttes til konvertering
                v.datatypeOrg1007 = ConvertTo1007.get1007datatype(v.datatypeOrg);
                v.datatype1007 = v.datatypeOrg1007;//Endelig datatype i 1007 der som udgangspunkt den samme som oprindelig, men kan blive ændret senere pga BRUGERKODER

                //Hent variabel beskrivelse fra dictonairy
                if (variabelDictonairy.TryGetValue(v.variabelNavn, out v.variabelBeskrivelse) == false)
                {
                    MessageBox.Show("Kunne ikke finde variabelbeskrivelse for variabel: " + v.variabelNavn + "!");
                }
             

                //Bestem om der er tale om kodelistenavn ved at strippe $ og . og efterfølgende test om der er tale om ren nummerisk værdi
                string tempStr = v.datatypeOrg.Replace('$', ' ').Trim();

                if (tempStr[tempStr.Length - 1] != '.') //FX f5.1
                {
                    //Alm variabel ... ikke en kodeliste
                    //Gælder for SPSS og STATA
                }
             else
                {
                    //Gælder kun SAS og løser en del af problemet med at SAS variabler altid indeholder et '.' i enden (den notation vi ellers havde reserveret til kodelister)
                    if ((tempStr.ToLower() == "yymmdd.") || (tempStr.ToLower() == "yymmdd10.") || (tempStr.ToLower() == "time.") || (tempStr.ToLower() == "time8.") ||
                    (tempStr.ToLower() == "e8601dt19.") || (tempStr.ToLower() == "e8601dt."))
                    {
                       //MessageBox.Show("Alm variabel " + v.datatypeOrg);
                    }
                 else
                    {
                        //!!! Kodelister har et punktum i enden fx 'nn.' om angiver at der er tale om kodeliste af nummerisk type eller '$nn.' son angiver at der er tale om en string 
                        //!!! så det er et problem at SAS variabler indeholder "f"  fx $f12. eller f12. 
                        //!!! SPSS kunne fx have en kodeliste som hedder f24. og det er hermed ikke mulig at skelne om der er tale om variabel eller kodeliste
                        tempStr = tempStr.Replace('.', ' ').Trim();
                        if ((systemNavn.ToLower() == "sas") && (tempStr[0] == 'f')) tempStr = tempStr.Substring(1);//HACK for at håndtere SAS !!!!
                        if (isINT(tempStr) == false)
                        {
                            v.kodelisteNavn = tempStr;// v.datatypeOrg.Replace('$', ' ').Replace('.', ' ').Trim();
                        }
                         //MessageBox.Show("Kodeliste " + v.kodelisteNavn);
                    }
                }

                vlist.Add(v);

                lnr++;
                line = richTextBoxMetadata.Lines[lnr + 1];
            }
          
            return (vlist);
        }


        /// <summary>
        /// Midlertidig liste som indeholder BRUGERKODER
        /// </summary>
        private void createBrugerKodeListe()
        {
            //Start med at slette evt. tidligere værdier
            brugerKodeListe.Clear();

            int lnr = GetLinePos("BRUGERKODE");
            if (lnr == -1) return;//ingen BRUGERKODER

            string line = richTextBoxMetadata.Lines[lnr + 1];
            while (line.Trim() != "")
            {
                //Vi skal ned og finde en linie som starter med variabelnavn
               EBNF_brugerkoder mv = new EBNF_brugerkoder();
               mv.values = new List<string>();

               string[] items = line.Split(' ');

               //Første element
               string head = items[0].Trim();
               //Fjern foranstillet '
               if (head[0] == '\'')
               {
                   head = head.Remove(0, 1);
               }

               for (int i = 0; i < items.Length; i++)
               {
                   if (i == 0)
                   {
                       //KodeListeNavn
                       mv.variabelNavn = items[i].Trim();
                      // MessageBox.Show("Tilføjer " + items[i].Trim());
                   }
                   else
                   {
                       //værdier fx '9''99'
                       string v = items[i].Trim();

                       //Fjern plinger '
                       v = v.Replace("\'","");//Fjern plinger 
                       mv.values.Add(v);
                   }
               }

               brugerKodeListe.Add(mv);
               
               lnr++;
               if (lnr + 1 >= richTextBoxMetadata.Lines.Length) break;
                   
               line = richTextBoxMetadata.Lines[lnr + 1];
            }
        }


        /// <summary>
        ///  Bestem om en variabel har en eller flere tilknyttede BRUGERKODE'er
        /// </summary>
        /// <param name="variabelNavn"></param>
        /// <param name="kode"></param>
        /// <returns></returns>
        private Boolean isBRUGERKODE(string variabelNavn, string kode)
        {
            //Hvis der slet ikke findes nogle missingvalues, så returneres false
            if (brugerKodeListe.Count == 0) return (false);

                foreach (EBNF_brugerkoder mv in brugerKodeListe)
                {
                    //Først skal vi finde ud af om der er værdier forbundet med en given liste
                    if (variabelNavn == mv.variabelNavn)
                    {
                        //her skal listen med værdier gennemløbes og holdes op mod "variabelNavn"
                        if (mv.values.Contains(kode))
                        { 
                            return (true);
                        }
                    }
                }

            return (false);
        }


        public string normalizeINT(string value)
        {
            if (value.Contains('.') == false)
            {
                return (value); //intet ændret 
            }
            
            string newValue = "";
            for (int i = value.Length - 1; i > 0; i--)
            {
                if (value[i] == '.')
                {
                    newValue = value.Substring(0, i); // fx 23.00 --> 23
                    break;
                }
            }

            return (newValue);
        }


        /// <summary>
        /// Parse KODELISTE sektion og indsæt i datastruktur
        /// </summary>
        /// <param name="variabler"></param>
        /// <returns></returns>
        private List<EBNF_kodeliste> getKODELISTE_LISTE()
        {
            List<EBNF_kodeliste> kodeListeListe = new List<EBNF_kodeliste>();
            EBNF_kodeliste kListe = null;
            EBNF_kode newKode = null;

            string preDefinedAll = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ_";

            //*** Først skabes simpel liste over kodelister ***
            int lnr = GetLinePos("KODELISTE");
            string line = richTextBoxMetadata.Lines[lnr + 1];

            while (line.Trim() != "")
            {
                //Hvis der er tale om en ny kode, så tilføj den ...
                if (line.Trim().Contains('\'') == false)
                {
                    kListe = new EBNF_kodeliste();
                    kListe.kodelisteNavn = line.Trim();
                    kListe.datatype1007 = "INTEGER";//Default integer intil andet er bevist !!! 
                    kodeListeListe.Add(kListe);

                    //Test for ulovlige tegn i kodelistenavn ...
                    for (int i = 0; i < kListe.kodelisteNavn.Length; i++)
                    {
                        char ch = kListe.kodelisteNavn.ToUpper()[i];
                        if (!preDefinedAll.Contains(ch))
                        {
                            LOG("Kodeliste " + kListe.kodelisteNavn + " " + " indeholder en eller flere ulovlige tegn!", Color.Red, false, 10);
                            break;
                        }
                    }

                    //Klar til at tilføje selve kodeværdierne
                    kListe.koder = new List<EBNF_kode>();

                    //Tilføj evt. brugerkoder fra brugerkodeliste så de også optræder på kodeliste ...
                    foreach (EBNF_brugerkoder brugerKoder in brugerKodeListe)
                    {
                        if (kListe.kodelisteNavn == brugerKoder.variabelNavn)
                        {
                            //!!! skal senere ændres til en test i stedet for en ændring af data !!!
                            //foreach (string brugerKodeValue in brugerKoder.values)
                            //{
                            //    //Tjek om den allerede findes i kodelisten ...
                            //    Boolean findes = false;
                            //    foreach (EBNF_kode kodeValue in kListe.koder)
                            //    {
                            //        if (kodeValue.kode == brugerKodeValue)
                            //        {
                            //            findes = true;
                            //            break;
                            //        }
                            //    }

                            //    if (findes == false) //ikke fundet, så vi tilføjer til listen ...
                            //    {
                            //        newKode = new EBNF_kode();
                            //        newKode.kode = brugerKodeValue;
                            //        newKode.value = "";
                            //        kListe.koder.Add(newKode);//tilføj kode til kodeliste
                                    
                            //        //Hvis en nummerisk værdi viser sig at være ikke-nummerisk fx 'u'. så ændres datatype til CHAR
                            //        if (kListe.datatype1007 == "INTEGER")
                            //        {
                            //            if (ConvertTo1007.isINT(newKode.kode) == false)
                            //            {
                            //                //Ret op på datatype i hovedtabels variabel og koden selv ...
                            //                kListe.datatype1007 = "CHAR";
                            //            }
                            //        }

                            //    }
                            //}
                        }
                    }  //foreach (EBNF_brugerkoder brugerKoder in brugerKodeListe)
                   
                }
             else
                {
                    
                    //Tilføj kode
                //    newKode = new EBNF_kode();

                    //Opbryd i elementer - første element er variabelnavn og resten er værdien (kan være flere elementer end 2 !)
                    string[] items = line.Trim().Split(' ');

                    string newKodeValue = items[0].Trim().Replace("\'", "");
                    newKodeValue = normalizeINT(newKodeValue);//Fjern evt .00

                    //Tjek om kode allerede findes i kodelisten ....
                    Boolean findes = false;
                    foreach (EBNF_kode kode in kListe.koder)
                    {
                        if (kode.kode == newKodeValue)
                        {
                           // MessageBox.Show(kode.kode + " findes allerede");
                            findes = true;
                            break;
                        }
                    }

                    if (findes == false)
                    {
                        //Tilføj kode
                        newKode = new EBNF_kode();
                        newKode.kode = newKodeValue;
                        newKode.value = line.Substring(items[0].Length).Replace("\'", "").Trim();//!!!! Kan evt ændres så kun foranstillet og efterstillet ' fjernes

                        //Hvis en nummerisk værdi viser sig at være ikke-nummerisk fx 'u'. så ændres datatype til CHAR
                        if (kListe.datatype1007 == "INTEGER")
                        {
                            if (ConvertTo1007.isINT(newKode.kode) == false)
                            {
                                //Ret op på datatype i hovedtabels variabel og koden selv ...
                                kListe.datatype1007 = "CHAR";
                            }
                        }

                        kListe.koder.Add(newKode);//tilføj kode til kodeliste
                    }
                }

                
                lnr++;
                line = richTextBoxMetadata.Lines[lnr + 1];
            
            } //while (line.Trim() != "")


            
            return (kodeListeListe);//Returnerer liste over kodelister ...
        }


        /// <summary>
        /// Tester om udtryk overholder SQL 99 krav til navngivning
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string isValidSQLname(string str)
        {
            string preDefinedAll = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ_";
            string preDefinedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ_";
            string preDefinedNumbers = "0123456789";

            str = str.ToUpper();

            //Test op mod beskyttede SQL 99 ord før evt quotes fjernes
            if (SQLReservedWords.Contains(str) == true)
            {
                return ("må ikke benytte SQL 99 beskyttet ord!");
            }

            //Tjek om udtryk er quoted "" og fjern herefter quotes
            if (str[0] == '\"' && str[str.Length - 1] == '\"')
            {
                str = str.Substring(1, str.Length - 2);//Quotes fjernes ...
            }

            //Test for max længde
            if (str.Length > 128)
            {
                return ("består af mere end 128 karakterer!");
            }

            //Test for start med nummer
            char start = str[0];
            if (preDefinedNumbers.Contains(start))
            {
                return ("må ikke starte med et nummer!");
            }

            //Test første karakter for ulovlige tegn ...
            if (!preDefinedChars.Contains(start))
            {
                 return ("indeholder en eller flere ulovlige tegn!");
            }

            //.... test de øvrige karakterer for ulovlige tegn
            for (int i = 1; i < str.Length ; i++)
            {
                char ch = str[i];
                if (!preDefinedAll.Contains(ch))
                {
                    return ("indeholder en eller flere ulovlige tegn!");
                }
            }

            return "";
        }

       
        /// <summary>
        /// Hoved procedure for indlæsning/parsning af metadata til datastruktur, som senere kan vises som treeView og udlæses til 1007+ 
        /// </summary>
        private void CreateMetadataObjekter()
        {
            //*** En eller flere tabeller ***
            EBNF_Sys.tabListe = new List<EBNF_tabel>();//Skab tom liste over tabeller
            
            //**** For hver tabel .... ****
            foreach (string table_folder in Directory.GetDirectories(Path.Combine(pathToFDfolder,"Data"),"*.*",SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.Combine(table_folder,Path.GetFileName(table_folder) + ".txt");
               // string fileName = Directory.GetFiles(table_folder, "*.txt", SearchOption.TopDirectoryOnly)[0];//Usikker måde at gøre dette her på .... !
              
                toolStripStatusLabel1.Text = "Status: indlæser metadata " + Path.GetFileName(fileName);
                statusStrip1.Update();
                
                //string data = File.ReadAllText(fileName);
                //string data = File.ReadAllText(fileName, Encoding.UTF8);

                toolStripStatusLabel1.Text = "Status: parser metadata " + Path.GetFileName(fileName);
                statusStrip1.Update();

                richTextBoxMetadata.Clear();
                richTextBoxMetadata.Text = File.ReadAllText(fileName); 

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //!!! Grundliggende EBNF syntaks test er ikke foretaget !!!
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                EBNF_tabel newTab = new EBNF_tabel();

                newTab.systemNavn = getSYSTEMNAVN();// "SPSS";
                //   EBNF_Sys.decimaltegn = getDECIMALTEGN();// ".";                
                
                newTab.tableNavn = getTABELNAVN();// Navn på tabel angivet i metadata
                
                //Test for startkarakter, ulovlige tegn, max længde på 128 og reserverede SQL ord med mindre de er ""
                string result = isValidSQLname(newTab.tableNavn);
                if (result != "")
                {
                    LOG("Tabelnavn " + newTab.tableNavn + " " + result, Color.Red, false, 10);
                }


                newTab.filePathCSV = Path.ChangeExtension(fileName, ".CSV");
                if (File.Exists(newTab.filePathCSV) == false) MessageBox.Show("Manglende CSV fil: " + newTab.filePathCSV);
           
                newTab.tabelBeskrivelse = getTABELBESKRIVELSE();// "Dette er en vilkårlig tabel.";

                newTab.PNList = getNØGLEVARIABEL();//"ID";
                newTab.tabelReferencer = getREFERENCETABEL();//Lige nu benyttes den ikke til noget ... multibel???

                //*** Variabler lægges i liste ***
                toolStripStatusLabel1.Text = "Status: Opbygger liste over variabler ...";
                statusStrip1.Update();
                //Liste med variabler hentes
                newTab.variabelListe = new List<EBNF_variabel>();//Laver klar til variabler
                newTab.variabelListe = getVARIABEL_LISTE(newTab.systemNavn);//Listen hentes (midlertidig brug af info om hvorvidt data kommer fra SAS ....)

                //Test for startkarakter, ulovlige tegn, max længde på 128 og reserverede SQL ord med mindre de er ""
                toolStripStatusLabel1.Text = "Status: Tester variabler for navngivning ...";
                statusStrip1.Update();
                for (int i = 0; i < newTab.variabelListe.Count; i++)
                {
                    result = isValidSQLname(newTab.variabelListe[i].variabelNavn);
                    if (result != "")
                    {
                        LOG("Kolonnenavn " + newTab.variabelListe[i].variabelNavn + " " + result, Color.Red, false, 10);
                    }
                }

                
              //  MessageBox.Show(newTab.variabelListe.Count.ToString());

                //*** En eller flere kodelister ***
                toolStripStatusLabel1.Text = "Status: Parser kodelister og BRUGERKODER ...";
                statusStrip1.Update();

                createBrugerKodeListe();//Lav temperer liste over BRUGERKODER

                //newTab.kodeListeListe = getKODELISTE_LISTE(ref newTab.variabelListe);
                newTab.kodeListeListe = getKODELISTE_LISTE();

                //Ret op på datatype i hovedtabels variabler ...
                //!!! skal ændres til at være en test i stedet for en opretning !!!
                //foreach (EBNF_variabel variabel in newTab.variabelListe)
                //{
                //    foreach (EBNF_kodeliste kodeList in newTab.kodeListeListe)
                //    {
                //        if (variabel.kodelisteNavn == kodeList.kodelisteNavn)
                //        {
                //            if (variabel.datatype1007 != kodeList.datatype1007)
                //            {
                //                //MessageBox.Show("Udskifter " + variabel.datatype1007 + " med " + kodeList.datatype1007);
                //                variabel.datatype1007 = kodeList.datatype1007;
                //                break;
                //            }
                //        }
                //    }
                //}


                //*** Til slut gemmes tabel ***
                EBNF_Sys.tabListe.Add(newTab);
            }//For hver tabel
         }


        /// <summary>
        /// Omdanner hierakiet til en træstruktur beregnet til visning
        /// </summary>
        private void CreateView()
        {
            //Systemnavn
          //  treeView1.Nodes.Add("Systemnavn="+EBNF_Sys.systemNavn);

            //Decimaltegn
         //   treeView1.Nodes.Add("Decimaltegn ="+ EBNF_Sys.decimaltegn);

            //Referencetabel
            //treeView1.Nodes.Add("Tabelreferencer =" + EBNF_Sys.tabelReferencer);

            //*** Tabeller ***
            for (int tabNr = 0; tabNr < EBNF_Sys.tabListe.Count ; tabNr++ )
            {
                TreeNode nodeTabel = new TreeNode();
                //Tabelnavn mv
                var temp1 = EBNF_Sys.tabListe[tabNr];
                string PN = string.Join(",", temp1.PNList.ToArray());//Smart måde at udlæse til kommasep. string
                nodeTabel.Text = temp1.tableNavn + " (PN=" + PN + ")" + " (" + temp1.tabelBeskrivelse + ")";

                //*** systemnavn ***
                if (temp1.systemNavn != "")
                {
                    nodeTabel.Nodes.Add("Systemnavn = " + temp1.systemNavn);
                }

                //*** Tabel referencer ***
                if (temp1.tabelReferencer != "")
                {
                    nodeTabel.Nodes.Add("TABEL_REFERENCER = " + temp1.tabelReferencer);
                }

                //*** Variabler ***
                int index = treeView1.Nodes.Count - 1;//Sidst tilføjede node
                for (int varNr = 0; varNr < EBNF_Sys.tabListe[tabNr].variabelListe.Count; varNr++)
                {
                    var temp2 = EBNF_Sys.tabListe[tabNr].variabelListe[varNr];
                   // treeView1.Nodes[index].Nodes.Add(temp2.variabelNavn + " (datatype="+temp2.datatypeOrg+" --> "+ temp2.datatype1007+")" + " (" + temp2.variabelBeskrivelse + ")");
                    nodeTabel.Nodes.Add(temp2.variabelNavn + " (datatype=" + temp2.datatypeOrg1007 + " --> " + temp2.datatype1007 + ")" + " (" + temp2.variabelBeskrivelse + ")");
                }
               

                //*** Kodelister ***
                for (int kodNr = 0; kodNr < EBNF_Sys.tabListe[tabNr].kodeListeListe.Count; kodNr++)
                {
                    TreeNode nodeKode = new TreeNode();
                    nodeKode.Text = EBNF_Sys.tabListe[tabNr].kodeListeListe[kodNr].kodelisteNavn;
                  
                    //Tilføj variabler til node
                    index = treeView1.Nodes.Count - 1;//Sidst tilføjede node
                    for (int kNr = 0; kNr < EBNF_Sys.tabListe[tabNr].kodeListeListe[kodNr].koder.Count; kNr++)
                    {
                        var temp3 = EBNF_Sys.tabListe[tabNr].kodeListeListe[kodNr].koder[kNr];
                        nodeKode.Nodes.Add(temp3.kode + " = " + temp3.value);
                    }

                    //Node med opl. om kodeliste tilføjes tabelnoden
                    nodeTabel.Nodes.Add(nodeKode);
                }
               
                //Tabelnoden med tilhørende variabler og kodelister tilføjes til træ
                treeView1.Nodes.Add(nodeTabel);

            }//For hver tabel i afleveringen
        }


        private bool Test_FD_mappestruktur(string FDfolderPath)
        {
            string folderName = Path.GetFileName(FDfolderPath);

            //Test for eksistens
            if (Directory.Exists(FDfolderPath) == false)
            {
                LOG("FD mappe ikke fundet!",Color.Red,false,10);
                return(false);
            }

            //Test for korrekt postfix
            string FDNrPrefix = folderName.Substring(0,3);
            if (FDNrPrefix != "FD.")
            {
                LOG("Mappen " + folderName + " ikke valid!\r\nKorrekt postfix \"FD.\" ikke fundet!", Color.Red, false, 10);
                return (false);
            }

            //Test for eksisterende FD nummer
            string FDNrStr = folderName.Substring(3);
            {
                if (FDNrStr.Length < 1)
                {
                    LOG("Mappen " + folderName + " ikke valid!\r\nMangler FD nummer (fx \"FD.19234\")!", Color.Red, false, 10);
                    return (false);
                }
                
                int n;
                bool isNumeric = int.TryParse(FDNrStr, out n);
                
                if (isNumeric == false)
                {
                    LOG("Mappen " + folderName + " ikke valid!\r\nMappen skal bestå af prefix \"FD.\" efterfuldt af et FD nummer (fx \"FD.19234\")!", Color.Red, false, 10);
                    return (false);
                }
            }
          
            //Der testes for om mappen DATA eksisterer
            if (Directory.Exists(Path.Combine(FDfolderPath, "Data")) == false)
            {
                LOG("Undermappen " + "\"\\" + folderName + "\\" + "Data\" ikke fundet!", Color.Red, false, 10);
                data_readyToTest = false; 
            }
           

            //Der testes for om mappen ContextDocumentation eksisterer
            if (Directory.Exists(Path.Combine(FDfolderPath, "ContextDocumentation")) == false)
            {
                LOG("Undermappen " + "\"\\" + folderName + "\\" + "ContextDocumentation\" ikke fundet!", Color.Red, false, 10);
                contextDoc_readyToTest = false;
            }


            //Der testes for om mappen Indices eksisterer
            if (Directory.Exists(Path.Combine(FDfolderPath, "Indices")) == false)
            {
                LOG("Undermappen " + "\"\\" + folderName + "\\" + "Indices\" ikke fundet!", Color.Red, false, 10);
                indices_readyToTest = false;
                metadata_readyToTest = false;
            }


            //Test for uvedkommende mapper i FD mappen
            if (Directory.GetDirectories(FDfolderPath, "*.*", SearchOption.TopDirectoryOnly).Length != 3)
            {
                LOG("Der er fundet uvedkommende mapper i " + "\\" + folderName + " !", Color.Red, false, 10);
                return (false);
            }


            return (true); // Ingen fejl
        }


        /// <summary>
        /// Detailtest af Indices
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_Indices(string FDfolderPath)
        {
            bool result = true;//Default er alt OK
           
            string indicesFolderPath = Path.Combine(FDfolderPath, "Indices");

            //Test for uvedkommende mapper i Indices mappen
            if (Directory.GetDirectories(indicesFolderPath).Length != 0)
            {
                LOG("Der er fundet uvedkommende mapper i " + "\\" + indicesFolderPath + " !", Color.Red, false, 10);
                result = false;
            }

            //Test for archiveIndex.xml 
            if (File.Exists(Path.Combine(indicesFolderPath, "archiveIndex.xml")) == false)
            {
                LOG("Indexfilen \"" + Path.Combine(indicesFolderPath, "archiveIndex.xml") + "\" ikke fundet!", Color.Red, false, 10);
                result = false;
            }
            else
            {
                //Her kan archiveIndex.xml parses og testes om der er sammenhæng mellem archiveIndex.xml AV nummer og FD nummer 
            }

            //Test for contextDocumentation.xml
            if (File.Exists(Path.Combine(indicesFolderPath, "contextDocumentationIndex.xml")) == false)
            {
                LOG("Indexfilen \"" + Path.Combine(indicesFolderPath, "contextDocumentationIndex.xml") + "\" ikke fundet!", Color.Red, false, 10);
                result = false;
            }


            //Her testes for unødige filer udover de 2 index !!!
            if (Directory.GetFiles(indicesFolderPath, "*.*", SearchOption.TopDirectoryOnly).Length != 2)
            {
                LOG("Der er fundet uvedkommende filer i mappen " + "\"" + indicesFolderPath + "\" !", Color.Red, false, 10);
                result = false;
            }

            return(result);
        }

        /// <summary>
        /// Detailtest af contextDocumentation
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_contextDocMappeindhold(string FDfolderPath)
        {
            bool result = true;

            //Test for om docCollection1 findes ... 
            string tmpStr = Path.Combine(Path.Combine(FDfolderPath, "ContextDocumentation"), "docCollection1");
            if (Directory.Exists(tmpStr) == false)
            {
                LOG("Undermappen " + "\"" + @tmpStr + "\\" + " ikke fundet!", Color.Red, false, 10);
                result = false;
            }
            else
            {
                //Test om alle mapper er nummeriske og at de hver indeholder mindst ét dokument af typen TIFF, MP3, JP2, eller MPG

                //Test at contextDocumentation.xml kan parses
                if (indices_OK == true)
                {
                    //Test sammenhæng mellem contextDocumentation og filerne i mappen
                }
            }


            return (result);
        }


        /// <summary>
        /// Detailtest af contextDocumentation
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_contextDoc_XML(string FDfolderPath)
        {
            bool result = true;

            //Test for om docCollection1 findes ... 
            string tmpStr = Path.Combine(Path.Combine(FDfolderPath, "ContextDocumentation"), "docCollection1");
            if (Directory.Exists(tmpStr) == false)
            {
                LOG("Undermappen " + "\"" + @tmpStr + "\\" + " ikke fundet!", Color.Red, false, 10);
                result = false;
            }
            else
            {
                //Test om alle mapper er nummeriske og at de hver indeholder mindst ét dokument af typen TIFF, MP3, JP2, eller MPG

                //Test at contextDocumentation.xml kan parses
                if (indices_OK == true)
                {
                    //Test sammenhæng mellem contextDocumentation og filerne i mappen
                }
            }


            return (result);
        }


        /// <summary>
        /// Detailtest af data
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_dataFolderStructure(string FDfolderPath)
        {
            //string folderName = Path.GetFileName(FDfolderPath);
            bool result = true;
         
            //Test at der findes de forventede filer i hver tabelmappe
            foreach (string table_folder in Directory.GetDirectories(Path.Combine(pathToFDfolder, "Data"), "*.*", SearchOption.TopDirectoryOnly))
            {
                bool tableOK = true;
                string fileNameTXT = Path.Combine(table_folder, Path.GetFileName(table_folder) + ".txt");
                if (File.Exists(fileNameTXT) == false)
                {
                    LOG("Manglende metadatafil " + fileNameTXT, Color.Red, false, 10);
                    tableOK = false;
                }
                string fileNameCSV = Path.Combine(table_folder, Path.GetFileName(table_folder) + ".csv");
                if (File.Exists(fileNameCSV) == false)
                {
                    LOG("Manglende datafil " + fileNameCSV, Color.Red, false, 10);
                    tableOK = false;
                }

                //Test for unødige filer i mappen
                if (tableOK == true)
                {
                    if (Directory.GetFiles(table_folder,"*.*", SearchOption.AllDirectories).Length != 2)
                    {
                        LOG("Der findes uvedkommende fil(er) i mappen " + table_folder, Color.Red, false, 10);
                        tableOK = false;
                    }
                }

                //Test for unødige mapper i mappen
                if (Directory.GetDirectories(table_folder, "*.*", SearchOption.AllDirectories).Length != 0)
                {
                    LOG("Der findes uvedkommende mappe(r) i mappen " + table_folder, Color.Red, false, 10);
                    tableOK = false;
                }
   
                //Overordnet fejl registreres
                if (tableOK == false) result = false;
            }


            return (result);
        }


        /// <summary>
        /// Detailtest af metadata
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_metadata(string FDfolderPath)
        {
            bool result = true;

            string folderName = Path.GetFileName(FDfolderPath);

            //metadata kan testes op mod EBNF, entydig navngivning af variabler og forretningsregler mv.


            return (result);
        }


        /// <summary>
        /// Detailtest af data parse mv
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_data_parse(string FDfolderPath)
        {
            bool result = true;

            string folderName = Path.GetFileName(FDfolderPath);

            // !!!!!!!!!!!!
            //Datafiler skal evt. parses og testes !? 
            //Lige nu sker indlæsning og test udelukkende når AV skabes ....
            //!!!!!!!!!!!!

            return (result);
        }



        private void ParseMaintable()
        {
            for (int tblNr = 0; tblNr < EBNF_Sys.tabListe.Count; tblNr++)
            {
                string fileName = EBNF_Sys.tabListe[tblNr].filePathCSV;

                toolStripStatusLabel1.Text = "Status: tester CSV fil " + Path.GetFileName(fileName);
                statusStrip1.Update();

                //*** Her indlæses data ud post for post ****
                openCSVDATA(fileName);

                //!!! første linie med kolonnenavne fjernes !!!!!
                for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].variabelListe.Count; ii++)
                {
                    //Hent næste post ... og tjek kolonnenavn i CSV med kolonnenavn i metadata
                    string dataCSV = getNextCSVDATA();
                    string metadataName = EBNF_Sys.tabListe[tblNr].variabelListe[ii].variabelNavn;
                    
                    if (dataCSV.Trim() != metadataName.Trim())
                    {
                        LOG("Kolonnenavn " + dataCSV.Trim() + " angivet i CSV fil svarer ikke overens med kolonnenavn angivet i metadata " + metadataName.Trim() + "!", Color.Red, false, 10);
                    }
                    //MessageBox.Show(dataCSV + "    " + metadataName);
                }

                //For hver række med data ...
                while (allTextGlobalPos < allText.Length - 1)
                {
                    //**** For hver post i række ...
                    int colCounter = 0;
                    for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].variabelListe.Count; ii++)
                    {
                        //Hent næste post ... ....
                        string dataCSV = getNextCSVDATA();

                       // string data1007 = ConvertTo1007.Convert_data_to_1007format(dataCSV, EBNF_Sys.tabListe[tblNr].variabelListe[ii].datatypeOrg);

                        colCounter++;

                        if ((allTextGlobalPos == allText.Length-1) && (colCounter != EBNF_Sys.tabListe[tblNr].variabelListe.Count))
                        {
                            LOG("Udlæsning af kolonner fra filen " + dataCSV.Trim() + " fejlede!", Color.Red, false, 10);
                            MessageBox.Show("Fejl:" + dataCSV.Trim() + "Position:" + allTextGlobalPos.ToString()+" ud af " + allText.Length.ToString());
                        }
                       
                    }
                }//While ...
            }// for (int tblNr = 0; tblNr < EBNF_Sys.tabListe.Count; tblNr++) ....
        }


        /// <summary>
        /// Detailtest af datatyper mv.
        /// </summary>
        /// <param name="FDfolderPath"></param>
        /// <returns></returns>
        private bool Test_data_typer(string FDfolderPath)
        {
            bool result = true;

            string folderName = Path.GetFileName(FDfolderPath);

            // !!!!!!!!!!!!
            //Datafiler skal evt. parses og testes !? 
            //Lige nu sker indlæsning og test udelukkende når AV skabes ....
            //!!!!!!!!!!!!

            return (result);
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //Sti til FD folder (global variabel)
            pathToFDfolder = @shellTreeView1.SelectedPath;
            Parse(pathToFDfolder);
        }

        private void Parse(string pathToFDfolder)
        {
            
            //Klargøring test af segmenter
            contextDoc_readyToTest = true;
            indices_readyToTest = true;
            metadata_readyToTest = true;
            data_readyToTest = true;

            LOG_Clear();
            treeView1.Nodes.Clear();
            
            //Test overordnede mappestruktur og navngivning og foretag en pretest af de 4 segmenter
            LOG("", false);
            LOG("----- Overordnet mappestruktur -----", true);
            bool FD_OK = Test_FD_mappestruktur(pathToFDfolder);


            //Hvis overordnede test er OK så fortsæt med detailtest de enkelte segmenter
            if (FD_OK == true)
            {
                LOG("Overordnet mappestruktur og navngivning testet OK", Color.Green, false, 10);
                
                //*** Indices ***
                LOG("", false);
                LOG("----- Indices -----", true);
                indices_OK = Test_Indices(pathToFDfolder);
               
                if (indices_OK == true) //Status efter test
                {
                    //Grøn
                    LOG("Indices test OK", Color.Green, false, 10);
                }
             else
                {
                    //Rød
                    LOG("Indices test fejlede!", Color.Red, false, 10);
            //        return;
                }

                //*** ContextDocumentation ***
                LOG("", false);
                LOG("----- ContextDocumentation mapper -----", true);

                contextDoc_mappeindhold_OK = Test_contextDocMappeindhold(pathToFDfolder);
                if (contextDoc_mappeindhold_OK == true)//Status efter test
                {
                    //Grøn
                    LOG("ContextDocumentation test af dokumenter OK", Color.Green, false, 10);
                }
             else
                {
                    //Rød
                    LOG("ContextDocumentation test af dokumenter fejlede!", Color.Red, false, 10);
                    //return;
                }


                //contextDocumentation metadata/xml testes hvis forudsætningerne er tilstede ...
                LOG("",false);
                LOG("----- ContextDocumentation XML -----", true);
                if (indices_OK == true)
                {
                    contextDoc_xml_OK = Test_contextDoc_XML(pathToFDfolder);
                    if (contextDoc_mappeindhold_OK == true)//Status efter test
                    {
                        //Grøn
                        LOG("ContextDocumentation metadata test OK", Color.Green, false, 10);
                    }
                    else
                    {
                        //Rød
                        LOG("ContextDocumentation metada test fejlede!", Color.Red, false, 10);
                        //return;
                    }
                }
                else
                {
                    LOG("Test af contextDocumentation metadata ikke udført pga manglende forudsætninger!", Color.Red, false, 10);
                }


                //*** Data mappe struktur***
                LOG("", false);
                LOG("----- Datamappe struktur -----", true);
                dataFolderStructure_OK = Test_dataFolderStructure(pathToFDfolder);
                if (dataFolderStructure_OK == true)  //Status efter test
                {
                    //Grøn
                    LOG("Test af Datamappens struktur OK", Color.Green, false, 10);
                }
                else
                {
                    //Rød
                    LOG("Test af Datamappens struktur fejlede!", Color.Red, false, 10);
                    //return;
                }

                LOG("", false);
                LOG("----- Metadata -----", true);
                if (dataFolderStructure_OK == true)
                {
                    //*** Metadata ***
                    //!!!!! ikke implementeret her !!!!!
                    metadata_OK = Test_metadata(pathToFDfolder);
                    if (metadata_OK == true)  //Status efter test
                    {
                        //Grøn
                        LOG("Metadata test OK", Color.Green, false, 10);
                    }
                    else
                    {
                        //Rød
                        LOG("Metadata test fejlede!", Color.Red, false, 10);
                        return;
                    }
                }
                else
                {
                    LOG("Test af Metadata ikke udført pga manglende forudsætninger!", Color.Red, false, 10);
                }


                LOG("", false);
                LOG("----- Data struktur og typer -----", true);
                if (dataFolderStructure_OK == true)
                {

                    //*** Data parse ***
                    toolStripStatusLabel1.Text = "Status: parser data ...";
                    statusStrip1.Update();
                    //!!!!!! ikke implementeret her !!!!!
                    data_parse_OK = Test_data_parse(pathToFDfolder);
                    if (data_parse_OK == true)  //Status efter test
                    {
                        //Grøn
                        LOG("Data parse OK", Color.Green, false, 10);
                    }
                    else
                    {
                        //Rød
                        LOG("Data parse fejlede!", Color.Red, false, 10);
                        return;
                    }


                    //*** Data type parse ***
                    //Datatyper kan kun testes hvis vi kender de enkelte kolonners datatype fra metadata
                    if (metadata_OK == true)
                    {

                        data_typer_OK = Test_data_typer(pathToFDfolder);
                        if (data_typer_OK == true)  //Status efter test
                        {
                            //Grøn
                            LOG("Datatype test OK", Color.Green, false, 10);
                        }
                        else
                        {
                            //Rød
                            LOG("Datatype test fejlede!", Color.Red, false, 10);
                            return;
                        }
                    }
                }
                else
                {
                    LOG("Test af Data ikke udført pga manglende forudsætninger!", Color.Red, false, 10);
                }


                //Hvis alle 4 deltest er OK, så kan vi gå videre med at indlæse FD metadata og teste navngivning etc.
                if (indices_OK = true && contextDoc_mappeindhold_OK == true && dataFolderStructure_OK == true && metadata_OK == true && data_parse_OK == true && data_typer_OK == true)
                {
                    LOG("",false);
                    LOG("----- Indlæsning og visning af afleveringen -----",true);
                    
                    //Sti til skemaer (global variabel)
                    pathToSchemas = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Schemas");
                

                    //Struktur, metadata (EBNF) afsluttet
                    EBNF_Sys = new EBNF_system();
                    brugerKodeListe = new List<EBNF_brugerkoder>(); //Midlertidig liste som benytter til opslag

                    //!!! EBNF syntaks test er ikke foretaget !!!
                    //Her sker pt. parse og indlæsning af metadafiler ... på sigt skal denne (EBNF) test ske på et tidligere tidspunkt eller smelte sammen?
                    LOG("Indlæser metadata ...",false);
                    toolStripStatusLabel1.Text = "Status: Indlæser metadata ...";
                    statusStrip1.Update();
                    CreateMetadataObjekter();

                    //!!! hvad hvis opbygning af struktur fejlede???? !!!!!
                    //**** Tester CSV fil tabel for tabel *****
                    ParseMaintable();


                    LOG("Opbygger visning ...", false);
                    toolStripStatusLabel1.Text = "Status: Opbygger view ...";
                    statusStrip1.Update();
                    CreateView();//Træstruktur vises ...

                    LOG("Indlæsning afsluttet", false);
                    toolStripStatusLabel1.Text = "Status: Done";
                    statusStrip1.Update();
                }
            }
            else
            {
                //Overordnetest test af FD fejlede og vi terminerer ...
                LOG("",false);
                LOG("Test afbrudt pga fejl i mappestruktur og//eller fejlagtig navngivning!", Color.Red, false, 10);
                toolStripStatusLabel1.Text = "Status: Fejl!";
                statusStrip1.Update();
            }
            
        }


        /// <summary>
        /// Indlæsning og klargøring af data fra CSV fil
        /// </summary>
        /// <param name="path"></param>
        private void openCSVDATA(string path)
        {
            allText = File.ReadAllText(path, Encoding.UTF8);

            //Her antager jeg at der ikke findes CR eller LF eller CRLF inden i tekst "", hvilket altså bør håndteres i endelig udgave af Athena 
            allText = allText.Replace(System.Environment.NewLine,";");

            allTextGlobalPos = 0;//Husk at nulstille ;-)
        }


        /// <summary>
        /// Parser som traverserer ned gennem data fra CSV og returnerer post
        /// </summary>
        /// <returns></returns>
        private string getNextCSVDATA()
        {
            Boolean ignoreSep = false;
            Boolean done = false;
            string result = "";

            while (done == false)
            {
                char value = allText[allTextGlobalPos];
           
                //Skift tilstand hver gang der forekommer et " tegn
                if (value == '"') ignoreSep = !ignoreSep;

                //Vi tjekker om separator skal separere eller ignoreres ...
                if (((value == ';') || (value == 10) || (value == 13)) && (ignoreSep == false))
                {
                    done = true;
                }
             else 
                {
                    //Resultat samles sammen
                    result = result + value;
                }

                allTextGlobalPos++;

                //Vi har læst sidste karakter i filen og ønsker derfor et exit
                if (allTextGlobalPos > (allText.Length - 1))
                {
                    done = true;
                }
            }

            return (result);
        }

        /// <summary>
        /// Procedure som skaber kodetabeler udfra informationer hentet fra metadatafil 
        /// </summary>
        /// <param name="tblNr"></param>
       private void createKodeTables(int tblNr)
       {
           toolStripStatusLabel1.Text = "Status: skaber kodetabeller til 1007 tabel " + tblNr.ToString();
           statusStrip1.Update();
           
           //For hver kodeliste ...
          for (int i = 0; i < EBNF_Sys.tabListe[tblNr].kodeListeListe.Count ; i++)
          {
              tableNum1007++; //Global tæller som holder styr på 1007 tabelnr fx table23
              string TableNumStr = "table" + tableNum1007.ToString();
              EBNF_Sys.tabListe[tblNr].kodeListeListe[i].tabelNavn1007 = TableNumStr;//Skal bruges når tabelIndex.xml skabes
             
              string outputFolder = Path.Combine(pathTo1007folder, "Tables") + "\\" + TableNumStr;
              Directory.CreateDirectory(outputFolder);

              string fileName = Path.Combine(outputFolder, TableNumStr + ".xml");
              XmlTextWriter xw = new XmlTextWriter(fileName, Encoding.UTF8);
              //Skab xmlfil og header
              xw.Formatting = Formatting.Indented;
              xw.WriteStartDocument();
              xw.WriteStartElement("table");

              xw.WriteAttributeString("xsi:schemaLocation", "http://www.sa.dk/xmlns/siard/1.0/schema0/" + TableNumStr + ".xsd " + TableNumStr + ".xsd");
              xw.WriteAttributeString("xmlns", "http://www.sa.dk/xmlns/siard/1.0/schema0/" + TableNumStr + ".xsd");
              xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");

              //For hver kode (row)
              int rowCount = 0;
              for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].kodeListeListe[i].koder.Count ; ii++)
              {
                  xw.WriteStartElement("row");

                  int fieldCounter = 1;
                  string variabelNavn = EBNF_Sys.tabListe[tblNr].kodeListeListe[i].koder[ii].kode;
                  xw.WriteElementString("c" + fieldCounter.ToString(), variabelNavn);

                  fieldCounter++;
                  string value = EBNF_Sys.tabListe[tblNr].kodeListeListe[i].koder[ii].value;
                  xw.WriteElementString("c" + fieldCounter.ToString(), value);//XML tegn fx & konverteres automatisk
                  
                  //fieldCounter++;
                  //string missingValue = EBNF_Sys.tabListe[tblNr].kodeListeListe[i].koder[ii].isMissingValue.ToString().ToLower();
                  //xw.WriteElementString("c" + fieldCounter.ToString(), missingValue);

                  //Row sluttes af
                  xw.WriteEndElement();

                  rowCount++;
              }

              EBNF_Sys.tabListe[tblNr].kodeListeListe[i].rowCount = rowCount;

              //Vi lukker og slukker tabel
              xw.WriteEndElement();
              xw.Flush();
              xw.Close();
          }
       }


  

        /// <summary>
        /// Procedure som skaber hovedtabel udfra CSV fil og informationer fra metadatafil
        /// </summary>
        /// <param name="tblNr"></param>
        private void createMainTable(int tblNr, Boolean specialNumeric)
        {
            string[] specialCodes = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                                     ".a", ".b", ".c", ".d", ".e", ".f", ".g", ".h", ".i", ".j", ".k", ".l", ".m", ".n", ".o", ".p", ".q", ".r", ".s", ".t", ".u", ".v", ".w", ".x", ".y", ".z"}; 
            
            tableNum1007++; //Global tæller som holder styr på 1007 tabelnr fx table23
            string TableNumStr = "table" + tableNum1007.ToString();

            string fileName = EBNF_Sys.tabListe[tblNr].filePathCSV;

            toolStripStatusLabel1.Text = "Status: Konverterer CSV hovedtabel " + Path.GetFileName(fileName) + " til 1007 tabel";
            statusStrip1.Update();

            string outputFolder = Path.Combine(pathTo1007folder, "Tables") + "\\" + TableNumStr;
            Directory.CreateDirectory(outputFolder);

            XmlTextWriter xw = new XmlTextWriter(Path.Combine(outputFolder, TableNumStr + ".xml"), Encoding.UTF8);

            xw.Formatting = Formatting.Indented;
            xw.WriteStartDocument();
            xw.WriteStartElement("table");
          
            xw.WriteAttributeString("xsi:schemaLocation", "http://www.sa.dk/xmlns/siard/1.0/schema0/" + TableNumStr + ".xsd " + TableNumStr + ".xsd");
            xw.WriteAttributeString("xmlns", "http://www.sa.dk/xmlns/siard/1.0/schema0/" + TableNumStr + ".xsd");
            xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");

            //*** Her spoles data ud post for post ****
            openCSVDATA(EBNF_Sys.tabListe[tblNr].filePathCSV);

            //!!! første linie med kolonnenavne fjernes !!!!!
            for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].variabelListe.Count; ii++)
            {
                //Hent næste post ... ....
                string dataCSV = getNextCSVDATA();
            }

            //For hver række med data ...
            int rowCount = 0;
            int fieldCounter = 1;

            while (allTextGlobalPos < allText.Length-1)
            {
                xw.WriteStartElement("row");

                //**** For hver post i række ...
                fieldCounter = 1;//Klar til næste
                for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].variabelListe.Count ; ii++)
                {
                    //Hent næste post ... ....
                    string dataCSV = getNextCSVDATA();

                    if (dataCSV.Contains("\"\"\"") == true)
                    {
                        dataCSV = dataCSV.Replace("\"\"\"","\"");
                    }

                    string data1007 = ConvertTo1007.Convert_data_to_1007format(dataCSV,EBNF_Sys.tabListe[tblNr].variabelListe[ii].datatypeOrg);
                   // string data1007 = ConvertTo1007.Convert_data_to_1007format(dataCSV, EBNF_Sys.tabListe[tblNr].variabelListe[ii].datatype1007);

                    //Hvis tomt, så marker den som værende nil
                    if (data1007 == "")
                    {
                        string elementName = "c" + fieldCounter.ToString() + " xsi:nil=\"true\"";
                        xw.WriteElementString(elementName,  data1007);

                        //Register at feltet må være nil
                        EBNF_Sys.tabListe[tblNr].variabelListe[ii].isNillable = true;//benyttes når tabelIndex.xml skal skabes
                    }
                    else
                    {
                        xw.WriteElementString("c" + fieldCounter.ToString(), data1007);//Beskyttede tegn fx & konverteres automatisk
                    }

                    //Hvis der ikke findes BRUGERKODER, så led efter specialkoder
                    //!!! mangler lige at forstå dette bedre !!!
                    if (specialNumeric == true)
                    {
                        //Tjek om data == en af specialkoderne fx ".u";
                        if (specialCodes.Contains(data1007)) 
                        {
                            //Hvis den ikke allerde findes på listen så tilføj den ...
                            if (EBNF_Sys.tabListe[tblNr].variabelListe[ii].specialKoder.Contains(data1007) == false)
                            {
                                EBNF_Sys.tabListe[tblNr].variabelListe[ii].specialKoder.Add(data1007);
                               // MessageBox.Show(data + " - Tilføjet til liste ...");
                            }
                        }

                    }

                    fieldCounter++;
                } // for (int ii = 0; ii < EBNF_Sys.tabListe[tblNr].variabelListe.Count ; ii++)

                //Row sluttes af
                xw.WriteEndElement(); 

                rowCount++;//RowCount optælles
            } //while (allTextPos < allText.Length-1)
           
            //*** slut ****

            //vi lukker og slukker tabel
            xw.WriteEndElement();
            xw.Flush();
            xw.Close();

            //Rowcount tilføjes
            EBNF_Sys.tabListe[tblNr].rowCount = rowCount;
            EBNF_Sys.tabListe[tblNr].tabelNavn1007 = TableNumStr; 
        }


        private void buttonSkabAV_Click(object sender, EventArgs e)
        {
            //Bestem AV ID 
            string AV = Path.GetFileName(shellTreeView1.SelectedPath).Remove(0, 3) + ".1";

            //Skab AV folder
            pathTo1007folder = Path.Combine(@"C:\", "AVID.SA." + AV);

            SkabAV(AV, pathTo1007folder);
        }


        private void SkabAV(string AV, string destination)
        {
            tableNum1007 = 0;//Global

       //     //Bestem AV ID 
       //     string AV = Path.GetFileName(shellTreeView1.SelectedPath).Remove(0,3) + ".1";

       //     //Skab AV folder
       ////     pathTo1007folder = Path.Combine(Directory.GetParent(shellTreeView1.SelectedPath).ToString(), "AVID.SA." + AV);
       //     pathTo1007folder = Path.Combine(@"C:\", "AVID.SA." + AV);


            if (Directory.Exists(pathTo1007folder))
            {
                DialogResult res = MessageBox.Show("AV mappen "+pathTo1007folder + " eksisterer og vil blive slettet\r\nØnsker du at fortsætte?","Spørgsmål?",MessageBoxButtons.YesNo);
                
                if (res != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    Directory.Delete(pathTo1007folder, true);
                }
            }

            Directory.CreateDirectory(pathTo1007folder);

            //Kopier ContextDocumentation (hvis de eksisterer)
            string inpath = Path.Combine(shellTreeView1.SelectedPath,"ContextDocumentation");
            string outpath = Path.Combine(pathTo1007folder,"ContextDocumentation");
            if (Directory.Exists(inpath) == true)
            {
                DirectoryCopy(inpath, outpath, true);
            }

            //Kopier Indices (hvis de eksisterer)
            inpath = Path.Combine(shellTreeView1.SelectedPath, "Indices");
            outpath = Path.Combine(pathTo1007folder, "Indices");
            if (Directory.Exists(inpath) == true)
            {
                DirectoryCopy(inpath, outpath, true);
            }

            //Schemas kommer vi selv med ...
            inpath = pathToSchemas;//Ligger fast sted hos os selv ... !!! evt. være en ressource til applikationen ?????
            outpath = Path.Combine(pathTo1007folder, "Schemas");
            if (Directory.Exists(inpath) == true)
            {
                DirectoryCopy(inpath, outpath, true);
            }
            else
            {
                MessageBox.Show("Kunne ikke finde mappen med standard skemaer! " + inpath);
                return;
            }



            //*** Skab  tabeller og AV'en xml filer ***
            //Skab tableIndex xml fil og xml header
            string tableIndexFileName = Path.Combine(pathTo1007folder, "Indices\\tableIndex.xml");
            XmlTextWriter ti = new XmlTextWriter(tableIndexFileName, Encoding.UTF8);
			ti.Formatting = Formatting.Indented;

        //!!!!! dbName og dataBaseProduct benyttes ikke ... afventer ny ADA !!!!!!!
        //!!! Midlertidig work around !!!!!!!
        string dbName = "ikke_relevant";
		string dataBaseProduct = "Microsoft SQL server";
		
            //Skab start af tableIndex
            TableIndexWriter.writeStartXML(ti,  dbName,  dataBaseProduct);
           
            //**** For hver hovedtabel *****
            for (int i = 0; i < EBNF_Sys.tabListe.Count; i++)
			{
                Boolean specialNumeric = false;//Benyttes når researchIndex skal skabes

                //Nødt til at bestemme det allerede nu fordi createMainTable(i) skal vide om der skal samles værdier op ...
                if (brugerKodeListe.Count > 0)
                {
                    specialNumeric = false;
                }
                else
                {
                    specialNumeric = true;
                }

                
                //**** Spoler CSV fil ud til tabel og opsaml evt. specialkoder *****
                createMainTable(i, specialNumeric);
               
                //**** Skaber kodetabeller udfra kodeværdier ****
                createKodeTables(i);

                //**** table og tableIndex.xml skabes ****
                //*** tableName, tableNumber, descriptions ***
                string tableName = EBNF_Sys.tabListe[i].tableNavn;
                string tableNumber = EBNF_Sys.tabListe[i].tabelNavn1007;
                string tableDescription = EBNF_Sys.tabListe[i].tabelBeskrivelse;

                TableIndexWriter.writeTableIndex(ti, tableName, tableNumber, tableDescription);//starter en ny tabel

                //*** Tilføjer felter (kolonner) ***
                for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                {
                    string columnName = EBNF_Sys.tabListe[i].variabelListe[ii].variabelNavn;
                    string columnID = "c" + (ii + 1).ToString();
                    string type = EBNF_Sys.tabListe[i].variabelListe[ii].datatype1007;
                    string typeOriginal = EBNF_Sys.tabListe[i].variabelListe[ii].datatypeOrg1007;
                    string description = EBNF_Sys.tabListe[i].variabelListe[ii].variabelBeskrivelse;
                    //string calcLength = reader.GetString("calcLength");
                    string nullable = EBNF_Sys.tabListe[i].variabelListe[ii].isNillable.ToString().ToLower();//skal være lowercase

                    TableIndexWriter.writeColumn(ti, columnName, columnID, type, typeOriginal, nullable, description);
                }
                TableIndexWriter.writeEndColumns(ti); //afslutter kolonner for den aktuelle tabel			

                //*** Tilføjer PN_nøgle, hvis der er nogen (primærnøgle er påkrævet jvf. skema) ***
                if (EBNF_Sys.tabListe[i].PNList.Count > 0)
                {
                    string pkName = "PK_" + EBNF_Sys.tabListe[i].tableNavn;//name er noget vi selv har defineret
                    TableIndexWriter.writePrimaryKey(ti, pkName, EBNF_Sys.tabListe[i].PNList);
                }


                //*** Relationer ***
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //!!! relationer til andre hovedtabeller i AV mangler !!!!!
                //!!! hvordan hindres cirkulær reference etc?
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                //Find lige ud af om der er nogle relationer til andre tabeller (hovedtabeller og kodetabeller)
                //For at hindre at der bliver skrevet et tomt ForeignKeys xml element (et tomt element er ulovligt jvf. skema)
                Boolean haveRelations = false;
                if (EBNF_Sys.tabListe[i].tabelReferencer.Length > 0)
                {
                        haveRelations = true;
                }
                
                for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                {
                    //For hver variabel ... hvis der er tale om ref til kodeliste, så lav den ...
                    if (EBNF_Sys.tabListe[i].variabelListe[ii].kodelisteNavn != "")
                    {
                        haveRelations = true;
                        break;
                    }
                }

                if (haveRelations)
                {
                    TableIndexWriter.writeForeignKeysStart(ti);
                    //Tilføjer relationer til hovedtabeller
                    for (int ii = 0; ii < EBNF_Sys.tabListe[i].tabelReferencer.Length; ii++)
                    {
                        //Hovedtabeller mangler !!!!!
                    }


                    //Tilføjer relationer til kodelister (kodetabeller) 
                    for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                    {
                        //For hver variabel ... hvis der er tale om ref til kodeliste, så lav den ...
                        if (EBNF_Sys.tabListe[i].variabelListe[ii].kodelisteNavn != "")
                        {
                            string fkName = "FK_" + EBNF_Sys.tabListe[i].tableNavn + "_" + EBNF_Sys.tabListe[i].variabelListe[ii].kodelisteNavn + "_" + ii.ToString();//Bare et navn, men skal være unikt og derfor tilføjes variablensløbenummer
                            string referencedTable = EBNF_Sys.tabListe[i].tableNavn + "_" + EBNF_Sys.tabListe[i].variabelListe[ii].kodelisteNavn;

                            string columnsFK = EBNF_Sys.tabListe[i].variabelListe[ii].variabelNavn;//oprindeligt navn på felt i hovedtabel
                            string columnsReferenced = "Kode";

                            TableIndexWriter.writeForeignKey(ti, fkName, referencedTable, columnsFK, columnsReferenced);
                        }
                    }
                    TableIndexWriter.writeForeignKeysEnd(ti); //foreignKeys
                }

                //Afslut hovedtabel med rowcount
                TableIndexWriter.writeEndTable(ti, EBNF_Sys.tabListe[i].rowCount.ToString());

  		
                //************ For hver kodeliste *************
                for (int klNr = 0; klNr < EBNF_Sys.tabListe[i].kodeListeListe.Count; klNr++)
                {
                    tableName = EBNF_Sys.tabListe[i].tableNavn+"_"+EBNF_Sys.tabListe[i].kodeListeListe[klNr].kodelisteNavn;//tabel_kodelistenavn
                    tableNumber = EBNF_Sys.tabListe[i].kodeListeListe[klNr].tabelNavn1007;
                    tableDescription = "Kodeliste til tabel " + EBNF_Sys.tabListe[i].tableNavn;

                    TableIndexWriter.writeTableIndex(ti, tableName, tableNumber, tableDescription);//starter en ny tabel

                    string columnName = "Kode";
                    string columnID = "c1";
                    string dataType = EBNF_Sys.tabListe[i].kodeListeListe[klNr].datatype1007;
                    string dataTypeOriginal = EBNF_Sys.tabListe[i].kodeListeListe[klNr].datatype1007;
                    string description = "Kode";
                    string nullable = "false";
                    TableIndexWriter.writeColumn(ti, columnName, columnID, dataType, dataTypeOriginal, nullable, description);

                    columnName = "Kodeværdi";
                    columnID = "c2";
                    dataType = "CHAR";//Altid CHAR (giver ikke mening at bestemmes ved test af indhold?)
                    dataTypeOriginal = "CHAR";//Altid CHAR (giver ikke mening at bestemmes ved test af indhold?)
                    description = "Kodeværdi";
                    nullable = "false";//skal være lowercase
                    TableIndexWriter.writeColumn(ti, columnName, columnID, dataType, dataTypeOriginal, nullable, description);

                    //columnName = "Missing_value";
                    //columnID = "c3";
                    //dataType = "BOOLEAN";//Altid boolsk !
                    //dataTypeOriginal = "BOOLEAN"; //Altid boolsk !
                    //description = "Angivelse af om koden er en Bruger null værdi / Missing value";
                    //nullable = "false";
                    //TableIndexWriter.writeColumn(xw, columnName, columnID, dataType, dataTypeOriginal, nullable, description);
                
                    TableIndexWriter.writeEndColumns(ti); //afslutter kolonner for den aktuelle tabel	
                    
                    //Tilføjer nøgler 
                    ti.WriteStartElement("primaryKey");
                    ti.WriteElementString("name", "PK_" + tableName);
                 

                    ti.WriteElementString("column", "Kode"); ///??????

                    ti.WriteEndElement(); //primaryKey

                    //Afslut tabel med rowcount
                    TableIndexWriter.writeEndTable(ti, EBNF_Sys.tabListe[i].kodeListeListe[klNr].rowCount.ToString());
                }//For hver kodetabel ...


            }//For hver hovedtabel

			//Luk og sluk tableIndex ...
            TableIndexWriter.writeEndDocument(ti);



            //***** researchIndex.xml ******
            //**** For hver hovedtabel *****

            //Skab researchIndex.xml fil og xml header
            string researchIndex = Path.Combine(pathTo1007folder, "Indices\\researchIndex.xml");
            XmlTextWriter ri = new XmlTextWriter(researchIndex, Encoding.UTF8);
            ri.Formatting = Formatting.Indented;
            ResearchIndexWriter.writeStartXML(ri);
            
            for (int i = 0; i < EBNF_Sys.tabListe.Count; i++)
            {
                Boolean specialNumeric = false;//Benyttes når researchIndex skal skabes
                
                //Hvis der er defineret BRUGERKODER, så behøver vi ikke forholde os til specialKoder ...
                if (brugerKodeListe.Count > 0)
                {
                    //Her skal BRUGERKODER indsættes i researchIndex
                    ResearchIndexWriter.writeTableIndex(ri, EBNF_Sys.tabListe[i].tabelNavn1007, EBNF_Sys.tabListe[i].systemNavn, specialNumeric);

                    //*** Hvis der findes reference til kodeliste med missing values ... ****
                    ResearchIndexWriter.writeStartColumns(ri);//columns samling start

                    //For hver kolonne/variabel ...
                    for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                    {

                        //Hvis variablen har en relation til en kodeliste, så .....
                        if (EBNF_Sys.tabListe[i].variabelListe[ii].kodelisteNavn != "")
                        {
                            //Vi er nødt til at vide om der overhoved er nogle brugerkoder ...
                            Boolean harBrugerkoder = false;
                            foreach (EBNF_brugerkoder mv in brugerKodeListe)
                            {
                                //Find kodeliste ved opslag ... og for hver kode som er missing value, indskriv som value ???? 
                                if (mv.variabelNavn == EBNF_Sys.tabListe[i].variabelListe[ii].variabelNavn)
                                {
                                    harBrugerkoder = true;
                                    break;
                                }
                            }

                            if (harBrugerkoder == true)
                            {
                                ResearchIndexWriter.writeStartColumn(ri, 'c' + (ii + 1).ToString());//column start

                                //Værdier for missing values skal fiskes fra kodeliste og indsættes her
                                ResearchIndexWriter.writeStartMissingValues(ri);

                                //Find værdier med udgangspunkt i variabelNavn....
                                foreach (EBNF_brugerkoder mv in brugerKodeListe)
                                {
                                    //Find kodeliste ved opslag ... og for hver kode som er missing value, indskriv som value ???? 
                                    if (mv.variabelNavn == EBNF_Sys.tabListe[i].variabelListe[ii].variabelNavn)
                                    {
                                        //For hver missing value 
                                        foreach (string value in mv.values)
                                        {
                                            ResearchIndexWriter.writeStartValue(ri, value);
                                        }
                                    }
                                }
                                ResearchIndexWriter.writeEndElement(ri);// MissingValues
                                ResearchIndexWriter.writeEndElement(ri);// column slut
                            }
                        }
                    }
                    ResearchIndexWriter.writeEndElement(ri);// columns samling slut

                    //Luk og sluk researchIndex ...
                    ResearchIndexWriter.writeEndDocument(ri);
                }
            else
                {
                    //Her skal specialkoder indsættes i researchIndex
                    
                    //Der er ingen BRUGERKODER, men er der nogle specialkoder ????
                    for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                    {
                        if (EBNF_Sys.tabListe[i].variabelListe[ii].specialKoder.Count > 0)
                        {
                            specialNumeric = true;
                            break;
                        }
                    }

                    //Her skal specialkoder indsættes i researchIndex
                    ResearchIndexWriter.writeTableIndex(ri, EBNF_Sys.tabListe[i].tabelNavn1007, EBNF_Sys.tabListe[i].systemNavn, specialNumeric);

                    //Hvis der er specialkoder, så indsættes de i researchIndex ....
                    if (specialNumeric == true)
                    {
                        ResearchIndexWriter.writeStartColumns(ri);//columns samling start
                        
                        //For hver kolonne/variabel ...
                        for (int ii = 0; ii < EBNF_Sys.tabListe[i].variabelListe.Count; ii++)
                        {
                            if (EBNF_Sys.tabListe[i].variabelListe[ii].specialKoder.Count > 0)
                            {

                                ResearchIndexWriter.writeStartColumn(ri, 'c' + (ii + 1).ToString());//column start
                                ResearchIndexWriter.writeStartMissingValues(ri);

                                for (int iii = 0; iii < EBNF_Sys.tabListe[i].variabelListe[ii].specialKoder.Count; iii++)
                                {
                                    ResearchIndexWriter.writeStartValue(ri, EBNF_Sys.tabListe[i].variabelListe[ii].specialKoder[iii]);
                                }
                                ResearchIndexWriter.writeEndElement(ri);// MissingValues
                                ResearchIndexWriter.writeEndElement(ri);// column slut
                            }
                        }
                        
                        ResearchIndexWriter.writeEndElement(ri);// columns samling slut
                    }

                    //Luk og sluk researchIndex ...
                    ResearchIndexWriter.writeEndDocument(ri);

                }//if brugerliste count ....


            }//For hver tabel ...


            ri.Flush();
            ri.Dispose();


            //Create fileIndex.xml
            toolStripStatusLabel1.Text = "Skaber fileIndex.xml";
            statusStrip1.Update();
            fileIndexCreator fi = new fileIndexCreator();
            fi.FileIndexCreate(pathTo1007folder);

            MessageBox.Show("Done");

            toolStripStatusLabel1.Text = "Done";
            statusStrip1.Update();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeView1.CollapseAll();
        }


        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = shellTreeView1.SelectedPath;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBoxMetadata.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
              
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = shellTreeView1.SelectedPath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBoxMetadata.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void shellTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            LOG_Clear();
            LOG("Mappen " + shellTreeView1.SelectedPath + " valgt",false);

            labelFD.Text = Path.GetFileName(shellTreeView1.SelectedPath);
            toolStripStatusLabel1.Text = shellTreeView1.SelectedPath;

            treeView1.Nodes.Clear();
            richTextBoxMetadata.Clear();
            listBox1.Items.Clear();

            string dataFolderPath = Path.Combine(shellTreeView1.SelectedPath, "Data");
            if (Directory.Exists(dataFolderPath))
            {
                int count = 0;
                foreach (string folder in Directory.GetDirectories(dataFolderPath,"Table??",SearchOption.TopDirectoryOnly))
                {
                    listBox1.Items.Add(Path.GetFileName(folder));
                    count++;
                }

                LOG("Der blev fundet " + count.ToString() + " datasæt",false);
            }

            LOG("", false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Benyttes til intern test af linier og linieskift ... vistnok!
            
            string result = "";
            richTextBoxMetadata.LoadFile(@"C:\DROPBOX\SA Forskningsdata\FD.18999\Data\table2\table2.csv", RichTextBoxStreamType.PlainText);
            int counter = 1;
            foreach (string line in richTextBoxMetadata.Lines)
            {
                if (line.Trim() != "")
                {
                    result = result + counter.ToString() + ';' + line + System.Environment.NewLine;
                    counter++;
                }
            }
            richTextBoxMetadata.Text = result;

            richTextBoxMetadata.SaveFile(@"C:\DROPBOX\SA Forskningsdata\FD.18999\Data\table2\ny_table2.csv.org", RichTextBoxStreamType.PlainText);
            MessageBox.Show("OK");
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            string metadataFilename = listBox1.Items[listBox1.SelectedIndex]+".txt";
            string dataFilename = listBox1.Items[listBox1.SelectedIndex] + ".csv";
            string pathToMetadata = Path.Combine(shellTreeView1.SelectedPath + "\\Data\\" , listBox1.Items[listBox1.SelectedIndex].ToString()) +"\\" + metadataFilename;
            string pathToData = Path.Combine(shellTreeView1.SelectedPath + "\\Data\\" , listBox1.Items[listBox1.SelectedIndex].ToString()) +"\\" + dataFilename;

            if (File.Exists(pathToMetadata))
            {
                richTextBoxMetadata.LoadFile(pathToMetadata, RichTextBoxStreamType.PlainText);
            }
            else
            {
                MessageBox.Show("Metadatafil " + pathToMetadata + " ikke fundet!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
           


        }
      


    }


    public class EBNF_system
    {
     //   public string systemNavn = "";//Kan forveksles med 1007 Systemnavn ... hvad med "OriginalFileFormat" ????
     //   public string decimaltegn = "";//Udgår sansynligvis ... ?
        public List<EBNF_tabel> tabListe;
    }

    public class EBNF_tabel
    {
        public string systemNavn = "";//Kan forveksles med 1007 Systemnavn ... hvad med "OriginalFileFormat" ????
        public string tableNavn = "";
        public List<string> PNList = new List<string>(); 
        public string tabelBeskrivelse = "";
        public string tabelReferencer = ""; //!!!! Mangler ... skal laves til en liste ?
        public List<EBNF_variabel> variabelListe;
        public List<EBNF_kodeliste> kodeListeListe;

        public string tabelNavn1007 = "";
        public string filePathCSV = "";//sti til datafil. Benyttes til at pege på CSV fil i forbindelse med indlæsning til 1007 XML tabel. Kan undværes (udledes) ...
        public int rowCount = 0;//Antal rækker i hovedtabel
    }

    public class EBNF_variabel
    {
        public string variabelNavn = "";
        public string variabelBeskrivelse = "";
        public string datatypeOrg = "";//Den datatype som oprindeligt var benyttet i systemet fx 'f8' (info gemmes ikke i 1007)
        public string datatypeOrg1007 = "";//Den datatype som var tænkt benyttet i 1007 fx INTEGER
        public string datatype1007 = "";//Den datatype som reelt benyttes i 1007 fx CHAR fordi feltet referere til kodetabel hvor PN ikke er numerisk pga koder
        public string kodelisteNavn = "";//refererer til evt. kodeliste (kan være tom hvis der er tale om alm. variabel)
        public bool isNillable = false; //Hvis én eller flere værdier er tomme, skal feltet angives som værende Nillable
        public List<string> specialKoder;//En samling af specialkodeværdier fx '.u' 'U'
    }


    public class EBNF_kodeliste
    {
        public string kodelisteNavn = "";//Svarer til variabelnavn i hovedtabel og kommer til at indgå i tabelnavn fx "hovedtabel_kodelisteNavn"
        public List<EBNF_kode> koder;//En samling af kodeværdier

        //Den datatype som benyttes i 1007. Hvis datatype er numerisk og data ikke er,
        //så ændres datatype til CHAR og hovedtabellens tilknyttede felt ændres ligeledes til CHAR
        public string datatype1007 = "";
       
        public string tabelNavn1007 = "";//!!! Benyttes eller beregnes?
        public int rowCount = 0;//Antal rækker i kodetabel

    }

    public class EBNF_kode
    {
        //fx '1' "Mand" false
        public string kode = "";//primærnøgle
        public string value = "";
        //public Boolean isBrugerkode = false; //Angivelse af om feltet rummer en "missing value" / "bruger nullværdi"
    }

    /// <summary>
    /// Objekt (temp) som indeholder variabelnavn samt BRUGERKODE værdier fx '99' '999'
    /// </summary>
    public class EBNF_brugerkoder
    {
        public string variabelNavn = "";//Navn på variabel som benytter koderne
        public List<string> values;
    }


}
