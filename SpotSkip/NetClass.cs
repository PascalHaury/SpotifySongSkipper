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
using System.Windows;
using System.Windows.Input;

namespace SpotSkip
{
    class NetClass
    {
        public bool CheckForUpdates(out string InstalledVersion, out string OnlineVersion, out string Size)
        {
            OnlineVersion = string.Empty;
            InstalledVersion = new Settings().UpdateVersionNumber();
            Size = string.Empty;
            foreach (string line in HttpGet("https://api.github.com/repos/theHaury/SpotifySongSkipper/releases/latest").Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("\"tag_name\""))
                {
                    OnlineVersion = line.Split(':').Last().Replace("\"", "").Replace(",", "");

                }
                else if (line.StartsWith("\"size\""))
                {
                    Size = CalculateFileSize(int.Parse(line.Split(':').Last().Replace("\"", "").Replace(",", "")));

                }

            }
            if ((double.Parse(OnlineVersion) > double.Parse(InstalledVersion)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string HttpGet(string URI)
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

        private string CalculateFileSize(int inFile)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (inFile == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(inFile);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(inFile) * num).ToString() + suf[place];
        }
    }
}
