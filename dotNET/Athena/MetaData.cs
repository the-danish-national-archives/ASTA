using Rigsarkiv.Athena.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// MetaData Converter
    /// </summary>
    public class MetaData : Converter
    {
        const string ResourcePrefix = "Rigsarkiv.Athena.Resources.{0}";
        const string ColumnNode = "<column><name>{0}</name><columnID>c{1}</columnID><type>{2}</type><typeOriginal>{3}</typeOriginal><nullable>false</nullable><description>{4}</description></column>";
        const string TableNode = "<table><name></name><folder></folder><description></description><columns></columns><primaryKey><name>PK_</name><column></column></primaryKey><foreignKeys></foreignKeys><rows></rows></table>";

        private dynamic _metadata = null;
        private XmlDocument _tableDocument = null;
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public MetaData(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            _logSection = "Metadata";
            _tableDocument = new XmlDocument();
            using (Stream stream = assembly.GetManifestResourceStream(string.Format(ResourcePrefix,"tableIndex.xml")))
            {
                _tableDocument.Load(stream);
            }
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
             _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Start Converting Metadata {0} -> {1}", _srcFolder, _destFolder) });
            if(LoadJson())
            {
                foreach(var table in (object[])_metadata)
                {
                    var tableInfo = ((Dictionary<string, object>)table);
                    var fragment = _tableDocument.CreateDocumentFragment();
                    fragment.InnerXml = TableNode;

                }

                result = true;
            }
            var message = result ? "End Converting Metadata" : "End Converting Metadata with errors";
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = message });
            return result;
        }

        private bool LoadJson()
        {
            var result = true;
            try
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Load metadata from: {0}", _srcPath) });
                var json = File.ReadAllText(_srcPath);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                _metadata = jss.Deserialize<object>(json);
            }
            catch (Exception ex)
            {
                result = false;
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("LoadJson Failed: {0}", ex.Message) });
            }
            return result;
        }
    }
}
