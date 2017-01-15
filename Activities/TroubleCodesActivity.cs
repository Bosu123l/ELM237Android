using Android.App;
using Android.OS;
using Android.Widget;
using OBDProject.Commands.CarStatus;
using OBDProject.Utils;
using System;
using System.Collections.Generic;
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
        private TroubleCodesCommand _troubleCodesCommand;
        private ClearTroubleCodesCommand _clearTroubleCodesCommand;
        private object _readFromDeviceLock;
        private LogManager _logManager;
        private ProgressDialog _progress;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TroubleCodesView);

            var bootstrap = Application as Bootstrap;
            if (bootstrap != null)
            {
                _bluetoothManager = bootstrap.BluetoothManager;
                _readFromDeviceLock = bootstrap.ReadFromDeviceLock;
                _logManager = bootstrap.LogManager;
            }
            _clearTroubleCodesCommand = new ClearTroubleCodesCommand(_bluetoothManager.Socket, _readFromDeviceLock,
                _logManager);
            _troubleCodesCommand = new TroubleCodesCommand(_bluetoothManager.Socket, _readFromDeviceLock, _logManager);
            _troubleCodesCommand.Response += _troubleCodesCommand_Response;

            await _troubleCodesCommand.ReadResult();

            _listView = FindViewById<ListView>(Resource.Id.TroubleList);

            _troubleCodes = new List<string>();

            _arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _troubleCodes);
            _listView.Adapter = _arrayAdapter;

            _clearButton = FindViewById<Button>(Resource.Id.RemoveFaultsButton);
            _clearButton.Click += _clearButton_Click;
            //_clearButton.Enabled = false;
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
        }

        private void _troubleCodesCommand_Response(object sender, string e)
        {
            _troubleCodes = new List<string>(e.Split('\n'));

         
            _troubleCodes = new List<string>()
            {
                "a",
                "b",
                "c",
                "d"
            }; //e.Split('\n').ToList();

            if (_progress != null)
            {
                _progress.Hide();
                _progress.Dispose();
            }

            _arrayAdapter.Clear();
            _arrayAdapter.AddAll(_troubleCodes);
            _arrayAdapter.NotifyDataSetChanged();
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
    }
}