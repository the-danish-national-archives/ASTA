using Rigsarkiv.Athena.Entities;
using Rigsarkiv.Athena.Logging;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Data converter
    /// https://lennilobel.wordpress.com/2009/09/02/streaming-into-linq-to-xml-using-c-custom-iterators-and-xmlreader/
    /// </summary>
    public class Data : Converter
    {
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Data(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            var path = string.Format(IndicesPath, _destFolderPath);
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            return true;
        }

        public Row GetRow(Table table,int index)
        {
            var result = new Row() { DestValues = new Dictionary<string, string>(), SrcValues = new Dictionary<string, string>(), ErrorsColumns = new List<string>() } ;
            var path = string.Format(TablePath, _destFolderPath, string.Format("{0}\\{0}.xml", table.Folder));
            if (File.Exists(path))
            {
                XNamespace tableNS = string.Format(TableXmlNs, table.Folder);
                XElement rowNode = StreamElement(path, index);
                table.Columns.ForEach(c =>
                {
                    var hasError = false;
                    var value = rowNode.Element(tableNS + c.Id).Value;
                    var newValue = GetConvertedValue(c.Type, value, out hasError);
                    result.SrcValues.Add(c.Id, value);
                    result.DestValues.Add(c.Id, newValue);
                    if (hasError)
                    {
                        result.ErrorsColumns.Add(c.Id);
                        table.Errors++;
                    }
                });
            }
            else
            {
                //CSV file
            }
            return result;
        }

        private static XElement StreamElement(string fileName, int index)
        {
            XElement result = null;
            int counter = 0;
            using (var rdr = XmlReader.Create(fileName))
            {
                rdr.MoveToContent();
                while (rdr.Read() && counter < index)
                {
                    if ((rdr.NodeType == XmlNodeType.Element) && (rdr.Name == "row"))
                    {
                        result = XNode.ReadFrom(rdr) as XElement;
                        counter++;
                    }
                }
                rdr.Close();
            }
            return result;
        }

        private string GetConvertedValue(string type,string value, out bool hasError)
        {
            switch (type)
            {
                case "INTEGER":
                    {
                        int result = -1;
                        hasError = int.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                case "DECIMAL":
                    {
                        float result = -1;
                        hasError = float.TryParse(value, out result);
                        return result.ToString();
                    }; break;
                /*case "DATE": result = "DATE"; break;
                case "TIME": result = "TIME"; break;
                case "TIMESTAMP": result = "TIMESTAMP"; break;*/
                default:
                    {
                        hasError = true;
                        return value;
                    }; break;
            }
        }
    }
}
