using Rigsarkiv.Athena.Logging;
using System.IO;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// 
    /// </summary>
    public class Structure : Converter
    {
        private string _destPath = null;
        private string _destFolder = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logManager"></param>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <param name="destFolder"></param>
        public Structure(LogManager logManager, string srcPath, string destPath, string destFolder) : base(logManager, srcPath)
        {
            _logSection = "Structure";
            _destPath = destPath;
            _destFolder = destFolder;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Run()
        {
            var folderName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
           folderName = folderName.Substring(0, folderName.LastIndexOf("."));
           _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = string.Format("Start Converting structure {0} -> {1}", folderName, _destFolder) });
            EnsureFolder();
           _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = "End Converting structure" });
        }

        private void EnsureFolder()
        {
            try
            {
                var path = string.Format("{0}\\{1}", _destPath, _destFolder);
                if (!Directory.Exists(path))
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Create path: {0}", path) });
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
                else
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Path exists: {0}", path) });
                }
            }
            catch (IOException ex)
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = ex.Message });
            }
        }
    }
}
