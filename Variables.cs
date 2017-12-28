using System;
using System.IO;
namespace SpotSkip
{
    class Variables
    {
        private static string storage1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockList.xml";
        private static string storage2 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\ErrorLog.log";
        private static int storage3 = 0;
        private static int storage4 = 0;
        private static bool storage5 = true;
        private static bool storage6 = false;
        private static int storage7 = 500;



        /// <summary>
        /// The path to the block list file
        /// </summary>
        public string BlockListFilePath
        {
            get
            {
                return storage1;
            }
            set
            {
                storage1 = value;
            }
        } 
        /// <summary>
        /// The path to the error log file
        /// </summary>
        public string ErrorLogFilePath
        {
            get
            {
                return storage2;
            }
            set
            {
                storage2 = value;
            }
        }
        /// <summary>
        /// Amount of how many songs you have skipped
        /// </summary>
        public int SongsSkipped
        {
            get
            {
                return storage3;
            }
            set
            {
                storage3 = value;
            }
        }
        /// <summary>
        /// Amount of how many songs you have played
        /// </summary>
        public int SongsPlayed
        {
            get
            {
                return storage4;
            }
            set
            {
                storage4 = value;
            }
        }
        /// <summary>
        /// Start Spotify at the startup of SpotSkip
        /// </summary>
        public bool StartSpotify
        {
            get
            {
                return storage5;
            }
            set
            {
                storage5 = value;
            }
        }
        /// <summary>
        /// Play a song in Spotify at the startup of SpotSkip
        /// </summary>
        public bool PlaySong
        {
            get
            {
                return storage6;
            }
            set
            {
                storage6 = value;
            }
        }
        /// <summary>
        /// The interval for the Refresh Timer
        /// </summary>
        public int TimerInterval
        {
            get
            {
                return storage7;
            }
            set
            {
                if (value <500)
                {
                    storage7 = 500;
                }
                else if (value >2000)
                {
                    storage7 = 2000;
                }
                else
                {
                    storage7 = value;
                }
            }
        }
    }
}
