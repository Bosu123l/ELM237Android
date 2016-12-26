using Android.Bluetooth;
using Android.Util;
using Java.Util;
using System;
using System.Threading.Tasks;

namespace OBDProject.Utils
{
    public class BluetoothManager
    {
        public const string Uuid = "00001101-0000-1000-8000-00805F9B34FB";

        public event EventHandler<bool> Connected;

        private BluetoothAdapter _bluetoothAdapter;

        private BluetoothAdapter _myAdapter;
        private BluetoothSocket _socket;
        private string _rawData;

        public BluetoothManager()
        {
        }

        public void Connect(string address)
        {
            _myAdapter = BluetoothAdapter.DefaultAdapter;
            if (_myAdapter == null)
            {
                //Device has no Bluetooth
            }
            if (!_myAdapter.IsEnabled)
            {
                _myAdapter.Enable();
            }
            BluetoothDevice d = _myAdapter.GetRemoteDevice(address);

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
            if (tempHandler != null)
            {
                tempHandler(this, connected);
            }
        }

        private void writeToOBD(byte[] command)
        {
            _socket.OutputStream.Write(command, 0, command.Length);
            _socket.OutputStream.Flush();
        }

        public string GetDataFromOdb(byte[] command)
        {
            writeToOBD(command);


            try
            {
                _rawData = string.Empty;
                int a = 0;

                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                char c;
                while (((a = (byte)_socket.InputStream.ReadByte()) > -1))
                {
                    c = (char)a;
                    if (c == '>')
                    {
                        break;
                    }
                    builder.Append(c);
                }

                _rawData = builder.ToString();
                Log.Info("-----------------------------------", "RawData: " + _rawData);
                return _rawData;
            }
            catch (System.Exception e)
            {
                Log.Info("", "" + e.Message);
                return string.Format("Error :{0}", e.Message);
            }
            finally
            {
                _socket.InputStream.Flush();
            }

        }
    }
}