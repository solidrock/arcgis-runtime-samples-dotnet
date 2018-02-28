// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Shared.Models;
using System.Collections.Generic;

namespace ArcGISRuntime
{
    /// <summary>
    /// Custom ArrayAdapter to display the list of Categories.
    /// </summary>
    internal class CategoriesAdapter : BaseAdapter<SearchableTreeNode>
    {
        private readonly List<SearchableTreeNode> _items;
        private readonly Activity _context;

        public CategoriesAdapter(Activity context, List<SearchableTreeNode> items)
        {
            this._items = items;
            this._context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override SearchableTreeNode this[int position] => _items[position];

        public override int Count => _items.Count;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = _context.LayoutInflater.Inflate(Resource.Layout.CategoriesLayout, parent, false);

            var name = view.FindViewById<TextView>(Resource.Id.nameTextView);

            name.Text = _items[position].Name;

            return view;
        }
    }
}