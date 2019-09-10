using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Xml;

namespace Athena_II_REM
{
    class fileIndexCreator
    {
        private FileStream CreateNewXmlFile(string fileIndexOut, out XmlTextWriter xw)
        {
            var xmlFileStream = new FileStream(fileIndexOut, FileMode.Create);
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

        /// <summary>
        /// Skriv værdier til indexfilen
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
        /// Skab fileIndex.xml
        /// </summary>
        /// <param name="AV"></param>
        /// <param name="drives"></param>
        /// <param name="fileIndexOut"></param>
        /// <returns></returns>
        public Boolean FileIndexCreate(string AV_path)
        {
            ArrayList al = new ArrayList();

            string[] fileNames = Directory.GetFiles(AV_path, "*.*", SearchOption.AllDirectories);
            al.AddRange(fileNames);

            if (al.Count == 0) return (false);

            al.Sort();

            string fileIndexOut = Path.Combine(AV_path, "Indices\\fileIndex.xml");
            if (File.Exists(fileIndexOut) == true) File.Delete(fileIndexOut);

            XmlTextWriter xw;
            var xmlFileStream = CreateNewXmlFile(fileIndexOut, out xw);

            //For hver fil ...
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    string currentFilePath = al[i].ToString();
                    string currentFileName = Path.GetFileName(currentFilePath);

                    if (currentFileName.ToLower() != "fileindex.xml")
                    {
                        string MD5 = CalculateHash(currentFilePath);
                        //Der skal benyttes relativ sti ...
                        int fileNameOffset = currentFilePath.IndexOf(Path.GetFileName(AV_path));
                        WriteValuesToIndexfile(xw, Path.GetDirectoryName(currentFilePath).Substring(fileNameOffset), currentFileName, MD5);
                    }
                }
            }
            catch (Exception)
            {
                xw.Close();
                xmlFileStream.Close();
                return (false);
            }

            xw.Flush();
            CloseXmlFileStream(xw, xmlFileStream);

            return (true);
        }

    }
}
