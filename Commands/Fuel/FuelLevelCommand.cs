using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    internal class FuelLevelCommand : BasicCommand
    {
        public FuelLevelCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 2F\r"), socket, "%", readFromDeviceLock, position)
        {
            Source = "FuelLevelCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = ((base.ReadedData[2] * 100f) / 255).ToString();

                OnResponse(string.Format("{0} {1} {2}", Source, value, base.Unit));
            }
        }
    }
}