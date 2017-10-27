using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

using System.IO;
using System.Windows.Threading;
using System.Runtime.InteropServices;
//using Ookii;
using System.Collections;
using System.Diagnostics;

namespace SpotSkip
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string FilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockList.xml";
        private string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "BlockLog.log";
        private string ErrorLogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "ErrorLog.log";
        DispatcherTimer SongChecker = new DispatcherTimer();
        string Artist = string.Empty;
        string Song = string.Empty;
        string Combo = string.Empty;
        string oldCombo = string.Empty;

        int BlockCounter = 0;
        int SongCounter = 0;
        bool hidden = true;
        bool boot = true;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        public MainWindow()
        {
            InitializeComponent();
            this.Height = 110;
            SongChecker.Interval = new TimeSpan(0, 0, 0, 0,500);
            SongChecker.Tick += SongChecker_Tick;
            createDefaultTable(FilePath);
            SongChecker.Start();
        }

        private void SongChecker_Tick(object sender, EventArgs e)
        {
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            bool block = false;

            if (proc == null)
            {
                this.Title = "Spotify not started...";
                BlockArtistButton.IsEnabled = false;
                BlockSongButton.IsEnabled = false;
                BlockComboButton.IsEnabled = false;
                CurrentlyPlayingTextBox.Text = "Spotify is not running!";
            }
            else if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Title = "Spotify paused...";
                BlockArtistButton.IsEnabled = false;
                BlockSongButton.IsEnabled = false;
                BlockComboButton.IsEnabled = false;
                CurrentlyPlayingTextBox.Text = "Paused";
            }
            else
            {
                this.Title = "Spotify running...";
                BlockArtistButton.IsEnabled = true;
                BlockSongButton.IsEnabled = true;
                BlockComboButton.IsEnabled = true;
                CurrentlyPlayingTextBox.Text = proc.MainWindowTitle;
                Combo = proc.MainWindowTitle;
                Artist = proc.MainWindowTitle.Split('-')[0].Remove(proc.MainWindowTitle.Split('-')[0].Length-1, 1);
                Song = proc.MainWindowTitle.Split('-')[1].Remove(0, 1);
                Combo = Artist + " - " + Song;
                XElement root = XElement.Parse(File.ReadAllText(FilePath));


                var blocksong = root.Descendants("Song");
                var blockArtist = root.Descendants("Artist");
                var blockCombo = root.Descendants("Combo");
                bool c = false; 

                if (boot)
                {
                    BlockListBox.Items.Add("Successfully loaded " + (blocksong.Count() + blockArtist.Count() + blockCombo.Count()).ToString() + " block-Rules.");
                    BlockListBox.Items.Add("Blocked Songs: " + blocksong.Count());
                    BlockListBox.Items.Add("Blocked Artists: " + blockArtist.Count());
                    BlockListBox.Items.Add("Blocked Combos: " + blockCombo.Count());
                    if (!File.Exists(LogFilePath) || File.ReadAllLines(LogFilePath).Count() < 1)
                    {
                        File.AppendAllText(LogFilePath, "Successfully loaded " + (blocksong.Count() + blockArtist.Count() + blockCombo.Count()).ToString() + " block-Rules." + "\r\n");
                        File.AppendAllText(LogFilePath, "Blocked Songs: " + blocksong.Count() + "\r\n");
                        File.AppendAllText(LogFilePath, "Blocked Artists: " + blockArtist.Count() + "\r\n");
                        File.AppendAllText(LogFilePath, "Blocked Combos: " + blockCombo.Count() + "\r\n");
                    }
                    BlockListBox.Items.Add(FilePath);
                    boot = false;
                }

                foreach (var Combo_ in blockCombo)
                {
                    if (Combo == Combo_.Value)
                    {
                        c = true;
                        block = true;
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                        BlockCounter++;
                        //SongCounter++;
                        //this.Title = "Combo block " + DateTime.Now.ToLongTimeString();
                        BlockListBox.Items.Add("[Combo Block] " + Combo + " [" + DateTime.Now.ToLongTimeString() + "]");
                        BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                        BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                        List<string> lines = File.ReadAllLines(LogFilePath).ToList();
                        lines = lines.GetRange(0, lines.Count - 1);
                        lines.Add("[" + DateTime.Now.ToShortTimeString() + "]\tComboSkip\tArtist: " + Artist + "\tSong: " + Song);
                        File.WriteAllLines(LogFilePath, lines.ToArray());
                    }
                }

                if (c == false)
                {
                    foreach (var Song_ in blocksong)
                    {
                        if (Song == Song_.Value)
                        {
                            block = true;
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                            BlockCounter++;
                            //SongCounter++;
                            //this.Title = "Song block " + DateTime.Now.ToLongTimeString();
                            BlockListBox.Items.Add("[Song Block] " + Combo + " [" + DateTime.Now.ToLongTimeString() + "]");
                            BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                            BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                            List<string> lines = File.ReadAllLines(LogFilePath).ToList();
                            lines = lines.GetRange(0, lines.Count - 1);
                            lines.Add("[" + DateTime.Now.ToShortTimeString() + "]\tSongSkip\tArtist: " + Artist + "\tSong: " + Song);
                            File.WriteAllLines(LogFilePath, lines.ToArray());
                        }
                    }
                    foreach (var Artist_ in blockArtist)
                    {
                        if (Artist == Artist_.Value)
                        {
                            block = true;
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                            BlockCounter++;
                            //SongCounter++;
                            //this.Title = "Artist block " + DateTime.Now.ToLongTimeString();
                            BlockListBox.Items.Add("[Artist Block] " + Combo + " [" + DateTime.Now.ToLongTimeString() + "]");
                            BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                            BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                            List<string> lines = File.ReadAllLines(LogFilePath).ToList();
                            lines = lines.GetRange(0, lines.Count - 1);
                            lines.Add("[" + DateTime.Now.ToShortTimeString() + "]\tArtistSkip\tArtist: " + Artist + "\tSong: " + Song);
                            File.WriteAllLines(LogFilePath, lines.ToArray());
                        }
                    }
                }
                
                c = false;
                if (oldCombo != Combo)
                {
                    SongCounter++;
                }
                if (!block)
                {
                    if (File.ReadLines(LogFilePath).Last().Contains("]\tPlaying\t\tArtist: " + Artist + "\tSong: " + Song) == false)
                    {
                        File.AppendAllText(LogFilePath, "[" + DateTime.Now.ToShortTimeString() + "]\tPlaying\t\tArtist: " + Artist + "\tSong: " + Song + "\r\n");
                    }
                }
                if (BlockCounter == 1)
                {
                    BlockCounterTextBlock.Text = "Blocked " + BlockCounter + "/" + SongCounter + " Song";
                }
                else
                {
                    BlockCounterTextBlock.Text = "Blocked " + BlockCounter + "/" + SongCounter + " Songs";
                }
                oldCombo = Combo;
            }
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
                    XmlNode BlockSongNode = doc.CreateElement("Songs");
                    XmlNode BlockArtistNode = doc.CreateElement("Artists");
                    XmlNode BlockComboNode = doc.CreateElement("Combos");
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

        private void BlockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            XmlNode BlockNode = doc.SelectNodes("/BlockList/Artists")[0];// .ChildNodes[1];
            XmlNode BlockArtist = doc.CreateElement("Artist");
            BlockArtist.InnerText = Artist;
            BlockNode.AppendChild(BlockArtist);
            doc.Save(FilePath);
        }

        private void BlockSongButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            XmlNode BlockNode = doc.SelectNodes("/BlockList/Songs")[0]; //doc.ChildNodes[1];
            XmlNode BlockSong = doc.CreateElement("Song");
            BlockSong.InnerText = Song;
            BlockNode.AppendChild(BlockSong);
            doc.Save(FilePath);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            XmlNode BlockNode = doc.SelectNodes("/BlockList/Combos")[0]; //.ChildNodes[1];
            XmlNode BlockCombo = doc.CreateElement("Combo");
            BlockCombo.InnerText = Artist + " - " + Song;
            BlockNode.AppendChild(BlockCombo);
            doc.Save(FilePath);
        }

        private void Hide_Show_BlockList_Click(object sender, RoutedEventArgs e)
        {
            if (hidden == true)
            {
                Hide_Show_BlockList.Content = "↑";
                this.Height = 320;
                hidden = false;
            }
            else
            {
                Hide_Show_BlockList.Content = "↓";
                this.Height = 110;
                hidden = true;
            }
        }

        private void BlockListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BlockListBox.SelectedItem.ToString() == FilePath)
            {
                Process.Start(Path.GetDirectoryName(FilePath));
            }
        }

        private void BlockListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(FilePath);
                if (BlockListBox.SelectedItem.ToString().StartsWith("[Combo Block]"))
                {
                    string rem = BlockListBox.SelectedItem.ToString().Replace("[Combo Block] ", "");
                    rem = rem.Remove((rem.ToString().Count() - 11), 11);
                    var RemCombo = doc.SelectNodes("BlockList/Combos/Combo[text()='" + rem + "']");
                    if (RemCombo.Count > 0)
                    {
                        RemoveBlockButton.IsEnabled = true;
                    }
                    else
                    {
                        RemoveBlockButton.IsEnabled = false;
                    }
                }
                else if (BlockListBox.SelectedItem.ToString().StartsWith("[Song Block]"))
                {
                    string rem = BlockListBox.SelectedItem.ToString().Replace("[Song Block] ", "");
                    rem = rem.Remove((rem.ToString().Count() - 11), 11);
                    var RemSong = doc.SelectNodes("BlockList/Combos/Song[text()='" + rem + "']");
                    if (RemSong.Count > 0)
                    {
                        RemoveBlockButton.IsEnabled = true;
                    }
                    else
                    {
                        RemoveBlockButton.IsEnabled = false;
                    }
                }
                else if (BlockListBox.SelectedItem.ToString().StartsWith("[Artist Block]"))
                {
                    string rem = BlockListBox.SelectedItem.ToString().Replace("[Artist Block] ", "");
                    rem = rem.Remove((rem.ToString().Count() - 11), 11);
                    var RemArtist = doc.SelectNodes("BlockList/Combos/Artist[text()='" + rem + "']");
                    if (RemArtist.Count > 0)
                    {
                        RemoveBlockButton.IsEnabled = true;
                    }
                    else
                    {
                        RemoveBlockButton.IsEnabled = false;
                    }
                }
                else
                {
                    RemoveBlockButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            { }

        }

        private void RemoveBlockButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FilePath);
            string rem = BlockListBox.SelectedItem.ToString().Replace("[Combo Block] ", "");
            rem = rem.ToString().Replace("[Song Block] ", "");
            rem = rem.ToString().Replace("[Artist Block] ", "");
            rem = rem.Remove((rem.ToString().Count() - 11), 11);
            var RemVar = doc.SelectNodes("BlockList/Combos/Combo[text()='" + rem + "']");
            foreach (XmlNode node in RemVar)
            {
                node.ParentNode.RemoveChild(node);
            }
            int tmp = BlockListBox.SelectedIndex;
            BlockListBox.SelectedIndex = 0;
            BlockListBox.Items.RemoveAt(tmp);
            BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
            BlockCounter--;
            doc.Save(FilePath);
        }
    }
}
