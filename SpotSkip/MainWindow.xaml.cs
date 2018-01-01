using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private Variables globalVars = new Variables();
        private Settings SettingsManager = new Settings();
        private Processes SpotProc = new Processes();

        private DispatcherTimer localSpotifySongChecker = new DispatcherTimer();
        private string lastSong = string.Empty;
        private string currentlyPlaying = string.Empty;
        private string spotifyStatus = string.Empty;

        private int blockedSongCounter = 0;
        private int playedSongCounter = 0;

        private bool isPlaying = false;
        private bool autoplay = false;

        private BlockListManager BLM = null;
        private bool blockListManagerActive = false;

        private SettingsWindow SW = null;
        private bool settingsWindowActive = false;

        int oldInterval = 0;

        public MainWindow()
        {
            InitializeComponent();
            initializeApplication();
        }

        private bool initializeApplication()
        {
            try
            {
                this.Title = "SpotSkip";
                SettingsManager.createDefaultTable();
                SettingsManager.readSettings();
                SettingsManager.UpdateVersionNumber();
                localSpotifySongChecker.Interval = new TimeSpan(0, 0, 0, 0, globalVars.TimerInterval);
                oldInterval = globalVars.TimerInterval;
                localSpotifySongChecker.Tick += localSpotifySongChecker_Tick;
                xmlWriter.createDefaultTable(globalVars.BlockListFilePath);
                localSpotifySongChecker.Start();
                TitleTextBlock.Text = "SpotSkip v" + new Variables().VersionNumber;

                if (globalVars.StartSpotify == true)
                {
                    SpotProc.StartSpotify();
                }

                return true;
            }
            catch (Exception ex)
            {
                xmlWriter.logError(ex);
                return false;
            }
        }

        private void localSpotifySongChecker_Tick(object sender, EventArgs e)
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
                        blockListManagerActive = true;
                    }
                }
                else
                {
                    if (BlockListManagerButton.Content.ToString() != "Open BLM")
                    {
                        BlockListManagerButton.Content = "Open BLM";
                        blockListManagerActive = false;
                    }
                }
            }
            //Check the status of the SettingWindow, set the
            //correct Button text and the status flag
            if (SW != null)
            {
                if (SW.IsVisible)
                {
                    if (SettingsButton.Content.ToString() != "Close SW")
                    {
                        SettingsButton.Content = "Close SW";
                        settingsWindowActive = true;
                    }
                }
                else
                {
                    if (SettingsButton.Content.ToString() != "Settings")
                    {
                        SettingsButton.Content = "Settings";
                        settingsWindowActive = false;
                    }
                }
            }

            //Check if Timerinterval was changed
            if (globalVars.TimerInterval != oldInterval)
            {
                localSpotifySongChecker.Interval = new TimeSpan(0, 0, 0, 0, globalVars.TimerInterval);
                oldInterval = globalVars.TimerInterval;
            }


            //Get the Current song playing on Spotify
            var spotifyProcess = Process.GetProcessesByName("Spotify").FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.MainWindowTitle));
            if (spotifyProcess == null)   //Spotify isn't started
            {
                spotifyStatus = " Spotify is not started...";
                TitleTextBlock.Text = "SpotSkip v" + new Variables().VersionNumber;
                //TitleTextBlock.Text = spotifyStatus;
                CurrentlyPlayingTextBox.Text = spotifyStatus;
                disableControls();
                isPlaying = false;
            }
            else if (string.Equals(spotifyProcess.MainWindowTitle, "Spotify", StringComparison.InvariantCultureIgnoreCase))   //Spotify is started but Paused
            {
                spotifyStatus = " Spotify is paused...";
                TitleTextBlock.Text = "SpotSkip v" + new Variables().VersionNumber;
                //TitleTextBlock.Text = spotifyStatus;
                CurrentlyPlayingTextBox.Text = spotifyStatus;
                disableControls();
                isPlaying = false;
                if (!autoplay && globalVars.PlaySong)
                {
                    keyEmu.playSong();
                    autoplay = true;
                }
            }
            else    //Spotify is Started and a song is Playing
            {
                
                enableControls();
                currentlyPlaying = spotifyProcess.MainWindowTitle;
                if (currentlyPlaying != lastSong || !isPlaying)   
                {
                    autoplay = true;
                    spotifyStatus = " Spotify is playing music...";
                    lastSong = spotifyProcess.MainWindowTitle;
                    //TitleTextBlock.Text = spotifyStatus;
                    CurrentlyPlayingTextBox.Text = currentlyPlaying;
                    playedSongCounter++;
                    if (xmlReader.searchForEntry(currentlyPlaying))
                    {
                        blockedSongCounter++;
                        keyEmu.skipSong();
                        globalVars.SongsSkipped++;
                    }
                    else
                    {
                        globalVars.SongsPlayed++;
                    }
                    TitleTextBlock.Text += " | " + blockedSongCounter +"/" + playedSongCounter + " songs Blocked";
                    isPlaying = true;
                    SettingsManager.updateLog(globalVars.SongsSkipped, globalVars.SongsPlayed);
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

        #region GUI
        private void BlockArtistButton_Click(object sender, RoutedEventArgs e)
        {
            string Artist = currentlyPlaying.Split('-')[0]; //get the current artist
            xmlWriter.addEntry(Artist.Remove(Artist.Length - 1, 1), FileIO_Write.BlockType.ArtistBlock);    //add the Artist top the blocklist
            keyEmu.skipSong();  //skip the current song
            if (globalVars.SongsPlayed>0)globalVars.SongsPlayed--;
            blockedSongCounter++;
            globalVars.SongsSkipped++;
            SettingsManager.updateLog(globalVars.SongsSkipped, globalVars.SongsPlayed);
        }

        private void BlockSongButton_Click(object sender, RoutedEventArgs e)
        {
            string Song = currentlyPlaying.Split('-')[1];   //get the current song
            xmlWriter.addEntry(Song.Remove(0, 1), FileIO_Write.BlockType.SongBlock);    //add the song top the blocklist
            keyEmu.skipSong();  //skip the current song
            if (globalVars.SongsPlayed > 0) globalVars.SongsPlayed--;
            blockedSongCounter++;
            globalVars.SongsSkipped++;
            SettingsManager.updateLog(globalVars.SongsSkipped, globalVars.SongsPlayed);
        }

        private void BlockComboButton_Click(object sender, RoutedEventArgs e)
        {
            string Combo = currentlyPlaying;    //get the current song/artist combo
            xmlWriter.addEntry(Combo, FileIO_Write.BlockType.ComboBlock);   //add the song/artist combo to the blocklist
            keyEmu.skipSong();  //skip the current song
            if (globalVars.SongsPlayed > 0) globalVars.SongsPlayed--;
            blockedSongCounter++;
            globalVars.SongsSkipped++;
            SettingsManager.updateLog(globalVars.SongsSkipped, globalVars.SongsPlayed);
        }

        private void BlockListManagerButton_Click(object sender, RoutedEventArgs e)
        {
           if (!blockListManagerActive)
            {
                BLM = new BlockListManager();
                BLM.Topmost = true;
                BLM.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                BLM.Show();
                blockListManagerActive = true;
                BlockListManagerButton.Content = "Close BLM";
            }
            else
            {
                BLM.Close();
                BLM = null;
                blockListManagerActive = false; ;
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
            if (SW != null)    //Close the BlockListManager if it is active
            {
                SW.Close();
                SW = null;
            }
        }

        private void MinimizeButtonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButtonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsManager.writeSettings(globalVars.StartSpotify, globalVars.PlaySong,globalVars.BlockListFilePath, globalVars.ErrorLogFilePath,globalVars.TimerInterval, globalVars.SongsSkipped, globalVars.SongsPlayed);
            Environment.Exit(0x01);
        }

        private void TitleTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!settingsWindowActive)
            {
                SW = new SettingsWindow();
                SW.Topmost = true;
                SW.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                SW.Show();
                settingsWindowActive = true;
                SettingsButton.Content = "Close SW";
            }
            else
            {
                SW.Close();
                SW = null;
                settingsWindowActive = false; ;
                SettingsButton.Content = "Settings";
            }
        }

        #endregion
    }
}