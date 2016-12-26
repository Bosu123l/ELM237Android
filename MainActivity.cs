using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using OBDProject.Activities;
using OBDProject.Commands;
using OBDProject.Resources;
using OBDProject.Utils;
using System;
using System.Timers;


namespace OBDProject
{
    [Activity(Label = "OBDProject", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public const double INTERVAL = 500;

        private const int REQUEST_CONNECT_DEVICE = 1;
        private const int CannotConnect = 2;

        private BluetoothManager _bluetoothManager;
        private string _address;

        private Timer _timer;

        private bool _previouseConnectionState;

        private ListView _listView;

        private ArrayAdapter _arrayAdapter;
        private Button _clearButton;

        private VehicleSpeedCommand _speedCommand;
        private ThrottlePositionCommand _throttleCommand;
        private EngineRPMCommand _engineCommand;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _clearButton = FindViewById<Button>(Resource.Id.ClearButton);
            _listView = FindViewById<ListView>(Resource.Id.ElementyODB);

            _bluetoothManager = new BluetoothManager();
            _bluetoothManager.Connected += _bluetoothManager_Connected;
            _timer = new Timer(INTERVAL);
            _timer.Stop();
            _timer.Elapsed += _timer_Elapsed;

            _speedCommand = new VehicleSpeedCommand();
            _speedCommand.Response += _command_Response;

            _throttleCommand = new ThrottlePositionCommand();
            _throttleCommand.Response += _command_Response;

            _engineCommand = new EngineRPMCommand();
            _engineCommand.Response += _command_Response;

            _arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);

            //  _arrayAdapter = new ArrayAdapter<string>(Application.Context, Resource.Id.ElementyODB);

            _listView.Adapter = _arrayAdapter;

            _clearButton.Click += _clearButton_Click;
            // Set our view from the "main" layout resource
            // SetContentView (Resource.Layout.Main);
        }

        private void _command_Response(object sender, string e)
        {
            _arrayAdapter.Add(e);
        }

        private void _clearButton_Click(object sender, System.EventArgs e)
        {
            _arrayAdapter.Clear();
        }

        private  void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunOnUiThread(async () =>
                {
                    _speedCommand.ReadValue(await _bluetoothManager.GetDataFromOdb(_speedCommand.Command,"speed"));
                    _throttleCommand.ReadValue(await _bluetoothManager.GetDataFromOdb(_throttleCommand.Command,"throttle"));
                    _engineCommand.ReadValue(await _bluetoothManager.GetDataFromOdb(_engineCommand.Command,"engineRPM"));

                });
               }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void _bluetoothManager_Connected(object sender, bool e)
        {
            if (e)
            {
                Toast.MakeText(Application.Context, string.Format("Połaczono z {0}", _address), ToastLength.Long).Show();
                if (!_timer.Enabled)
                {
                    _timer.Start();
                }

            }
            else
            {
                if (_previouseConnectionState)
                {
                    Toast.MakeText(Application.Context, "Stracono Połaczenie", ToastLength.Long).Show();
                }
                else
                {
                    Toast.MakeText(Application.Context, "Brak połaczenia", ToastLength.Long).Show();
                }
            }
            _previouseConnectionState = e;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.scan:
                    var serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, 1);

                    return true;

                case Resource.Id.discoverable:

                    return true;

                default:
                    return false;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _address = data.Extras.GetString(DeviceListActivity.DeviceAddress);

            switch (requestCode)
            {
                case REQUEST_CONNECT_DEVICE:
                    {
                        if (resultCode == Result.Ok)
                        {
                            if (!string.IsNullOrEmpty(_address))
                            {
                                _bluetoothManager.Connect(_address);

                            }
                        }
                        break;
                    }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}