using Android.Bluetooth;
using Android.Util;
using OBDProject.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OBDProject.Commands
{
    public abstract class BasicCommand
    {
        public const string NoData = "data not available";

        public object ReadFromDeviceLock;

        public string ReadyData
        {
            get;
            private set;
        }

        public int Position
        {
            get;
        }

        protected readonly Regex WhitespacePattern = new Regex(@"\s+");
        protected readonly Regex BusinitPattern = new Regex(@"(BUS INIT) | (BUSINIT) | (\\.)");
        protected readonly Regex SearchingPattern = new Regex(@"SEARCHING");
        protected readonly Regex DigitsLettersPattern = new Regex(@"([0-9A-F])+");
        protected readonly Regex UnableToConnect = new Regex(@"(UNABLETOCONNECT) | (UNABLE TO CONNECT)");

        protected List<int> ReadedData;
        protected string Unit;

        public string Source
        {
            get; protected set;
        }

        protected readonly BluetoothSocket Socket;
        protected readonly byte[] Command;
        protected LogManager LogManager;

        public event EventHandler<string> Response;

        protected BasicCommand(byte[] command, BluetoothSocket socket, object readFromDeviceLock, LogManager logManager) : this(command, socket, string.Empty, readFromDeviceLock, 0, logManager)
        {
        }

        protected BasicCommand(byte[] command, BluetoothSocket socket, string unit, object readFromDeviceLock, int position, LogManager logManager)
        {
            if (logManager == null)
            {
                throw new ArgumentException("Manager log cannot be null!");
            }

            if (command == null)
            {
                throw new ArgumentNullException("command cannot be null!");
            }
            if (socket == null)
            {
                //throw new ArgumentNullException("Socket Cannot be null!");
            }

            Command = command;
            Socket = socket;
            ReadyData = string.Empty;
            ReadFromDeviceLock = readFromDeviceLock;
            Unit = unit;
            Position = position;
            LogManager = logManager;
        }

        protected abstract void PrepereFindResult();

        protected void OnResponse(string response)
        {
            var tempHandler = Response;
            tempHandler?.Invoke(this, response);
        }

        protected void SendCommand()
        {
            try
            {
                Socket.OutputStream.Write(Command, 0, Command.Length);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error while send command: {0}", e.Message));
            }
            finally
            {
                Socket.OutputStream.Flush();
            }
        }

        public virtual async Task ReadResult()
        {
            await Task.Run(() =>
            {
                lock (ReadFromDeviceLock)
                {
                    try
                    {
                        SendCommand();
                        ReadedData = FillBuffer(CheckForErrors(ReadRawData()));
                        PrepereFindResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("++++++ERROR++++++", ex.Message);
                        LogManager.ErrorWriteLine(ex.Message);
                    }
                }
            });
        }

        protected virtual string ReadRawData()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            char c;
            int a = 0;
            try
            {
                while ((a = (byte)Socket.InputStream.ReadByte()) > -1)
                {
                    c = (char)a;

                    if (c == '>')
                    {
                        break;
                    }
                    builder.Append(c);
                }

                Log.Info("++++++ORYGINAL MESSAGE++++++", builder.ToString());
            }
            catch (Exception ex)
            {
                LogManager.ErrorWriteLine(ex.Message);
                throw new Exception(string.Format("Error while data was readed: {0}", ex.Message));
            }
            finally
            {
                Socket.InputStream.Flush();
            }
            var clearedData = ClearResponseByRegex(builder.ToString(), SearchingPattern);
            clearedData = ClearResponseByRegex(clearedData, WhitespacePattern);

            Log.Info("++++++CLEARED MESSAGE++++++", clearedData);
            LogManager.InfoWriteLine(string.Format("{0}{1}", "++++++CLEARED MESSAGE++++++", clearedData));
            return clearedData;
        }

        private string ClearResponseByRegex(string data, Regex regexPattern)
        {
            return regexPattern.Replace(data, "");
        }

        private bool CheckIfMatchPattern(string data, Regex regexPattern)
        {
            var match = regexPattern.Match(data);
            return match.Success;
        }

        protected virtual string CheckForErrors(string data)
        {
            var temp = CheckIfNull(data);
            if (CheckIfMatchPattern(temp, UnableToConnect))
            {
                throw new ArgumentException("Unable connect!");
            }
            return temp;
        }

        protected virtual List<int> FillBuffer(string data)
        {
            var buffer = new List<int>();

            try
            {
                var clearedResponde = ClearResponseByRegex(data, WhitespacePattern);
                clearedResponde = ClearResponseByRegex(clearedResponde, BusinitPattern);

                if (!CheckIfMatchPattern(clearedResponde, DigitsLettersPattern))
                {
                    throw new InvalidOperationException(data);
                }

                int begin = 0;
                int end = 2;
                while (end <= clearedResponde.Length)
                {
                    buffer.Add(Convert.ToInt32(clearedResponde.Substring(begin, 2), 16));
                    begin = end;
                    end += 2;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while fillBuffer: {0}", ex.Message));
            }
            LogManager.InfoWriteLine(string.Format("{0}{1}", "++++++LIST OF NUMBERS IN MESSAGE++++++", string.Join(",", buffer.Select(x => x.ToString()))));

            return buffer;
        }

        private static string CheckIfNull(string s)
        {
            return s?.Replace(@"\s", "").ToUpper() ?? "";
        }
    }
}