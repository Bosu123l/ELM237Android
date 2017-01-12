using System;
using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class VehicleSpeedCommand : BasicCommand
    {
        public VehicleSpeedCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 0D\r"), socket, "km/h", readFromDeviceLock, position)
        {
            //01	Show current data
            //0D	1	Vehicle speed	0	255	km/h    A
            Source = "Vehicle Speed";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, base.ReadedData[2], base.Unit));
            }
        }
    }
}