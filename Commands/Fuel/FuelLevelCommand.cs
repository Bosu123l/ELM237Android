using System;
using System.Linq;
using System.Text;
using Android.Bluetooth;

namespace OBDProject.Commands.Fuel
{
    internal class FuelLevelCommand : BasicCommand
    {
        public FuelLevelCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 2F\r"), socket, "%", readFromDeviceLock, position)
        {
            Source = "Fuel Level";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = ((base.ReadedData[2] * 100f) / 255).ToString();

                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
            }
        }
    }
}