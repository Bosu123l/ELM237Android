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