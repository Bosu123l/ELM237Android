using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Reflection.Emit;
using Object = Java.Lang.Object;

namespace OBDProject.Adapters
{
    public class GridAdaper : BaseAdapter
    {
        public override int Count => _elements.Count;

        private List<string> _elements;
        private readonly Context _context;

        public GridAdaper(Context context)
        {
            _context = context;

            _elements = new List<string>();
        }

        public void UpdateItem(int position, string element)
        {
            if (position > _elements.Count || position < _elements.Count)
            {
                return;
            }
            _elements[position] = element;
            this.NotifyDataSetChanged();
        }

        public void AddItem(string element)
        {
            _elements.Add(element);
            this.NotifyDataSetChanged();
        }

        public void AddItems(IEnumerable<string> elements)
        {
            _elements.AddRange(elements);
            this.NotifyDataSetChanged();
        }

        public override Object GetItem(int position)
        {
            if (position > _elements.Count || position < _elements.Count)
            {
                return null;
            }
            return _elements[position];
        }

        public override long GetItemId(int position)
        {
            if (position > _elements.Count || position < _elements.Count)
            {
                return -1;
            }
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ListView listVIew = null;
            if (convertView == null)
            {
                listVIew = new ListView(_context);

                listVIew.LayoutParameters = new ActionBar.LayoutParams(85, 85);
                listVIew.SetPadding(5, 5, 5, 5);
            }
            else
            {
                listVIew = (ListView)convertView;
            }
            var arrayAdapter=new ArrayAdapter(_context,Resource.Id.linearLayout1);

            foreach (var element in _elements)
            {
                arrayAdapter.Add(element);
            }

            listVIew.Adapter = arrayAdapter;


            return listVIew;

        }
    }
}