using System;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;

namespace SpotSkip
{
    class FileIO_Write
    {
        private Variables globalVars = new Variables();
        public enum BlockType { SongBlock, ArtistBlock, ComboBlock, none };

        public bool createDefaultTable(string path_to_File)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path_to_File)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path_to_File));
            }
            if (!File.Exists(path_to_File))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    XmlNode docnode = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                    doc.AppendChild(docnode);

                    XmlNode BlockListNode = doc.CreateElement("BlockList");
                    XmlNode APIComboNode = doc.CreateElement("SpotifyAPIData");
                    XmlNode BlockSongNode = doc.CreateElement("Songs");
                    XmlNode BlockArtistNode = doc.CreateElement("Artists");
                    XmlNode BlockComboNode = doc.CreateElement("Combos");

                    BlockListNode.AppendChild(APIComboNode);
                    BlockListNode.AppendChild(BlockSongNode);
                    BlockListNode.AppendChild(BlockArtistNode);
                    BlockListNode.AppendChild(BlockComboNode);

                    doc.AppendChild(BlockListNode);
                    doc.Save(path_to_File);
                    return true;
                }
                catch (Exception ex)
                {
                    logError(ex);
                    return false;
                }
            }
            return false;
        }

        public bool addEntry(string Entry, BlockType BT)
        {
            try
            {
                string type = string.Empty;
                XmlDocument doc = new XmlDocument();
                XmlAttribute Date = doc.CreateAttribute("Date");
                XmlAttribute Song = doc.CreateAttribute("Song");
                XmlAttribute Artist = doc.CreateAttribute("Artist");

                switch (BT)
                {
                    case BlockType.ArtistBlock:
                        type = "Artist";
                        Date.Value = DateTime.Now.ToString();
                        Song.Value = " - ";
                        Artist.Value = Entry;
                        break;
                    case BlockType.ComboBlock:
                        type = "Combo";
                        Date.Value = DateTime.Now.ToString();
                        Song.Value = Entry.Split('-')[1].Remove(0, 1);
                        Artist.Value = Entry.Split('-')[0].Remove(Entry.Split('-')[0].Length - 1, 1);
                        break;
                    case BlockType.SongBlock:
                        type = "Song";
                        Date.Value = DateTime.Now.ToString();
                        Song.Value = Entry;
                        Artist.Value = " - ";
                        break;
                }
                doc.Load(globalVars.BlockListFilePath);

                XmlNode BlockNode = doc.SelectNodes("/BlockList/" + type + "s")[0];// .ChildNodes[1];
                XmlNode BlockEntry = doc.CreateElement(type);
                BlockEntry.Attributes.Append(Song);
                BlockEntry.Attributes.Append(Artist);
                BlockEntry.Attributes.Append(Date);
                BlockEntry.InnerText = Entry;
                BlockNode.AppendChild(BlockEntry);
                doc.Save(globalVars.BlockListFilePath);
                return true;
            }
            catch (Exception ex)
            {
                logError(ex);
                return false;
            }
        }

        public bool removeEntry(string Entry, BlockType BT)
        {
            try
            {
                string NodeSelect = string.Empty;
                switch (BT)
                {
                    case BlockType.ArtistBlock:
                        NodeSelect = "BlockList/Artists/Artist[text()='" + Entry + "']";
                        break;
                    case BlockType.ComboBlock:
                        NodeSelect = "BlockList/Combos/Combo[text()='" + Entry + "']";
                        break;
                    case BlockType.SongBlock:
                        NodeSelect = "BlockList/Songs/Song[text()='" + Entry + "']";
                        break;
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(globalVars.BlockListFilePath);
                XmlNodeList RemVar = doc.SelectNodes(NodeSelect);
                foreach (XmlNode node in RemVar)
                {
                    node.ParentNode.RemoveChild(node);
                }
                doc.Save(globalVars.BlockListFilePath);

                return true;
            }
            catch (Exception ex)
            {
                logError(ex);
                return false;
            }
        }

        public bool logError(Exception _ex)
        {
            try
            {
                File.AppendAllLines(globalVars.ErrorLogFilePath, new String[] { _ex.ToString() });
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(_ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool logError(string _ex)
        {
            try
            {
                File.AppendAllLines(globalVars.ErrorLogFilePath, new String[] { _ex.ToString() });
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(_ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

    class FileIO_Read
    {
        private Variables globalVars = new Variables();

        public bool searchForEntry(string CurrentlyPlaying)
        {
            try
            {
                XElement root = XElement.Parse(File.ReadAllText(globalVars.BlockListFilePath));
                string Song = CurrentlyPlaying.Split('-')[1];
                string Artist = CurrentlyPlaying.Split('-')[0];
                Song = Song.Remove(0, 1);
                Artist = Artist.Remove(Artist.Length - 1, 1);
                string Combo = Artist + " - " + Song;

                var blocksong = root.Descendants("Song");
                var blockArtist = root.Descendants("Artist");
                var blockCombo = root.Descendants("Combo");

                foreach (var _Combo in blockCombo)
                {
                    if (Combo == _Combo.Value)
                    {
                        return true;
                    }
                }

                foreach (var _Artist in blockArtist)
                {
                    if (Artist == _Artist.Value)
                    {
                        return true;
                    }
                }

                foreach (var _Song in blocksong)
                {
                    if (_Song.Value.ToLower().Contains(Song.ToLower()))
                    {
                        return true;
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }
    }

    class Settings
    {
        private string ApplicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        private string SettingsFilePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Settings.xml";

        public bool createDefaultTable()
        {
            try
            {
                Variables defaultVars = new Variables();

                if (File.Exists(SettingsFilePath))
                {
                    File.Move(SettingsFilePath, defaultVars.SettingsFilePath);
                    SettingsFilePath = defaultVars.SettingsFilePath;
                }
                else
                {
                    SettingsFilePath = defaultVars.SettingsFilePath;
                }


                if (!File.Exists(SettingsFilePath))
                {
                    XmlDocument doc = new XmlDocument();
                    //Attributes
                    XmlAttribute BlockListFile = doc.CreateAttribute("Path");
                    XmlAttribute ErrorLogFile = doc.CreateAttribute("Path");
                    XmlAttribute StartSpotify = doc.CreateAttribute("StartSpotify");
                    XmlAttribute TimerInterval = doc.CreateAttribute("Interval");
                    XmlAttribute PlaySong = doc.CreateAttribute("PlaySong");
                    XmlAttribute ProgramVersion = doc.CreateAttribute("ProgramVersion");

                    BlockListFile.Value = defaultVars.BlockListFilePath;
                    ErrorLogFile.Value = defaultVars.ErrorLogFilePath;
                    StartSpotify.Value = defaultVars.StartSpotify.ToString();
                    PlaySong.Value = defaultVars.PlaySong.ToString();
                    TimerInterval.Value = defaultVars.TimerInterval.ToString();
                    ProgramVersion.Value = defaultVars.VersionNumber;

                    XmlNode docnode = doc.CreateXmlDeclaration("1.0", "utf-8", null);
                    doc.AppendChild(docnode);

                    XmlNode entries = doc.CreateElement("Data");

                    XmlNode SettingsNode = doc.CreateElement("Settings");
                    XmlNode Setting = doc.CreateElement("BlockListFile");
                    Setting.Attributes.Append(BlockListFile);
                    SettingsNode.AppendChild(Setting);
                    Setting = doc.CreateElement("ErrorLogFile");
                    Setting.Attributes.Append(ErrorLogFile);
                    SettingsNode.AppendChild(Setting);

                    Setting = doc.CreateElement("Behaviour");
                    Setting.Attributes.Append(StartSpotify);
                    Setting.Attributes.Append(PlaySong);
                    SettingsNode.AppendChild(Setting);

                    Setting = doc.CreateElement("Timer");
                    Setting.Attributes.Append(TimerInterval);
                    SettingsNode.AppendChild(Setting);

                    Setting = doc.CreateElement("Version");
                    Setting.Attributes.Append(ProgramVersion);
                    SettingsNode.AppendChild(Setting);
                    entries.AppendChild(SettingsNode);

                    XmlAttribute SongsPlayed = doc.CreateAttribute("SongsPlayed");
                    XmlAttribute SongsSkipped = doc.CreateAttribute("SongsSkipped");
                    SongsPlayed.Value = "0";
                    SongsSkipped.Value = "0";

                    XmlNode logNode = doc.CreateElement("Logging");
                    XmlNode Entry = doc.CreateElement("SongsPlayed");
                    Entry.Attributes.Append(SongsPlayed);
                    logNode.AppendChild(Entry);
                    Entry = doc.CreateElement("SongsSkipped");
                    Entry.Attributes.Append(SongsSkipped);
                    logNode.AppendChild(Entry);
                    entries.AppendChild(logNode);
                    doc.AppendChild(entries);
                    doc.Save(SettingsFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }

        public bool readSettings()
        {
            try
            {
                Variables setVars = new Variables();
                SettingsFilePath = setVars.SettingsFilePath;
                if (File.Exists(SettingsFilePath))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(SettingsFilePath);

                    XmlNode BLF = xmlDoc.SelectSingleNode("Data/Settings/BlockListFile");
                    setVars.BlockListFilePath = BLF.Attributes["Path"].Value;

                    XmlNode ELF = xmlDoc.SelectSingleNode("Data/Settings/ErrorLogFile");
                    setVars.ErrorLogFilePath = ELF.Attributes["Path"].Value;

                    XmlNode Behaviour = xmlDoc.SelectSingleNode("Data/Settings/Behaviour");
                    setVars.StartSpotify = bool.Parse(Behaviour.Attributes["StartSpotify"].Value);
                    setVars.PlaySong = bool.Parse(Behaviour.Attributes["PlaySong"].Value);

                    XmlNode Timer = xmlDoc.SelectSingleNode("Data/Settings/Timer");
                    setVars.TimerInterval = int.Parse(Timer.Attributes["Interval"].Value);

                    XmlNode Skipped = xmlDoc.SelectSingleNode("Data/Logging/SongsSkipped");
                    setVars.SongsSkipped = int.Parse(Skipped.Attributes["SongsSkipped"].Value);

                    XmlNode Played = xmlDoc.SelectSingleNode("Data/Logging/SongsPlayed");
                    setVars.SongsPlayed = int.Parse(Played.Attributes["SongsPlayed"].Value);

                    xmlDoc.Save(SettingsFilePath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }

        public bool writeSettings(bool startSpotify, bool playSong, string BlockListFilePath, string ErrorLogFilePath,int Interval, int songsSkipped, int songsPlayed)
        {
            try
            {
                Variables setVars = new Variables();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(SettingsFilePath);

                XmlNode BlockList = xmlDoc.SelectSingleNode("Data/Settings/BlockListFile");
                BlockList.Attributes["Path"].Value = BlockListFilePath.ToString();

                XmlNode ErrorLog = xmlDoc.SelectSingleNode("Data/Settings/ErrorLogFile");
                ErrorLog.Attributes["Path"].Value = ErrorLogFilePath.ToString();

                XmlNode Behaviour = xmlDoc.SelectSingleNode("Data/Settings/Behaviour");
                Behaviour.Attributes["StartSpotify"].Value = startSpotify.ToString();
                Behaviour.Attributes["PlaySong"].Value = playSong.ToString();

                XmlNode TimerInterval = xmlDoc.SelectSingleNode("Data/Settings/Timer");
                TimerInterval.Attributes["Interval"].Value = Interval.ToString();

                XmlNode VersionNumber = xmlDoc.SelectSingleNode("Data/Settings/Version");
                VersionNumber.Attributes["ProgramVersion"].Value = setVars.VersionNumber;

                XmlNode Skipped = xmlDoc.SelectSingleNode("Data/Logging/SongsSkipped");
                Skipped.Attributes["SongsSkipped"].Value = songsSkipped.ToString();

                XmlNode Played = xmlDoc.SelectSingleNode("Data/Logging/SongsPlayed");
                Played.Attributes["SongsPlayed"].Value = songsPlayed.ToString();

                xmlDoc.Save(SettingsFilePath);
                return true;
            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }

        public bool UpdateVersionNumber()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    Variables setVars = new Variables();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(SettingsFilePath);

                    XmlNode Version = xmlDoc.SelectSingleNode("Data/Settings/Version");
                    Version.Attributes["ProgramVersion"].Value = setVars.VersionNumber;
                    
                    xmlDoc.Save(SettingsFilePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }

        /// <summary>
        /// Updates the log-Variables in the logfile
        /// </summary>
        /// <param name="SongsSkipped">Amount of how many songs were skipped</param>
        /// <param name="SongsPlayed">Amount of how many songs were played</param>
        /// <returns>[BOOL] success/fail</returns>
        public bool updateLog(int SongsSkipped, int SongsPlayed)
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    Variables setVars = new Variables();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(SettingsFilePath);

                    XmlNode Skipped = xmlDoc.SelectSingleNode("Data/Logging/SongsSkipped");
                    Skipped.Attributes["SongsSkipped"].Value = SongsSkipped.ToString();

                    XmlNode Played = xmlDoc.SelectSingleNode("Data/Logging/SongsPlayed");
                    Played.Attributes["SongsPlayed"].Value = SongsPlayed.ToString();

                    xmlDoc.Save(SettingsFilePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                new FileIO_Write().logError(ex);
                return false;
            }
        }
    }

    class Processes
    {
        public bool StartSpotify()
        {
            var spotifyProcess = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (spotifyProcess == null)
            {
                string executable;
                RegistryKey key;

                // search in: CurrentUser
                key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    executable = subkey.GetValue("DisplayIcon").ToString();
                    if (executable != null)
                    {
                        if (executable.Contains("Spotify.exe"))
                        {
                            Process.Start(executable);
                            return true;
                        }
                    }
                }

                // search in: LocalMachine_32
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    executable = subkey.GetValue("DisplayIcon").ToString();
                    if (executable != null)
                    {
                        if (executable.Contains("Spotify.exe"))
                        {
                            Process.Start(executable);
                            return true;
                        }
                    }
                }

                // search in: LocalMachine_64
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
                foreach (String keyName in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    executable = subkey.GetValue("DisplayIcon").ToString();
                    if (executable != null)
                    {
                        if (executable.Contains("Spotify.exe"))
                        {
                            Process.Start(executable);
                            return true;
                        }
                    }
                }
            }
            // NOT FOUND
            return false;
        }
    }
}
