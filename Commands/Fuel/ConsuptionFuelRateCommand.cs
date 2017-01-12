using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class ConsuptionFuelRateCommand : BasicCommand
    {
        public ConsuptionFuelRateCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 5E\r"), socket, "L/h", readFromDeviceLock, position)
        {
            Source = "ConsuptionFuelRateCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = ((base.ReadedData[2] * 256) + base.ReadedData[3]) / 20;

                OnResponse(string.Format("{0} {1} {2}", Source, value, base.Unit));
            }
        }
    }
}