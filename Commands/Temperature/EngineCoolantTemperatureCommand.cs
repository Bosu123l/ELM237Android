using Android.Bluetooth;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    internal class EngineCoolantTemperatureCommand : BasicCommand
    {
        public EngineCoolantTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 05\r"), socket, "°C", readFromDeviceLock, position)
        {
            Source = "Engine Coolant Temperature";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = base.ReadedData[2] - 40;

                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
            }
        }
    }
}