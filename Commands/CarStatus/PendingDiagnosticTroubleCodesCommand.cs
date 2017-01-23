using Android.Bluetooth;
using OBDProject.Utils;
using System.Text;
using System.Threading;

namespace OBDProject.Commands.CarStatus
{
    public class PendingDiagnosticTroubleCodesCommand : TroubleCodesCommandBasic
    {
        public PendingDiagnosticTroubleCodesCommand(BluetoothSocket socket, SemaphoreSlim semaphoreSlim,
            LogManager logManager)
            : base(Encoding.ASCII.GetBytes("07\r"), socket, semaphoreSlim, logManager)
        {
            Source = string.Empty;
        }
    }
}