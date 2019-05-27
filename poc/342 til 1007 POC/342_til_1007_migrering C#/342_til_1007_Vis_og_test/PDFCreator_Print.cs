using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace PDFCreator_Print
{
    class PDFCreator_Print
    {
        //Sti til den process som udfører jobbet
        string PDFCreatorExePath = @"C:\Program Files\PDFCreator\Pdfcreator.exe";

        //Navn på den process som der skal ventes på mellem hvert job 
        string processName = "PDFCreator";

        //Liste over filtyper som skal behandles som textfiler
        List<string> extRenameToTXT = new List<string>() { ".XML", ".XSD" };    

        
        /// <summary>
        /// Procedure som venter på at process afsluttes
        /// </summary>
        /// <param name="PName"></param>
        /// <returns></returns>
        private Boolean isProcessRunning(string PName)
        {
            try
            {
                Process[] pname = Process.GetProcessesByName(PName);
                if (pname.Length == 0)
                {
                    return (false);
                }
                else
                {
                    return (true);
                }
            }
            catch
            {
                return (true);//Ikke rigtig mulig at finde på noget andet lige nu ....
            }
        }


        public string KonverterDokument(string file, string outputFilename)
        {
            if (file.Trim() == "") return ("");

            string targetPath = Path.GetDirectoryName(file);
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(file);//PDFCreator tilføjer selv tif
            //string outputFilename = Path.Combine(targetPath, filenameWithoutExt);
            string ext = Path.GetExtension(file).ToUpper();

            //Hvis originalfilen er en tif, så undlad at gøre noget
            if ((ext == ".TIF") || (ext == ".TIFF"))
            {
                return ("TIFF fil springes over: " + Path.GetFileName(file));
            }


            string sourceFileName = "";

            //Evt. nødt til at arbejde med kopi af fil som er omdøbt til TXT
            if (extRenameToTXT.Contains(ext) == true)
            {
                //Benytter stream copy fordi File.Copy() låser file handle !       
                using (var src = File.OpenRead(@file))
                {
                    using (var dest = File.OpenWrite(outputFilename + ".txt"))
                    {
                        src.CopyTo(dest); //blocks until finished
                    }
                }
                
                sourceFileName = outputFilename + ".txt";
            }
            else
            {
                sourceFileName = file;
            }

            //this.Text = "Påbegynder konvertering af: " + Path.GetFileName(file);
            //this.Update();

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = PDFCreatorExePath;
            info.Arguments = "/PrintFile=\"" + sourceFileName + "\" " + "/OutputFile=\"" + outputFilename + "\" " + "/Profile=\"<Standardprofil>\"";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;

            Process p = new Process();
            p.StartInfo = info;
            p.Start();
            p.WaitForExit();


            //TODO: KonverterDokument, Hvad med timeout her ... hvis et dokument er meget stort, kan det tage lang tid
            int timeoutCounter = 0;
            while (isProcessRunning(processName) == true)//"PDFCreator" eller "UltraISO" eller "ISOcmd"
            {
                timeoutCounter++;
                //this.Text = "Skaber: " + filenameWithoutExt + ".tif" + " (" + timeoutCounter.ToString() + ")";
                //this.Update();
                //Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
            }

            p.Dispose();

            //Oprydning af omdøbte filer ... 
            if (extRenameToTXT.Contains(ext) == true)
            {
                timeoutCounter = 0;
                while (File.Exists(sourceFileName))
                {
                    timeoutCounter++;
                    //this.Text = "Sletter tempfile tilhørende " + filenameWithoutExt + ".tif" + " (" + timeoutCounter.ToString() + ")";
                    //this.Update();
                    System.Threading.Thread.Sleep(1000);    //Løser problem med File.Copy() som låser file handle !              
                    try
                    {
                        File.Delete(sourceFileName);
                    }
                    catch
                    {
                        //Intet
                        if (timeoutCounter > 60 * 10) //Sat til 10 minutter, men meget store dokumenter kan evt. vare længere
                        {
                            //TODO: KonverterDokument, Mangler at håndtere timeout
                            return ("Kunne ikke slette midlertidig fil: " + sourceFileName + " skal efterfølgende slettes manuelt");

                        }
                    }
                }
            }

            return ("");
        }


    }
}
