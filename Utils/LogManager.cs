using System;
using System.IO;
using System.Security.Permissions;

namespace OBDProject.Utils
{

    public class LogManager : IDisposable
    {
        public string Log
        {
            get; private set;
        }

        private const string LogFileName = "log";
        private const string Configuration = "savedConfiguration";

        private const string _startInfo = "Program started";
        private const string _readedInfo = "Readed:";
        private const string _info = "Info:";
        private const string _errorInfo = "Error:";
        private const string _warringInfo = "Warring:";

        public LogManager()
        {
            Log = string.Format("{0}: {1}: {2}{3}", _startInfo, DateTime.Now, _startInfo, System.Environment.NewLine);
        }

        public void ReadedDataWriteLine(string line)
        {
            Log += string.Format("{0}: {1}: {2}{3}", _readedInfo, DateTime.Now, line, System.Environment.NewLine);
        }

        public void InfoWriteLine(string line)
        {
            Log += string.Format("{0}: {1}: {2}{3}", _info, DateTime.Now, line, System.Environment.NewLine);
        }

        public void ErrorWriteLine(string line)
        {
            Log += string.Format("{0}: {1}: {2}{3}", _errorInfo, DateTime.Now, line, System.Environment.NewLine);
        }

        public void WarringWriteLine(string line)
        {
            Log += string.Format("{0}: {1}: {2}{3}", _warringInfo, DateTime.Now, line, System.Environment.NewLine);
        }

        public void SaveLogFile()
        {
            var path = string.Format("{0}/{1}", Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "ObdProject/ObdLogs");
            var filename = Path.Combine(path, string.Format("{0}{1}.txt", DateTime.Now, LogFileName));
            try
            {
                File.WriteAllText(filename, Log);
            }
            catch (Exception e)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(path);

                    File.WriteAllText(filename, Log);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }

            }
        }

        public void Dispose()
        {
            SaveLogFile();
            Log = null;
        }
    }
}