using System;
using System.Collections.Generic;
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

        public void ReadValue(List<int> data)
        {
            double value=0;
            try
            {

                value = ((double)100 * data[2]) / (double)255;
                OnResponse(string.Format("{0} %", value));
            }
            catch (Exception e)
            {
                OnResponse(string.Format("Error {0} %", value));
            }

        }
    }
}