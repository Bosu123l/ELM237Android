using Android.Bluetooth;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    public class EngineOilTemperatureCommand : BasicCommand
    {
        public EngineOilTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock) : base(Encoding.ASCII.GetBytes("01 5C\r"), socket, "°C", readFromDeviceLock)
        {
            Source = "EngineOilTemperatureCommand";
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