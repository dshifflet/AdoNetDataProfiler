using System.IO;

namespace DataProfiler
{
    public static class ProfiledConfiguration
    {
        public static bool LogIds { get; set; }
        public static bool LogData { get; set; }
        public static string Session { get; private set; }
        private static DirectoryInfo _logDataTo;
        public static DirectoryInfo LogDataTo { 
            get { return _logDataTo; }
            set 
            {
                value.Create();
                _logDataTo = value;                
            }
        }

        static ProfiledConfiguration()
        {
            Session = RandomGenerator.GetRandomWord();
        }
    }
}