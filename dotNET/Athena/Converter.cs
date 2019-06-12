using Rigsarkiv.Athena.Logging;

namespace Rigsarkiv.Athena
{
    public class Converter
    {
        protected LogManager _logManager = null;
        protected string _srcPath = null;
        protected string _destPath = null;
        protected string _destFolder = null;
        protected string _logSection = "";
        protected string _destFolderPath = null;
        protected string _srcFolder = null;

        public Converter(LogManager logManager,string srcPath, string destPath, string destFolder)
        {
            _logManager = logManager;
            _srcPath = srcPath;
            _destPath = destPath;
            _destFolder = destFolder;
            _destFolderPath = string.Format("{0}\\{1}", _destPath, _destFolder);
            var folderName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
            _srcFolder = folderName.Substring(0, folderName.LastIndexOf("."));
        }

        public virtual bool Run()
        {
            return true;
        }
    }
}
