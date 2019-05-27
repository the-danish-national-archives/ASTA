using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
//using System.Security.Cryptography;
//using System.Diagnostics;

namespace _342_til_1007_vis_test_konverter
{
    
    public partial class Form1 : Form
    {
        //AV_info AVInfo = new AV_info();
        //ArkverTab arkverTab = new ArkverTab();
        
        //List<GeninfoTab> geninfoList = new List<GeninfoTab>();
        //List<Skaber> skaberList = new List<Skaber>();
        //List<DokumentTab> dokumentList = new List<DokumentTab>();
        //List<string> DPList = new List<string>();
        //List<string> tabelList = new List<string>();
        AV AV;
        int tabelnummmer1007Global = 0;


        public Form1()
        {
            InitializeComponent();
            ReadIni();
        }

        /// <summary>
        /// Benyttes udelukkende til at kopiere Schema filer
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        private void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        
        private void ReadIni()
        {
            string iniFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),"app.ini");
            IniFile myIni = new IniFile(iniFilePath);
            string latestDestDriveLetter = myIni.IniReadValue("stier", "latestDestDriveLetter");
            for (int i = 0; i < comboBoxDest.Items.Count;i++)
            {
                if (comboBoxDest.Items[i].ToString() == latestDestDriveLetter)
                {
                    comboBoxDest.SelectedIndex = i;
                    return;
                }
            }
           
            //Hvis inifil ikke indeholder et valid drev
            comboBoxDest.SelectedIndex = 0;
        }



        /// <summary>
        /// Udgave bestemmes  A,B,C eller D
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string Get_Udgave(string path)
        {
            //*** Udgave bestemmes ***
            string udgave = "";

            //Der er reelt kun 3 muligheder for udgavemapper A,B,C, men D medtages for en sikkerheds skyld
            if (Directory.Exists(Path.Combine(path, "A")) == true)
            {
                udgave = "A";
                return udgave;
            }
            else if (Directory.Exists(Path.Combine(path, "B")) == true)
            {
                udgave = "B";
                return udgave;
            }
            else if (Directory.Exists(Path.Combine(path, "C")) == true)
            {
                udgave = "C";
                return udgave;
            }
            else if (Directory.Exists(Path.Combine(path, "C")) == true)
            {
                udgave = "D";
                return udgave;
            }

            return udgave;
        }


        /// <summary>
        /// Rent dump af metadatafilen
        /// </summary>
        private void Vis_MetadataTekst()
        {
                richTextBoxMetadata.Text = AV.AV_Info.metadata.metadataTekst;
        }


        /// <summary>
        /// Resume af AV,ens tabeller, antal kolonner, nøgler etc.
        /// </summary>
        private void Vis_MetadataResume()
        {
            string LOG = "";
            LOG = LOG + "Antal tabeller: " + AV.AV_Info.dataFiler.tabelFilListe.Count.ToString() + "\r\n";
            LOG = LOG + "\r\n";
            //For hver tabel
            foreach (AV.Tabel tabel in AV.AV_Info.metadata.tabelList)
            {
                LOG = LOG + "Tabel: " + tabel.titel + "\r\n";
                LOG = LOG + "Posttype: " + tabel.posttype + "\r\n";
                LOG = LOG + "Feltdefinitioner (kolonner): " + tabel.feltDefList.Count.ToString() + "\r\n";
                LOG = LOG + "Primærnøgle består af: " + tabel.PN.felter.Count.ToString() + " felter\r\n";
                LOG = LOG + "Fremmednøgler: " + tabel.FNList.Count.ToString() + "\r\n";
                LOG = LOG + "Kodelister: " + tabel.KodedefList.Count.ToString() + "\r\n";
                LOG = LOG + "\r\n";
            }
            LOG = LOG + "Der blev fundet " + AV.AV_Info.metadata.SAQList.Count.ToString() + " SAQ" + "\r\n";
            richTextBoxLOG.Text = richTextBoxLOG.Text + LOG;
        }


        /// <summary>
        /// Viser en del af dokumenterne i listbox, samt det samlede antal
        /// </summary>
        private void Vis_Dokumenter()
        {
            int counter = 0;
            //Visning
            foreach (AV.DokumentTab d in AV.AV_Info.metadata.dokumentList)
            {
                listBoxDokumenter.Items.Add("DokID:" + d.dokID + "  MedieID:" + d.medieID + "  UnderDir:" + d.underdir);
                counter++;
                if (counter > 99) break;
            }

            if (counter == 0) labelDokumenter.Text = "Dokumenter (0)";
            else
                labelDokumenter.Text = "Dokumenter (" + counter.ToString() + " ud af " + AV.AV_Info.metadata.dokumentList.Count.ToString() + " er listet)";
        }


        private void Vis_Geninfo()
        {
            foreach (AV.GeninfoTab g in AV.AV_Info.metadata.geninfoList)
            {
                listBoxGeninfo.Items.Add("MedieID:" + g.medieID + "  Dokbib:" + g.dokbib + "  Beskrivelse:" + g.beskrivelse);
            }


            labelGeninfo.Text = "Geninfo dokumenter (" + AV.AV_Info.metadata.geninfoList.Count.ToString() + ")";
        }



        private void Vis_Skaber()
        {
            labelSkaber.Text = "Skaber (" + AV.AV_Info.metadata.skaberList.Count.ToString() + ")";

            foreach (AV.Skaber s in AV.AV_Info.metadata.skaberList)
           {
               listBoxSkaber.Items.Add("Skaber:" + s.skaber.Trim() + "  Startdato:" + s.startdato + "  Slutdato:" + s.slutdato);
           }
        }
        
    

        private void Vis_ArkverTab()
        {
                richTextBoxArkver.AppendText("ID: " + AV.AV_Info.metadata.arkverTab.ID + "\r\n");//8 karakter lang AV ID
                richTextBoxArkver.AppendText("Afltype: " + AV.AV_Info.metadata.arkverTab.Afltype + "\r\n");
                richTextBoxArkver.AppendText("MedieID: " + AV.AV_Info.metadata.arkverTab.MedieID + "\r\n");
                richTextBoxArkver.AppendText("TidligereAfl: " + AV.AV_Info.metadata.arkverTab.TidligereAfl + "\r\n");
                richTextBoxArkver.AppendText("Systemnavn: " + AV.AV_Info.metadata.arkverTab.systemnavn + "\r\n");
                richTextBoxArkver.AppendText("Startdato: " + AV.AV_Info.metadata.arkverTab.startdato + "\r\n");
                richTextBoxArkver.AppendText("Slutdato: " + AV.AV_Info.metadata.arkverTab.slutdato);
        }


        private void Vis_Datapakker()
        {

            //*** Led efter datapakker og tilføj til liste ***
            foreach (string dp in AV.AV_Info.dataFiler.DPList)
            {
                listBoxDP.Items.Add(dp);
            }

            label_DP_liste.Text = "Datapakker (" + AV.AV_Info.dataFiler.DPList.Count.ToString() + ")";
        }


        private void Vis_FilmapTab()
        {

            //*** Led efter filer og mapper og tilføj til liste ***
            foreach (AV.FilmapTab filmap in AV.AV_Info.metadata.filemapList)
            {
                listBoxFilmap.Items.Add(filmap.fil + " (MedieID:" + filmap.medieID+")");
            }

            labelFilmap.Text = "Filer og mapper (" + AV.AV_Info.metadata.filemapList.Count.ToString() + ")";
        }


        /// <summary>
        /// Visning af tabeller udfra metadata (benyttes ikke)
        /// </summary>
        private void Vis_Tabeller()
        {
            //TODO: Vis_Tabeller, visning benyttes ikke ... slet?
            foreach (AV.Tabel tabel in AV.AV_Info.metadata.tabelList)
            {
                listBoxTable.Items.Add(tabel.titel);
            }

            labelTabeller.Text = "Tabeller (" + AV.AV_Info.metadata.tabelList.Count.ToString() + ")";
        }

        /// <summary>
        /// Visning af tabeller udfra liste over fysiske filer
        /// </summary>
        private void Vis_TabelFiler()
        {
            int unikke_tabeller_count = 0;
            foreach (string t in AV.AV_Info.dataFiler.tabelFilListe)
            {
                string ext = Path.GetExtension(Path.GetFileName(t)).ToUpper();
                // MessageBox.Show(ext);
                if ((ext == ".ARK") || (ext == ".001"))
                {
                    unikke_tabeller_count++;
                }

                listBoxTable.Items.Add(t);
            }

            labelTabeller.Text = "Tabeller (" + unikke_tabeller_count.ToString() + " unikke)";
        }


        private void Indlæs_342(string AV_342_Path, bool visGUI)
        {
                        
            //GUI resettes
            listBoxDP.Items.Clear();
            label_DP_liste.Text = "Datapakker (0)";

            listBoxFilmap.Items.Clear();
            labelFilmap.Text = "Filer og mapper (0)";

            listBoxGeninfo.Items.Clear();
            labelGeninfo.Text = "Geninfo dokumenter (0)";

            listBoxTable.Items.Clear();
            labelTabeller.Text = "Tabeller (0)";

            richTextBoxArkver.Clear();
            labelGeninfo.Text = "Geninfo dokumenter (0)";

            listBoxSkaber.Items.Clear();
            labelSkaber.Text = "Skaber (0)";

            listBoxDokumenter.Items.Clear();
            labelDokumenter.Text = "Dokumenter (0)";

            richTextBoxMetadata.Clear();
                        
           // toolStripStatusLabel1.Text = "";
           // Application.DoEvents();

            //*** Selve indlæsningen af den aktuelle AV ***
            if (Directory.Exists(AV_342_Path) == true)
            {
                labelAV.Text = AV_342_Path;

                if ((Path.GetFileName(AV_342_Path).Length == 8) && (Path.GetFileName(AV_342_Path).Substring(0, 3) == "000"))
                {
                    AV = new AV();
                    
                    AV.AV_Info.AVID = Path.GetFileName(AV_342_Path);
                    AV.AV_Info.udgave = Get_Udgave(AV_342_Path);
                    AV.AV_Info.AV342folderPath = AV_342_Path;
                    int res = -1;
                    if (int.TryParse(AV.AV_Info.AVID, out res) == true)
                    {
                        AV.AV_Info.AV_NR = res.ToString();
                    }
                    else 
                    {
                        MessageBox.Show("Fejl AVID: " + AV.AV_Info.AVID);
                        return;
                    }

                    toolStripStatusLabel1.Text = "Indlæser: " + AV.AV_Info.AVID;
                    Application.DoEvents();
                    
                    
                    //**** Indlæsning af oplysninger fra filstruktur ****

                    richTextBoxLOG.Text = richTextBoxLOG.Text +"***********************************************\r\n";
                    richTextBoxLOG.Text = richTextBoxLOG.Text + "Parsning påbegyndt af AV " + AV.AV_Info.AVID + "\r\n";
                    richTextBoxLOG.Text = richTextBoxLOG.Text + "***********************************************\r\n";

                    //*** Her sker selve indlæsningen fra 342 .tab filer og metadata filen***
                    AV.Indlæs_342_metadata(AV_342_Path);
                    bool success = true; //Success bestemmes ved gennemløb af metadataLog   
                    foreach (AV.FejlMetadata fejl in AV.AV_Info.metadata.fejlListe)
                    {
                        if (fejl.Severity > 0) 
                        {
                            success = false;
                            break;
                        }
                    }
         

                    if (success == true)
                    {
                         richTextBoxLOG.Text = richTextBoxLOG.Text + "Parsning afsluttet uden fejl" + "\r\n\r\n"; ;
                         toolStripStatusLabel1.Text = "Parse af AV: " + AV.AV_Info.AVID + " afsluttet";
                    }
                 else
                    {
                        richTextBoxLOG.Text = richTextBoxLOG.Text + "Parsning afsluttet med fejl !!!" + "\r\n\r\n"; ;
                        toolStripStatusLabel1.Text = "Fejl i forbindelse med indlæsning og parse af AV: "+ AV.AV_Info.AVID;
            // MessageBox.Show("Fejl i forbindelse med indlæsning og parse af AV: "+ AV.AV_Info.AVID);
                    }

                    
                    //*** Her sker en validering af tabelfiler og dokumentstier ***
                    if (success == true)
                    {
                        AV.Indlæs_342_tabel_og_dokument_filer(AV_342_Path);
                        foreach (AV.FejlDatafiler fejl in AV.AV_Info.dataFiler.fejlListe) //Success bestemmes ved gennemløb af fejldatafilerLog   
                        {
                            if (fejl.Severity > 0)
                            {
                                success = false;
                                break;
                            }
                        }
                    }


                    //*** Visning af 342 indhold ***
                    if (visGUI == true)
                    {
                        Vis_ArkverTab();
                        Vis_Skaber();
                        Vis_Geninfo();
                        Vis_Datapakker();
                        Vis_FilmapTab();
                        Vis_TabelFiler();
                        Vis_Dokumenter();
                        Vis_MetadataTekst();
                        Vis_MetadataResume();

                        toolStripStatusLabel1.Text = "Indlæsning og parse afsluttet";
                    }
                }
             else
                {
                    toolStripStatusLabel1.Text = "Klar til indlæsning og parse af AV";
                    labelAV.Text = "<Vælg AV mappe>";
                    Application.DoEvents();
                }

                
            }  
        }


        private void listBoxGeninfo_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(AV.AV_Info.metadata.geninfoList[listBoxGeninfo.SelectedIndex].geninfoDocfolderPath) == true)
            {
                string[] doks = Directory.GetFiles(AV.AV_Info.metadata.geninfoList[listBoxGeninfo.SelectedIndex].geninfoDocfolderPath);
                if (doks.Length > 0)
                {
                    string firstFile = doks[0];
                    //MessageBox.Show(firstFile);
                    System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo(firstFile, "OPEN");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo = startinfo;
                    proc.Start();
                }
                else
                {
                    MessageBox.Show("Mappe indeholder ingen filer: " + AV.AV_Info.metadata.geninfoList[listBoxGeninfo.SelectedIndex].geninfoDocfolderPath);
                }
            }
            else
            {
                MessageBox.Show("Mappen eksisterer ikke: " + AV.AV_Info.metadata.geninfoList[listBoxGeninfo.SelectedIndex].geninfoDocfolderPath);
            }
        }

        private void listBoxDokumenter_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(AV.AV_Info.metadata.dokumentList[listBoxDokumenter.SelectedIndex].docPath) == true)
            {
                string[] doks = Directory.GetFiles(AV.AV_Info.metadata.dokumentList[listBoxDokumenter.SelectedIndex].docPath);
                string firstFile = doks[0];
                //MessageBox.Show(firstFile);
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo(firstFile, "OPEN");
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = startinfo;
                proc.Start();
            }
            else
            {
                MessageBox.Show("Mappen eksisterer ikke: " + AV.AV_Info.metadata.dokumentList[listBoxDokumenter.SelectedIndex].docPath);
            }
        }

        private void shellTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            richTextBoxLOG.Clear();

            if (Directory.Exists(shellTreeView1.SelectedPath) == true)
            {
                Indlæs_342(shellTreeView1.SelectedPath, true);

                //*** Log vises altid, men der tilføjes ikke til Excel CSV fil****
                LOG_ShowAll();
            }
        }


        private void DirectoryDeleteAll(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }


        /// <summary>
        /// Den grundlæggende 1007 mappestruktur skabes.
        /// </summary>
        private void Skab1007mappeStruktur()
        {
            if (AV.AV_Info.AVID == "") return;

            //string AV_Path = AV.folderPath;
            if (Directory.Exists(AV.AV_Info.AV342folderPath) == true)
            {
                //!!! TODO: skal destinations drev kunne vælges?
                string driveLetter = comboBoxDest.SelectedItem.ToString();
                string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");

                if (Directory.Exists(AV_1007_path) == true)
                {
                    DirectoryDeleteAll(AV_1007_path);
                }

                Directory.CreateDirectory(AV_1007_path);
                Directory.CreateDirectory(AV_1007_path + "\\" + "ContextDocumentation"+"\\"+"docCollection1");

                Directory.CreateDirectory(AV_1007_path + "\\" + "Indices");

                string source = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Schemas");
                string dest = AV_1007_path + "\\" + "Schemas";
                CopyFolder(source, dest);

                //Directory.CreateDirectory(root + AV_1007 + "\\" + "Schemas");
                    
                Directory.CreateDirectory(AV_1007_path + "\\" + "Tables");
            }
            else
            {
                MessageBox.Show(AV.AV_Info.AV342folderPath + " ikke fundet!");
            }
        }


        private void Skab1007archiveIndex()
        {
            if (AV.AV_Info.AVID == "") return;

            toolStripStatusLabel1.Text = "Skaber archiveIndex";
            Application.DoEvents();

            string AV342_path = AV.AV_Info.AV342folderPath;//shellTreeView1.SelectedPath;
            if (Directory.Exists(AV342_path) == true)
            {
                if ((Path.GetFileName(AV342_path).Length == 8) && (Path.GetFileName(AV342_path).Substring(0, 3) == "000"))//TODO: Skab1007archiveIndex, vel kun nødvendig hvis procedure benyttes direkte
                {
                    string driveLetter = comboBoxDest.SelectedItem.ToString();
                    string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
                    string archiveIndexFilePath = Path.Combine(AV_1007_path + "\\" + "Indices", "archiveIndex.xml");

                    string templateFile = Path.GetDirectoryName(Application.ExecutablePath)+"\\archiveIndex.txt";

                    if (File.Exists(templateFile) == false)
                    {
                        AV.LOG(AV.FejlTypeKonvertering.ArchiveIndex,2, "Kunne ikke finde templatefilen archiveIndex.txt!", templateFile);
                        return;//TODO: Skab1007archiveIndex, returværdi ???
                    }
                    string archiveIndex = File.ReadAllText(templateFile, Encoding.UTF8);
                    archiveIndex = archiveIndex.Replace("<archiveInformationPackageID></archiveInformationPackageID>", "<archiveInformationPackageID>" + "AVID.SA." + AV.AV_Info.AV_NR + "</archiveInformationPackageID>");
                    archiveIndex = archiveIndex.Replace("<archivePeriodStart></archivePeriodStart>", "<archivePeriodStart>" + AV.AV_Info.metadata.arkverTab.startdato.Insert(4, "-").Insert(7, "-") + "</archivePeriodStart>");
                    archiveIndex = archiveIndex.Replace("<archivePeriodEnd></archivePeriodEnd>", "<archivePeriodEnd>" + AV.AV_Info.metadata.arkverTab.slutdato.Insert(4, "-").Insert(7, "-") + "</archivePeriodEnd>");

                    //Her skal itereres gennem skabere ...
                    string archiveCreatorList = "<archiveCreatorList>" + "\r\n";

                    foreach (AV.Skaber skaber in AV.AV_Info.metadata.skaberList)
                    { 
                       archiveCreatorList = archiveCreatorList + "<creatorName>"+ skaber.skaber + "</creatorName>" + "\r\n";
                       archiveCreatorList = archiveCreatorList + "<creationPeriodStart>"+ skaber.startdato.Insert(4, "-").Insert(7, "-") + "</creationPeriodStart>" + "\r\n";
                       archiveCreatorList = archiveCreatorList + "<creationPeriodEnd>"+ skaber.slutdato.Insert(4, "-").Insert(7, "-") + "</creationPeriodEnd>" + "\r\n";
                    }

                    archiveCreatorList = archiveCreatorList + "</archiveCreatorList>";
                    archiveIndex = archiveIndex.Replace("<archiveCreatorList></archiveCreatorList>", archiveCreatorList);

                    //TODO: Skab1007archiveIndex, vi har en 342 afltype (A,1,2,3) som kan/skal oversættes til de to 1007 elementer "archiveInformationPacketType" og "archiveType"

                    archiveIndex = archiveIndex.Replace("<systemName></systemName>", "<systemName>" + AV.AV_Info.metadata.arkverTab.systemnavn + "</systemName>");
                    archiveIndex = archiveIndex.Replace("<systemPurpose></systemPurpose>", "<systemPurpose>" + AV.AV_Info.metadata.arkverTab.systemnavn + "</systemPurpose>");//TODO: Skab1007archiveIndex, hellere N/A end kopi af Systemnavn?
                    archiveIndex = archiveIndex.Replace("<systemContent></systemContent>", "<systemContent>" + AV.AV_Info.metadata.arkverTab.systemnavn + "</systemContent>");//TODO: Skab1007archiveIndex, hellere N/A end kopi af Systemnavn?


                    if (AV.AV_Info.metadata.arkverTab.TidligereAfl != "")
                    {
                        archiveIndex = archiveIndex.Replace("<predecessorName></predecessorName>", "<predecessorName>" + AV.AV_Info.metadata.arkverTab.TidligereAfl + "</predecessorName>");
                    }
                    else 
                    {
                        archiveIndex = archiveIndex.Replace("<predecessorName></predecessorName>", "");
                    }

                    archiveIndex = archiveIndex.Replace("<containsDigitalDocuments></containsDigitalDocuments>", "<containsDigitalDocuments>" + (AV.AV_Info.metadata.dokumentList.Count > 0).ToString().ToLower() + "</containsDigitalDocuments>");

                    File.WriteAllText(archiveIndexFilePath, archiveIndex, Encoding.UTF8);
                }
            }
        }



        private void Skab1007contextDocumentation()
        {
            if (AV.AV_Info.AVID == "") return;

            toolStripStatusLabel1.Text = "Skaber contextDokumentation";
            Application.DoEvents();

            string AV342_path = AV.AV_Info.AV342folderPath;//shellTreeView1.SelectedPath;
            if (Directory.Exists(AV342_path) == true)
            {
                if ((Path.GetFileName(AV342_path).Length == 8) && (Path.GetFileName(AV342_path).Substring(0, 3) == "000"))//TODO: Skab1007contextDocumentation, vel kun nødvendig hvis procedure benyttes direkte
                {
                    //*** AV bestemmes ***
                    //string AV = Path.GetFileName(AV_Path);
                   // string AV_NR = Path.GetFileName(AV_Path).Substring(3, 5);
                    string driveLetter = comboBoxDest.SelectedItem.ToString();
                    string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
                    string docCollectionRoth = AV_1007_path + "\\" + "ContextDocumentation\\docCollection1";
                    string contextDocumentationFilePath = Path.Combine(AV_1007_path + "\\"+ "Indices","contextDocumentationIndex.xml");
                    //MessageBox.Show(contextDocumentationFilePath);

                    //Skab contextDocumentation.xml incl hoved
                    string contextDocumentation = File.ReadAllText("contextDocumentationIndex.head.txt", Encoding.UTF8);//TODO: Skab1007contextDocumentation, her bør benyttes en fuld sti
                    for (int i = 0; i < AV.AV_Info.metadata.geninfoList.Count; i++)
                    {
                        bool isConverted = false;// Flag som bestemmer om der skal tilføjes elementerne <archivalMigrationInformation> og <authorInstitution> (FSK dokumenter)
                        
                        string newDocFolder = Path.Combine(docCollectionRoth, (i+1).ToString());
                        Directory.CreateDirectory(newDocFolder);
                        //Analyser dokument
                        string[] docs = Directory.GetFiles(AV.AV_Info.metadata.geninfoList[i].geninfoDocfolderPath);

                        if (docs.Length < 1)
                        {
                            // MessageBox.Show("Geninfo dokumentmappe indeholder ingen fil(er): " + AV.AVInfo.geninfoList[i].geninfoDocfolderPath);
                            AV.LOG(AV.FejlTypeMetadata.GeninfoTab, 2, "Geninfo dokumentmappe indeholder ingen fil(er)", AV.AV_Info.metadata.geninfoList[i].geninfoDocfolderPath);
                        }
                     else
                        if (docs.Length == 1)
                        {
                            string ext = Path.GetExtension(docs[0]).ToLower();
                            if ((ext == ".tif") || (ext == ".mp3") || (ext == ".mp2")|| (ext == ".mpg"))
                            {
                                File.Copy(docs[0], Path.Combine(newDocFolder, "1" + ext));
                            }
                            else 
                            {
                                //TODO: Skab1007contextDocumentation, titel på dokument bliver altid 1.tif.txt (navn på tempfil) og vil fremgå i dokumentet

                                //FSK dokumenter TIFF,es ved hjælp af PDFCreator
                                PDFCreator_Print.PDFCreator_Print TIFF_Printer = new PDFCreator_Print.PDFCreator_Print();
                                string outputFilename = Path.Combine(newDocFolder, "1.tif");
                                string msg = TIFF_Printer.KonverterDokument(docs[0], outputFilename);
                                AV.LOG(AV.FejlTypeKonvertering.ContextDocumentation, 0, "Geninfo dokument " + docs[0] + " konverteret til TIFF " + outputFilename, "");

                                isConverted = true;//Flag som fortæller at elementerne <archivalMigrationInformation> og <authorInstitution> skal tilføjes senere

                            }
                        }
                     else //flere filer ...
                        {
                            Array.Sort(docs);
                            
                            //Her antages at der er tale om flere singlepage tif
                            //TODO: Skab1007contextDocumentation, !TEST af multipage mangler!
                            string ext = Path.GetExtension(docs[0]).ToLower();
                            if (ext == ".tif")
                            {
                                for (int ii = 0; ii < docs.Length; ii++)
                                {
                                    File.Copy(docs[ii], Path.Combine(newDocFolder, (ii + 1).ToString() + ext));
                                }

                            }
                          else
                            {
                                //TODO: bør vel ske i forbindlse med metadata indlæsning?
                                AV.LOG(AV.FejlTypeKonvertering.ContextDocumentation, 2, "Multible filer af uventet type .." + ext, docs[0]);
                            }
                        }

                        //Tilføj elementer til contextDocumentation.xml
                        string contextDocumentationBody = File.ReadAllText("contextDocumentationIndex.body.txt", Encoding.UTF8);//Template
                        contextDocumentationBody = contextDocumentationBody.Replace("<documentID></documentID>", "<documentID>" + (i + 1).ToString() + "</documentID>");
                        contextDocumentationBody = contextDocumentationBody.Replace("<documentTitle></documentTitle>", "<documentTitle>" + AV.AV_Info.metadata.geninfoList[i].beskrivelse + "</documentTitle>");
                        //Der findes ingen documentDescription i 342 så vi genbruger documentTitle
                        contextDocumentationBody = contextDocumentationBody.Replace("<documentDescription></documentDescription>", "<documentDescription>" + AV.AV_Info.metadata.geninfoList[i].beskrivelse + "</documentDescription>");
                     
                        
                        if (isConverted == true)
                        {
                            contextDocumentationBody = contextDocumentationBody.Replace("<authorInstitution></authorInstitution>", "<authorInstitution>Rigsarkivet</authorInstitution>");
                            contextDocumentationBody = contextDocumentationBody.Replace("<archivalMigrationInformation>false</archivalMigrationInformation>", "<archivalMigrationInformation>true</archivalMigrationInformation>");
                        }
                        else
                        {
                            //TODO: Skab1007archiveIndex, skal elementet authorInstitution helt væk hvis tomt? 
                            contextDocumentationBody = contextDocumentationBody.Replace("<authorInstitution></authorInstitution>", "<authorInstitution>N/A</authorInstitution>");
                        }

                        contextDocumentation = contextDocumentation + contextDocumentationBody;//element opsamles
                    }
                    //Tilføj slut element i contextDocumentation.xml
                    string contextDocumentationTail = File.ReadAllText("contextDocumentationIndex.tail.txt",Encoding.UTF8);
                    contextDocumentation = contextDocumentation + contextDocumentationTail;
                    File.WriteAllText(contextDocumentationFilePath, contextDocumentation, Encoding.UTF8);
                }
            }
        }




        private void buttonParseAlle_Click(object sender, EventArgs e)
        {
            richTextBoxLOG.Clear();//Multible kørsler, så richTextBoxLOG cleares kun i start

            File.WriteAllText(@"H:\Logfiler\!Fejliste.csv", "AVID;Mappestruktur;ArkverTab;SkaberTab;FilmapTab;DokumentTab;GeninfoTab;MetadataTekst;Link til logfil" + "\r\n");

            toolStripProgressBar1.Maximum = Directory.GetDirectories(shellTreeView1.SelectedPath, "*.*", SearchOption.TopDirectoryOnly).Length;
            toolStripProgressBar1.Value = 0;
            foreach (string folder in Directory.GetDirectories(shellTreeView1.SelectedPath,"*.*",SearchOption.TopDirectoryOnly))
            {
                toolStripProgressBar1.Value++;
                richTextBoxLOG.Clear();

                if (Directory.Exists(folder) == true)
                {
                    Indlæs_342(folder, false);

                    //*** Log vises, og der skrives til Excel CSV fil hvis der er fundet fejl****
                    string excelLogLinie = LOG_ShowAll();
                    if (excelLogLinie != "") File.AppendAllText(@"H:\Logfiler\!Fejliste.csv", excelLogLinie + "\r\n");
                }
                else
                {
                    MessageBox.Show("Kunne ikke finde "+folder);
                }
            }

            MessageBox.Show("Done");
        }


        private DataTypeKonverteringsResultat DataTypeKonverter(string dataString342, string datatype342)
        {
            Int64 tempInt = -1;//TODO: DataTypeTest, er det muligt at initialisere som null?
            float tempFloat = -1;//TODO: DataTypeTest, er det muligt at initialisere som null?
           
            DataTypeKonverteringsResultat result = new DataTypeKonverteringsResultat();

            if (dataString342 == "") return (result);

            //Benyttes til at returnere konverteret data 
            string dataString1007 = string.Empty;

            switch (datatype342)
            {
                case "string":
                        dataString1007 = System.Security.SecurityElement.Escape(dataString342);//XML beskyttede tegn skal konverteres fx & < > ;
                        break;

                case "num":
                        if (dataString342.Trim() == "")
                        {
                            result.data1007 = "";//NULL
                        }
                     else
                        if (Int64.TryParse(dataString342, out tempInt) == false)
                        {
                            result.fejlmeddelelse = "NUM fejl: '" + dataString342 + "'";
                            result.data1007 = dataString342.Trim();//data returneres på trods af fejl
                        }
                        else
                        {
                            result.data1007 = dataString342.Trim();//Ingen konvertering (foranstillede nuller kan have betydning fx CPR og journalnumre
                        }
                        break;

                case "real":
                        if (float.TryParse(dataString342, out tempFloat) == false)
                        {
                            result.fejlmeddelelse = "REAL fejl: '" + dataString342 + "'";
                            result.data1007 = dataString342.Trim();//Ingen konvertering (hvad når der er fejl?)
                        }
                        else
                        {
                            //TODO: DataTypeTest, vil kunne strippe nuller ved at benytte dataString1007 = tempFloat.ToString();
                            //MessageBox.Show(dataString342 + " --> " + tempFloat.ToString());
                            result.data1007 =  tempFloat.ToString();
                        }
                        break;

                case "exp":
                        //TODO: DataTypeTest, mangler at håndtere EXP (hvis der findes nogle?), men SQL/XML datatype kan være Decimal og eller Float
                        MessageBox.Show("EXP datatype konvertering ikke implementeret: '" + dataString342 + "'");
                        break;

              case "date":
                        if (dataString342.Trim() == "")
                        {
                            result.data1007 = "";
                        }
                      else
                        if (dataString342.Trim().Length != 8)
                        {
                            result.fejlmeddelelse = "Fejl i dato bredde (bør være 8, men er " + dataString342.Trim().Length.ToString()+")";
                            result.data1007 = dataString342;//Ingen konvertering selvom der er fejl
                        }
                      else
                        {
                            DateTime datoTest;
                            if (DateTime.TryParseExact(dataString342.Trim(), "yyyyMMdd", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                            {
                                result.fejlmeddelelse = "Fejl i datoformat: " + dataString342;
                                result.data1007 = dataString342;//Ingen konvertering selvom der er fejl
                            }
                            else
                            {
                                result.data1007 = datoTest.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("da-DK"));//bindestreger tilføjes
                            }
                        }
                        break;

              case "time":
                        if (dataString342.Trim() == "")
                        {
                            result.data1007 = "";
                        }
                        else
                            if (dataString342.Trim().Length < 6)
                            {
                                result.fejlmeddelelse = "Fejl i time: " + dataString342.Trim();
                                result.data1007 = dataString342;//Ingen konvertering selvom der er fejl
                            }
                            else
                            {
                                DateTime datoTest;

                                if (DateTime.TryParseExact(dataString342.Trim(), "hhmmss,FFFFFFF", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                                {
                                    result.fejlmeddelelse = "Fejl i time: " + dataString342;
                                    result.data1007 = dataString342.Trim();//Ingen konvertering selvom der er fejl
                                }
                            }
                        break;

               case "timestamp":
                        if (dataString342.Trim() == "")
                        {
                            result.data1007 = "";
                        }
                      else
                        if (dataString342.Trim().Length < 17)
                        {
                            result.fejlmeddelelse = "Fejl i timestamp: " + dataString342.Trim();
                            result.data1007 = dataString342;//Ingen konvertering selvom der er fejl
                        }
                      else
                        {
                            DateTime datoTest;

                            if (DateTime.TryParseExact(dataString342.Trim(), "yyyyMMddThhmmss,FFFFFFF", CultureInfo.GetCultureInfo("da-DK"), DateTimeStyles.None, out datoTest) == false)
                            {
                                result.fejlmeddelelse = "Fejl i timestamp: " + dataString342;
                                result.data1007 = dataString342.Trim();//Ingen konvertering selvom der er fejl
                            }
                        }

                        break;
               default:
                        MessageBox.Show("Datatype: '" + datatype342 + "' ikke understøttet!!!");
                        result.fejlmeddelelse = "Datatype: '" + datatype342 + "' ikke understøttet!!!";
                        result.data1007 = dataString342;
                        break;

            }

            return (result);
        }





         private bool Skab1007tabelIndex()
         {
             if (AV.AV_Info.AVID == "") return (false);

             toolStripStatusLabel1.Text = "Skaber tableIndex.xml";
             Application.DoEvents();
             string driveLetter = comboBoxDest.SelectedItem.ToString();
             string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
             string tableIndexFileName = Path.Combine(AV_1007_path, "Indices\\tableIndex.xml");

             XmlTextWriter ti = new XmlTextWriter(tableIndexFileName, Encoding.UTF8);
    		 ti.Formatting = Formatting.Indented;

             //TODO: Skab1007tabelIndex, dbName og dataBaseProduct benyttes ikke ... afventer ny ADA !!!!!!!
             //TODO: Skab1007tabelIndex, Midlertidig work around !!!!!!!
             string dbName = "ikke_relevant";
		     string dataBaseProduct = "Microsoft SQL server";

		     TableIndexWriter tiw = new TableIndexWriter();

             //Skab start af tableIndex
             tiw.writeStartXML(ti,  dbName,  dataBaseProduct);


             //*** For hver alm. tabel ****
             foreach (AV.Tabel tabelMetadata in AV.AV_Info.metadata.tabelList)
             {

                 //*** tableName, tableNumber, descriptions ***
                 string tableName = tabelMetadata.titel;
                 //TODO: Skab1007tabelIndex, Tabeltitel må ikke være nummerisk 
                 int tempInt = 0;
                 if (int.TryParse(tableName[0].ToString(),out tempInt) == true)
                 {
                     AV.LOG(AV.FejlTypeKonvertering.Tabel, 1, "Tabelnavn må ikke starte med et nummer", tableName);
                 }
                 
                 string tableNumber = "table"+tabelMetadata.tabelnummmer1007.ToString();
                 string tableDescription = tabelMetadata.tabelInfo;

                 tiw.writeTableIndex(ti, tableName, tableNumber, tableDescription);//starter en ny tabel

                 //*** Tilføjer felter (kolonner) ***
                 for (int i = 0; i < tabelMetadata.feltDefList.Count; i++)
                 {
                     string columnName = tabelMetadata.feltDefList[i].titel;
                     //TODO: Skab1007tabelIndex, Tabelfelt må ikke være nummerisk 
                     if (int.TryParse(columnName[0].ToString(), out tempInt) == true)
                     {
                         AV.LOG(AV.FejlTypeKonvertering.Feltdefinition, 1, "Et feltnavn må ikke starte med et nummer", "Tabel: " + tableName + " Felt:" + columnName);
                     }

                     string columnID = "c" + (i + 1).ToString();
                     string type = "";  //tildeles værdi i senere switch ...
                     string typeOriginal = tabelMetadata.feltDefList[i].datatype;//Fritekst ... ingen udfalsrum
                     string description = tabelMetadata.feltDefList[i].feltinfo;
                     string nullable = tabelMetadata.feltDefList[i].nullable.ToString().ToLower();//skal være lowercase

                     switch (tabelMetadata.feltDefList[i].datatype)
                            {
                                case "num":
                                    type = "INTEGER";
                                    break;
                                case "real":
                                    type = "REAL";
                                    break;
                                case "exp":
                                    type = "REAL";//TODO: Skab1007tabelIndex, Er dette OK eller skal vi gøre noget andet? Findes der overhoved nogle exp i vores 342
                                    break;
                                case "string":
                                    type = "VARCHAR" + "("+ tabelMetadata.feltDefList[i].bredde.ToString() +")";
                                    break;
                                case "date":
                                    type = "DATE";
                                    break;
                                case "time":
                                    type = "TIME";
                                    break;
                                case "timestamp":
                                    type = "TIMESTAMP";
                                    break;
                            }


                     tiw.writeColumn(ti, columnName, columnID, type, typeOriginal, nullable, description);
                 }
                 tiw.writeEndColumns(ti); //afslutter kolonner for den aktuelle tabel	


                 //*** Tilføjer PN_nøgle ***
                 if (tabelMetadata.PN.felter.Count > 0)
                 {
                     string pkName = "PK_" + tabelMetadata.titel;//Primærnøglens navn er noget vi selv er nødt til at definerer (findes ikke i 342)
                     tiw.writePrimaryKey(ti, pkName, tabelMetadata.PN.felter);
                 }



                 //*** Tilføjer fremmednøgler for almindelige tabeller i systemet ***
                 //For hver FN ...
                 foreach (AV.FN FN in tabelMetadata.FNList)
                 {
                     tiw.writeForeignKeysStart(ti);
                     //For hver felt (faktisk et par, men opsamlet til hver sin liste)
                     for (int i = 0; i < FN.PrimærTabelFelter.Count; i++)
                     {
                         tiw.writeForeignKey(ti, FN.titel, FN.fremmeTabelTitel, FN.PrimærTabelFelter[i], FN.FremmedTabelFelter[i]);
                     }
            
                     tiw.writeForeignKeysEnd(ti);
                 }

                 //*** Herefter tilføjer fremmednøgler for kodelistetabellerne for den aktuelle tabel ***
                 foreach (AV.Kodedef kodeDef in tabelMetadata.KodedefList)
                 {
                     //TODO: Skab1007tabelIndex, For hver kodeliste skal der skabes en relation som tilføjes den aktuelle hovedtabel
                 }


                 //*** Antallet af ROWS tilføjes til sidst sammen med </table> ***
                 tiw.writeEndTable(ti, tabelMetadata.rowCount.ToString());//<rows>xxx</rows> og </table>
           


                 //************************************************** Kodelistetabeller ***************************************************************************
                 //*** Kodelisterne for den aktuelle tabel og tilføjes til tableIndex.xml som selvstændige tabeller ***
                 foreach (AV.Kodedef kodeDef in tabelMetadata.KodedefList)
                 {
                     //TODO: Skab1007tabelIndex, For hver kodeliste blev der skabt en tabel som tilføjes ny indgang i tableIndex.xml
                     //*** tableName, tableNumber, descriptions ***
                     string tableNameKodeliste = tabelMetadata.titel + "_" + kodeDef.kodetitel;//Her laves et sammensat navn
                     string tableNumberKodeliste = "table" + kodeDef.tabelnummmer1007.ToString();
                     string tableDescriptionKodeliste = "Opsalgstabel til tabel " + tabelMetadata.titel;

                     tiw.writeTableIndex(ti, tableNameKodeliste, tableNumberKodeliste, tableDescriptionKodeliste);//starter en ny tabel

                     //TODO: Skab1007tabelIndex, ... datatype _342_til_1007_Vis_og_test typeOrininal kan findes ved at lave opslag i tabellens liste over felter

                     string columnNameKode = kodeDef.kodetitel;//TODO: Skab1007tabelIndex, columnName skal vel være samme navn som feltet i tabellen (altså kodeDef.kodetitel)
                     string columnIDKode = "c1";//altid c1 eftersom der kun er 2 kolonner i en opslagstabel
                     string typeKode = "";  //bestemmes senere i switch
                     string typeOriginalKode = kodeDef.datatype;//Fritekst ... ingen udfalsrum
                     string descriptionKode = "Kode";//TODO: Skab1007tabelIndex, er navngivning "Kode" af C1 OK ?
                     string nullableKode = "false";//skal være lowercase

                     switch (kodeDef.datatype)
                            {
                                case "num":
                                    typeKode = "INTEGER";
                                    break;
                                case "real":
                                    typeKode = "REAL";
                                    break;
                                case "exp":
                                    typeKode = "REAL";//TODO: Skab1007tabelIndex, Er dette OK eller skal vi gøre noget andet? Findes der overhoved nogle exp i vores 342
                                    break;
                                case "string":
                                    typeKode = "VARCHAR" + "(" + kodeDef.koder.Count.ToString() + ")";
                                    break;
                                case "date":
                                    typeKode = "DATE";
                                    break;
                                case "time":
                                    typeKode = "TIME";
                                    break;
                                case "timestamp":
                                    typeKode = "TIMESTAMP";
                                    break;
                            }


                     tiw.writeColumn(ti, columnNameKode, columnIDKode, typeKode, typeOriginalKode, nullableKode, descriptionKode);

                     //Bredde på value kolonne kenders ikke, så den skal beregnes ...
                     int bredde = 0;
                     foreach (KeyValuePair<string, string> pair in kodeDef.koder)
                     {
                         if (pair.Value.Length > bredde) bredde = pair.Value.Length;
                     }
                     columnNameKode = "Value";//TODO: Skab1007tabelIndex, afspejler columnName "Value" i c2 kolonnens indhold OK ?
                     columnIDKode = "c2";//altid c2 eftersom der kun er 2 kolonner i en opslagstabel
                     typeKode = "VARCHAR("+bredde.ToString()+")";
                     typeOriginalKode = "string";//TODO: Skab1007tabelIndex, kodevalue altid string eller skal type bestemmes ???
                     descriptionKode = "Kodeværdi";//TODO: Skab1007tabelIndex, kodevalue beskrivelse af kolonne OK ?
                     nullableKode = kodeDef.nullable.ToString().ToLower();//skal være lowercase

                     tiw.writeColumn(ti, columnNameKode, columnIDKode, typeKode, typeOriginalKode, nullableKode, descriptionKode);

                     //TODO: Skab1007tabelIndex, kodevalue c1 angives som værende primærnøgle, er det OK?
                     //*** Tilføjer PN_nøgle for kodelisteTabel ***
                     string pkNameKode = "PK_" + kodeDef.kodetitel;//Primærnøglens navn er noget vi selv er nødt til at definerer (findes ikke i 342)
                     List<string> pnList = new List<string>();
                     pnList.Add(kodeDef.kodetitel);
                     tiw.writePrimaryKey(ti, pkNameKode, pnList);//Kræver en liste som parameter ... gasp
        


                     tiw.writeEndColumns(ti); //afslutter kolonner for den aktuelle tabel	
                     tiw.writeEndTable(ti, kodeDef.koder.Count.ToString());//<rows>xxx</rows> og </table>
                 }//foreach (AV_342_Info.Kodedef kodeDef in tabelMetadata.KodedefList)

             }//For hver tabel ...



             tiw.writeEndDocument(ti);//afslutning af tableIndex.xml
             return (true);//TODO: Skab1007tabelIndex, benytter ikke returkoden og procedure skal evt. ikke returnere noget ???
         }


        private int Skab1007tabel(List<string> tabelFragments, AV.Tabel tabelMetadata, string AV_1007_path)
        {
            //TODO: Skab1007tabel, procedure bør benytte en bools returværdi    
            
            //1007 tabelnummerering

            tabelnummmer1007Global++;//Første tabel skal have nummeret 1 og  tabelnummmer1007Global er initialiseret til 0
            //Opsamles senere til tableIndex.xml

                int rowcount = 0;

                string destFolderPath = Path.Combine(AV_1007_path,"Tables\\"+"table"+ tabelnummmer1007Global.ToString());
                Directory.CreateDirectory(destFolderPath);
            
                string destFilePath = Path.Combine(destFolderPath, "table" + tabelnummmer1007Global.ToString() + ".xml");

                using (StreamWriter streamwriter = new StreamWriter(destFilePath, true, Encoding.UTF8, 65536))
                {

                //Her skal xml header for tabel skabes
                string rows = ""; 
                streamwriter.Write("<table>" + "\r\n");

                foreach (string tabelFragment in tabelFragments)
                {
                    toolStripStatusLabel1.Text = "Udlæser tabel: " + tabelFragment;
                    Application.DoEvents();

                    //AV.AVInfo.LOG = AV.AVInfo.LOG + "Udlæser tabel: " + tabelFragment + "\r\n";

                    byte[] byteBuffer = new byte[1024];
                    //Int64 currentFilePos = 0;

                    if (File.Exists(tabelFragment) == false)
                    {
                        AV.LOG(AV.FejlTypeKonvertering.Tabel, 2, "Manglende tabelfil!", tabelFragment);
                        return(-1);//fejl
                    }

                    BinaryReader br = new BinaryReader(File.Open(tabelFragment, FileMode.Open,FileAccess.Read));
                    int pos = 0;
                    int length = (int)br.BaseStream.Length;

                    if (tabelMetadata.posttype.ToLower() == "fast")
                    {
                        int errorCounter = 0; 
                        while (pos < length)
                        {

                            List<MyRow> rowList = new List<MyRow>();

                            rows = rows + "<row>"+"\r\n";
                           
                            for (int i = 0; i < tabelMetadata.feltDefList.Count; i++)
                            {
                                int bredde = -1;
                                int.TryParse(tabelMetadata.feltDefList[i].bredde, out bredde);
                                //Selve læsningen ind i buffer
                                byteBuffer = br.ReadBytes(bredde);
                                pos = pos + bredde;

                                string dataString342 = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(byteBuffer, 0, bredde);

                                //*** Her foretages test og konvertering af data ***
                                DataTypeKonverteringsResultat result = new DataTypeKonverteringsResultat();
                                result = DataTypeKonverter(dataString342.Trim(), tabelMetadata.feltDefList[i].datatype.Trim());

                                //Skal kolonne kunne være tom (opsamling til senere brug) ???
                                if (result.data1007 == "") tabelMetadata.feltDefList[i].nullable = true;

                                if ((result.fejlmeddelelse != "") && (errorCounter < 100))
                                {
                                    //AV.AVInfo.LOG = AV.AVInfo.LOG + result.fejlmeddelelse + " Tabel: " + tabelMetadata.titel +  " Felt: " + tabelMetadata.feltDefList[i].titel + "Byte pos: " + pos.ToString() + "\r\n";
                                    //TODO: Skab1007tabel, AV.AVInfo.LOG opbygges samtidigt med struktureret log (hvad med tid, antal mv. ?????)

                                    if (errorCounter < 50)//Ophør med at logge efter bestemt antal fejl 
                                    {
                                        AV.LOG(AV.FejlTypeKonvertering.Datatype, 2, result.fejlmeddelelse + " Tabel: " + tabelMetadata.titel + " Felt: " + tabelMetadata.feltDefList[i].titel, "Byte pos: " + pos.ToString());
                                    }
                                    errorCounter++;
                                    
                                    if (errorCounter == 50)//Trigges ved bestemt antal fejl
                                    {
                                        //TODO: Skab1007tabel, skal der tilføjes en maxErrorCount i AV_342_Info. ??? 
                                        AV.LOG(AV.FejlTypeKonvertering.Datatype, 1, "!!! Test af datatyper afsluttet pga antallet af fejl !!!", "");
                                     }
                                }

                                rows = rows + "<c" + (i + 1).ToString() + ">" + result.data1007 + @"</c" + (i + 1).ToString() + ">" + "\r\n"; 
                            }//for (int i = 0; i < tabelMetadata.feltDefList.Count; i++)

                            rows = rows + @"</row>" + "\r\n"; //row afsluttes
                            streamwriter.Write(rows); //row skrives til stream
                        
                            rows = "";//Row er skrevet til fil og nulstilles
                            rowcount++;

                            
                       }//while (pos < length)
                    }
                 else
                    {
                        //Her angiver bredde hvor mange bytes som skal læses fra stream for at vide hvor mange bytes det pågældende feltindhold er  
                        MessageBox.Show("Håndtering af variabel feltbredde ikke implementeret endnu !!!");
                        //TODO: Skab1007tabel, Håndtering af variabel feltbredde ikke implementeret endnu
                    }

                    br.Close();//inputfil lukkes
                }//foreach (string tabelFragment in tabelFragments)
                
                //Her skal xml afsluttes
                streamwriter.Write("</table>");
                }//Using streamwriter ...

                return(rowcount);
        }


        /// <summary>
        /// Udlæsning af kodelister til opslagstabeller
        /// </summary>
        /// <param name="KodedefList"></param>
        /// <param name="parentTabelShortname"></param>
        private void Skab1007KodelisteTabel(AV.Kodedef kodeDef, string parentTabelShortname)
        {
            //int rowcount = 0;
            //1007 tabelnummerering
            tabelnummmer1007Global++;  

            string driveLetter = comboBoxDest.SelectedItem.ToString();
            string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
            //Unikt "oprindeligt navn"
            string originalTablename = parentTabelShortname + "_" + kodeDef.kodetitel;//for kodelister gælder at vi opfinder et oprindeligt tabelnavn
            string destFolderPath = Path.Combine(AV_1007_path, "Tables\\" + "table" + tabelnummmer1007Global.ToString());
            Directory.CreateDirectory(destFolderPath);

            string destFilePath = Path.Combine(destFolderPath, originalTablename + ".xml");

            destFilePath = Path.Combine(destFolderPath, "table" + tabelnummmer1007Global.ToString() + ".xml");

            using (StreamWriter streamwriter = new StreamWriter(destFilePath, true, Encoding.UTF8, 65536))
            {
                //Her skal xml header for tabel skabes
                string rows ="<table>" + "\r\n";

                for (int i = 0; i < kodeDef.koder.Count;i++ )
                {
                        
                    rows = rows + "<row>" + "\r\n";
                    rows = rows + "<c1>" + kodeDef.koder[i].Key.ToString() + "</c1>" + "\r\n"; 
                    rows = rows + "<c2>" + kodeDef.koder[i].Value.ToString() + "</c2>" + "\r\n"; 
                    rows = rows + "</row>" + "\r\n";
                }

                rows = rows + "</table>" + "\r\n"; 
                streamwriter.Write(rows);
            }
    
        }



        /// <summary>
        /// Skaber hovedtabeller og udlæser kodelister som opslagstabeller
        /// </summary>
        private void Skab1007tabeller()
        {
            if (AV.AV_Info.AVID == "") return;
           
            string AV342_path = AV.AV_Info.AV342folderPath;//shellTreeView1.SelectedPath;
            if (Directory.Exists(AV342_path) == true)
            {
                tabelnummmer1007Global = 0;
                //For hver tabel angivet i metadata
                string driveLetter = comboBoxDest.SelectedItem.ToString();
                string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
                Directory.CreateDirectory(AV_1007_path);
                
                //For hver tabel
                for (int i = 0; i < AV.AV_Info.metadata.tabelList.Count;i++ )
                {
                    //Lav liste over fragmenter som skal samles
                    string tabelType = AV.AV_Info.metadata.tabelList[i].posttype;
                    string tabelShortname = AV.AV_Info.metadata.tabelList[i].titel;
                    List<string> tabelFragments = new List<string>();
                    foreach (string tabelFile in AV.AV_Info.dataFiler.tabelFilListe)
                    {
                        if (Path.GetFileNameWithoutExtension(tabelFile).ToLower() == tabelShortname.ToLower())
                        {
                            tabelFragments.Add(tabelFile.ToLower());
                            // MessageBox.Show(tabelFile);
                        }
                        tabelFragments.Sort();
                    }

                    //*** skab 1007 xml tabel udfra sorteret liste over tabellens fragmenter ***
                    int rowCount = Skab1007tabel(tabelFragments, AV.AV_Info.metadata.tabelList[i], AV_1007_path);
                    
                    if (rowCount == -1)
                    {
                        //Fejl
                    }

                    AV.AV_Info.metadata.tabelList[i].rowCount = rowCount;//opsamles til brug i tableIndex.xml
                    AV.AV_Info.metadata.tabelList[i].tabelnummmer1007 = tabelnummmer1007Global;//opsamles til brug i tableIndex.xml

                    //*** skab topslagstabeller udfra kodelister tilhørende den aktuelle tabel ***
                    for (int ii = 0; ii < AV.AV_Info.metadata.tabelList[i].KodedefList.Count;ii++ )
                    {
                        Skab1007KodelisteTabel(AV.AV_Info.metadata.tabelList[i].KodedefList[ii], tabelShortname);
                        AV.AV_Info.metadata.tabelList[i].KodedefList[ii].tabelnummmer1007 = tabelnummmer1007Global;//opsamles til brug i tableIndex.xml
                    }
                }

            }
        }


        private void buttonTabeller_Click(object sender, EventArgs e)
        {
            Skab1007tabeller();
        }


        private void shellTreeView1_MouseClick(object sender, MouseEventArgs e)
        {
           // richTextBoxLOG.Clear();
           // Indlæs_342(shellTreeView1.SelectedPath);
        }

        private bool Skab1007fileIndex()
        {
            if (AV.AV_Info.AVID == "") return(false);

            toolStripStatusLabel1.Text = "Skaber fileIndex.xml";
            Application.DoEvents();
            string driveLetter = comboBoxDest.SelectedItem.ToString();
            string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
            string fileIndexOut = Path.Combine(AV_1007_path, "Indices\\fileIndex.xml");
            FileIndexWriter fiw = new FileIndexWriter();

            if (fiw.FileIndexCreate(AV_1007_path, fileIndexOut) == true)
            {
                return(true);
            }
            else
            {
                MessageBox.Show("fileIndex.xml fejlede!!!");
                return (false);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Skab1007fileIndex();
        }

        private void buttonFolders_Click(object sender, EventArgs e)
        {
            Skab1007mappeStruktur();
        }


        private void buttonContext_Click(object sender, EventArgs e)
        {
            Skab1007contextDocumentation();
        }


        private void Skab1007dokumenter()
        {
            if (AV.AV_Info.AVID == "") return;
            if (AV.AV_Info.metadata.dokumentList.Count == 0)
            {
                AV.LOG(AV.FejlTypeKonvertering.Documents, 0, "Ingen dokumenter", "");
                return;
            }

            toolStripStatusLabel1.Text = "Udlæser dokumenter";
            Application.DoEvents();

            string driveLetter = comboBoxDest.SelectedItem.ToString();
            string AV_1007_path = Path.Combine(driveLetter, "AVID.SA." + AV.AV_Info.AV_NR + ".1");
            Directory.CreateDirectory(Path.Combine(AV_1007_path, "Indices"));
            string docIndex1007FilePath = Path.Combine(AV_1007_path, "Indices\\docIndex.xml");

            DocIndexWriter diw = new DocIndexWriter();
            bool result = diw.Dokument_SkabIndex_og_KopierFiler(ref AV, docIndex1007FilePath, AV_1007_path);
            if (result == false)
            {
                AV.LOG(AV.FejlTypeKonvertering.Documents, 2, "Fejl i forbindelse med kopiering af dokument 342 --> 1007 mappe: ", docIndex1007FilePath);
            }
            //TODO: Skab1007dokumenter, returværdi benyttes ikke endnu
        }


        private void button4_Click(object sender, EventArgs e)
        {
            Skab1007dokumenter();
        }

        private void buttonSkab1007_Click(object sender, EventArgs e)
        {
            richTextBoxLOG.Clear();

            if (AV == null) return;
            if (AV.AV_Info.AVID == "") return;
            if (Directory.Exists(AV.AV_Info.AV342folderPath) == false) return;

            //AV.AVInfo.LOG indeholder allerede logning af parsning
            //TODO: Skab1007 i batchmode eller igangsat vha. programparameter ... kræver forudgående parsning som (når vi er i GUI) sker ved valg af 342mappe
            richTextBoxLOG.Text = richTextBoxLOG.Text + "***********************************************\r\n";
            richTextBoxLOG.Text = richTextBoxLOG.Text + "Konvertering påbegyndt AV " + AV.AV_Info.AVID + "\r\n";
            richTextBoxLOG.Text = richTextBoxLOG.Text + "***********************************************\r\n";
            Application.DoEvents();

            //TODO: alle kald af Skabxxxx skal returnere boolsk værdi hvis fatal fejl
            Skab1007mappeStruktur();
            Skab1007archiveIndex();
            Skab1007contextDocumentation();
            Skab1007tabeller();
            Skab1007tabelIndex();
            Skab1007dokumenter();
            //TODO: Skab1007SAQ() procedure mangler helt !!!
            Skab1007fileIndex();

            richTextBoxLOG.Text = richTextBoxLOG.Text + "***********************************************\r\n";
            richTextBoxLOG.Text = richTextBoxLOG.Text + "Konvertering afsluttet" + "\r\n";
            richTextBoxLOG.Text = richTextBoxLOG.Text + "***********************************************\r\n";

            LOG_ShowAll();//Udlæsning af fejl sorteret på type

            MessageBox.Show(AV.AV_Info.AVID + " skabt");
        }

       

        private void listBoxTable_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBoxTable.SelectedIndex > -1)
            {
                string tabelFileName = listBoxTable.Items[listBoxTable.SelectedIndex].ToString();
                string tabelShortFileName = Path.GetFileNameWithoutExtension(listBoxTable.Items[listBoxTable.SelectedIndex].ToString()).ToLower();
                //MessageBox.Show(tabelShortFileName);
                for (int i = 0; i < AV.AV_Info.metadata.tabelList.Count; i++)
                {
                    if (tabelShortFileName == AV.AV_Info.metadata.tabelList[i].titel.ToLower())
                    {
                        List<MyRow> rowList = new List<MyRow>();
                        foreach (AV.FeltDef feltDef in AV.AV_Info.metadata.tabelList[i].feltDefList)
                        {
                            MyRow row = new MyRow();
                            row.feltnavn = feltDef.titel;
                            row.feltBeskrivelse = feltDef.feltinfo;
                            row.datatype = feltDef.datatype;
                            row.bredde = feltDef.bredde;
                            rowList.Add(row);
                        }

                        VisDataForm vd = new VisDataForm(tabelFileName, rowList, AV.AV_Info.metadata.tabelList[i].posttype);
                        vd.ShowDialog();
                        return;
                    }
                }
             
            } //if (listBoxTable.SelectedIndex > -1)
        }



        private void button3_Click(object sender, EventArgs e)
        {
            richTextBoxLOG.Clear();
            LOG_ShowAll();
        }


        private void LOG_SaveFile(string AV342, bool haveErrors)
        {
            string fullFilePath = "";
            Directory.CreateDirectory(@"H:\Logfiler");

            //TODO: LOG_SaveFile, lige nu hvor der testes gemmes kun log for fejlede AV,er
            //TODO: LOG_SaveFile, hardcoded sti til logfiler !!!
            if (haveErrors == true)
            {
                fullFilePath = @"H:\Logfiler\" + AV342 + "_fejl_" + DateTime.Today.Year + "_" + DateTime.Today.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + ".doc";
                try
                {
                    richTextBoxLOG.SaveFile(fullFilePath, RichTextBoxStreamType.RichText);
                }
                    catch
                {
                        //Intet ... fil er sansynligvis bare åben i Word
                }
            }
            else
            {
               //TODO: lige nu er logfiler uden fejl slået fra, men skal aktiveres når vi for alvor går i gang med at konvertere 
               // fullFilePath = @"H:\Logfiler\" + AV342 + "_OK_" + DateTime.Today.Year + "_" + DateTime.Today.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + ".doc";
               // richTextBoxLOG.SaveFile(fullFilePath, RichTextBoxStreamType.RichText);
            }
           

        }


         private void LOG_Overskrift(string tekst, Color color)
         {

             string textToFormat = string.Empty;
             textToFormat = textToFormat + "\r\n";
             textToFormat = textToFormat + tekst + "\r\n";
             //textToFormat = textToFormat + "OK" + "\r\n";
             richTextBoxLOG.SelectionStart = this.richTextBoxLOG.Text.Length;
             richTextBoxLOG.SelectionFont = new Font(this.richTextBoxLOG.Font, FontStyle.Bold);
             richTextBoxLOG.SelectionColor = color;

             richTextBoxLOG.AppendText(textToFormat);
         }

 
        private string LOG_ShowAll()
        {
            if (AV.AV_Info == null) return ("");
            
            //Klargøring af linie opsamling til Excel
            string excelLogLinie =  "AV_"+AV.AV_Info.AV_NR.PadLeft(8,'0') + ";";

            //Overordnet angivelse af om der er fundet fejl (skal der gemmes en linie til excel)
            bool haveError = false;

            //*** Visning af metadata fejl sorteret efter fejltype ***
            foreach (string fejlType in Enum.GetNames(typeof(AV.FejlTypeMetadata)))
            {
                int errorCount = 0;//Optæller antallet af fejl for hver enkelt fejltype
                foreach (AV.FejlMetadata fejl in AV.AV_Info.metadata.fejlListe)
                {
                    if ((fejl.fejlType.ToString() == fejlType) && (fejl.Severity > 0))
                    {
                        errorCount++;
                        haveError = true;//nu ved vi at linie til Excel skal gemmes senere
                    }
                }


                //*** Overskrift på fejltype ***
                if (errorCount == 0)
                {
                    //Opsamling til CSV --> excel
                    excelLogLinie = excelLogLinie + ";";//Tom celle
                    LOG_Overskrift(fejlType, Color.Green);
                }
             else
                {
                    //Opsamling til CSV --> excel
                    excelLogLinie = excelLogLinie + errorCount + ";";//Antallet af fejl for bestemt fejltype
                    LOG_Overskrift(fejlType, Color.Red);
                }


                //Selve meddelelserne vises på GUI
                foreach (AV.FejlMetadata fejl in AV.AV_Info.metadata.fejlListe)
                {
                    if (fejl.fejlType.ToString() == fejlType)
                    {
                        //Selve fejlene
                        richTextBoxLOG.SelectionStart = this.richTextBoxLOG.Text.Length;

                        if (fejl.Severity == 0)
                        {
                            richTextBoxLOG.SelectionColor = Color.Black;
                        }
                    else
                        if (fejl.Severity == 1)
                        {
                            richTextBoxLOG.SelectionColor = Color.Goldenrod;
                        }
                    else
                        if (fejl.Severity == 2)
                        {
                            richTextBoxLOG.SelectionColor = Color.Red;
                        }
                     
                        richTextBoxLOG.AppendText(fejl.fejlMelding + " " + fejl.location + "\r\n");
                    }
                }

            }
         
            //Hvis der er fundet fejl, tilføjer resultat til CSV fil --> Excel 
            if (haveError == true)
            {
                //TODO: LOG_ShowAll, Lige for nu, så gemmer vi kun fejl fra selve Parse og ikke test af tabeller og dokumenter ... dette bør ændres når vi begynder at konvertere
                //TODO: LOG_ShowAll, hardcoded sti til logfiler !!!
                string AV342 = AV.AV_Info.AV_NR.PadLeft(8, '0');
                string fullFilePath = @"H:\Logfiler\" + AV342 + "_fejl_" + DateTime.Today.Year + "_" + DateTime.Today.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Today.Day.ToString().PadLeft(2, '0') + ".doc";
                excelLogLinie = excelLogLinie + "=HYPERLINK(\"" + fullFilePath +"\");";  //Excel link til logfil
            }


            //*** Datafilerne ***
            foreach (string fejlType in Enum.GetNames(typeof(AV.FejlTypeDatafiler)))
            {
                int errorCount = 0;
                foreach (AV.FejlDatafiler fejl in AV.AV_Info.dataFiler.fejlListe)
                {
                    if ((fejl.fejlType.ToString() == fejlType) && (fejl.Severity > 0)) errorCount++;
                }

                //*** Overskrift på fejltype ***
                if (errorCount == 0)
                {
                    LOG_Overskrift(fejlType, Color.Green);
                }
                else
                {
                    LOG_Overskrift(fejlType, Color.Red);
                }

                foreach (AV.FejlDatafiler fejl in AV.AV_Info.dataFiler.fejlListe)
                {
                    if (fejl.fejlType.ToString() == fejlType)
                    {
                        //Selve fejlene
                        richTextBoxLOG.SelectionStart = this.richTextBoxLOG.Text.Length;

                        if (fejl.Severity == 0)
                        {
                            richTextBoxLOG.SelectionColor = Color.Black;
                        }
                        else
                            if (fejl.Severity == 1)
                            {
                                richTextBoxLOG.SelectionColor = Color.Goldenrod;
                            }
                            else
                                if (fejl.Severity == 2)
                                {
                                    richTextBoxLOG.SelectionColor = Color.Red;
                                }

                        richTextBoxLOG.AppendText(fejl.fejlMelding + " " + fejl.location + "\r\n");
                    }
                }
            }
           

            //Reset
            richTextBoxLOG.SelectionStart = this.richTextBoxLOG.Text.Length;
            richTextBoxLOG.SelectionColor = Color.Black;
            richTextBoxLOG.SelectionFont = new Font(this.richTextBoxLOG.Font, FontStyle.Regular);

            Application.DoEvents();


            LOG_SaveFile(AV.AV_Info.AV_NR.PadLeft(8, '0'), haveError);

            if (haveError == true)
            {
                return (excelLogLinie);
            }
            else
            {
                return ("");
            }
        }




        private void comboBoxDest_SelectedIndexChanged(object sender, EventArgs e)
        {
            string iniFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "app.ini");
            IniFile myIni = new IniFile(iniFilePath);
            string latestDestDriveLetter = comboBoxDest.Items[comboBoxDest.SelectedIndex].ToString();
            myIni.IniWriteValue("stier", "latestDestDriveLetter", latestDestDriveLetter);
        }

        private void buttonMetadataTekstSøg_Click(object sender, EventArgs e)
        {
            foreach (string linie in richTextBoxMetadata.Lines)
            {
                if (linie.ToLower().Contains(textBoxMetadataSøg.Text.ToLower()) == true)
                {
                    richTextBoxMetadata.SelectionStart =  richTextBoxMetadata.Text.ToLower().IndexOf(textBoxMetadataSøg.Text.ToLower());
                    richTextBoxMetadata.Focus();
                }
            }
        }


    }


    public class MyRow
    {
        public string feltnavn;
        public string feltBeskrivelse;
        public string datatype;
        public string bredde;
        public string data342;
        public string data1007;
    }

    /// <summary>
    /// Placeholder for test og konvertering af 342 datatype til 1007 datatype
    /// </summary>
    public class DataTypeKonverteringsResultat
    {
        public string data1007 = "";
        public string fejlmeddelelse = "";
    }

}
