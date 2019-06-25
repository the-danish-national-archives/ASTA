using Rigsarkiv.Athena.Logging;
using System.IO;
using System.Xml.Linq;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// Index files
    /// </summary>
    public class Index : Converter
    {
        const string FileIndex = "fileIndex.xml";
        private XDocument _fileIndexXDocument = null;

        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Index(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath, destPath, destFolder)
        {
            _logSection = "Index";
            using (Stream stream = _assembly.GetManifestResourceStream(string.Format(ResourcePrefix, FileIndex)))
            {
                _tableIndexXDocument = XDocument.Load(stream);
            }
        }

        /// <summary>
        /// start converter
        /// </summary>
        /// <returns></returns>
        public override bool Run()
        {
            var result = false;
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Start Indexing files at {0}", _destFolderPath) });
            var files = Directory.GetFiles(_destFolderPath, "*.*", SearchOption.AllDirectories);

            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = (result ? "End Indexing files" : "End Indexing files with errors") });
            return result;
        }
    }
}
