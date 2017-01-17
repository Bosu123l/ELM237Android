using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using OBDProject.Utils;
using System;
using System.Linq;

namespace OBDProject.Activities
{
    [Activity(Label = "Bluethooth Device list:", Icon = "@drawable/Auto", Theme = "@style/MyCustomTheme")]
    public class DeviceListActivity : Activity
    {
        public const string ActivityReturned = "DeviceListActivity";
        private const string TAG = "DeviceListActivity";
        public const string ConnectedStatus = "deviceConnectionStatus";
        public const string DeviceAddress = "deviceAddress";

        public static ArrayAdapter<string> PairedDevicesArrayAdapter;
        public static ArrayAdapter<string> NewDevicesArrayAdapter;

        public string DeviceName
        {
            get; private set;
        }

        private Receiver _receiver;

        private BluetoothAdapter _bluetoothAdapter;
        private Button _findButton;
        private bool _connecting;
        private ListView _newDevicesListView;
        private ListView _pairedListView;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            _connecting = false;
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.device_list);

            SetResult(Result.Canceled);



            _findButton = FindViewById<Button>(Resource.Id.button_scan);

            PairedDevicesArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            NewDevicesArrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);

            _pairedListView = FindViewById<ListView>(Resource.Id.paired_devices);
            _pairedListView.Adapter = PairedDevicesArrayAdapter;
            _newDevicesListView = FindViewById<ListView>(Resource.Id.new_devices);
            _newDevicesListView.Adapter = NewDevicesArrayAdapter;

            _receiver = new Receiver(this);
            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            RegisterReceiver(_receiver, filter);

            filter = new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished);
            RegisterReceiver(_receiver, filter);

            // Get the local Bluetooth adapter
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // Get a set of currently paired devices
            var pairedDevices = _bluetoothAdapter.BondedDevices;

            // If there are paired devices, add each one to the ArrayAdapter
            if (pairedDevices.Count > 0)
            {
                FindViewById<View>(Resource.Id.title_paired_devices).Visibility = ViewStates.Visible;

                foreach (var device in pairedDevices)
                {
                    PairedDevicesArrayAdapter.Add(string.Format("{0}{1}{2}", device.Name, System.Environment.NewLine, device.Address));
                }
            }
            else
            {
                string noDevices = Resources.GetText(Resource.String.none_paired);
                PairedDevicesArrayAdapter.Add(noDevices);
            }

            _newDevicesListView.ItemClick += NewDevices_ItemClick;
            _pairedListView.ItemClick += NewDevices_ItemClick;
            _findButton.Click += _findButton_Click;

        }



        private void NewDevices_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            Intent intent = new Intent();

            var address = string.Empty;

            // Simulate some work here in order for the progress spinner to update

            _bluetoothAdapter.CancelDiscovery();

            // Get the device MAC address, which is the last 17 chars in the View
            var info = (e.View as TextView).Text.ToString();
            address = info.Substring(info.Length - 17);

            var singleOrDefault = _bluetoothAdapter.BondedDevices.SingleOrDefault(x => x.Address.Contains(address));
            if (singleOrDefault != null)
                DeviceName = singleOrDefault.Name;

            intent.PutExtra(ActivityResults.DeviceName, DeviceName);
            intent.PutExtra(ActivityResults.ActivityClosed, ActivityReturned);
            intent.PutExtra(ActivityResults.AddressOfSelectedDevice, address);
            SetResult(Result.Ok, intent);

            Finish();
        }

        private void _findButton_Click(object sender, EventArgs e)
        {
            DoDiscovery();
            (sender as View).Visibility = ViewStates.Gone;
        }

        protected override void OnDestroy()
        {
            // Make sure we're not doing discovery anymore
            if (_bluetoothAdapter != null)
            {
                _bluetoothAdapter.CancelDiscovery();
            }

            // Unregister broadcast listeners
            UnregisterReceiver(_receiver);

            base.OnDestroy();
        }

        private void DoDiscovery()
        {
            // Indicate scanning in the title
            SetProgressBarIndeterminateVisibility(true);
            SetTitle(Resource.String.scanning);

            // Turn on sub-title for new devices
            FindViewById<View>(Resource.Id.title_new_devices).Visibility = ViewStates.Visible;

            // If we're already discovering, stop it
            if (_bluetoothAdapter.IsDiscovering)
            {
                _bluetoothAdapter.CancelDiscovery();
            }

            // Request discover from BluetoothAdapter
            _bluetoothAdapter.StartDiscovery();
        }
    }
}