using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class ThrottlePositionCommand : BasicCommand
    {
        public ThrottlePositionCommand(BluetoothSocket socket, object readFromDeviceLock) : base(Encoding.ASCII.GetBytes("01 11\r"), socket, "%", readFromDeviceLock)
        {
            //01	Show current data
            //11	1	Throttle position	0	100	 %	(100/255)*A
            Source = "ThrottlePositionCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.readedData.Any())
            {
                var value = (100f * base.readedData[2]) / 255f;
                OnResponse(string.Format("{0} {1} {3}", Source, value, base.Unit));
            }
        }
    }
}