using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class ThrottlePositionCommand : BasicCommand
    {
        public ThrottlePositionCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 11\r"), socket, "%", readFromDeviceLock, position, logManager)
        {
            //01	Show current data
            //11	1	Throttle position	0	100	 %	(100/255)*A
            Source = "Throttle Position";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;
            if (base.ReadedData.Any())
            {
                value = ((100f * base.ReadedData[2]) / 255f).ToString();
            }

            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}