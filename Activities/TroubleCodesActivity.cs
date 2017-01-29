using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using OBDProject.Commands.CarStatus;
using OBDProject.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OBDProject.Activities
{
    [Activity(Label = "Bluethooth Device list:", Icon = "@drawable/Auto", Theme = "@style/MyCustomTheme")]
    public class TroubleCodesActivity : Activity
    {
        private List<string> _troubleCodes;
        private ArrayAdapter<string> _arrayAdapter;
        private ListView _listView;
        private Button _clearButton;
        private BluetoothManager _bluetoothManager;

        private ClearTroubleCodesCommand _clearTroubleCodesCommand;
        private object _readFromDeviceLock;
        private LogManager _logManager;
        private ProgressDialog _progress;

        private TroubleCodesCommand _troubleCodesCommand;
        private PendingDiagnosticTroubleCodesCommand _diagnosticTroubleCodesCommand;
        private PermanentDiagnosticTroubleCodesCommand _permanentDiagnosticTroubleCodesCommand;

        private SemaphoreSlim _semaphoreSlim;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TroubleCodesView);

            var bootstrap = Application as Bootstrap;
            if (bootstrap != null)
            {
                _bluetoothManager = bootstrap.BluetoothManager;
                if (_bluetoothManager.Socket == null || !_bluetoothManager.Socket.IsConnected)
                {
                    ShowAlert("No connection with bluetooth device!");
                }
                _readFromDeviceLock = bootstrap.ReadFromDeviceLock;
                _logManager = bootstrap.LogManager;
            }

            _listView = FindViewById<ListView>(Resource.Id.TroubleList);

            _troubleCodes = new List<string>();

            _arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _troubleCodes);
            _listView.Adapter = _arrayAdapter;

            _semaphoreSlim = new SemaphoreSlim(1);

            _progress = new ProgressDialog(this);

            _clearTroubleCodesCommand = new ClearTroubleCodesCommand(_bluetoothManager.Socket, _readFromDeviceLock,
                _logManager);
            _troubleCodesCommand = new TroubleCodesCommand(_bluetoothManager.Socket, _semaphoreSlim, _logManager);
            _diagnosticTroubleCodesCommand = new PendingDiagnosticTroubleCodesCommand(_bluetoothManager.Socket, _semaphoreSlim, _logManager);
            _permanentDiagnosticTroubleCodesCommand = new PermanentDiagnosticTroubleCodesCommand(_bluetoothManager.Socket, _semaphoreSlim, _logManager);

            _troubleCodesCommand.Response += _troubleCodesCommand_Response;
            _diagnosticTroubleCodesCommand.Response += _troubleCodesCommand_Response;
            _permanentDiagnosticTroubleCodesCommand.Response += _troubleCodesCommand_Response;

            await _troubleCodesCommand.ReadResult();
            await _diagnosticTroubleCodesCommand.ReadResult();
            await _permanentDiagnosticTroubleCodesCommand.ReadResult();

            _clearButton = FindViewById<Button>(Resource.Id.RemoveFaultsButton);
            _clearButton.Click += _clearButton_Click;
            //_clearButton.Enabled = false;
        }

        private void UpdateList()
        {
            _arrayAdapter.Clear();
            _arrayAdapter.AddAll(_troubleCodes);
            _arrayAdapter.NotifyDataSetChanged();
        }

        private async void _clearButton_Click(object sender, EventArgs e)
        {

            try
            {
                _clearTroubleCodesCommand.ClearCodes();

                await _troubleCodesCommand.ReadResult();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Application.Context, "Brak po³aczenia z urz¹dzeniem!", ToastLength.Long).Show();
                _logManager.ErrorWriteLine(ex.Message);
            }

            await _troubleCodesCommand.ReadResult();
            await _diagnosticTroubleCodesCommand.ReadResult();
            await _permanentDiagnosticTroubleCodesCommand.ReadResult();

            ShowProgressBar("Refreshing Trouble Codes", "Refreshing Trouble Codes");
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu_trouble_Activity, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.refreshTroubleCode:
                    {
                        _troubleCodes.Clear();
                        Task.Run(async () =>
                        {
                            await _troubleCodesCommand.ReadResult();
                            await _diagnosticTroubleCodesCommand.ReadResult();
                            await _permanentDiagnosticTroubleCodesCommand.ReadResult();
                        });

                        ShowProgressBar("Refreshing Trouble Codes", "Refreshing Trouble Codes");

                        return true;
                    }
                case Resource.Id.BackTroubleCode:
                    {
                        Finish();
                        return true;
                    }
                default:
                    return false;
            }
        }

        private void _troubleCodesCommand_Response(object sender, string e)
        {
            if (string.IsNullOrEmpty(e))
            {
                return;
            }
            RunOnUiThread(() =>
            {
                var tempErrorCodes = e;
                try
                {
                    _progress.Cancel();
                 
                }
                catch (Exception ex)
                {
                    ;
                }

                _troubleCodes.AddRange(new List<string>(tempErrorCodes.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None)));
                for (int i = 0; i < _troubleCodes.Count; i++)
                {
                    if (string.IsNullOrEmpty(_troubleCodes[i]))
                    {
                        _troubleCodes.RemoveAt(i);
                    }
                }
                UpdateList();

                if (_progress != null)
                {
                    try
                    {
                        _progress.Hide();
                        _progress.Dispose();
                    }
                    catch (Exception exception)
                    {
                        ;
                    }
                   
                }

               
            });
           
        }

        private void ShowProgressBar(string dialogMessage, string toastMessage)
        {
            _progress = new ProgressDialog(this);
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

        public void ShowAlert(string str)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(str);
            alert.SetCancelable(false);
            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
                Finish();
            });

            //run the alert in UI thread to display in the screen
            RunOnUiThread(() =>
            {
                alert.Show();
            });
        }
    }
}