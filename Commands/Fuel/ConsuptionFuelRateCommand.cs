using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class ConsuptionFuelRateCommand : BasicCommand
    {
        public ConsuptionFuelRateCommand(BluetoothSocket socket, object readFromDeviceLock) : base(Encoding.ASCII.GetBytes("01 5E\r"), socket, "L/h", readFromDeviceLock)
        {
            Source = "ConsuptionFuelRateCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.readedData.Any())
            {
                var value = ((base.readedData[2] * 256) + base.readedData[3]) / 20;

                OnResponse(string.Format("{0} {1} {2}", Source, value, base.Unit));
            }
        }
    }
}