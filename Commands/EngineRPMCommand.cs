using System;
using System.Collections.Generic;
using System.Text;

namespace OBDProject.Commands
{
    public class EngineRPMCommand : BasicCommand
    {
        public EngineRPMCommand() : base(Encoding.ASCII.GetBytes("01 0C\r"))
        {
            //01	Show current data
            // 0C	2	Engine RPM	0	16,383.75	rpm	{\displaystyle {\frac {256A+B}{4}}} {\displaystyle {\frac {256A+B}{4}}}
        }

        public void ReadValue(List<int> data)
        {
            double value = 0;
            try
            {
                value = (data[2] * 256f + data[3]) / 4;
                OnResponse(string.Format("{0} RPM", value));
            }
            catch (Exception e)
            {
                OnResponse(string.Format("{0} RPM ", value));
            }
        }
    }
}