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
    /// Interaktionslogik für BlockListManager.xaml
    /// </summary>
    public partial class BlockListManager : Window
    {

        private string BlockListFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockListV2.xml";
        private string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "BlockLog.log";
        private string ErrorLogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\" + DateTime.Today.ToShortDateString() + "ErrorLog.log";
        DispatcherTimer refreshTimer = new DispatcherTimer();
        private enum BlockType { SongBlock, ArtistBlock, ComboBlock, none };

        int countdown = 5;

        public BlockListManager()
        {
            InitializeComponent();
            LoadBlockList();
            FileSystemWatcher fsw = new FileSystemWatcher();
            fsw.Path = Path.GetDirectoryName(BlockListFilePath);
            fsw.Filter = "BlockListV2.xml";
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.IncludeSubdirectories = false;
            fsw.Changed += new FileSystemEventHandler(Fsw_Changed);
            fsw.EnableRaisingEvents = true;
            //refreshTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            //refreshTimer.Tick += RefreshTimer_Tick;
            //refreshTimer.Start();
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => LoadBlockList());
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            this.Title = "BlockListManager - " + countdown + "sec. until refresh";
            countdown--;
            if (countdown < 0)
            {
                LoadBlockList();
                countdown = 5;
            }
            
        }

        void LoadBlockList()
        {
            SongBlockListBox.Items.Clear();
            ArtistBlockListBox.Items.Clear();
            ComboBlockListBox.Items.Clear();

            XElement root = XElement.Parse(File.ReadAllText(BlockListFilePath));

            var blocksong = root.Descendants("Song");
            var blockArtist = root.Descendants("Artist");
            var blockCombo = root.Descendants("Combo");
            foreach (var song in blocksong)
            {
                if (song.HasAttributes)
                {
                    SongBlockListBox.Items.Add("\"" + song.Value + "\" | " + song.Attribute("Date").Value);
                }
                else
                {
                    SongBlockListBox.Items.Add("\"" + song.Value + "\"");
                }
            }

            foreach (var Artist in blockArtist)
            {
                if (Artist.HasAttributes)
                {
                    ArtistBlockListBox.Items.Add("\"" + Artist.Value + "\" | " + Artist.Attribute("Date").Value);
                }
                else
                {
                    ArtistBlockListBox.Items.Add("\"" + Artist.Value + "\"");
                }
            }

            foreach (var Combo in blockCombo)
            {
                if (Combo.HasAttributes)
                {
                    ComboBlockListBox.Items.Add("\"" + Combo.Value + "\" | " + Combo.Attribute("Date").Value);
                }
                else
                {
                    ComboBlockListBox.Items.Add("\"" + Combo.Value + "\"");
                }
            }
            SongGrid.Header = "[" + blocksong.Count() + "] Songs blocked";
            ArtistGrid.Header = "[" + blockArtist.Count() + "] Artists blocked";
            ComboGrid.Header = "[" + blockCombo.Count() + "] Combos blocked";


        }

        private void UnblockSongButton_Click(object sender, RoutedEventArgs e)
        {
            string unblock = SongBlockListBox.SelectedItem.ToString();
            if (unblock.Contains(" | "))
            {
                unblock = unblock.Remove(0, 1).Split('|')[0];
                unblock = unblock.Remove(unblock.Count() - 2, 2);
            }
            else
            {
                unblock = unblock.Remove(0, 1);
                unblock = unblock.Remove(unblock.Count() - 1, 1);
            }
            removeEntry(unblock, BlockType.SongBlock);
            LoadBlockList();

        }

        private void UnblockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            string unblock = ArtistBlockListBox.SelectedItem.ToString();
            if (unblock.Contains(" | "))
            {
                unblock = unblock.Remove(0, 1).Split('|')[0];
                unblock = unblock.Remove(unblock.Count() - 2, 2);
            }
            else
            {
                unblock = unblock.Remove(0, 1);
                unblock = unblock.Remove(unblock.Count() - 1, 1);
            }
            removeEntry(unblock, BlockType.ArtistBlock);
            LoadBlockList();

        }

        private void UnblockComboButton_Click(object sender, RoutedEventArgs e)
        {
            string unblock = ComboBlockListBox.SelectedItem.ToString();
            if (unblock.Contains(" | "))
            {
                unblock = unblock.Remove(0, 1).Split('|')[0];
                unblock = unblock.Remove(unblock.Count() - 2, 2);
            }
            else
            {
                unblock = unblock.Remove(0, 1);
                unblock = unblock.Remove(unblock.Count() - 1, 1);
            }
            removeEntry(unblock, BlockType.ComboBlock);
            LoadBlockList();
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}
