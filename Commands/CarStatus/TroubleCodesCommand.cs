using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OBDProject.Utils;

namespace OBDProject.Commands.CarStatus
{
    public class TroubleCodesCommand : BasicCommand
    {
        protected  static char[] DtcLetters = { 'P', 'C', 'B', 'U' };
        protected  static char[] HexArray = "0123456789ABCDEF".ToCharArray();
        public TroubleCodesCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("03\r"), socket, "RPM", readFromDeviceLock, position, logManager)
        {
        }

        protected override void PrepereFindResult()
        {
            throw new NotImplementedException();
        }
    }
}