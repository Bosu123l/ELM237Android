using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Fuel
{
    public class FuelTypeCommand : BasicCommand
    {
        public FuelTypeCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 51\r"), socket, " ", readFromDeviceLock, position, logManager)
        {
            Source = "Fuel Type";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;
            if (base.ReadedData.Any())
            {
                value = ((FuelType)base.ReadedData[2]).ToString();
            }
            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}