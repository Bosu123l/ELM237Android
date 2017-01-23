using Android.Bluetooth;
using OBDProject.Utils;
using System.Text;
using System.Threading;

namespace OBDProject.Commands.CarStatus
{
    internal class PermanentDiagnosticTroubleCodesCommand : TroubleCodesCommandBasic
    {
        public PermanentDiagnosticTroubleCodesCommand(BluetoothSocket socket, SemaphoreSlim semaphoreSlim,
            LogManager logManager)
            : base(Encoding.ASCII.GetBytes("0A\r"), socket, semaphoreSlim, logManager)
        {
            Source = string.Empty;
        }
    }
}