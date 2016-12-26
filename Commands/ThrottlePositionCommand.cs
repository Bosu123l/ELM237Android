using System;
using System.Text;

namespace OBDProject.Commands
{
    public class ThrottlePositionCommand : BasicCommand
    {
        public ThrottlePositionCommand() : base(Encoding.ASCII.GetBytes("01 11\r"))
        {
            //01	Show current data
            //11	1	Throttle position	0	100	 %	(100/255)*A
        }

        public void ReadValue(string rowData)
        {
            double value = Convert.ToSingle(rowData);

            value = ((double)100 * value) / (double)255;

            OnResponse(string.Format("{0} %", value));
        }
    }
}