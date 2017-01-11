using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Collections.Generic;
using OBDProject.Utils;

namespace OBDProject.Activities
{
    [Activity(Label = "SelectDataToReadActivity")]
    public class SelectDataToReadActivity : Activity
    {
        public const string ActivityReturned = "SelectDataToReadActivity";


        private readonly List<string> _elementsToDisplay = new List<string>()
        {
          "Vehicle Speed",
          "Throttle Position",
          "Engine RPM",
          "Consuption Fuel Rate",
          "Fuel Level",
          "Fuel Pressure",
          "Fuel Type",
          "Engine Oil Temperature",
          "Engine Coolant Temperature"
        };

        private Button _confirmButton;
        private ListView _listView;
        private ArrayAdapter _arrayAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectDataToReadView);

            _arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemMultipleChoice);
            _listView = FindViewById<ListView>(Resource.Id.SelectDataToReadMultipleChoice);
            _confirmButton = FindViewById<Button>(Resource.Id.ConfirmButton);
            foreach (var element in _elementsToDisplay)
            {
                _arrayAdapter.Add(element);
            }
            _listView.Adapter = _arrayAdapter;
            _listView.ChoiceMode = ChoiceMode.Multiple;

            _confirmButton.Click += _confirmButton_Click;


            ActivityResults.ActivityClosed = "SelectDataToReadActivity";
        }

        private void _confirmButton_Click(object sender, System.EventArgs e)
        {
            Intent intent = new Intent();

            var selectedItemsIndexes = new List<int>();
            var tempCheckedItemPositions = _listView.CheckedItemPositions;
            for (int i = 0; i < tempCheckedItemPositions.Size(); i++)
            {
                selectedItemsIndexes.Add(tempCheckedItemPositions.KeyAt(i));
            }
            intent.PutExtra(ActivityResults.ActivityClosed, ActivityReturned);
            intent.PutExtra(ActivityResults.SelectedData, selectedItemsIndexes.ToArray());
            SetResult(Result.Ok, intent);

            Finish();
        }
    }
}