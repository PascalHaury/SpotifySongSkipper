using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.IO.Compression;

using System.Xml;
using System.Xml.Linq;

namespace SpotSkipUpdate
{
    class Program
    {
        private static string SettingsFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\Settings.xml";

        static void Main(string[] args)
        {
            string UpdateFolderPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "") + "\\Update";
            string OldVersionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "") + "\\Old";
            string AppPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
            string SpotSkipPath = AppPath.Replace("\\SpotSkipUpdater", "");
            string UpdateZipFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", "");
            //AppPath = AppPath.Replace(AppPath.Split('\\').Last(), "");

            string OnlineVersion = string.Empty;
            string InstalledVersion = string.Empty;
            string Size = string.Empty;
            string DownloadURL = string.Empty;
            string UpdateFileName = string.Empty;

            //Console.WriteLine("DBG: AppPath: " + AppPath);
            //Console.ReadLine();

            Console.WriteLine("Checking for Updates:");
            InstalledVersion = GetVersionInstalled();

            foreach (string line in HttpGet("https://api.github.com/repos/theHaury/SpotifySongSkipper/releases/latest").Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("\"tag_name\""))
                {
                    OnlineVersion = line.Split(':').Last().Replace("\"", "").Replace(",", "");

                    Console.WriteLine("Online version: " + OnlineVersion);
                    Console.WriteLine("Installed version: " + InstalledVersion);
                }
                else if (line.StartsWith("\"size\""))
                {
                    Size = line.Split(':').Last().Replace("\"", "").Replace(",", "");
                    Console.WriteLine("Update size: " + CalculateFileSize(int.Parse(Size)));
                }
                else if (line.StartsWith("\"browser_download_url\""))
                {
                    DownloadURL = "https:" + line.Split(':').Last().Replace("\"", "");
                    DownloadURL = DownloadURL.Remove(DownloadURL.Count() - 3, 3);
                    //Console.WriteLine("URL: " + DownloadURL);
                }
            }

            if ((double.Parse(InstalledVersion) >= double.Parse(OnlineVersion)) && InstalledVersion != "0.0")
            {
                Console.WriteLine("No update available...\r\nPress any key to exit...");
                Console.ReadKey();
            }
            else
            {
                UpdateFileName = DownloadURL.Split('/').Last();
                UpdateZipFile = AppPath + "\\" + DownloadURL.Split('/').Last();

                Console.WriteLine("\r\nDownloading new Version...");
                using (var client = new WebClient())
                {
                    client.DownloadFile(DownloadURL, UpdateFileName);
                }

                if (Directory.Exists(OldVersionPath))
                {
                    Directory.Delete(OldVersionPath, true);
                }
                Directory.CreateDirectory(UpdateFolderPath);
                Directory.CreateDirectory(OldVersionPath);

                foreach (string file in Directory.GetFiles(SpotSkipPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (!file.Contains("SpotSkipUpdate.exe"))
                    {
                        if (!file.Contains(UpdateFileName))
                        {
                            if (file.EndsWith(".exe") || file.EndsWith(".dll"))
                            {
                                string FileName = Path.GetFileName(file);
                                File.Move(file, OldVersionPath + "\\" + FileName);
                            }
                        }
                    }

                }
                ZipFile.ExtractToDirectory(UpdateZipFile, UpdateFolderPath);
                foreach (string file in Directory.GetFiles(UpdateFolderPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    string FileName = Path.GetFileName(file);
                    File.Move(file, SpotSkipPath + "\\" + FileName);
                }
                File.Delete(UpdateZipFile);
                Directory.Delete(UpdateFolderPath, true);
                Console.WriteLine("Done!\r\nPress any key to exit...");
                Console.ReadKey();
                Console.WriteLine("Starting SpotSkip now...");
                System.Diagnostics.Process.Start(SpotSkipPath + "\\SpotSkip.exe");

            }
        }

        public static string HttpGet(string URI)
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead(URI);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            s = s.Replace(",", ",\r\n");
            data.Close();
            reader.Close();
            return s;
        }

        private static string CalculateFileSize(int inFile)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (inFile == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(inFile);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(inFile) * num).ToString() + suf[place];
        }

        private static string GetVersionInstalled()
        {
            try
            {
                XElement root = XElement.Parse(File.ReadAllText(SettingsFilePath));

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(SettingsFilePath);
                XmlNode Version = xmlDoc.SelectSingleNode("Data/Settings/Version");
                return Version.Attributes["ProgramVersion"].Value;
            }
            catch (Exception)
            {
                return "0.0";
            }
        }
    }
}
