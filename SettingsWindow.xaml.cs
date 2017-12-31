using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Ookii;
using System.Xml;
using System.Xml.Linq;

namespace SpotSkip
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void LoadSettings()
        {
            this.Title = "SpotSkip Settings";
            Variables getVars = new Variables();
            loadStatistics();
            ErrorLogFilePathTextBox.Text = getVars.ErrorLogFilePath;
            BlockListFilePathTextBox.Text = getVars.BlockListFilePath;
            SpotifyAutoPlayCheckBox.IsChecked =getVars.PlaySong;
            SpotifyAutoStartCheckBox.IsChecked = getVars.StartSpotify;
            FrequencySlider.Value = getVars.TimerInterval;
            initializeFileSystemWatcher();
        }

        private void initializeFileSystemWatcher()
        {
            Variables getVars = new Variables();
            FileSystemWatcher fsw = new FileSystemWatcher();
            fsw.Path = Path.GetDirectoryName(getVars.SettingsFilePath);
            fsw.Filter = Path.GetFileName(getVars.SettingsFilePath);
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.IncludeSubdirectories = false;
            fsw.Changed += new FileSystemEventHandler(Fsw_Changed);
            fsw.EnableRaisingEvents = true;
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => loadStatistics());
        }

        private void loadStatistics()
        {
            Variables getVars = new Variables();
            XElement root = XElement.Parse(File.ReadAllText(getVars.BlockListFilePath));

            var blocksong = root.Descendants("Song");
            var blockArtist = root.Descendants("Artist");
            var blockCombo = root.Descendants("Combo");
            SongsPlayedLabel.Content = "Songs played: " + getVars.SongsPlayed;
            SongsSkippedLabel.Content = "Songs skipped: " + getVars.SongsSkipped;
            SongBlockLabel.Content = "Songs blocked: " + blocksong.Count();
            ArtistBlockLabel.Content = "Artists blocked: " + blockArtist.Count();
            ComboBlockLabel.Content = "Combos blocked: " + blockCombo.Count();
            TotalBlocksLabel.Content = "Total blocks: " + (blocksong.Count() + blockArtist.Count() + blockCombo.Count()).ToString();
        }

        #region GUI
        private void BlockListFilePathTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BlockListFilePathTextBox.Text == "Blocklist File Path" && BlockListFilePathTextBox.IsReadOnly == false)
            {
                BlockListFilePathTextBox.Text = "";
            }
        }

        private void BlockListFilePathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (BlockListFilePathTextBox.Text == "")
            {
                BlockListFilePathTextBox.Text = "Blocklist File Path";
            }
        }

        private void MinimizeButtonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButtonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void TitleTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void ErrorLogFilePathTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorLogFilePathTextBox.Text == "Errorlog File Path" && ErrorLogFilePathTextBox.IsReadOnly == false)
            {
                ErrorLogFilePathTextBox.Text = "";
            }
        }

        private void ErrorLogFilePathTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorLogFilePathTextBox.Text == "")
            {
                ErrorLogFilePathTextBox.Text = "Errorlog File Path";
            }
        }

        private void OverrideBlockListPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)OverrideBlockListPathCheckBox.IsChecked)
            {
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog BlockListSelector = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                BlockListSelector.Description = "Please select a folder...";
                BlockListSelector.UseDescriptionForTitle = true;
                BlockListSelector.ShowNewFolderButton = true;
                BlockListSelector.SelectedPath = BlockListFilePathTextBox.Text;
                if ((bool)BlockListSelector.ShowDialog())
                {
                    BlockListFilePathTextBox.Text = BlockListSelector.SelectedPath + "\\BlockList.xml";
                    BlockListFilePathTextBox.IsReadOnly = false;
                }
                else
                {
                    BlockListFilePathTextBox.IsReadOnly = true;
                    BlockListFilePathTextBox.Text = new Variables().BlockListFilePath;
                    OverrideBlockListPathCheckBox.IsChecked = false;
                }
            }
            else
            {
                BlockListFilePathTextBox.IsReadOnly = true;
                BlockListFilePathTextBox.Text = new Variables().BlockListFilePath;
            }
        }

        private void OverrideErrorLogPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)OverrideErrorLogPathCheckBox.IsChecked)
            {
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog ErrorLogSelector = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                ErrorLogSelector.Description = "Please select a folder...";
                ErrorLogSelector.UseDescriptionForTitle = true;
                ErrorLogSelector.ShowNewFolderButton = true;
                ErrorLogSelector.SelectedPath = ErrorLogFilePathTextBox.Text;
                if ((bool)ErrorLogSelector.ShowDialog())
                {
                    ErrorLogFilePathTextBox.Text = ErrorLogSelector.SelectedPath + "ErrorLog.log";
                    ErrorLogFilePathTextBox.IsReadOnly = false;
                }
                else
                {
                    ErrorLogFilePathTextBox.IsReadOnly = true;
                    ErrorLogFilePathTextBox.Text = new Variables().ErrorLogFilePath;
                    OverrideErrorLogPathCheckBox.IsChecked = false;
                }
                
            }
            else
            {
                ErrorLogFilePathTextBox.IsReadOnly = true;
                ErrorLogFilePathTextBox.Text = new Variables().ErrorLogFilePath;
            }
        }
        #endregion

        private void SpotifyAutoPlayCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)SpotifyAutoPlayCheckBox.IsChecked)
            {
                MessageBoxResult Result = MessageBox.Show("The auto-play function is experimental,\r\nso it might not work properly...\r\nDo you want to activate it?", "Are you sure", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (Result == MessageBoxResult.Yes)
                {
                    SpotifyAutoPlayCheckBox.IsChecked = true;
                }
                else
                {
                    SpotifyAutoPlayCheckBox.IsChecked = false;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.Content = "Saving";
            Variables setVars = new Variables();
            bool BlockListPathChanged = false;
            bool ErrorLogPathChanged = false;
            string BlockListPath_old = setVars.BlockListFilePath;
            string ErrorLogPath_old = setVars.ErrorLogFilePath;
            if (setVars.ErrorLogFilePath != ErrorLogFilePathTextBox.Text)
            {
                ErrorLogPathChanged = true;
                setVars.ErrorLogFilePath = ErrorLogFilePathTextBox.Text;
            }
            if (setVars.BlockListFilePath != BlockListFilePathTextBox.Text)
            {
                BlockListPathChanged = true;
                setVars.BlockListFilePath = BlockListFilePathTextBox.Text;
            }
            if (setVars.PlaySong != (bool)SpotifyAutoPlayCheckBox.IsChecked)
            {
                setVars.PlaySong = (bool)SpotifyAutoPlayCheckBox.IsChecked;
            }
            if (setVars.StartSpotify != (bool)SpotifyAutoStartCheckBox.IsChecked)
            {
                setVars.StartSpotify = (bool)SpotifyAutoStartCheckBox.IsChecked;
            }

            if (setVars.TimerInterval != FrequencySlider.Value)
            {
                setVars.TimerInterval = Convert.ToInt32(FrequencySlider.Value.ToString("000"));
            }
            #region Copy/Create Error/BlockListFile
            MessageBoxResult MR;
            if (BlockListPathChanged && ErrorLogPathChanged)
            {
                MR = MessageBox.Show("Blocklist and Errorlog paths were changed.\r\nCopy the existing files?","Saving...", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (MR == MessageBoxResult.Yes)
                {
                    if (System.IO.File.Exists(setVars.BlockListFilePath))
                    {
                        System.IO.File.Delete(setVars.BlockListFilePath);
                    }
                    

                    if (System.IO.File.Exists(setVars.ErrorLogFilePath))
                    {
                        System.IO.File.Delete(setVars.ErrorLogFilePath);
                    }
                   


                    if (System.IO.Directory.Exists(setVars.BlockListFilePath))
                    {
                        System.IO.File.Copy(BlockListPath_old, setVars.BlockListFilePath);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                        System.IO.File.Copy(BlockListPath_old, setVars.BlockListFilePath);
                    }
                    if (System.IO.Directory.Exists(setVars.ErrorLogFilePath))
                    {
                        System.IO.File.Copy(ErrorLogPath_old, setVars.ErrorLogFilePath);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                        System.IO.File.Copy(ErrorLogPath_old, setVars.ErrorLogFilePath);
                    }
                }
                else
                {
                    if (File.Exists(setVars.BlockListFilePath))
                    {
                        if (System.IO.Directory.Exists(setVars.BlockListFilePath))
                        {
                            new FileIO_Write().createDefaultTable(setVars.BlockListFilePath);
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                            new FileIO_Write().createDefaultTable(setVars.BlockListFilePath);
                        }
                    }
                    if (File.Exists(setVars.ErrorLogFilePath))
                    {
                        if (System.IO.Directory.Exists(setVars.ErrorLogFilePath))
                        {
                            new FileIO_Write().logError("");
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                            new FileIO_Write().logError("");
                        }
                    }
                }
            }
            else if (BlockListPathChanged)
            {
                MR = MessageBox.Show("Blocklist path was changed.\r\nCopy the existing file?", "Saving...", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (MR == MessageBoxResult.Yes)
                {
                    if (System.IO.File.Exists(setVars.BlockListFilePath))
                    {
                        System.IO.File.Delete(setVars.BlockListFilePath);
                    }


                    if (System.IO.Directory.Exists(setVars.BlockListFilePath))
                    {
                        System.IO.File.Copy(BlockListPath_old, setVars.BlockListFilePath);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                        if (System.IO.File.Exists(setVars.BlockListFilePath))
                        {
                            System.IO.File.Delete(setVars.BlockListFilePath);
                            System.IO.File.Copy(BlockListPath_old, setVars.BlockListFilePath);
                        }
                        else
                        {
                            System.IO.File.Copy(BlockListPath_old, setVars.BlockListFilePath);
                        }
                    }
                }
                else
                {
                    if (!File.Exists(setVars.BlockListFilePath))
                    {
                        if (System.IO.Directory.Exists(setVars.BlockListFilePath))
                        {
                            new FileIO_Write().createDefaultTable(setVars.BlockListFilePath);
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                            new FileIO_Write().createDefaultTable(setVars.BlockListFilePath);
                        }
                    }
                }
            }
            else if (ErrorLogPathChanged)
            {
                MR = MessageBox.Show("Errorlog path was changed.\r\nCopy the existing file?", "Saving...", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (MR == MessageBoxResult.Yes)
                {
                    if (System.IO.File.Exists(setVars.ErrorLogFilePath))
                    {
                        System.IO.File.Delete(setVars.ErrorLogFilePath);
                    }
                   

                    if (System.IO.Directory.Exists(setVars.ErrorLogFilePath))
                    {
                        System.IO.File.Copy(ErrorLogPath_old, setVars.ErrorLogFilePath);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                        System.IO.File.Copy(ErrorLogPath_old, setVars.ErrorLogFilePath);
                    }
                }
                else
                {
                    if (!File.Exists(setVars.ErrorLogFilePath))
                    {
                        if (System.IO.Directory.Exists(setVars.ErrorLogFilePath))
                        {
                            new FileIO_Write().logError("");
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                            new FileIO_Write().logError("");
                        }
                    }
                }
            }
            #endregion

            new Settings().writeSettings(setVars.StartSpotify, setVars.PlaySong, setVars.BlockListFilePath, setVars.ErrorLogFilePath, setVars.TimerInterval, setVars.SongsSkipped, setVars.SongsPlayed);

            SaveButton.Content = "Saved";
            this.Close();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FrequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                UpdateFrequencyTextBlock.Text = ((1.00 / (double)FrequencySlider.Value)*1000.00).ToString("0.00") + " Hz";
            }
            catch (Exception)
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void OverrideBlockListPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
