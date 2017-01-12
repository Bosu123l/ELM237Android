using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    internal class EngineCoolantTemperatureCommand : BasicCommand
    {
        public EngineCoolantTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock, int position) : base(Encoding.ASCII.GetBytes("01 05\r"), socket, "°C", readFromDeviceLock, position)
        {
            Source = "EngineCoolantTemperatureCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = base.ReadedData[2] - 40;

                OnResponse(string.Format("{0} {1} {2}", Source, value, base.Unit));
            }
        }
    }
}