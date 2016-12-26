using System.Collections.Generic;
using OBDProject.Commands;
using System.Text;

namespace OBDProject.Resources
{
    public class VehicleSpeedCommand : BasicCommand
    {
        public VehicleSpeedCommand() : base(Encoding.ASCII.GetBytes("01 0D\r"))
        {
            //01	Show current data
            //0D	1	Vehicle speed	0	255	km/h    A
        }

        public void ReadValue(List<int> data)
        {
            OnResponse(string.Format("{0} km/H", data[2]));
        }
    }
}