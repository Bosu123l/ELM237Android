using Android.Bluetooth;
using Java.Util;
using System;

namespace OBDProject.Utils
{
    public class BluetoothManager : IDisposable
    {
        public const string Uuid = "00001101-0000-1000-8000-00805F9B34FB";

        public BluetoothSocket Socket
        {
            get
            {
                return _socket;
            }
            private set { _socket = value; }
        }

        public event EventHandler<bool> Connected;

        private BluetoothAdapter _bluetoothAdapter;
        private BluetoothSocket _socket;
    
        public BluetoothManager()
        {
        }

        public void Connect(string address)
        {
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (_bluetoothAdapter == null)
            {
                //Device has no Bluetooth
            }
            if (_bluetoothAdapter != null && !_bluetoothAdapter.IsEnabled)
            {
                _bluetoothAdapter.Enable();
            }
            BluetoothDevice d = _bluetoothAdapter.GetRemoteDevice(address);

            _socket = d.CreateRfcommSocketToServiceRecord(UUID.FromString(Uuid));
            try
            {
                _socket.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            OnConnected(_socket.IsConnected);
        }

        protected void OnConnected(bool connected)
        {
            var tempHandler = Connected;
            tempHandler?.Invoke(this, connected);
        }

        public void Dispose()
        {
            _bluetoothAdapter?.Dispose();
            _socket?.Dispose();
        }


        //private void writeToOBD(byte[] command)
        //{
        //    _socket.OutputStream.Write(command, 0, command.Length);
        //    _socket.OutputStream.Flush();
        //}

        //public async Task<List<int>> GetDataFromOdb(byte[] command, string source)
        //{
        //    writeToOBD(command);
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            var buffer = new List<int>();
        //            var rawData = string.Empty;
        //            int a = 0;
        //            List<byte> oryginalMessage = new List<byte>();
        //            System.Text.StringBuilder builder = new System.Text.StringBuilder();
        //            char c;
        //            while (((a = (byte)_socket.InputStream.ReadByte()) > -1))
        //            {
        //                c = (char)a;

        //                if (c == '>')
        //                {
        //                    break;
        //                }
        //                builder.Append(c);
        //            }

        //            rawData = builder.ToString();

        //            buffer = formatMessage(rawData, source);

        //            if (!buffer.Any())
        //            {
        //                throw new Exception("EMPTY");
        //            }

        //            Log.Info(source, "List of Numbers" + string.Join(",", buffer.Select(x => x.ToString())));
        //            Log.Info(source, "oryginal message: " + rawData);
        //            return buffer;
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Info("", "" + e.Message);
        //            return new List<int>();
        //        }
        //        finally
        //        {
        //            _socket.InputStream.Flush();
        //        }
        //    });
        //}

        //private List<int> formatMessage(string rawData, string source)
        //{
        //    var buffer = new List<int>();
        //    Regex digitsLettersPattern = new Regex("([0-9A-F])+", RegexOptions.IgnoreCase);
        //    string message = string.Empty;

        //    Regex white = new Regex(@"\s+");
        //    Regex dot = new Regex(@"\.");

        //    message = white.Replace(rawData, "");
        //    message = dot.Replace(message, "");

        //    Match match = digitsLettersPattern.Match(message);

        //    //if (match.Success)
        //    //{
        //    //    return buffer;
        //    //}
        //    Log.Info(source, "cleared message: " + message);
        //    // read string each two chars
        //    buffer.Clear();

        //    try
        //    {
        //        message = StringToHex(message);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Info(source, "Error while convert to hex string");
        //    }

        //    int begin = 0;
        //    int end = 2;
        //    while (end <= message.Length)
        //    {
        //        buffer.Add(Convert.ToInt32(message.Substring(begin, 2), 16));
        //        begin = end;
        //        end += 2;
        //    }
        //    return buffer;
        //}

        //private string StringToHex(string message)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var text in message)
        //    {
        //        sb.Append(Convert.ToInt32(text).ToString("x") + " ");
        //    }
        //    return sb.ToString();
        //}
    }
}