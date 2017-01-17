using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Fuel
{
    internal class FuelPressureCommand : BasicCommand
    {
        public FuelPressureCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 0A\r"), socket, "kPa", readFromDeviceLock, position, logManager)
        {
            Source = "Fuel Pressure";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;
            if (base.ReadedData.Any())
            {
                value = (base.ReadedData[2] * 3).ToString();
            }
            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}