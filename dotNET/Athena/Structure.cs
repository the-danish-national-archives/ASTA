using Rigsarkiv.Athena.Logging;
using System.IO;
using System.Reflection;

namespace Rigsarkiv.Athena
{
    /// <summary>
    /// 
    /// </summary>
    public class Structure : Converter
    {
        const string ContextDocumentationPath = "{0}\\ContextDocumentation";
        const string IndicesPath = "{0}\\Indices";
        const string SchemasPath = "{0}\\Schemas";
        const string SchemasLocalSharedPath = "{0}\\Schemas\\localShared";
        const string SchemasStandardPath = "{0}\\Schemas\\standard";
        private string _destPath = null;
        private string _destFolder = null;
        private string _destFolderPath = null;

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
            _destFolderPath = string.Format("{0}\\{1}", _destPath, _destFolder);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Run()
        {
            var folderName = _srcPath.Substring(_srcPath.LastIndexOf("\\") + 1);
            folderName = folderName.Substring(0, folderName.LastIndexOf("."));
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = string.Format("Start Converting structure {0} -> {1}", folderName, _destFolder) });
            if (EnsureRootFolder() && CopyFolders() && EnsureSchemas())
            {
                
            }
            _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = "", Message = "End Converting structure" });
        }

        private bool EnsureRootFolder()
        {
            var result = true;
            try
            {
                if (Directory.Exists(_destFolderPath))
                {
                    _logManager.Add(new LogEntity() { Level = LogLevel.Warning, Section = _logSection, Message = string.Format("Delete exists Path: {0}", _destFolderPath) });
                    Directory.Delete(_destFolderPath, true);
                }
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Create path: {0}", _destFolderPath) });
                DirectoryInfo di = Directory.CreateDirectory(_destFolderPath);
            }
            catch (IOException ex)
            {
                result = false;
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureRootFolder Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool EnsureSchemas()
        {
            var result = true;
            try
            {
                var path = string.Format(SchemasPath, _destFolderPath);
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure Schema : {0}", path) });
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(string.Format(SchemasLocalSharedPath, _destFolderPath));
                path = string.Format(SchemasStandardPath, _destFolderPath);
                Directory.CreateDirectory(path);
                var assembly = Assembly.GetExecutingAssembly();
                foreach(string name in assembly.GetManifestResourceNames())
                {
                    var names = name.Split('.');
                    var fileName = string.Format("{0}.{1}", names[names.Length - 2], names[names.Length - 1]);
                    _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("Ensure resource : {0}", fileName) });
                    using (Stream stream = assembly.GetManifestResourceStream(name))
                    {
                        using (var fileStream = new FileStream(string.Format("{0}\\{1}", path, fileName), FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                result = false;
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("EnsureSchemas Failed: {0}", ex.Message) });
            }
            return result;
        }

        private bool CopyFolders()
        {
            var result = true;
            var srcRootPath = _srcPath.Substring(0, _srcPath.LastIndexOf("."));

            var srcPath = string.Format(ContextDocumentationPath, srcRootPath);
            var destPath = string.Format(ContextDocumentationPath, _destFolderPath);            
            result = CopyFolder(srcPath, destPath);

            if(result)
            {
                srcPath = string.Format(IndicesPath, srcRootPath);
                destPath = string.Format(IndicesPath, _destFolderPath);
                result = CopyFolder(srcPath, destPath);
            }
            return result;
        }

        private bool CopyFolder(string srcPath,string destPath)
        {
            var result = true;
            try
            {
                _logManager.Add(new LogEntity() { Level = LogLevel.Info, Section = _logSection, Message = string.Format("copy {0} -> {1}", srcPath, destPath) });
                Directory.CreateDirectory(destPath);
                foreach (string dirPath in Directory.GetDirectories(srcPath, "*",SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(srcPath, destPath));
                }
                foreach (string newPath in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(srcPath, destPath), true);
                }
            }
            catch (IOException ex)
            {
                result = false;
                _logManager.Add(new LogEntity() { Level = LogLevel.Error, Section = _logSection, Message = string.Format("CopyFolder Failed: {0}", ex.Message) });
            }
            return result;
        }
    }
}
