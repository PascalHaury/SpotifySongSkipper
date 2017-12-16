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
        private string CurrentlyPlaying = string.Empty;
        private string SpotifyStatus = string.Empty;

        private int BlockedSongCounter = 0;

        private bool IsPlaying = false;

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
            var SpotifyProcess = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (SpotifyProcess == null)   //Spotify isn't started
            {
                SpotifyStatus = "Spotify is not started...";
                this.Title = SpotifyStatus;
                CurrentlyPlayingTextBox.Text = SpotifyStatus;
                disableControls();
                IsPlaying = false;
            }
            else if (string.Equals(SpotifyProcess.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))   //Spotify is started but Paused
            {
                SpotifyStatus = "Spotify is paused...";
                this.Title = SpotifyStatus;
                CurrentlyPlayingTextBox.Text = SpotifyStatus;
                disableControls();
                IsPlaying = false;
            }
            else    //Spotify is Started and a song is Playing
            {
                
                enableControls();
                if (CurrentlyPlaying != SpotifyProcess.MainWindowTitle || !IsPlaying)   
                {
                    SpotifyStatus = "Spotify is playing music...";
                    this.Title = SpotifyStatus;
                    CurrentlyPlaying = SpotifyProcess.MainWindowTitle;
                    CurrentlyPlayingTextBox.Text = CurrentlyPlaying;
                    if (xmlReader.searchForEntry(CurrentlyPlaying))
                    {
                        BlockedSongCounter++;
                        keyEmu.SkipSong();
                    }
                    BlockCounterTextBlock.Text = BlockedSongCounter + " Blocked";
                    IsPlaying = true;
                }
            }
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
            keyEmu.SkipSong();  //skip the current song
        }

        private void BlockComboButton_Click(object sender, RoutedEventArgs e)
        {
            string Combo = CurrentlyPlaying;    //get the current song/artist combo
            xmlWriter.AddEntry(Combo, FileIO_Write.BlockType.ComboBlock);   //add the song/artist combo to the blocklist
            keyEmu.SkipSong();  //skip the current song
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
            if (BLM != null)    //Close the BlockListManager if it is active
            {
                BLM.Close();
                BLM = null;
            }
        }
    }
}