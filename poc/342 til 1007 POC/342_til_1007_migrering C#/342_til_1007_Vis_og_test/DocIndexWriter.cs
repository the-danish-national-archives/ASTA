using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace _342_til_1007_vis_test_konverter
{
    class DocIndexWriter
    {



        private FileStream CreateNewXmlFile(string docIndexOut, out XmlTextWriter xw)
        {
            var xmlFileStream = new FileStream(docIndexOut, FileMode.Create);
            //opretter ny fileIndex.xml fil med en xml deklaration             
            xw = new XmlTextWriter(xmlFileStream, new UTF8Encoding());
            xw.Formatting = Formatting.Indented;
            xw.WriteStartDocument();
            xw.WriteStartElement("fileIndex"); //root element
            xw.WriteAttributeString("xsi:schemaLocation",
                                    "http://www.sa.dk/xmlns/diark/1.0 ../Schemas/standard/fileIndex.xsd");
            xw.WriteAttributeString("xmlns", "http://www.sa.dk/xmlns/diark/1.0");
            xw.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            return xmlFileStream;
        }

        /// <summary>
        /// Gemmer xml filen
        /// </summary>
        /// <param name="docIndexOut"></param>
        private void SaveNewFileIndex(string docIndexOut)
        {
            try
            {
                if (File.Exists(docIndexOut) == true)
                    File.Delete(docIndexOut);

                File.Move(docIndexOut + ".tmp", docIndexOut);
            }
            catch
            {
                MessageBox.Show("Fejl i forbindelse med omdøbning af " + docIndexOut + ".tmp til " + docIndexOut + "!");
            }
        }

        /// <summary>
        ///  Writes the values to docIndexfile.
        /// </summary>
        /// <param name="xw">The indexfile as a stream</param>
        /// <param name="folder">The folder.</param>
        /// <param name="shortFilename">The short filename.</param>
        /// <param name="hash">The hash.</param>
        private void WriteValuesToIndexfile(XmlTextWriter xw, string ID, string medieID, string docCollection, string originalFilename, string fileType)
        {
           
        //<dID>1</dID>
        //<mID>1</mID>
        //<dCf>docCollection1</dCf>
        //<oFn>gpl-3.0.odt</oFn>
        //<aFt>tif</aFt>
            
            
            xw.WriteStartElement("doc");
            xw.WriteElementString("dID", ID);
            xw.WriteElementString("mID", medieID);
            xw.WriteElementString("dCf", docCollection);
            xw.WriteElementString("oFn", originalFilename);
            xw.WriteElementString("aFt", fileType);
            xw.WriteEndElement();
            // xw.Flush();
        }

        /// <summary>
        /// Closes the XML file stream.
        /// </summary>
        /// <param name="xw">The xw.</param>
        /// <param name="xmlFileStream">The XML file stream.</param>
        private void CloseXmlFileStream(XmlTextWriter xw, FileStream xmlFileStream)
        {
            //Slut element skrives
            xw.WriteEndElement();
            xw.WriteEndDocument();
            xw.Flush();
            xw.Close();
            xmlFileStream.Close();
        }

        /// <summary>
        /// Skaber docIndex.xml og kopierer/omdøber dokumenter fra 342 til 1007
        /// </summary>
        /// <param name="AV"></param>
        /// <param name="docIndex1007FilePath"></param>
        /// <param name="AV_1007_path"></param>
        /// <returns></returns>
        public Boolean Dokument_SkabIndex_og_KopierFiler(ref AV AV, string docIndex1007FilePath, string AV_1007_path)
        {
            int docCollectionNumber = 1;
            int docCounter = 0;

            //Skab ny fileIndex.xml.tmp efter at listen over filer er skabt
            XmlTextWriter xw;
            var xmlFileStream = CreateNewXmlFile(docIndex1007FilePath, out xw);

            //For hvert dokument 
            foreach (AV.DokumentTab d in AV.AV_Info.metadata.dokumentList)
            {
                //Her skal docCollection mappe skabes løbende (med counter som benyttes til navngivning af docCollection mapper)
                docCounter++;
                if (docCounter > 10000)
                {
                    docCollectionNumber++;
                    docCounter = 1;
                }
                string docCollectionName = "docCollection" + docCollectionNumber.ToString();
                string docFolderPath = Path.Combine(AV_1007_path+"\\Documents", docCollectionName + "\\" + d.dokID.ToString());
                Directory.CreateDirectory(docFolderPath);

                try
                {
                    //string ID, string medieID, string docCollection, string originalFilename, string fileType
                    //TODO: Dokument_SkabIndex_og_KopierFiler, Bemærk at pID (parent ID ikke medtages eftersom det altid er tomt når vi kommer fra 342) ... er det korrekt?
                   // Filtype udledes ved at finde første fil i mappen
                    string[] filer = Directory.GetFiles(d.docPath);
                    Array.Sort(filer);//Herefter kan filer kopieres med løbenummer
                    string ext =  Path.GetExtension(filer[0]).Replace(".", "");
                    string originalFileName = "00000001." + ext;
                    if (File.Exists(Path.Combine(d.docPath, originalFileName)) == false)
                    {
                       // MessageBox.Show("Fejl: " + d.docPath);
                        //TODO: Dokument_SkabIndex_og_KopierFiler, dette er håndteret i forbindelse med indlæsning af dokumenter, så vi burde ikke kunne komme hertil
                    }
                  
                    // MessageBox.Show(ext);
                    Array.Sort(filer);//Herefter kan filer kopieres med løbenummer
                    foreach (string file in filer)
                    {
                        int docNumber = -1;
                        int.TryParse(Path.GetFileNameWithoutExtension(file),out docNumber);
                        //Kopier til ny mappe 
                        string newFilename = docNumber.ToString() + Path.GetExtension(file); //Strip foranstillede 00000 og behold extension
                        File.Copy(file , Path.Combine(docFolderPath,newFilename),true);
                    }

                    WriteValuesToIndexfile(xw, d.dokID, "1", docCollectionName, originalFileName, ext);
                    xw.Flush();
                    Application.DoEvents();
                }
                catch (Exception)
                {
                    xw.Close();
                    xmlFileStream.Close();
                    MessageBox.Show("Fejl i forb. med kopiering af dokument: " + docFolderPath );
                    File.Delete(docIndex1007FilePath);//slet den oprindelige docIndex.xml

                    return (false);
                }
            }

            xw.Flush();
            CloseXmlFileStream(xw, xmlFileStream);

           // SaveNewFileIndex(docIndexPath);//gem fileIndex.xml.tmp --> fileindex.xml

            return (true);
        }
    
    }
}
