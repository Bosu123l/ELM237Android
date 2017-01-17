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

        public string DeviceName { get; set; }


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
    }
}