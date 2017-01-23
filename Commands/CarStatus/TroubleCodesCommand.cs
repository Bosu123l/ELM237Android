using Android.Bluetooth;
using OBDProject.Utils;
using System.Text;
using System.Threading;

namespace OBDProject.Commands.CarStatus
{
    public class TroubleCodesCommand : TroubleCodesCommandBasic
    {
        public TroubleCodesCommand(BluetoothSocket socket, SemaphoreSlim semaphoreSlim,
            LogManager logManager)
            : base(Encoding.ASCII.GetBytes("03\r"), socket, semaphoreSlim, logManager)
        {
            Source = string.Empty;
        }
    }
}