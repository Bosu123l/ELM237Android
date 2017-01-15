﻿using System;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using OBDProject.Utils;

namespace OBDProject.Commands.Fuel
{
    public class FuelTypeCommand : BasicCommand
    {
        public FuelTypeCommand(BluetoothSocket socket, object readFromDeviceLock, int position, LogManager logManager) : base(Encoding.ASCII.GetBytes("01 51\r"), socket, " ", readFromDeviceLock, position, logManager)
        {
            Source = "Fuel Type";
        }

        protected override void PrepereFindResult()
        {
            if (base.ReadedData.Any())
            {
                var value = (FuelType)base.ReadedData[2];
                OnResponse(string.Format("{0}{1}{2} {3}", Source, Environment.NewLine, value, base.Unit));

            }
        }
    }
}