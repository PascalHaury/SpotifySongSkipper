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
        private KeyboardEmulation keyEmu = new KeyboardEmulation();
        private FileIO_Write xmlWriter = new FileIO_Write();
        private FileIO_Read xmlReader = new FileIO_Read();
        private Variables vars = new Variables();

        DispatcherTimer LocalSpotifySongChecker = new DispatcherTimer();
        string CurrentlyPlaying = string.Empty;

        private int SongCounter = 0;
        private int BlockedSongCounter = 0;

        string status = string.Empty;

        BlockListManager BLM = null;
        bool BlockListManagerActive = false;

        public MainWindow()
        {
            InitializeComponent();
            initializeApplication();
        }

        private bool initializeApplication()
        {
            try
            {
                this.Height = 105;
                LocalSpotifySongChecker.Interval = new TimeSpan(0, 0, 0, 0, 500);
                LocalSpotifySongChecker.Tick += LocalSpotifySongChecker_Tick;
                xmlWriter.createDefaultTable(vars.BlockListFilePath);
                LocalSpotifySongChecker.Start();

                return true;
            }
            catch (Exception ex)
            {
                xmlWriter.LogError(ex);
                return false;
            }
        }

        private void LocalSpotifySongChecker_Tick(object sender, EventArgs e)
        {
            //Check the status of the BlockListManager, set the
            //correct Button text and the status flag
            if (BLM != null)
            {
                if (BLM.IsVisible)
                {
                    if (BlockListManagerButton.Content.ToString() != "Close BLM")
                    {
                        BlockListManagerButton.Content = "Close BLM";
                        BlockListManagerActive = true;
                    }
                }
                else
                {
                    if (BlockListManagerButton.Content.ToString() != "Open BLM")
                    {
                        BlockListManagerButton.Content = "Open BLM";
                        BlockListManagerActive = false;
                    }
                }
            }

            //Get the Current song playing on Spotify
            var proc = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (proc == null)   //Spotify isn't started
            {
                status = "Spotify is not started...";
                this.Title = status;
                CurrentlyPlayingTextBox.Text = status;
                disableControls();
            }
            else if (string.Equals(proc.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))   //Spotify is started but Paused
            {
                status = "Spotify is paused...";
                this.Title = status;
                CurrentlyPlayingTextBox.Text = status;
                disableControls();
            }
            else    //Spotify is Started and a song is Playing
            {
                enableControls();
                if (CurrentlyPlaying != proc.MainWindowTitle)
                {
                    SongCounter++;
                    CurrentlyPlaying = proc.MainWindowTitle;
                    CurrentlyPlayingTextBox.Text = CurrentlyPlaying;
                    if (xmlReader.searchForEntry(CurrentlyPlaying))
                    {
                        BlockedSongCounter++;
                        keyEmu.SkipSong();
                    }
                    BlockCounterTextBlock.Text = BlockedSongCounter + " Blocked";
                }
            }
            //this.Title = "AVG: " + timeaverage.TotalMilliseconds.ToString("0.000") + "ms MIN: " + min.TotalMilliseconds.ToString("0.000") + "ms, MAX: " + max.TotalMilliseconds.ToString("0.000") + "ms";
        }

        private void enableControls()
        {
            BlockArtistButton.IsEnabled = true;
            BlockSongButton.IsEnabled = true;
            BlockComboButton.IsEnabled = true;
        }    //should be self explaining

        private void disableControls()
        {
            BlockArtistButton.IsEnabled = false;
            BlockSongButton.IsEnabled = false;
            BlockComboButton.IsEnabled = false;
        }   //should be self explaining

        private void BlockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            string Artist = CurrentlyPlaying.Split('-')[0]; //get the current artist
            xmlWriter.AddEntry(Artist.Remove(Artist.Length - 1, 1), FileIO_Write.BlockType.ArtistBlock);    //add the Artist top the blocklist
            keyEmu.SkipSong();  //skip the current song
        }

        private void BlockSongButton_Click(object sender, RoutedEventArgs e)
        {
            string Song = CurrentlyPlaying.Split('-')[1];   //get the current song
            xmlWriter.AddEntry(Song.Remove(0, 1), FileIO_Write.BlockType.SongBlock);    //add the song top the blocklist
            keyEmu.SkipSong();//skip the current song
        }

        private void BlockComboButton_Click(object sender, RoutedEventArgs e)
        {
            //string Song = CurrentlyPlaying.Split('-')[1];   //get the current song
            //string Artist = CurrentlyPlaying.Split('-')[0]; //get the current artist
            //Song = Song.Remove(0, 1);                       //remove some blank spaces from the Split operation
            //Artist = Artist.Remove(Artist.Length - 1, 1);   //remove some blank spaces from the Split operation
            string Combo = CurrentlyPlaying;
            xmlWriter.AddEntry(Combo, FileIO_Write.BlockType.ComboBlock);
            keyEmu.SkipSong();
        }

        private void BlockListManagerButton_Click(object sender, RoutedEventArgs e)
        {
           if (!BlockListManagerActive)
            {
                BLM = new BlockListManager();
                BLM.Show();
                BlockListManagerActive = true;
                BlockListManagerButton.Content = "Close BLM";
            }
            else
            {
                BLM.Close();
                BLM = null;
                BlockListManagerActive = false; ;
                BlockListManagerButton.Content = "Open BLM";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (BLM != null)
            {
                BLM.Close();
                BLM = null;
            }
        }
    }
}