using System.IO;
using System.Text.RegularExpressions;

namespace DataProfiler
{
    public static class ProfiledConfiguration
    {
        //todo this is definitely not thread safe
        //also I am not sure about this approach
        //I need to shoe horn this into some existing stuff so...

        public static bool LogIds { get; set; }
        public static bool LogData { get; set; }
        public static string Session { get; private set; }        
        public static DirectoryInfo LogDataTo { 
            get { return _logDataTo; }
            set 
            {
                value.Create();
                _logDataTo = value;                
            }
        }
        public static Regex IdMatcher { get; set; }

        //NH changes the field names.  Is Painful...
        internal static Regex NhFieldSelect  = new Regex("SELECT (.*) FROM");
        internal static Regex NhInnerFieldSelect = new Regex("(.*) as (.*)");

        private static DirectoryInfo _logDataTo;


        static ProfiledConfiguration()
        {
            Session = RandomGenerator.GetRandomWord();
            IdMatcher = new Regex("(.*)_ID$");
        }
    }
}