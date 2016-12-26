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

namespace OBDProject.Commands
{
    public class BasicCommand
    {
        public readonly byte[] Command;

        public event EventHandler<string> Response;

        public BasicCommand(byte[] command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command cannot by null!");
            }
            Command = command;
        }
        protected void OnResponse(string response)
        {
            var tempHandler = Response;
            tempHandler?.Invoke(this, response);
        }
    }
}