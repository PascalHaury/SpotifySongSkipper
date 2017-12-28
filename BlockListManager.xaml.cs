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
        Variables getVars = new Variables();

        private List<string> BlockSongList = new List<string>();
        private List<string> BlockArtistList = new List<string>();
        private List<string> BlockComboList = new List<string>();

        private enum BlockType { SongBlock, ArtistBlock, ComboBlock, none };

        public BlockListManager()
        {
            InitializeComponent();
            this.Title = "SotSkip Blocklist Manager";
            LoadBlockList();
            FileSystemWatcher fsw = new FileSystemWatcher();
            fsw.Path = Path.GetDirectoryName(getVars.BlockListFilePath);
            fsw.Filter = Path.GetFileName(getVars.BlockListFilePath);
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.IncludeSubdirectories = false;
            fsw.Changed += new FileSystemEventHandler(Fsw_Changed);
            fsw.EnableRaisingEvents = true;
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => LoadBlockList());
        }

        void LoadBlockList()
        {
            SongBlockListBox.Items.Clear();
            ArtistBlockListBox.Items.Clear();
            ComboBlockListBox.Items.Clear();
            BlockSongList.Clear();
            BlockArtistList.Clear();
            BlockComboList.Clear();

            XElement root = XElement.Parse(File.ReadAllText(getVars.BlockListFilePath));

            var blocksong = root.Descendants("Song");
            var blockArtist = root.Descendants("Artist");
            var blockCombo = root.Descendants("Combo");
            foreach (var song in blocksong)
            {
                BlockSongList.Add(song.Value);
                if (SearchSongTextBox.Text != "Search...")
                {
                    SongBlockListBox.Items.Clear();
                    foreach (string entry in BlockSongList)
                    {
                        if (entry.ToLower().Contains(SearchSongTextBox.Text.ToLower()))
                        {
                            SongBlockListBox.Items.Add("\"" + entry + "\"");
                        }
                    }
                }
                else
                {
                    SongBlockListBox.Items.Add("\"" + song.Value + "\"");
                }
            }

            foreach (var Artist in blockArtist)
            {
                BlockArtistList.Add(Artist.Value);
                if (SearchArtistTextBox.Text != "Search...")
                {
                    ArtistBlockListBox.Items.Clear();
                    foreach (string entry in BlockArtistList)
                    {
                        if (entry.ToLower().Contains(SearchArtistTextBox.Text.ToLower()))
                        {
                            ArtistBlockListBox.Items.Add("\"" + entry + "\"");
                        }
                    }
                }
                else
                {
                    ArtistBlockListBox.Items.Add("\"" + Artist.Value + "\"");
                }
            }

            foreach (var Combo in blockCombo)
            {
                BlockComboList.Add(Combo.Value);
                if (SearchComboTextBox.Text != "Search...")
                {
                    ComboBlockListBox.Items.Clear();
                    foreach (string entry in BlockComboList)
                    {
                        if (entry.ToLower().Contains(SearchComboTextBox.Text.ToLower()))
                        {
                            ComboBlockListBox.Items.Add("\"" + entry + "\"");
                        }
                    }
                }
                else
                {
                    ComboBlockListBox.Items.Add("\"" + Combo.Value + "\"");
                }
            }
            SongGrid.Header = "[" + BlockSongList.Count() + "] Songs blocked";
            ArtistGrid.Header = "[" + BlockArtistList.Count() + "] Artists blocked";
            ComboGrid.Header = "[" + BlockComboList.Count() + "] Combos blocked";


        }

        private void UnblockSongButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string Song in SongBlockListBox.SelectedItems)
            {
                string unblock = Song;
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
            }
            LoadBlockList();

        }

        private void UnblockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string Artist in ArtistBlockListBox.SelectedItems)
            {
                string unblock = Artist;
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
            }
            LoadBlockList();

        }

        private void UnblockComboButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (string combo in ComboBlockListBox.SelectedItems)
            {
                string unblock = combo;
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
            }
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
                doc.Load(getVars.BlockListFilePath);
                XmlNodeList RemVar = doc.SelectNodes(NodeSelect);
                foreach (XmlNode node in RemVar)
                {
                    node.ParentNode.RemoveChild(node);
                }
                doc.Save(getVars.BlockListFilePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void TitleTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseButtonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void SearchSongTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchSongTextBox.Text == "Search...")
            {
                SearchSongTextBox.Text = "";
            }
        }

        private void SearchArtistTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchArtistTextBox.Text == "Search...")
            {
                SearchArtistTextBox.Text = "";
            }
        }

        private void SearchComboTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchComboTextBox.Text == "Search...")
            {
                SearchComboTextBox.Text = "";
            }
        }

        private void SearchSongTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchSongTextBox.Text == "")
            {
                SearchSongTextBox.Text = "Search...";
            }
        }

        private void SearchArtistTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchArtistTextBox.Text == "")
            {
                SearchArtistTextBox.Text = "Search...";
            }
        }

        private void SearchComboTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchComboTextBox.Text == "")
            {
                SearchComboTextBox.Text = "Search...";
            }
        }

        private void SearchSongTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchSongTextBox.Text != "Search...")
            {
                SongBlockListBox.Items.Clear();
                foreach (string entry in BlockSongList)
                {
                    if (entry.ToLower().Contains(SearchSongTextBox.Text.ToLower()))
                    {
                        SongBlockListBox.Items.Add("\"" + entry + "\"");
                    }
                }
                SongGrid.Header = "[" + SongBlockListBox.Items.Count + "] Songs found";
            }
            else
            {
                SongGrid.Header = "[" + BlockSongList.Count() + "] Songs blocked";
            }
        }

        private void SearchArtistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchArtistTextBox.Text != "Search...")
            {
                ArtistBlockListBox.Items.Clear();
                foreach (string entry in BlockArtistList)
                {
                    if (entry.ToLower().Contains(SearchArtistTextBox.Text.ToLower()))
                    {
                        ArtistBlockListBox.Items.Add("\"" + entry + "\"");
                    }
                }
                ArtistGrid.Header = "[" + ArtistBlockListBox.Items.Count + "] Artists found";
            }
            else
            {
                ArtistGrid.Header = "[" + BlockArtistList.Count() + "] Artists blocked";
            }
        }

        private void SearchComboTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchComboTextBox.Text != "Search...")
            {
                ComboBlockListBox.Items.Clear();
                foreach (string entry in BlockComboList)
                {
                    if (entry.ToLower().Contains(SearchComboTextBox.Text.ToLower()))
                    {
                        ComboBlockListBox.Items.Add("\"" + entry + "\"");
                    }
                }
                ComboGrid.Header = "[" + ComboBlockListBox.Items.Count + "] Combos found";

            }
            else
            {
                ComboGrid.Header = "[" + BlockComboList.Count() + "] Combos blocked";
            }
        }

        private void SearchSongTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchSongTextBox.Text = "Search...";
                Keyboard.ClearFocus();
                LoadBlockList();
            }
        }

        private void SearchArtistTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchArtistTextBox.Text = "Search...";
                Keyboard.ClearFocus();
                LoadBlockList();
            }
        }

        private void SearchComboTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchComboTextBox.Text = "Search...";
                Keyboard.ClearFocus();
                LoadBlockList();
            }
        }

        private void SongBlockListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SongBlockListBox.SelectedIndex = -1;
            }
        }

        private void ArtistBlockListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ArtistBlockListBox.SelectedIndex = -1;
            }
        }

        private void ComboBlockListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ComboBlockListBox.SelectedIndex = -1;
            }
        }
    }
}