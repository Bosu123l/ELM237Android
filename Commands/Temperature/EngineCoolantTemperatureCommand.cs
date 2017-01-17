using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    internal class EngineCoolantTemperatureCommand : BasicCommand
    {
        public EngineCoolantTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 05\r"), socket, "°C", readFromDeviceLock, position, logManager)
        {
            Source = "Engine Coolant Temperature";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;

            if (base.ReadedData.Any())
            {
                value = (base.ReadedData[2] - 40).ToString();
            }

            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}