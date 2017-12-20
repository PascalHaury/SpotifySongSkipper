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
using System.Windows.Shapes;

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
            LoadSettings();
        }

        private void LoadSettings()
        {
            Variables getVars = new Variables();
            ErrorLogFilePathTextBox.Text = getVars.ErrorLogFilePath;
            BlockListFilePathTextBox.Text = getVars.BlockListFilePath;
            SpotifyAutoPlayCheckBox.IsChecked =getVars.PlaySong;
            SpotifyAutoStartCheckBox.IsChecked = getVars.StartSpotify;

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
                MessageBoxResult Result = MessageBox.Show("The Override function is experimental,\r\nso it might not work properly...\r\nDo you want to activate it?", "Are you sure", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (Result == MessageBoxResult.Yes)
                {
                    OverrideBlockListPathCheckBox.IsChecked = true;
                }
                else
                {
                    OverrideBlockListPathCheckBox.IsChecked = false;
                }
            }
            if ((bool)OverrideBlockListPathCheckBox.IsChecked)
            {
                BlockListFilePathTextBox.IsReadOnly = false;
            }
            else
            {
                BlockListFilePathTextBox.IsReadOnly = true;
            }
        }

        private void OverrideErrorLogPathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)OverrideErrorLogPathCheckBox.IsChecked)
            {
                MessageBoxResult Result = MessageBox.Show("The Override function is experimental,\r\nso it might not work properly...\r\nDo you want to activate it?", "Are you sure", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No);
                if (Result == MessageBoxResult.Yes)
                {
                    OverrideErrorLogPathCheckBox.IsChecked = true;
                }
                else
                {
                    OverrideErrorLogPathCheckBox.IsChecked = false;
                }
            }
            if ((bool)OverrideErrorLogPathCheckBox.IsChecked)
            {
                ErrorLogFilePathTextBox.IsReadOnly = false;
            }
            else
            {
                ErrorLogFilePathTextBox.IsReadOnly = true;
            }
        }
        #endregion

        private void SpotifyAutoStartCheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

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
            if (setVars.ErrorLogFilePath != ErrorLogFilePathTextBox.Text)
            {
                setVars.ErrorLogFilePath = ErrorLogFilePathTextBox.Text;
            }
            if (setVars.BlockListFilePath != BlockListFilePathTextBox.Text)
            {
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

            Settings WriteSettings = new Settings();
            WriteSettings.writeSettings(setVars.StartSpotify, setVars.PlaySong, setVars.BlockListFilePath, setVars.ErrorLogFilePath);
            SaveButton.Content = "Saved";
        }

        private void OverrideBlockListPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void OverrideErrorLogPathCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
