using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Fuel
{
    internal class FuelPressureCommand : BasicCommand
    {
        public FuelPressureCommand(BluetoothSocket socket, object readFromDeviceLock) : base(Encoding.ASCII.GetBytes("01 0A\r"), socket, "kPa", readFromDeviceLock)
        {
            Source = "FuelPressureCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.readedData.Any())
            {
                var value = base.readedData[2] * 3;

                OnResponse(string.Format("{0} {1} {@}", Source, value, base.Unit));
            }
        }
    }
}