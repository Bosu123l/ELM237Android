using Android.App;
using Android.Bluetooth;
using Android.Content;
using OBDProject.Activities;

namespace OBDProject.Utils
{
    public class Receiver : BroadcastReceiver
    {
        private readonly Activity _chat;

        public Receiver(Activity chat)
        {
            _chat = chat;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            string action = intent.Action;

            // When discovery finds a device
            if (action == BluetoothDevice.ActionFound)
            {
                // Get the BluetoothDevice object from the Intent
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                // If it's already paired, skip it, because it's been listed already
                if (device.BondState != Bond.Bonded)
                {
                    DeviceListActivity.NewDevicesArrayAdapter.Add(string.Format("{0}{1}{2}", device.Name, System.Environment.NewLine, device.Address));
                }
                // When discovery is finished, change the Activity title
            }
            else if (action == BluetoothAdapter.ActionDiscoveryFinished)
            {
                _chat.SetProgressBarIndeterminateVisibility(false);
                _chat.SetTitle(Resource.String.select_device);
                if (DeviceListActivity.NewDevicesArrayAdapter.Count == 0)
                {
                    var noDevices = _chat.Resources.GetText(Resource.String.none_found).ToString();
                    DeviceListActivity.NewDevicesArrayAdapter.Add(noDevices);
                }
            }
        }
    }
}