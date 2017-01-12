using Android.Bluetooth;
using OBDProject.Utils;
using System;
using System.Linq;
using System.Text;

namespace OBDProject.Commands
{
    public class EngineRPMCommand : BasicCommand
    {
        public EngineRPMCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 0C\r"), socket, "RPM", readFromDeviceLock, position, logManager)
        {
            //01	Show current data
            // 0C	2	Engine RPM	0	16,383.75	rpm	{\displaystyle {\frac {256A+B}{4}}} {\displaystyle {\frac {256A+B}{4}}}
            Source = "Engine RPM";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = (base.ReadedData[2] * 256f + base.ReadedData[3]) / 4;
                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));
            }
        }
    }
}