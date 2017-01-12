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
    [Activity(Label = "Readed data:", Theme = "@style/MyCustomTheme", MainLauncher = true, Icon = "@drawable/Auto")]
    public class MainActivity : Activity
    {
        private LogManager _logManager;

        private const double _interval = 500;
        private object _readFromDeviceLock;
        private BluetoothManager _bluetoothManager;
        private string _address;
        private Timer _timer;
        private bool _previouseConnectionState;

        private GridView _gridView;
        private ArrayAdapter _arrayAdapter;
        private Button _clearButton;

        private List<int> _indexesOfSelectedElements;
        private List<string> _dataFromSelectedElements;

        #region Commands

        private List<BasicCommand> _basicCommands;

        #endregion Commands

        private int _tempCounter;
        private int _tempCounterForIndex;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _clearButton = FindViewById<Button>(Resource.Id.refreshButton);
            _gridView = FindViewById<GridView>(Resource.Id.ElementyODB);

            _bluetoothManager = new BluetoothManager();
            _bluetoothManager.Connected += _bluetoothManager_Connected;
            _timer = new Timer(_interval);
            _timer.Stop();
            _timer.Elapsed += _timer_Elapsed;

            _readFromDeviceLock = new object();

            _logManager = new LogManager();

            _dataFromSelectedElements = new List<string>();

            _arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.TextViewRow, _dataFromSelectedElements);

            _gridView.Adapter = _arrayAdapter;

            _gridView.NumColumns = 2;

            _clearButton.Click += _clearButton_Click;

            _basicCommands = new List<BasicCommand>();
        }

        protected override void OnDestroy()
        {
            ClearCommandCollection();

            _bluetoothManager.Dispose();
            _logManager.SaveLogFile();
            base.OnDestroy();

            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        private void _command_Response(object sender, string e)
        {
            if (_indexesOfSelectedElements == null || _indexesOfSelectedElements.Count.Equals(0))
            {
                ShowAlert("Elements to display is not selected!");
                return;
            }

            _logManager.ReadedDataWriteLine(e);
            RunOnUiThread(() =>
                {
                    if (_dataFromSelectedElements.Count >= _indexesOfSelectedElements.Count)
                    {
                        var tempBasicCommand = sender as BasicCommand;
                        if (tempBasicCommand != null)
                        {
                            int index = tempBasicCommand.Position;
                            _dataFromSelectedElements[index] = e;
                            if (e.Contains("Speed"))
                            {
                                showNotifaction(e);
                            }
                        }
                        if (_tempCounterForIndex >= _indexesOfSelectedElements.Count)
                        {
                            _tempCounterForIndex = 0;
                        }

                        //_dataFromSelectedElements[_tempCounterForIndex++] = string.Format("{0}{1}{2}", "otrzymano",
                        //    System.Environment.NewLine, _tempCounter++);
                        //LogManager.ReadedDataWriteLine(string.Format("{0}{1}{2}", "otrzymano",
                        //    System.Environment.NewLine, _tempCounter));
                        //showNotifaction(string.Format("{0}{1}{2}", "otrzymano",
                        //    System.Environment.NewLine, _tempCounter));

                    }
                    else
                    {
                        _dataFromSelectedElements.Add(e);
                        // _dataFromSelectedElements.Add("początek");
                    }
                    _arrayAdapter.Clear();
                    _arrayAdapter.AddAll(_dataFromSelectedElements);
                    _arrayAdapter.NotifyDataSetChanged();
                });
        }

        private void showNotifaction(string data)
        {
            // Instantiate the builder and set notification elements:
            Notification.Builder builder = new Notification.Builder(this)
                .SetContentTitle("Sample Notification")
                .SetContentText(data)
                .SetSmallIcon(Resource.Drawable.Auto);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);
        }

        private void _clearButton_Click(object sender, System.EventArgs e)
        {
            _arrayAdapter.Clear();
            //_command_Response(this, nameof(_arrayAdapter));
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
                _logManager.InfoWriteLine(string.Format("Połaczono z {0}", _address));
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
                    _logManager.WarringWriteLine("Stracono Połaczenie");
                }
                else
                {
                    Toast.MakeText(Application.Context, "Brak połaczenia", ToastLength.Long).Show();
                    _logManager.WarringWriteLine("Brak połaczenia");
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
                case Resource.Id.closeApplication:
                    this.FinishAffinity();

                    return true;

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
            _dataFromSelectedElements.Clear();
            _arrayAdapter.Clear();
            _arrayAdapter.NotifyDataSetChanged();
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
                _logManager.ErrorWriteLine("Cleaning Command Colection ERROR!");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            var resultFromActivity = data == null ? string.Empty : data.GetStringExtra(ActivityResults.ActivityClosed);

            if (resultCode == Result.Ok)
            {
                ClearCommandCollection();
                switch (resultFromActivity)
                {
                    case DeviceListActivity.ActivityReturned:
                        {
                            if (_indexesOfSelectedElements == null)
                            {
                                ShowAlert("Elements to display is not selected!");
                                return;
                            }
                            _address = data.Extras.GetString(ActivityResults.AddressOfSelectedDevice);

                            _bluetoothManager.Connect(_address);

                            ClearCommandCollection();
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
                            _indexesOfSelectedElements = selectedElements.ToList();
                        }
                        break;
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void AddSelectedElements()
        {
            int position = 0;
            if (_indexesOfSelectedElements.Contains(0))
            {
                _basicCommands.Add(new VehicleSpeedCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(1))
            {
                _basicCommands.Add(new ThrottlePositionCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(2))
            {
                _basicCommands.Add(new EngineRPMCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(3))
            {
                _basicCommands.Add(new ConsuptionFuelRateCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(4))
            {
                _basicCommands.Add(new FuelLevelCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(5))
            {
                _basicCommands.Add(new FuelPressureCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(6))
            {
                _basicCommands.Add(new FuelTypeCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(7))
            {
                _basicCommands.Add(new EngineOilTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
            }
            if (_indexesOfSelectedElements.Contains(8))
            {
                _basicCommands.Add(new EngineCoolantTemperatureCommand(_bluetoothManager.Socket, _readFromDeviceLock, position++, _logManager));
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