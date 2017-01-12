using Android.Bluetooth;
using System;
using System.Linq;
using System.Text;
using OBDProject.Utils;

namespace OBDProject.Commands.Fuel
{
    internal class FuelPressureCommand : BasicCommand
    {
        public FuelPressureCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 0A\r"), socket, "kPa", readFromDeviceLock, position, logManager)
        {
            Source = "FuelPressureCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = base.ReadedData[2] * 3;

                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
            }
        }
    }
}