using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands.Temperature
{
    public class EngineOilTemperatureCommand : BasicCommand
    {
        public EngineOilTemperatureCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 5C\r"), socket, "°C", readFromDeviceLock, position, logManager)
        {
            Source = "Engine Oil Temperature";
        }

        protected override void PrepereFindResult()
        {
            string value = NoData;
            if (base.ReadedData.Any())
            {
                value = (base.ReadedData[2] - 40).ToString();
            }

            OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
        }
    }
}