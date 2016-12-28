using Android.Bluetooth;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OBDProject.Commands
{
    public abstract class BasicCommand
    {
        public object ReadFromDeviceLock;

        public string ReadyData
        {
            get;
            private set;
        }

        protected readonly Regex WhitespacePattern = new Regex(@"\s+");
        protected readonly Regex BusinitPattern = new Regex(@"(BUS INIT) | (BUSINIT) | (\\.)");
        protected readonly Regex SearchingPattern = new Regex(@"SEARCHING");
        protected readonly Regex DigitsLettersPattern = new Regex(@"([0-9A-F])+");
        protected readonly Regex UnableToConnect = new Regex(@"(UNABLETOCONNECT) | (UNABLE TO CONNECT");

        protected List<int> readedData;
        protected string Unit;

        private readonly BluetoothSocket _socket;
        private readonly byte[] _command;

        public event EventHandler<string> Response;

        protected BasicCommand(byte[] command, BluetoothSocket socket, string unit, object readFromDeviceLock)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command cannot by null!");
            }
            if (socket == null)
            {
                throw new ArgumentNullException("_socket Cannot be null!");
            }
            if (string.IsNullOrEmpty(unit))
            {
                throw new ArgumentNullException("Unit Cannot be empty!");
            }
            _command = command;
            _socket = socket;
            ReadyData = string.Empty;
            ReadFromDeviceLock = readFromDeviceLock;
        }

        protected abstract void PrepereFindResult();

        protected void OnResponse(string response)
        {
            var tempHandler = Response;
            tempHandler?.Invoke(this, response);
        }

        private void SendCommand()
        {
            try
            {
                _socket.OutputStream.Write(_command, 0, _command.Length);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error while send command: {0}", e.Message));
            }
            finally
            {
                _socket.OutputStream.Flush();
            }
        }

        public async Task ReadResult()
        {
            await Task.Run(() =>
            {
                lock (ReadFromDeviceLock)
                {
                    try
                    {
                        SendCommand();
                        readedData = FillBuffer(CheckForErrors(ReadRawData()));
                        PrepereFindResult();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("++++++ERROR++++++", ex.Message);
                    }
                }
            });
        }

        private string ReadRawData()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            char c;
            int a = 0;
            try
            {
                while ((a = (byte)_socket.InputStream.ReadByte()) > -1)
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
                throw new Exception(string.Format("Error while data was readed: {0}", ex.Message));
            }
            finally
            {
                _socket.InputStream.Flush();
            }
            var clearedData = ClearResponseByRegex(builder.ToString(), SearchingPattern);
            clearedData = ClearResponseByRegex(clearedData, WhitespacePattern);

            Log.Info("++++++CLEARED MESSAGE++++++", clearedData);

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

        private string CheckForErrors(string data)
        {
            var temp = CheckIfNull(data);
            if (CheckIfMatchPattern(temp, UnableToConnect))
            {
                throw new ArgumentException("Unable connect!");
            }
            return temp;
        }

        private List<int> FillBuffer(string data)
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

            Log.Info("++++++LIST OF NUMBERS IN MESSAGE++++++", string.Join(",", buffer.Select(x => x.ToString())));

            return buffer;
        }

        private static string CheckIfNull(string s)
        {
            return s?.Replace(@"\s", "").ToUpper() ?? "";
        }
    }
}