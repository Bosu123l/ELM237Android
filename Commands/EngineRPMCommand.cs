using System;
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

        public void ReadValue(string rowData)
        {
            double value = Convert.ToSingle(rowData);

            OnResponse(string.Format("{0} RPM NOT SCALED!!!", value));
        }
    }
}