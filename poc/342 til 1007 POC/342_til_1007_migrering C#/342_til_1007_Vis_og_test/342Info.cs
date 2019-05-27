using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace _342_til_1007_vis_test_konverter
{
    class AV
    {
        //Klasse som samler alle oplysninger vedr. en 342 AV både udfra filstruktur og fra parset metadata
        public AV_info AV_Info;

        
        /// <summary>
        /// Indlæsning af metadata (ingen test af tabelfiler eller dokumenter)
        /// </summary>
        /// <param name="AV342folderPath"></param>
        public void Indlæs_342_metadata(string AV342folderPath)
        {
            Indlæs_ArkverTab(AV342folderPath);
            Indlæs_Skaber(AV342folderPath);
            Indlæs_Geninfo(AV342folderPath);
            Indlæs_FilmapTab(AV342folderPath);
            Indlæs_Datapakker(AV342folderPath);
            Indlæs_DokmapTab(AV342folderPath);
            Indlæs_Metadata(AV342folderPath);

            //***** Parse af metadata tekstfilen *****
            ParseMetadataTekst();

        }


        /// <summary>
        /// Indlæsning og test af tabeller og dokumenter
        /// </summary>
        /// <param name="AV342folderPath"></param>
        /// <returns></returns>
        public void Indlæs_342_tabel_og_dokument_filer(string AV342folderPath)
        {
            Valider_TabelFilListe(AV342folderPath); //her sker tilgang via filsystem !!!
            Valider_DokumentFiler(AV342folderPath);//her sker tilgang via filsystem !!!
        }
       


        public AV()
        {
            AV_Info = new AV_info();
        }

        /// <summary>
        /// Tilføjer en fejl til struktureret fejlliste
        /// </summary>
        /// <param name="fejlType"></param>
        /// <param name="fejlmelding"></param>
        /// <param name="filePath"></param>
        public void LOG(AV.FejlTypeDatafiler fejlType, int severity, string fejlmelding, string filePath)
        {
            AV.FejlDatafiler fejl = new AV.FejlDatafiler();
            fejl.fejlType = fejlType;
            fejl.Severity = severity;
            fejl.fejlMelding = fejlmelding;
            fejl.location = filePath;
            AV_Info.dataFiler.fejlListe.Add(fejl);
        }
       
        /// <summary>
        /// Tilføjer en fejl til struktureret fejlliste
        /// </summary>
        /// <param name="fejlType"></param>
        /// <param name="fejlmelding"></param>
        /// <param name="filePath"></param>
        public void LOG(AV.FejlTypeMetadata fejlType, int severity, string fejlmelding, string filePath)
        {
            AV.FejlMetadata fejl = new AV.FejlMetadata();
            fejl.fejlType = fejlType;
            fejl.Severity = severity;
            fejl.fejlMelding = fejlmelding;
            fejl.location = filePath;
            AV_Info.metadata.fejlListe.Add(fejl);
        }

        /// <summary>
        /// Tilføjer en fejl til struktureret fejlliste
        /// </summary>
        /// <param name="fejlType"></param>
        /// <param name="fejlmelding"></param>
        /// <param name="filePath"></param>
        public void LOG(AV.FejlTypeKonvertering fejlType, int severity, string fejlmelding, string filePath)
        {
            AV.FejlKonvertering fejl = new AV.FejlKonvertering();
            fejl.fejlType = fejlType;
            fejl.Severity = severity;
            fejl.fejlMelding = fejlmelding;
            fejl.location = filePath;
            AV_Info.Konvertering.fejlListe.Add(fejl);
        }


        private void Indlæs_Datapakker(string path)
        {

            //*** Udgave bestemmes ***
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false) 
            {
                return;
            }

            //*** Led efter samtlige datapakker og tilføj til liste ***
            foreach (string dp in Directory.GetDirectories(udgave_path, "*.*", SearchOption.TopDirectoryOnly))
            {
                AV_Info.dataFiler.DPList.Add(dp);
            }
        }


        private void Indlæs_FilmapTab(string path)
        {
            //*** Udgave bestemmes ***
            string udgave = AV_Info.udgave;// Get_Udgave(path);
            string udgave_path = Path.Combine(path, udgave);

            if (Directory.Exists(udgave_path) == false)
            {
                return;
            }
            
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToFilemapTab = path + "\\" + AV_Info.udgave + "\\" + DP_001 + "\\" + AV_Info.AVID + "\\" + "filmap.tab";

            if (File.Exists(pathToFilemapTab) == false)
            {
                LOG(AV.FejlTypeMetadata.FilmapTab,2, "Filemap.tab ikke fundet!", pathToFilemapTab);
                return; 
            }

            //Test at filemap.tab mod 20 (12+8) stemmer
            FileInfo inf = new FileInfo(pathToFilemapTab);
            if (inf.Length % 20 == 0)
            {
                //MessageBox.Show(pathToArkver);
                string filmapAll = File.ReadAllText(pathToFilemapTab, Encoding.Default);
                StringReader rd = new StringReader(filmapAll);

                int read = 0;
                while (read < inf.Length)
                {
                    char[] buffer = new char[20];
                    rd.ReadBlock(buffer, 0, 20);
                    string record = new string(buffer, 0, 20);

                    FilmapTab f = new FilmapTab();
                    f.fil = record.Substring(0, 12).Trim();//Skal trimmes fordi filnavn kan være mindre end 12 karakterer
                    f.medieID = record.Substring(12, 8);

                    //path til mappe
                    string filePath = path + "\\" + AV_Info.udgave + "\\" + f.medieID + "\\" + AV_Info.AVID + "\\" + f.fil;

                    f.filePath = filePath;
                    AV_Info.metadata.filemapList.Add(f);

                    read = read + 20;
                }
            }
          else
            {
                LOG(AV.FejlTypeMetadata.FilmapTab, 2,"Længden på filemap.tab er fejlbehæftet (længde mod 20 > 0)!", pathToFilemapTab);
            }

        }




        private bool Indlæs_DokmapTab(string path)
        {
            //*** Udgave bestemmes ***
            string udgave = AV_Info.udgave;// Get_Udgave(path);
            string udgave_path = Path.Combine(path, udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return (false);
            }
                        
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToDokmap = path + "\\" + AV_Info.udgave + "\\" + DP_001 + "\\" + AV_Info.AVID + "\\" + "dokmap.tab";

            //Hvis dokmap.tab ikke findes ... så tjek om den er defineret i filemap.tab
            if (File.Exists(pathToDokmap) == false)
            {
                //Filmap.tab findes ikke, så tjek lige at den heller ikke er angivet i filemap.tab
                bool fundet = false;
                foreach (FilmapTab filmap in AV_Info.metadata.filemapList)
                {
                    if (filmap.fil.ToLower() == "dokmap.tab")
                    {
                        fundet = true;
                        break;
                    }
                }

                if (fundet == true)
                {
                    LOG(AV.FejlTypeMetadata.DokumentTab, 2, "Dokmap.tab er defineret i filmap.tab, men ikke fundet på disk!)", pathToDokmap);
                    //AVInfo.LOG = AVInfo.LOG + "Dokmap.tab er defineret i filmap.tab, men ikke fundet på disk!)" + "\r\n"; ;
                    return(false); //Her skal returneres fejl ...                 
                }

                LOG(AV.FejlTypeMetadata.DokumentTab, 0, "AV indeholder ikke dokumenter", "");
                //AVInfo.LOG = AVInfo.LOG + "AV indeholder ikke dokumenter" + "\r\n"; ;
                
                return(true); //Her returneres OK, der ikke er tale om en fejl
            }
            
            //Test at dokmap.tab mod 24 stemmer
            FileInfo inf = new FileInfo(pathToDokmap);
            if (inf.Length % 24 == 0)
            {
                //MessageBox.Show(pathToArkver);
                string dokmapAll = File.ReadAllText(pathToDokmap, Encoding.Default);
                StringReader rd = new StringReader(dokmapAll);

                int read = 0;
                while (read < inf.Length)
                {
                    char[] buffer = new char[24];
                    rd.ReadBlock(buffer, 0, 24);
                    string record = new string(buffer, 0, 24);

                    DokumentTab d = new DokumentTab();
                    d.dokID = record.Substring(0, 8);
                    d.medieID = record.Substring(8, 8);
                    d.underdir = record.Substring(16, 8);
                    //Visning
                    // listBoxDokumenter.Items.Add("DokID:" + d.dokID + "  MedieID:" + d.medieID + "  UnderDir:" + d.underdir);

                    //path til mappe
                    string docFolderPath = path + "\\" + AV_Info.udgave + "\\" + d.medieID + "\\" + AV_Info.AVID + "\\" + d.underdir + "\\" + d.dokID;

                    d.docPath = docFolderPath;
                    AV_Info.metadata.dokumentList.Add(d);

                
                    read = read + 24;
                }
                //MessageBox.Show(arkverAll);
            }
            else
            {
                // MessageBox.Show((inf.Length / 24).ToString());
                LOG(AV.FejlTypeMetadata.DokumentTab, 2, "Længden på Dokmap.tab er fejlbehæftet (længde mod 24 > 0)!", pathToDokmap);
                return(false);
            }

           
            return (true);
        }

        private void Valider_DokumentFiler(string path)
        {
            int errorCount = 0;

            //For hver fysisk dokumentmappe
            foreach (DokumentTab dok in AV_Info.metadata.dokumentList) 
            {
                string docPath = dok.docPath;

                if (Directory.Exists(docPath) == false)
                {
                    LOG(FejlTypeDatafiler.Dokumenter, 1, "Manglende dokumentmappe!", docPath);
                    errorCount++;

                    if (errorCount > 100)
                    {
                        LOG(FejlTypeDatafiler.Dokumenter, 2, "Test for manglende dokumentmapper stoppet pga antallet af fejl (100)", docPath);
                        break;
                    }

                    continue;
                }

                //Tjek om fil(er) findes
                string[] filer = Directory.GetFiles(docPath);

                if (filer.Length < 1)
                {
                    MessageBox.Show("! Tom dokumentmappe: " + docPath);
                    LOG(FejlTypeDatafiler.Dokumenter, 1, "Tom dokumentmappe!", docPath);
                }
                else
                {
                    Array.Sort(filer);//Nu bør filer[0] repræsentere 1.*
                    string ext = Path.GetExtension(filer[0]).Replace(".", "");
                    string originalFileName = "00000001." + ext;
                    //MessageBox.Show(originalFileName);
                    if (File.Exists(Path.Combine(docPath, originalFileName)) == false)
                    {
                        // MessageBox.Show("! Fejl forløbende navngivning af fil: " + filer[0]);
                        LOG(FejlTypeDatafiler.Dokumenter, 2, "Fejl navngivning (forventede " + originalFileName+")", filer[0]);
                    }
                    // TODO: Indlæs_Dokumenter, hvis der er flere enkeltfiler, så bør deres navngivning (side nummer) også tjekkes?
                }
            }
        }


        private void Valider_TabelFilListe(string path)
        {

            //*** Udgave bestemmes ***
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return;
            }


            //*** Liste over alle tabeller (incl fragmenter) skabes ***
            //For hver fysisk datapakke ...
            foreach (string dp_path in AV_Info.dataFiler.DPList)
            {
                //Sti til tabeller bestemmes
                string table_path = dp_path + "\\" + AV_Info.AVID;
                //MessageBox.Show(table_path);
                foreach (string tabelPath in Directory.GetFiles(table_path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    int res = -1;
                    string ext = Path.GetExtension(tabelPath).Replace(".", "").ToUpper();

                    //Vi leder efter ARK filer ... 
                    if (ext == "ARK")
                    {
                        AV_Info.dataFiler.tabelFilListe.Add(tabelPath);

                        bool fundet = false;
                        foreach (AV.Tabel tabel in AV_Info.metadata.tabelList)
                        {
                            string tabelShortname = Path.GetFileNameWithoutExtension(tabelPath);
                            if (tabelShortname == tabel.titel)
                            {
                                fundet = true;
                                break;
                            }
                        }

                        if (fundet == false)
                        {
                            LOG(FejlTypeDatafiler.Tabeller, 2, "Tabelfil ikke registreret i metadata!", tabelPath);
                        }

                    }
                 else
                    //... og fragmenter 001-NNN 
                    if (int.TryParse(ext, out res) == true)
                    {
                        AV_Info.dataFiler.tabelFilListe.Add(tabelPath);
                    }
                  
                }
            }

            if (AV_Info.dataFiler.tabelFilListe.Count == 0)
            {
                LOG(AV.FejlTypeDatafiler.Tabeller, 2, "Ingen tabelfiler!", AV_Info.AVID);
            }
        }


        private void Indlæs_Geninfo(string path)
        {
            
            //*** Udgavesti bestemmes ***
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return;
            }
                        
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToGeninfo = Path.Combine(udgave_path, DP_001 + "\\" + AV_Info.AVID + "\\" + "geninfo.tab");

            if (File.Exists(pathToGeninfo) == false)
            {
                LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Geninfo.tab ikke fundet!", pathToGeninfo);
                return;
            }

            //Hver record i geninfo.tab fylder ialt 272 bytes
            //256, 8,8

            //Test at skaber.tab mod 272 stemmer
            FileInfo inf = new FileInfo(pathToGeninfo);
            if (inf.Length % 272 == 0)
            {
                //MessageBox.Show(pathToArkver);
                string geninfoAll = File.ReadAllText(pathToGeninfo, Encoding.Default);
                StringReader rd = new StringReader(geninfoAll);

                int read = 0;
                while (read < inf.Length)
                {
                    char[] buffer = new char[272];
                    rd.ReadBlock(buffer, 0, 272);
                    string record = new string(buffer, 0, 272);

                    GeninfoTab g = new GeninfoTab();
                    g.medieID = record.Substring(0, 8).Trim();
                    g.dokbib = record.Substring(8, 8).Trim();
                    g.beskrivelse = record.Substring(16, 256).Trim();

                    string dokpath = path + "\\" + AV_Info.udgave + "\\" + g.medieID + "\\" + AV_Info.AVID + "\\" + g.dokbib;//Kan ligge i anden datapakke end den første ! 
                    g.geninfoDocfolderPath = dokpath;
                    AV_Info.metadata.geninfoList.Add(g);

                    //  listBoxGeninfo.Items.Add("MedieID:" + g.medieID + "  Dokbib:" + g.dokbib + "  Beskrivelse:" + g.beskrivelse);

                    //Tjek om fil findes
                    //MessageBox.Show(dokpath);
                    if (Directory.Exists(dokpath) == false)
                    {
                        LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Manglende geninfo mappe!", dokpath);
                    }
                 else
                    if (Directory.GetFiles(dokpath).Length < 1)
                    {
                        LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Manglende geninfo dokument!", dokpath);
                   
                        if (Directory.GetDirectories(dokpath).Length > 0)
                        {
                            LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Uventet mappe i geninfo dokumentmappe!", dokpath);
                        }
                    }

                    read = read + 272;
                }
            }
            else
            {
                //listBoxGeninfo.Items.Add("!!! FEJL !!! Længden på geninfo.tab er fejlbehæftet (længde mod 272 > 0");
                LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Længden på geninfo.tab er fejlbehæftet (længde mod 272 > 0)!", pathToGeninfo);
            }
        }


        private void Indlæs_Skaber(string path)
        {
            //*** Udgave bestemmes ***
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return;
            }
            
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToSkaber = path + "\\" + AV_Info.udgave + "\\" + DP_001 + "\\" + AV_Info.AVID + "\\" + "skaber.tab";


            if (File.Exists(pathToSkaber) == false)
            {

                LOG(AV.FejlTypeMetadata.SkaberTab, 2, "Skaber.tab ikke fundet!", pathToSkaber);
                return;

            }

            //skaber.tab fylder ialt 272 bytes
            //8,8,256

            //Test at skaber.tab er præcis 272 bytes lang
            FileInfo inf = new FileInfo(pathToSkaber);
            if (inf.Length % 272 == 0)
            {
                //MessageBox.Show(pathToArkver);
                string skaberAll = File.ReadAllText(pathToSkaber, Encoding.Default);
                StringReader rd = new StringReader(skaberAll);

                int read = 0;
                while (read < inf.Length)
                {
                    char[] buffer = new char[272];
                    rd.ReadBlock(buffer, 0, 272);
                    string record = new string(buffer, 0, 272);
                    Skaber s = new Skaber();
                    s.skaber = record.Substring(0, 256).Trim();//Vi kan ligeså godt trimme allerede nu
                    s.startdato = record.Substring(256, 8);
                    s.slutdato = record.Substring(264, 8);
 
                    AV_Info.metadata.skaberList.Add(s);

                    read = read + 272;
                }
            }
            else
            {
                LOG(AV.FejlTypeMetadata.SkaberTab, 2, "Længden på skaber.tab er fejlbehæftet (længde mod 272 > 0)!", pathToSkaber);
                //MessageBox.Show("Længden på skaber.tab er fejlbehæftet (længde mod 272 > 0");
            }
        }

        private bool Indlæs_Metadata(string path)
        {
            //*** AV bestemmes ***
            //string AV = Path.GetFileName(path);
            //string AV_NR = Path.GetFileName(path).Substring(3, 5);

            //*** Udgave bestemmes ***
            //string udgave = Get_Udgave(path);
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return (false);
            }
            
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToMetadata = path + "\\" + AV_Info.udgave + "\\" + DP_001 + "\\" + AV_Info.AVID + "\\" + AV_Info.AVID + ".xml";

            AV_Info.metadata.metadataTekstFilePath = pathToMetadata;

            if (File.Exists(pathToMetadata) == false)
            {
                LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Metadatafil ikke fundet!", pathToMetadata);
                return (false);
            }
            else
            {
                AV_Info.metadata.metadataTekst = File.ReadAllText(pathToMetadata, Encoding.Default);
                AV_Info.metadata.metadataTekst = AV_Info.metadata.metadataTekst.Replace("&gt;", ">");
                AV_Info.metadata.metadataTekst = AV_Info.metadata.metadataTekst.Replace("&lt;", "<");
                AV_Info.metadata.metadataTekst = AV_Info.metadata.metadataTekst.Replace("&apos;", "'");
                AV_Info.metadata.metadataTekst = AV_Info.metadata.metadataTekst.Replace("&quot;", "\"");
                AV_Info.metadata.metadataTekst = AV_Info.metadata.metadataTekst.Replace("&amp;", "&");
            }

            return (true);
        }

        private void Indlæs_ArkverTab(string path)
        {
            //*** Udgave sti bestemmes ***
            string udgave_path = Path.Combine(path, AV_Info.udgave);
            if (Directory.Exists(udgave_path) == false)
            {
                return;
            }
            
            string DP_001 = AV_Info.AV_NR.PadLeft(5, '0') + "001";//Navn på første datapakke

            string pathToArkver = path + "\\" + AV_Info.udgave + "\\" + DP_001 + "\\" + "arkver.tab";

            //Arkver.tab fylder ialt 297 bytes
            //8,1,8,8,256,8,8
            //Test at arkver.tab er præcis 297 bytes lang
            FileInfo inf = new FileInfo(pathToArkver);
            if (inf.Length == 297)
            {
                string arkverAll = File.ReadAllText(pathToArkver, Encoding.Default);
                AV_Info.metadata.arkverTab.ID = arkverAll.Substring(0, 8).Trim();
                AV_Info.metadata.arkverTab.Afltype = arkverAll.Substring(8, 1).Trim();
                AV_Info.metadata.arkverTab.MedieID = arkverAll.Substring(9, 8).Trim();
                AV_Info.metadata.arkverTab.TidligereAfl = arkverAll.Substring(17, 8).Trim();
                AV_Info.metadata.arkverTab.systemnavn = arkverAll.Substring(25, 256).Trim();
                AV_Info.metadata.arkverTab.startdato = arkverAll.Substring(281, 8).Trim();
                AV_Info.metadata.arkverTab.slutdato = arkverAll.Substring(289, 8).Trim();
                AV_Info.metadata.arkverTab.arkverTabFilePath = pathToArkver;

            }
            else
            {
                LOG(AV.FejlTypeMetadata.ArkverTab, 2, "Længden på arkver.tab er forskellig fra 297 bytes!", pathToArkver);
            }
        }


        /// <summary>
        /// Benyttes til at returnerer tekst mellem 2 opmærkninger (fx <Titel>Tbl_adresser</Titel>)
        /// </summary>
        /// <param name="startTag"></param>
        /// <param name="endTag"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private string GetTextElement(string startTag, string endTag, string metadata)
        {
            string res = "";

            MatchCollection startList = System.Text.RegularExpressions.Regex.Matches(metadata, startTag, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            MatchCollection slutList = System.Text.RegularExpressions.Regex.Matches(metadata, endTag, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            //manglende start eller slut element
            if (startList.Count != slutList.Count)
            {
                return (null);
            }

            //Tjek om der overhoved er fundet nogen
            if (startList.Count == 0)
            {
                return ("");
            }

            //Position i teksten er ens, bør ikke kunne forekomme!
            if (startList[0].Index == slutList[0].Index)
            {
                return (null);
            }

            //Resultat hvor startudtryk er strippet væk
            res = metadata.Substring(startList[0].Index + startTag.Length, slutList[0].Index - (startList[0].Index + startTag.Length)).Trim();
            return (res);
        }


        /// <summary>
        /// Benyttes til at returnerer tekster mellem flere opmærkninger (fx <Kode>123</Kode><Kode>Udflyttet</Kode>)
        /// </summary>
        /// <param name="startTag"></param>
        /// <param name="endTag"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private List<string> GetTextElements(string startTag, string endTag, string metadata)
        {
            List<string> resList = new List<string>();

            MatchCollection startList = System.Text.RegularExpressions.Regex.Matches(metadata, startTag, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            MatchCollection slutList = System.Text.RegularExpressions.Regex.Matches(metadata, endTag, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (startList.Count != slutList.Count)
            {
                return (null);
            }

            if (startList.Count == 0)
            {
                return (null);
            }

            for (int i = 0; i < startList.Count; i++)
            {
                //Resultat hvor startudtryk er strippet væk
                resList.Add(metadata.Substring(startList[i].Index + startTag.Length, slutList[i].Index - (startList[i].Index + startTag.Length)).Trim());
            }

            return (resList);
        }


        /// <summary>
        /// Parser metadata og lægger data ind i klasse. 
        /// </summary>
        /// <returns></returns>
        public void ParseMetadataTekst()
        {
            List<string> tblList = new List<string>();
            tblList = GetTextElements("<tabel>", "</tabel>", AV_Info.metadata.metadataTekst);

            if (tblList == null)
            {
                LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Fejl i angivelse af tabel(ler) element(er)  AV: " + AV_Info.AVID,"");
                return;//Fatal fejl
            }

            foreach (string tabelText in tblList)
            {
                //Tabel
                Tabel tabel = new Tabel();

                tabel.titel = GetTextElement("<titel>", "</titel>", tabelText).Trim() ;

                tabel.tabelInfo = GetTextElement("<tabelinfo>", "</tabelinfo>", tabelText).Trim();
               
                tabel.posttype = GetTextElement("<posttype>", "</posttype>", tabelText).Trim().ToLower();
                if ((tabel.posttype != "fast") && (tabel.posttype != "variabel"))
                {
                    MessageBox.Show("Fejlagtig angivelse af posttype AV: "+ AV_Info.AVID + " tabel:" +tabel.titel);
                    LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Fejagtig angivelse af posttype AV: " + AV_Info.AVID + " tabel:" + tabel.titel, "");
                }

                //Feltdefinition
                List<string> feltdefTextList = new List<string>();
                feltdefTextList = GetTextElements("<feltdef>", "</feltdef>", tabelText);
                if (feltdefTextList != null)
                {
                    foreach (string feltdefText in feltdefTextList)
                    {
                        FeltDef feltdef = new FeltDef();

                        feltdef.titel = GetTextElement("<titel>", "</titel>", feltdefText);

                        feltdef.datatype = GetTextElement("<datatype>", "</datatype>", feltdefText).ToLower();
                        //Her testes om datatype er angivet og valid
                        if (feltdef.datatype != "")
                        {
                            bool fejl = true;
                            switch (feltdef.datatype)
                            {
                                case "num":
                                    fejl = false;
                                    break;
                                case "real":
                                    fejl = false;
                                    break;
                                case "exp":
                                    fejl = false;
                                    break;
                                case "string":
                                    fejl = false;
                                    break;
                                case "date":
                                    fejl = false;
                                    break;
                                case "time":
                                    fejl = false;
                                    break;
                                case "timestamp":
                                    fejl = false;
                                    break;
                            }

                            if (fejl == true)
                            {
                                MessageBox.Show("Forkert angivet dataformat i tabel AV:" + AV_Info.AVID + " Tabel:" + tabel.titel + " (" + feltdef.datatype + ")");
                                LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Forkert angivet dataformat i tabel AV:" + AV_Info.AVID + " Tabel:" + tabel.titel + " (" + feltdef.datatype + ")", "");
                            }
                        }

                        feltdef.bredde = GetTextElement("<bredde>", "</bredde>", feltdefText).Trim().ToLower();
                        feltdef.feltinfo = GetTextElement("<feltinfo>", "</feltinfo>", feltdefText).Trim();
                        feltdef.feltfunktion = GetTextElement("<feltfunk>", "</feltfunk>", feltdefText).ToLower().Trim();
                        
                        //Her testes om feltfunktion angivelse er valid
                        if (feltdef.feltfunktion != "")
                        {
                            bool fejl = true;
                            switch (feltdef.feltfunktion)
                            {
                                case "sagsidentifikation":
                                    fejl = false;
                                    break;
                                case "lagringsform":
                                    fejl = false;
                                    break;
                                case "dokumentidentifikation":
                                    fejl = false;
                                    break;
                                case "sagstitel":
                                    fejl = false;
                                    break;
                                case "afleveret":
                                    fejl = false;
                                    break;
                                case "dokumenttitel":
                                    fejl = false;
                                    break;
                                case "dokumentdato":
                                    fejl = false;
                                    break;
                                case "afsender/modtager":
                                    fejl = false;
                                    break;
                                case "myndighedsidentifikation":
                                    fejl = false;
                                    break;
                            }

                            if (fejl == true)
                            {
                                LOG(FejlTypeMetadata.MetadataTekst, 2, "Forkert angivet feltfunktion i tabel AV:" + AV_Info.AVID + " Tabel:" + tabel.titel + " Felt:" + feltdef.titel + " (" + feltdef.feltfunktion + ")", "");
                            }
                        }

                        //Data lægges ind i liste
                        tabel.feltDefList.Add(feltdef);
                        
                    }
                }

                //Nøgledef PN
                List<string> PNTextList = new List<string>();
                PNTextList = GetTextElements("<pn>", "</pn>", tabelText);
                if (PNTextList != null)
                {
                    if (PNTextList.Count > 1)
                    {
                        LOG(FejlTypeMetadata.MetadataTekst, 2, "Ups ... tabel med flere primærnøgler!", "");
                    }
                 
                    //Her er hardkodet at der kun kan være angivet én primærnøgle !
                    List<string> PNTitelList = new List<string>();
                    PNTitelList = GetTextElements("<titel>", "</titel>", PNTextList[0]);
                    foreach (string titel in PNTitelList)
                    {
                        //MessageBox.Show(titel);
                        tabel.PN.felter.Add(titel.Trim());
                    }
                }

                
                //**** Nøgledef FN ****
                List<string> FNTextList = new List<string>();
                FNTextList = GetTextElements("<fn>", "</fn>", tabelText);//den samlede FN tekst
                 
                if (FNTextList != null)
                {
                    foreach (string FNText in FNTextList)
                    {
                    //TODO: FN, benyt 2 lister i stedet for genbrug af en ... så kan de sammenlignes (øh sammenlignes med hvad!)
                        List<string> titelList = new List<string>();

                        FN fn = new FN();

                        //Her skal liste over nøgle(r) læses før <fremmedtabel>   
                        int pos = FNText.ToLower().IndexOf("<fremmedtabel>");
                        titelList = GetTextElements("<titel>", "</titel>", FNText.Substring(0, pos));//Kun første del af den samlede FN text
                        
                        if (titelList != null)
                        foreach (string titel in titelList)
                        {
                            fn.PrimærTabelFelter.Add(titel);
                        }

                        //Herefter er det fremmednøglenjs felter hvor første titel er navnet på fremmedtabellen og de efterfølgende er navnet på felte(r) i fremmedtabellen
                        //Første titel element er navnet på fremmedtabel 
                        //Efterfølgende titel elementer er fremmednøglens felter som læses ind
                        string fremmedtabelText = GetTextElement("<fremmedtabel>", "</fremmedtabel>", FNText);

                        titelList = GetTextElements("<titel>", "</titel>", fremmedtabelText);//
                        if (titelList != null)
                        {
                            fn.fremmeTabelTitel = titelList[0];//Første titel er navn på tabellen som der refereres til 
                            fn.titel = "FN_" + fn.fremmeTabelTitel;//... det benytter vi lige til at lave et FN navn som ikke eksisterer i 342
                            for (int i = 1; i < titelList.Count; i++)
                            {
                                //Efterfølgende titler er felter i fremmedtabellen
                                fn.FremmedTabelFelter.Add(titelList[i]);
                            }
                        }
                        
                        //*** Kardinalitet benyttes ikke i forbindelse med skabelse af 1007, men er medtaget som og i den indlæste datastruktur! ***
                        List<string> kardinalitetList = new List<string>();
                        kardinalitetList = GetTextElements("<kardinalitet>", "</kardinalitet>", FNText);//
                        if (kardinalitetList != null)
                        {
                            if (kardinalitetList.Count == 2)
                            {
                                fn.kardinalitet1 = kardinalitetList[0];
                                fn.kardinalitet2 = kardinalitetList[1];
                            }
                            else
                            {
                                LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Fejl kardinalitet har ikke 2 angivelser! AV:" + AV_Info.AVID + " Tabel:" + tabel.titel, "");
                            }
                        }
                        else
                        {
                            LOG(AV.FejlTypeMetadata.MetadataTekst, 2, "Fejl kardinalitet = null! AV:" + AV_Info.AVID + " Tabel:" + tabel.titel, "");
                        }

                        //FN tilføjes tabel
                        tabel.FNList.Add(fn);
                    }
                }


                //Kodedef
                List<string> kodedefTextList = new List<string>();
                kodedefTextList = GetTextElements("<kodedef>", "</kodedef>", tabelText);
                if (kodedefTextList != null)
                {
                    foreach (string kodedefText in kodedefTextList)
                    {

                        Kodedef kodedef = new Kodedef();
                        kodedef.kodetitel = GetTextElement("<titel>", "</titel>", kodedefText).Trim();

                        //Findes kodedef.kodetitel også som et felt i tabellen? og hvad er dens datatype?
                        bool fundet = false;
                        foreach (FeltDef def in tabel.feltDefList)
                        {
                            if (def.titel.ToLower() == kodedef.kodetitel.ToLower())
                            {
                                kodedef.datatype = def.datatype;
                                fundet = true;
                                break;
                            }
                        }

                        if (fundet == false)
                        {
                            LOG(FejlTypeMetadata.MetadataTekst, 2, "Kodeliste " + kodedef.kodetitel + " ikke refereret i tabel: " + tabel.titel, "");
                        }

                        List<string> kodeList = new List<string>();
                        kodeList = GetTextElements("<kode>", "</kode>", kodedefText);
                        bool isOdd = false;
                        string kode1 = "";
                        
                    //Opsamler kodepar i sorteret liste som fejler hvis Key ikke er unik, men fortsætter indlæsning
                    SortedList<string,string> KeyTestList = new SortedList<string,string>();
                        
                        foreach (string kode in kodeList)
                        {
                            //Hver anden gang skal der tilføjes til liste ....       
                            if (isOdd == true)
                            {
                                KeyValuePair<string, string> kv = new KeyValuePair<string, string>(kode1,kode);
                                kodedef.koder.Add(kv);

                                try
                                {
                                    //kode1 skal være unik i liste ... ellers kastes en exception (hvilket vi udnytter)
                                    KeyTestList.Add(kode1, kode);
                                }
                                catch
                                {
                                    LOG(FejlTypeMetadata.MetadataTekst, 2, "Kode ikke unik!" + AV_Info.AVID + " Tabel:" + tabel.titel + "Kodeliste: " + kodedef.kodetitel + " Kode: " + kode1, "");
                                }
 
                            }
                          else
                            {
                                //Gem midlertidig til value er læst
                                kode1 = kode;
                            }

                            isOdd = !isOdd;
                        } // foreach (string kode in kodeList)

                        //Test til slut om der findes en key uden value ...
                        if (isOdd)
                        {
                            LOG(FejlTypeMetadata.MetadataTekst, 2, "Manglende kodepar værdi!" + AV_Info.AVID + " Tabel:" + tabel.titel, "");
                        }

                        //Kodeliste tilføjes
                        tabel.KodedefList.Add(kodedef);
                    }
                }

                //Tilføj til sidst tabel til metadata
                AV_Info.metadata.tabelList.Add(tabel);
            }// foreach (string tabel in tblList) ...


            List<string> saqTextList = new List<string>();
            saqTextList = GetTextElements("<saq>", "</saq>", AV_Info.metadata.metadataTekst);
            if (saqTextList != null)
            {
                foreach (string saqText in saqTextList)
                {
                    //Her opsamles saqinfo og saqdata
                    SAQ saq = new SAQ();
                    saq.SAQInfo = GetTextElement("<saqinfo>", "</saqinfo>", saqText);
                    saq.SAQData = GetTextElement("<saqdata>", "</saqdata>", saqText);
                    AV_Info.metadata.SAQList.Add(saq);
                }
            }

        }



        public class AV_info
        {
            //Helt overordnede oplysninger om AV
            public string AVID = "";
            public string AV_NR = "";
            public string udgave = "";
            public string AV342folderPath = "";
     
            //Data fra .tabfiler samt oplysninger fra metadatafilen, lagt i fast struktur
            public Metadata metadata = new Metadata();

            //Klasse som rummer oplysninger om datapakker og tabeller (.ARK filer og fragmenter)
            public DataFiler dataFiler = new DataFiler();

            //Selvstændig klasse som rummer oplysninger om konvertering til 1007
            public Konvertering Konvertering = new Konvertering();
        }


        public class DataFiler
        {
            //Lister over fysiske datapakker og .ARK filer (tabeller og fragmenter) 
            public List<string> DPList = new List<string>();//indeholder komplet liste over 342 datapakker på disk
            public List<string> tabelFilListe = new List<string>();//indeholder komplet liste over 342 tabeller jv indhold i mapper (.ark filer)

            //Struktureret fejliste
            public List<FejlDatafiler> fejlListe = new List<FejlDatafiler>();
        }


        public class Konvertering
        {

            //Struktureret fejliste
            public List<FejlKonvertering> fejlListe = new List<FejlKonvertering>();
        }


        public class ArkverTab
        {
            public string ID = "";
            public string Afltype = "";
            public string MedieID = "";
            public string TidligereAfl = "";
            public string systemnavn = "";
            public string startdato = "";
            public string slutdato = "";
            public string arkverTabFilePath = "";
        }

        public class Skaber
        {
            public string skaber = "";
            public string startdato = "";
            public string slutdato = "";
        }


        public class GeninfoTab
        {
            public string medieID = "";
            public string dokbib = "";
            public string beskrivelse = "";
            public string geninfoDocfolderPath = "";
        }


        public class DokumentTab
        {
            public string dokID = "";
            public string medieID = "";
            public string underdir = "";
            public string docPath = "";
        }

        public class FilmapTab
        {
            public string fil = "";
            public string medieID = "";
            public string filePath = "";
        }

        public enum FejlTypeMetadata { Mappestruktur, ArkverTab, SkaberTab, FilmapTab, DokumentTab, GeninfoTab, MetadataTekst };
        public class FejlMetadata
        {
            public FejlTypeMetadata fejlType;//Enumeration
            public int Severity = 0;//0 = info 1= hint 2= fejl
            public string fejlMelding = "";
            public string location = "";//evt sti til mappe eller fil hvor fejl optræder
        }

        public enum FejlTypeKonvertering { ArchiveIndex, ContextDocumentation, Documents, Tabel, Feltdefinition, Datatype, Kodeværdi, PN, FN, SAQ };
        public class FejlKonvertering
        {
            public FejlTypeKonvertering fejlType;//Enumeration
            public int Severity = 0;//0 = info 1= hint 2= fejl
            public string fejlMelding = "";
            public string location = "";//evt sti til mappe eller fil hvor fejl optræder
        }

        public enum FejlTypeDatafiler { Tabeller, Dokumenter };
        public class FejlDatafiler
        {
            public FejlTypeDatafiler fejlType;//Enumeration
            public int Severity = 0;//0 = info 1= hint 2= fejl
            public string fejlMelding = "";
            public string location = "";//evt sti til mappe eller fil hvor fejl optræder
        }


        //*** Metadata ***
        public class Metadata
        {
            public string metadataTekst = "";
            public string metadataTekstFilePath = "";
           
            //Data fra metadatafilen
            public List<Tabel> tabelList = new List<Tabel>();
            public List<SAQ> SAQList = new List<SAQ>();

            //Data fra .tab filerne
            public ArkverTab arkverTab = new ArkverTab();//indeholder info fra arkver.tab
            public List<Skaber> skaberList = new List<Skaber>();//indeholder info fra skaber.tab
            public List<GeninfoTab> geninfoList = new List<GeninfoTab>();//indeholder info fra geninfo.tab
            public List<FilmapTab> filemapList = new List<FilmapTab>();//indeholder info fra filemap.tab
            public List<DokumentTab> dokumentList = new List<DokumentTab>();//indeholder info fra dokmap.tab

            //Struktureret fejliste
            public List<FejlMetadata> fejlListe = new List<FejlMetadata>();
        }


        public class Tabel
        {
            public string titel = "";
            public string tabelInfo = "";
            public string posttype = "";
            public List<FeltDef> feltDefList = new List<FeltDef>();
            public PN PN = new PN();
            public List<FN> FNList = new List<FN>();
            public List<Kodedef> KodedefList = new List<Kodedef>();
            //Opsamling til tableIndex.xml
            public int tabelnummmer1007 = 0;
            public int rowCount = 0;
        }


        public class PN
        {
            public List<string> felter = new List<string>();
        }


        public class FN
        {
            public string titel = "";//navn på fremmednøgle (bare en label som er uden betyning)
            public string fremmeTabelTitel = "";
            public List<string> PrimærTabelFelter = new List<string>();
            public List<string> FremmedTabelFelter = new List<string>();
            public string kardinalitet1 = "";
            public string kardinalitet2 = "";
        }


        public class FeltDef
        {
            public string titel = "";
            public string datatype = "";
            public string bredde = "";
            public string feltinfo = "";
            public string feltfunktion = "";
            //Opsamling til tableIndex.xml
            public bool nullable = false;
        }

        public class Kodedef
        {
            public string kodetitel = "";
            //TODO: Klassen Kodedef, tilføje sammensat 1007 "oprindelig tabelnavn" til opsamling, eller (som nu) opdater tabelindex.xml løbende i forb. med skabelse af opslagstabellerne?
            public List<KeyValuePair<string,string>> koder = new List<KeyValuePair<string,string>>();

            //Opsamling til tableIndex.xml (hver kodeliste bliver til en tabel i 1007)
            public int tabelnummmer1007 = 0;
            public string datatype = "";
            public bool nullable = false;//Key må ikke være nullable eftersom den er primær, men Value feltet må vel godt?
        }

        public class SAQ
        {
            public string SAQInfo = "";
            public string SAQData = "";
        }

    }//AV
}
