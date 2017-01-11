using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OBDProject.Activities
{
    [Activity(Label = "SelectDataToReadActivity")]
    public class SelectDataToReadActivity : Activity
    {
        private ArrayAdapter _arrayAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectDataToReadView);
            _arrayAdapter= new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemMultipleChoice);

            

            // Create your application here
        }
    }
}