using System;
using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class ThrottlePositionCommand : BasicCommand
    {
        public ThrottlePositionCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 11\r"), socket, "%", readFromDeviceLock, position)
        {
            //01	Show current data
            //11	1	Throttle position	0	100	 %	(100/255)*A
            Source = "Throttle Position";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = (100f * base.ReadedData[2]) / 255f;
                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
            }
        }
    }
}