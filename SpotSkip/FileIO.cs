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
    class FileIO_Write
    {
        private Variables vars = new Variables();
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
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }

        public bool AddEntry(string Entry, BlockType BT)
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
                doc.Load(vars.BlockListFilePath);

                XmlNode BlockNode = doc.SelectNodes("/BlockList/" + type + "s")[0];// .ChildNodes[1];
                XmlNode BlockEntry = doc.CreateElement(type);
                BlockEntry.Attributes.Append(Song);
                BlockEntry.Attributes.Append(Artist);
                BlockEntry.Attributes.Append(Date);
                BlockEntry.InnerText = Entry;
                BlockNode.AppendChild(BlockEntry);
                doc.Save(vars.BlockListFilePath);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
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
                doc.Load(vars.BlockListFilePath);
                XmlNodeList RemVar = doc.SelectNodes(NodeSelect);
                foreach (XmlNode node in RemVar)
                {
                    node.ParentNode.RemoveChild(node);
                }
                doc.Save(vars.BlockListFilePath);

                return true;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        public bool LogError(Exception _ex)
        {
            try
            {
                File.AppendAllLines(vars.ErrorLogFilePath, new String[] { _ex.ToString() });
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
        private Variables vars = new Variables();

        public bool searchForEntry(string CurrentlyPlaying)
        {
            XElement root = XElement.Parse(File.ReadAllText(vars.BlockListFilePath));
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

            foreach (var _Song in blocksong)
            {
                if (Song == _Song.Value)
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
            return false;
        }


    }
}
