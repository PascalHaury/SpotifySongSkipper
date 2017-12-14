using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

using System.IO;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SpotSkip
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string BlockListFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockListV2.xml";
        private string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "BlockLog.log";
        private string ErrorLogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "ErrorLog.log";
        DispatcherTimer LocalSpotifySongChecker = new DispatcherTimer();
        DispatcherTimer SongCheckerAPI = new DispatcherTimer();
        string CurrentlyPlaying = string.Empty;
        string[] Separator = new string[] { " - " };

        bool paused = true;
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        private int SongCounter = 0;
        private int BlockedSongCounter = 0;

        private List<BlockType> LastBlockenum = new List<BlockType>();
        private List<string> LastBlock = new List<string>();

        //DEBUG VARS
        
        BlockListManager BLM = null;
        bool BlockListManagerActive = false;
        public MainWindow()
        {
            InitializeComponent();
            initializeApplication();
            
        }

        #region AppSystem
        private enum BlockType { SongBlock, ArtistBlock, ComboBlock, none };

        private bool initializeApplication()
        {
            try
            {
                this.Height = 105;
                LocalSpotifySongChecker.Interval = new TimeSpan(0, 0, 0, 0, 500);
                LocalSpotifySongChecker.Tick += LocalSpotifySongChecker_Tick;
                createDefaultTable(BlockListFilePath);
                LocalSpotifySongChecker.Start();

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        private void LocalSpotifySongChecker_Tick(object sender, EventArgs e)
        {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (proc == null)
            {
                this.Title = "Spotify not started...";
                BlockArtistButton.IsEnabled = false;
                BlockSongButton.IsEnabled = false;
                BlockComboButton.IsEnabled = false;
                CurrentlyPlayingTextBox.Text = "Spotify is not running!";
                paused = true;
            }
            else if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Title = "Spotify paused...";
                BlockArtistButton.IsEnabled = false;
                BlockSongButton.IsEnabled = false;
                BlockComboButton.IsEnabled = false;
                CurrentlyPlayingTextBox.Text = "Paused";
                paused = true;
            }
            else
            {
                
                BlockArtistButton.IsEnabled = true;
                BlockSongButton.IsEnabled = true;
                BlockComboButton.IsEnabled = true;
                if (CurrentlyPlaying != proc.MainWindowTitle || paused)
                {
                    SongCounter++;
                    CurrentlyPlaying = proc.MainWindowTitle;
                    CurrentlyPlayingTextBox.Text = CurrentlyPlaying;
                    if (CurrentSongBlocked(CurrentlyPlaying))
                    {
                        BlockedSongCounter++;
                        SkipSong();
                    }
                    paused = false;
                    BlockCounterTextBlock.Text = BlockedSongCounter + " Blocked";
                }
            }
            //this.Title = "AVG: " + timeaverage.TotalMilliseconds.ToString("0.000") + "ms MIN: " + min.TotalMilliseconds.ToString("0.000") + "ms, MAX: " + max.TotalMilliseconds.ToString("0.000") + "ms";
        }

        private bool CurrentSongBlocked(string CurrentlyPlaying)
        {
            XElement root = XElement.Parse(File.ReadAllText(BlockListFilePath));
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
                    LastBlock.Add(Combo);
                    LastBlockenum.Add(BlockType.ComboBlock);
                    return true;
                }
            }

            foreach (var _Song in blocksong)
            {
                if (Song == _Song.Value)
                {
                    LastBlock.Add(Song);
                    LastBlockenum.Add(BlockType.SongBlock);
                    return true;
                }
            }

            foreach (var _Artist in blockArtist)
            {
                if (Artist == _Artist.Value)
                {
                    LastBlock.Add(Artist);
                    LastBlockenum.Add(BlockType.ArtistBlock);
                    return true;
                }
            }
            return false;
        }

        private void SkipSong()
        {
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        private bool createDefaultTable(string path_to_File)
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
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }
        #endregion


        #region HelperFunctions
        private bool LogError(Exception _ex)
        {
            try
            {
                File.AppendAllLines(ErrorLogFilePath, new String[] { _ex.ToString() });
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(_ex.ToString(), "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool AddEntry(string Entry, BlockType BT)
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
                doc.Load(BlockListFilePath);
                
                XmlNode BlockNode = doc.SelectNodes("/BlockList/" + type + "s")[0];// .ChildNodes[1];
                XmlNode BlockEntry = doc.CreateElement(type);
                BlockEntry.Attributes.Append(Song);
                BlockEntry.Attributes.Append(Artist);
                BlockEntry.Attributes.Append(Date);
                BlockEntry.InnerText = Entry;
                BlockNode.AppendChild(BlockEntry);
                doc.Save(BlockListFilePath);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        private bool removeEntry(string Entry, BlockType BT)
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
                doc.Load(BlockListFilePath);
                XmlNodeList RemVar = doc.SelectNodes(NodeSelect);
                foreach (XmlNode node in RemVar)
                {
                    node.ParentNode.RemoveChild(node);
                }
                doc.Save(BlockListFilePath);

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }
        #endregion


        #region GUI
        private void BlockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            string Artist = CurrentlyPlaying.Split('-')[0];
            AddEntry(Artist.Remove(Artist.Length - 1, 1), BlockType.ArtistBlock);
            SkipSong();
        }

        private void BlockSongButton_Click(object sender, RoutedEventArgs e)
        {
            string Song = CurrentlyPlaying.Split('-')[1];
            AddEntry(Song.Remove(0, 1), BlockType.SongBlock);
            SkipSong();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Song = CurrentlyPlaying.Split('-')[1];
            string Artist = CurrentlyPlaying.Split('-')[0];
            Song = Song.Remove(0, 1);
            Artist = Artist.Remove(Artist.Length - 1, 1);
            string Combo = Artist + " - " + Song;
            AddEntry(Combo, BlockType.ComboBlock);
            SkipSong();
        }

        private void RemoveBlockButton_Click(object sender, RoutedEventArgs e)
        {
           if (!BlockListManagerActive)
            {
                BLM = new BlockListManager();
                BLM.Show();
                BlockListManagerActive = true;
                OpenBlockListManagerButton.Content = "Close BLM";
            }
            else
            {
                BLM.Close();
                BLM = null;
                BlockListManagerActive = false; ;
                OpenBlockListManagerButton.Content = "Open BLM";
            }
        }
        #endregion

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}