using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    internal class EngineCoolantTemperatureCommand : BasicCommand
    {
        public EngineCoolantTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock) : base(Encoding.ASCII.GetBytes("01 05\r"), socket, "°C", readFromDeviceLock)
        {
            Source = "EngineCoolantTemperatureCommand";
        }

        protected override void PrepereFindResult()
        {
            if (base.readedData.Any())
            {
                var value = base.readedData[2] - 40;

                OnResponse(string.Format("{0} {1} {2}", Source, value, base.Unit));
            }
        }
    }
}