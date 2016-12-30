using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using OBDProject.Activities;
using OBDProject.Commands;
using OBDProject.Commands.Fuel;
using OBDProject.Commands.Temperature;
using OBDProject.Utils;
using System;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;

namespace OBDProject
{
    [Activity(Label = "Android OBDII", MainLauncher = true, Icon = "@drawable/Auto")]
    public class MainActivity : Activity
    {
        public const double INTERVAL = 500;

        private object _readFromDeviceLock;

        private const int REQUEST_CONNECT_DEVICE = 1;
        private const int CannotConnect = 2;

        private BluetoothManager _bluetoothManager;
        private string _address;

        private Timer _timer;

        private bool _previouseConnectionState;

        private ListView _listView;

        private ArrayAdapter _arrayAdapter;
        private Button _clearButton;

        #region Commands

        private List<BasicCommand> _basicCommands;

        #endregion Commands

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

            _readFromDeviceLock = new object();

            _arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);

            _listView.Adapter = _arrayAdapter;

            _clearButton.Click += _clearButton_Click;

            _basicCommands = new List<BasicCommand>();
        }

        protected override void OnDestroy()
        {
            ClearCommandCollection();

            _bluetoothManager.Dispose();
            base.OnDestroy();
        }

        private void _command_Response(object sender, string e)
        {
            Log.Info("++++PRZETWORZONE!+++++", e);
            _arrayAdapter.Add(e);
        }

        private void _clearButton_Click(object sender, System.EventArgs e)
        {
            _arrayAdapter.Clear();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                RunOnUiThread(async () =>
                {
                    foreach (var basicCommand in _basicCommands)
                    {
                        await basicCommand.ReadResult();
                    }
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
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

        private void ClearCommandCollection()
        {
            try
            {
                foreach (var basicCommand in _basicCommands)
                {
                    basicCommand.Response -= _command_Response;
                }
                _basicCommands.Clear();
            }
            catch (Exception ex)
            {
                Log.Error("Cleaning Command Colection ERROR!", ex.Message);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case REQUEST_CONNECT_DEVICE:
                        {
                            _address = data.Extras.GetString(DeviceListActivity.DeviceAddress);

                            _bluetoothManager.Connect(_address);

                            ClearCommandCollection();

                            _basicCommands.Add(new VehicleSpeedCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new ThrottlePositionCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new EngineRPMCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new ConsuptionFuelRateCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new FuelLevelCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new FuelPressureCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new FuelTypeCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new EngineOilTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock));
                            _basicCommands.Add(new EngineCoolantTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock));

                            foreach (var basicCommand in _basicCommands)
                            {
                                basicCommand.Response += _command_Response;
                            }

                            break;
                        }
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}