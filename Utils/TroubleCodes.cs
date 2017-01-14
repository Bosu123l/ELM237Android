using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OBDProject.Utils
{
    public class TroubleCodes
    {
        public const string AIR_INTAKE_TEMP = "Air Intake Temperature";
        public const string AMBIENT_AIR_TEMP = "Ambient Air Temperature";
        public const string ENGINE_COOLANT_TEMP = "Engine Coolant Temperature";
        public const string BAROMETRIC_PRESSURE = "Barometric Pressure";
        public const string FUEL_PRESSURE = "Fuel Pressure";
        public const string INTAKE_MANIFOLD_PRESSURE = "Intake Manifold Pressure";
        public const string ENGINE_LOAD = "Engine Load";
        public const string ENGINE_RUNTIME = "Engine Runtime";
        public const string ENGINE_RPM = "Engine RPM";
        public const string SPEED = "Vehicle Speed";
        public const string MAF = "Mass Air Flow";
        public const string THROTTLE_POS = "Throttle Position";
        public const string TROUBLE_CODES = "Trouble Codes";
        public const string PENDING_TROUBLE_CODES = "Pending Trouble Codes";
        public const string PERMANENT_TROUBLE_CODES = "Permanent Trouble Codes";
        public const string FUEL_LEVEL = "Fuel Level";
        public const string FUEL_TYPE = "Fuel Type";
        public const string FUEL_CONSUMPTION_RATE = "Fuel Consumption Rate";
        public const string TIMING_ADVANCE = "Timing Advance";
        public const string DTC_NUMBER = "Diagnostic Trouble Codes";
        public const string EQUIV_RATIO = "Command Equivalence Ratio";
        public const string DISTANCE_TRAVELED_AFTER_CODES_CLEARED = "Distance since codes cleared";
        public const string CONTROL_MODULE_VOLTAGE = "Control Module Power Supply ";
        public const string ENGINE_FUEL_RATE = "Engine Fuel Rate";
        public const string FUEL_RAIL_PRESSURE = "Fuel Rail Pressure";
        public const string VIN = "Vehicle Identification Number (VIN)";
        public const string DISTANCE_TRAVELED_MIL_ON = "Distance traveled with MIL on";
        public const string TIME_TRAVELED_MIL_ON = "Time run with MIL on";
        public const string TIME_SINCE_TC_CLEARED = "Time since trouble codes cleared";
        public const string REL_THROTTLE_POS = "Relative throttle position";
        public const string PIDS_01_20 = "Available PIDs 01-20";
        public const string PIDS_21_40 = "Available PIDs 21-40";
        public const string PIDS_41_60 = "Available PIDs 41-60";
        public const string ABS_LOAD = "Absolute load";
        public const string ENGINE_OIL_TEMP = "Engine oil temperature";
        public const string AIR_FUEL_RATIO = "Air/Fuel Ratio";
        public const string WIDEBAND_AIR_FUEL_RATIO = "Wideband Air/Fuel Ratio";
        public const string DESCRIBE_PROTOCOL = "Describe protocol";
        public const string DESCRIBE_PROTOCOL_NUMBER = "Describe protocol number";
        public const string IGNITION_MONITOR = "Ignition monitor";

    }
}