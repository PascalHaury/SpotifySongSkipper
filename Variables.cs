using System;

namespace SpotSkip
{
    class Variables
    {
        private static string storage1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\BlockListV2.xml";
        private static string storage2 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\SpotSkip\ErrorLog.log";
        private static int storage3 = 0;
        private static int storage4 = 0;
        private static bool storage5 = true;
        private static bool storage6 = false;



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
    }
}
