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
        DispatcherTimer SongChecker = new DispatcherTimer();
        string Artist = string.Empty;
        string Song = string.Empty;
        string Combo = string.Empty;
        int BlockCounter = 0;
        bool hidden = true;

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
                bool c = false; ;


                foreach (var Combo_ in blockCombo)
                {
                    if (Combo == Combo_.Value)
                    {
                        c = true;
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                        BlockCounter++;
                        //this.Title = "Combo block " + DateTime.Now.ToLongTimeString();
                        BlockListBox.Items.Add("[Combo Block] - " + Combo + " - [" + DateTime.Now.ToLongTimeString() + "]");
                        BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                        BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                    }
                }

                if (c == false)
                {
                    foreach (var Song_ in blocksong)
                    {
                        if (Song == Song_.Value)
                        {
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                            BlockCounter++;
                            //this.Title = "Song block " + DateTime.Now.ToLongTimeString();
                            BlockListBox.Items.Add("[Song Block]" + Combo + " [" + DateTime.Now.ToLongTimeString() + "]");
                            BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                            BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                        }
                    }
                    foreach (var Artist_ in blockArtist)
                    {
                        if (Artist.Contains(Artist_.Value))
                        {
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
                            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
                            BlockCounter++;
                            //this.Title = "Artist block " + DateTime.Now.ToLongTimeString();
                            BlockListBox.Items.Add("[Artist Block]" + Combo + " [" + DateTime.Now.ToLongTimeString() + "]");
                            BlockListBox.SelectedIndex = BlockListBox.Items.Count - 1;
                            BlockListBox.ScrollIntoView(BlockListBox.SelectedItem);
                        }
                    }
                }
                c = false;
                if (BlockCounter == 1)
                {
                    BlockCounterTextBlock.Text = "Blocked " + BlockCounter + " Song";
                }
                else
                {
                    BlockCounterTextBlock.Text = "Blocked " + BlockCounter + " Songs";
                }
                
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
                this.Height = 290;
                hidden = false;
            }
            else
            {
                Hide_Show_BlockList.Content = "↓";
                this.Height = 110;
                hidden = true;
            }
        }
    }
}
