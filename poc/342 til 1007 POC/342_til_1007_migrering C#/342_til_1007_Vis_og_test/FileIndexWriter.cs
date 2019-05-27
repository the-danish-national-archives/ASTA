using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Diagnostics;


namespace _342_til_1007_vis_test_konverter
{
    class FileIndexWriter
    {

        /// <summary>
        /// Calculates the hash.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private string CalculateHash(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            MD5 md5 = MD5.Create();
            byte[] checsum = md5.ComputeHash(stream);
            string hash = BitConverter.ToString(checsum).Replace("-", string.Empty).ToUpper();
            stream.Close();
            return hash;
        }

        private FileStream CreateNewXmlFile(string fileIndexOut, out XmlTextWriter xw)
        {
            var xmlFileStream = new FileStream(fileIndexOut + ".tmp", FileMode.Create);
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
        /// <param name="fileIndexOut"></param>
        private void SaveNewFileIndex(string fileIndexOut)
        {
            try
            {
                if (File.Exists(fileIndexOut) == true)
                    File.Delete(fileIndexOut);

                File.Move(fileIndexOut + ".tmp", fileIndexOut);
            }
            catch
            {
                MessageBox.Show("Fejl i forbindelse med omdøbning af " + fileIndexOut + ".tmp til " + fileIndexOut + "!");
            }
        }

        /// <summary>
        ///  Writes the values to indexfile.
        /// </summary>
        /// <param name="xw">The indexfile as a stream</param>
        /// <param name="folder">The folder.</param>
        /// <param name="shortFilename">The short filename.</param>
        /// <param name="hash">The hash.</param>
        private void WriteValuesToIndexfile(XmlTextWriter xw, string folder, string shortFilename, string hash)
        {
            xw.WriteStartElement("f");
            xw.WriteElementString("foN", folder);
            xw.WriteElementString("fiN", shortFilename);
            xw.WriteElementString("md5", hash);
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
        /// Skab fileIndex.xml.
        /// Stien til den nye fileIndex.xml kan være en anden end stien til AV'ens eksisterende fileIndex.xml
        /// </summary>
        /// <param name="AV"></param>
        /// <param name="drives"></param>
        /// <param name="fileIndexOut"></param>
        /// <returns></returns>
        public Boolean FileIndexCreate(string sourceFolder, string fileIndexPath)
        {
            //For hver fil ...
            string[] fileList = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);

            //Skab ny fileIndex.xml.tmp efter at listen over filer er skabt
            XmlTextWriter xw;
            var xmlFileStream = CreateNewXmlFile(fileIndexPath, out xw);


            foreach (string filePath in fileList)
            {
                try
                {
                    //string currentFilePath = filePath;
                    string currentFileName = Path.GetFileName(filePath);

                    if (currentFileName.ToLower() != "fileindex.xml")
                    {
                        string MD5 = CalculateHash(filePath);
                        WriteValuesToIndexfile(xw, Path.GetDirectoryName(filePath).Substring(3), currentFileName, MD5);
                    }

                    xw.Flush();
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    xw.Close();
                    xmlFileStream.Close();
                    File.Delete(fileIndexPath);//slet ufuldstændig fileIndex.xml
                    MessageBox.Show(ex.Message);
                    return (false);
                }
            }

            xw.Flush();
            CloseXmlFileStream(xw, xmlFileStream);

            SaveNewFileIndex(fileIndexPath);//gem fileIndex.xml.tmp --> fileindex.xml

            return (true);
        }




    }
}
