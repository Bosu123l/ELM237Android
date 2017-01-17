using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Fuel
{
    public class ConsuptionFuelRateCommand : BasicCommand
    {
        public ConsuptionFuelRateCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 5E\r"), socket, "L/h", readFromDeviceLock, position, logManager)
        {
            Source = "Consuption Fuel Rate";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;
            if (base.ReadedData.Any())
            {
                value = (((base.ReadedData[2] * 256) + base.ReadedData[3]) / 20).ToString();
            }
            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}