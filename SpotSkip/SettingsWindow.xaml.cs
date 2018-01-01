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
using System.Diagnostics;

namespace SpotSkip
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        private bool UpdEna = false;
        private string LocalUpdateBehaviour = string.Empty;
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void LoadSettings()
        {
            this.Title = "SpotSkip Settings";
            new Settings().ReadSettings();
            Variables getVars = new Variables();
            LoadStatistics();
            ErrorLogFilePathTextBox.Text = getVars.ErrorLogFilePath;
            BlockListFilePathTextBox.Text = getVars.BlockListFilePath;
            SpotifyAutoPlayCheckBox.IsChecked =getVars.PlaySong;
            SpotifyAutoStartCheckBox.IsChecked = getVars.StartSpotify;
            FrequencySlider.Value = getVars.TimerInterval;
            switch (getVars.UpdateBehaviour)
            {
                case "None":
                    UpdateBehaviourComboBox.SelectedIndex = 0;
                    break;
                case "Inform":
                    UpdateBehaviourComboBox.SelectedIndex = 1;
                    break;
                case "Auto":
                    UpdateBehaviourComboBox.SelectedIndex = 2;
                    break;
                default:
                    UpdateBehaviourComboBox.SelectedIndex = 2;
                    break;
            }
            InitializeFileSystemWatcher();
            //UpdEna = true;
        }

        private void InitializeFileSystemWatcher()
        {
            Variables getVars = new Variables();
            FileSystemWatcher fsw = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(getVars.SettingsFilePath),
                Filter = Path.GetFileName(getVars.SettingsFilePath),
                NotifyFilter = NotifyFilters.LastWrite,
                IncludeSubdirectories = false
            };
            fsw.Changed += new FileSystemEventHandler(Fsw_Changed);
            fsw.EnableRaisingEvents = true;
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => LoadStatistics());
        }

        private void LoadStatistics()
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
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog BlockListSelector = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
                {
                    Description = "Please select a folder...",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true,
                    SelectedPath = BlockListFilePathTextBox.Text
                };
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
                Ookii.Dialogs.Wpf.VistaFolderBrowserDialog ErrorLogSelector = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
                {
                    Description = "Please select a folder...",
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true,
                    SelectedPath = ErrorLogFilePathTextBox.Text
                };
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
            if (setVars.UpdateBehaviour != LocalUpdateBehaviour)
            {
                setVars.UpdateBehaviour = LocalUpdateBehaviour;
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
                            new FileIO_Write().CreateDefaultTable(setVars.BlockListFilePath);
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                            new FileIO_Write().CreateDefaultTable(setVars.BlockListFilePath);
                        }
                    }
                    if (File.Exists(setVars.ErrorLogFilePath))
                    {
                        if (System.IO.Directory.Exists(setVars.ErrorLogFilePath))
                        {
                            new FileIO_Write().LogError("");
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                            new FileIO_Write().LogError("");
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
                            new FileIO_Write().CreateDefaultTable(setVars.BlockListFilePath);
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.BlockListFilePath));
                            new FileIO_Write().CreateDefaultTable(setVars.BlockListFilePath);
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
                            new FileIO_Write().LogError("");
                        }
                        else
                        {
                            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(setVars.ErrorLogFilePath));
                            new FileIO_Write().LogError("");
                        }
                    }
                }
            }
            #endregion

            new Settings().WriteSettings(setVars.StartSpotify, setVars.PlaySong, setVars.BlockListFilePath, setVars.ErrorLogFilePath, setVars.TimerInterval, setVars.SongsSkipped, setVars.SongsPlayed, setVars.UpdateBehaviour);

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

        private void ManualUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            NetClass NetFunct = new NetClass();
            string OnlineVersion = string.Empty;
            string InstalledVerion = string.Empty;
            string Size = string.Empty;

            if (NetFunct.CheckForUpdates(out InstalledVerion, out OnlineVersion, out Size))
            {
                MessageBoxResult MRes = MessageBox.Show("A new Update was found!\r\nInstalled version: " + InstalledVerion + "\r\nOnline version: " + OnlineVersion + "\r\nUpdate size: " + Size + "\r\n\r\nDo you want to update?", "Update", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (MRes == MessageBoxResult.Yes)
                {
                    Process.Start(new Variables().UpdaterPath);
                }
            }
            else
            {
                MessageBox.Show("No Update was found!\r\nInstalled version: " + InstalledVerion + "\r\nOnline version: " + OnlineVersion, "Update", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
        }

        private void UpdateBehaviourComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (UpdEna)
                {
                    LocalUpdateBehaviour = UpdateBehaviourComboBox.SelectedItem.ToString();
                    if (LocalUpdateBehaviour.Contains("Do not Update"))
                    {
                        LocalUpdateBehaviour = "None";
                    }
                    else if (LocalUpdateBehaviour.Contains("Search for Updates, but don't install"))
                    {
                        LocalUpdateBehaviour = "Inform";
                    }
                    else if (LocalUpdateBehaviour.Contains("Auto Updates"))
                    {
                        LocalUpdateBehaviour = "Auto";
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void UpdateBehaviourComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdEna = true;
        }
    }
}
