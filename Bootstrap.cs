using Android.App;
using Android.Runtime;
using OBDProject.Utils;
using System;

namespace OBDProject
{
    [Application]
    public class Bootstrap : Application
    {
        public BluetoothManager BluetoothManager
        {
            get
            {
                if (_bluetoothManager == null)
                {
                    _bluetoothManager = new BluetoothManager();
                }
                return _bluetoothManager;
            }
        }

        public LogManager LogManager
        {
            get
            {
                if (_logManager == null)
                {
                    _logManager = new LogManager();
                }
                return _logManager;
            }
        }

        public object ReadFromDeviceLock
        {
            get
            {
                if (_readFromDeviceLock == null)
                {
                    _readFromDeviceLock = new object();
                }
                return _readFromDeviceLock;
            }
        }

        private object _readFromDeviceLock;
        private LogManager _logManager;
        private BluetoothManager _bluetoothManager;

        public Bootstrap(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
    }
}