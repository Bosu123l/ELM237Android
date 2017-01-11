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
using System.Linq;
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

        private List<int> _indexesOfSelecedElements;

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
            RunOnUiThread(() =>
            {
                _arrayAdapter.Add(e);
            });
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
                case Resource.Id.selectCommand:
                    var selectDataIntent = new Intent(this, typeof(SelectDataToReadActivity));
                    StartActivityForResult(selectDataIntent, 1);

                    return true;

                case Resource.Id.scan:
                    var deviceListIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(deviceListIntent, 1);

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
            var resultFromActivity = data == null ? string.Empty : data.GetStringExtra(ActivityResults.ActivityClosed);

            if (resultCode == Result.Ok)
            {
                switch (resultFromActivity)
                {
                    case DeviceListActivity.ActivityReturned:
                        {
                            if (_indexesOfSelecedElements == null)
                            {
                                ShowAlert("Elements to display is not selected!");
                                return;
                            }
                            _address = data.Extras.GetString(ActivityResults.AddressOfSelectedDevice);

                            _bluetoothManager.Connect(_address);

                            ClearCommandCollection();
                            //"Vehicle Speed",
                            //Throttle Position
                            // "Engine RPM",
                            // "Consuption Fuel Rate",
                            // "Fuel Level",
                            // "Fuel Pressure",
                            // "Fuel Type",
                            // "Engine Oil Temperature",
                            // "Engine Coolant Temperature"
                            AddSelectedElements();

                            foreach (var basicCommand in _basicCommands)
                            {
                                basicCommand.Response += _command_Response;
                            }

                            break;
                        }
                    case SelectDataToReadActivity.ActivityReturned:
                        {
                            var selectedElements = data.GetIntArrayExtra(ActivityResults.SelectedData);
                            _indexesOfSelecedElements = selectedElements.ToList();
                        }
                        break;
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void AddSelectedElements()
        {
            if (_indexesOfSelecedElements.Contains(0))
            {
                _basicCommands.Add(new VehicleSpeedCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(1))
            {
                _basicCommands.Add(new ThrottlePositionCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(2))
            {
                _basicCommands.Add(new EngineRPMCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(3))
            {
                _basicCommands.Add(new ConsuptionFuelRateCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(4))
            {
                _basicCommands.Add(new FuelLevelCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(5))
            {
                _basicCommands.Add(new FuelPressureCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(6))
            {
                _basicCommands.Add(new FuelTypeCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(7))
            {
                _basicCommands.Add(new EngineOilTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
            if (_indexesOfSelecedElements.Contains(8))
            {
                _basicCommands.Add(new EngineCoolantTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock));
            }
        }

        public void ShowAlert(string str)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(str);
            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
                // write your own set of instructions
            });

            //run the alert in UI thread to display in the screen
            RunOnUiThread(() =>
            {
                alert.Show();
            });
        }
    }
}