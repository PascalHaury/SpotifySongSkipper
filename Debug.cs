using System;

namespace SpotSkip
{
    class Debug
    {
        public bool debugEnabled = false;

        private DateTime started;
        private DateTime stopped;
        private TimeSpan timeDiff;


        public void startTimeMeasure()
        {
            if (debugEnabled) started = DateTime.Now;
        }

        public void stopTimeMeasure()
        {
            if (debugEnabled) stopped = DateTime.Now;
        }

        public TimeSpan getTimeDiff()
        {
            timeDiff = stopped - started;
            return timeDiff;
        }

    }
}
