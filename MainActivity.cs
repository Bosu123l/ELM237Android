using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using OBDProject.Activities;
using OBDProject.Commands;
using OBDProject.Commands.CarStatus;
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
    [Activity(Label = "OBDProject", Theme = "@style/MyCustomTheme", MainLauncher = true, Icon = "@drawable/Auto")]
    public class MainActivity : Activity
    {
        private LogManager _logManager;

        private const double _interval = 100;
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

        private TroubleCodesCommand _troubleCodesCommand;
        private List<BasicCommand> _basicCommands;

        #endregion Commands

        private int _tempCounter;
        private int _tempCounterForIndex;

        private ProgressDialog _progress;
        private List<string> _sourceNames = new List<string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _clearButton = FindViewById<Button>(Resource.Id.refreshButton);
            _gridView = FindViewById<GridView>(Resource.Id.ElementyODB);

            var bootstrap = Application as Bootstrap;
            if (bootstrap != null)
            {
                _bluetoothManager = bootstrap.BluetoothManager;
                _readFromDeviceLock = bootstrap.ReadFromDeviceLock;
                _logManager = bootstrap.LogManager;
            }

            ActionBar.SetLogo(Resource.Drawable.noConnection);
            ActionBar.SetDisplayUseLogoEnabled(true);
            ActionBar.Show();

            _bluetoothManager.Connected -= _bluetoothManager_Connected;
            _bluetoothManager.Connected += _bluetoothManager_Connected;

            _timer = new Timer(_interval);
            _timer.Stop();
            _timer.Elapsed += _timer_Elapsed;

            _dataFromSelectedElements = new List<string>();

            _arrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.TextViewRow, _dataFromSelectedElements);

            _gridView.Adapter = _arrayAdapter;

            _gridView.NumColumns = 2;

            _clearButton.Click += _clearButton_Click;

            _progress = new ProgressDialog(this);

            _basicCommands = new List<BasicCommand>();

            ShowNotifaction("No connection with device.", Resource.Drawable.noConnection);
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
                var temp = e.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
                if (temp.Length > 0)
                {
                    if (!_dataFromSelectedElements.Any(x => x.Contains(temp.First())))
                    {
                        _dataFromSelectedElements.Add(e);
                        UpdateList();
                        return;
                    }
                }

                var tempBasicCommand = sender as BasicCommand;
                if (tempBasicCommand != null)
                {
                    int index = tempBasicCommand.Position;
                    if (index >= _dataFromSelectedElements.Count)
                    {
                        return;
                    }
                    _dataFromSelectedElements[index] = e;
                }
                if (_tempCounterForIndex >= _indexesOfSelectedElements.Count)
                {
                    _tempCounterForIndex = 0;
                }

                UpdateList();
            });
        }

        private void UpdateList()
        {
            _arrayAdapter.Clear();
            _arrayAdapter.AddAll(_dataFromSelectedElements);
            _arrayAdapter.NotifyDataSetChanged();
        }

        private void ShowNotifaction(string data, int iconId)
        {
            // Instantiate the builder and set notification elements:
            Notification.Builder builder = new Notification.Builder(this)
                .SetContentTitle("Connection Status")
                .SetContentText(data)
                .SetSmallIcon(iconId);

            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            if (notificationManager != null) notificationManager.Notify(notificationId, notification);
        }

        private void _clearButton_Click(object sender, System.EventArgs e)
        {
            //if (_basicCommands.Count == 0)
            //{
            //    AddSelectedElements();
            //}

            //var rand = new Random();
            //_command_Response(this, string.Format("{0} {1} {2} {3}", _basicCommands[rand.Next(0, _indexesOfSelectedElements.Count)].Source, System.Environment.NewLine, "test",_tempCounterForIndex++));

            _arrayAdapter.Clear();
            _sourceNames.Clear();
            _dataFromSelectedElements.Clear();
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
            base.OnCreateOptionsMenu(menu);

            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.TroubleCodes:
                    if (_timer.Enabled)
                    {
                        _timer.Start();
                    }
                    var troubleCodesIntent = new Intent(this, typeof(TroubleCodesActivity));
                    StartActivityForResult(troubleCodesIntent, 1);
                    return true;

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

        private void ShowProgressBar(string dialogMessage, string toastMessage)
        {
            _progress.Indeterminate = true;
            _progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            _progress.SetMessage(dialogMessage);
            _progress.SetCancelable(false);

            RunOnUiThread(() =>
            {
                _progress.Show();
                RunOnUiThread(() => Toast.MakeText(this, toastMessage, ToastLength.Long).Show());
            });
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

                            ShowNotifaction(string.Format("Connected with {0}", data.Extras.GetString(ActivityResults.DeviceName)), Resource.Drawable.Connection);

                            ClearCommandCollection();
                            AddSelectedElements();

                            foreach (var basicCommand in _basicCommands)
                            {
                                basicCommand.Response += _command_Response;
                                _sourceNames.Add(basicCommand.Source);
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