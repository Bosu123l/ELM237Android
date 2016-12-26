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

        public void ReadValue(string rowData)
        {
            OnResponse(string.Format("{0} km/H", rowData));
        }
    }
}