using Android.Bluetooth;
using OBDProject.Utils;
using System.Text;

namespace OBDProject.Commands.CarStatus
{
    public class ClearTroubleCodesCommand : BasicCommand
    {
        public ClearTroubleCodesCommand(BluetoothSocket socket, object readFromDeviceLock, LogManager logManager) : base(Encoding.ASCII.GetBytes("04\r"), socket, readFromDeviceLock, logManager)
        {
        }

        protected override void PrepereFindResult()
        {
        }

        public void ClearCodes()
        {
            base.SendCommand();
        }
    }
}