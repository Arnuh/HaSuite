/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HaCreator.CustomControls {
	public abstract class PropertyChangeNotifierBase : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected bool SetField<T>(ref T field, T value, string propertyName) {
			if (EqualityComparer<T>.Default.Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}

	public class HaListItem : PropertyChangeNotifierBase {
		private string caption;
		private object data;
		private bool selected;

		public HaListItem(string text, object data) {
			caption = text;
			this.data = data;
			selected = false;
		}

		public string Text {
			get => caption;
			set => SetField(ref caption, value, "Text");
		}

		public Brush Background => selected ? Brushes.LightBlue : Brushes.White;

		public object Tag => data;

		public bool Selected {
			get => selected;
			set => SetField(ref selected, value, "Background");
		}
	}

	public class HaListItemCollection : PropertyChangeNotifierBase {
		private ObservableCollection<HaListItem> items = new ObservableCollection<HaListItem>();

		public HaListItemCollection() {
		}

		public ObservableCollection<HaListItem> Items => items;
	}

	/// <summary>
	/// Interaction logic for HaList.xaml
	/// </summary>
	public partial class HaList : UserControl {
		private int selectedIndex = -1;
		private HaListItemCollection dataContext = new HaListItemCollection();

		public HaList() {
			InitializeComponent();
			DataContext = dataContext;
			PreviewKeyDown += HaList_PreviewKeyDown;
		}

		private void HaList_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Up) {
				if (IsEnabled && SelectedIndex != 0) SelectedIndex--;

				e.Handled = true;
			}
			else if (e.Key == Key.Down) {
				if (IsEnabled && SelectedIndex != Items.Count - 1) SelectedIndex++;

				e.Handled = true;
			}
		}

		public ObservableCollection<HaListItem> Items => dataContext.Items;

		public void ClearItems() {
			Items.Clear();
			selectedIndex = -1;
			((ScrollViewer) FindName("scrollView")).ScrollToVerticalOffset(0);
		}

		public int SelectedIndex {
			get => selectedIndex;
			set {
				if (value < 0 || value >= Items.Count)
					return;

				if (selectedIndex != -1)
					Items[selectedIndex].Selected = false;
				selectedIndex = value;
				Items[selectedIndex].Selected = true;

				// Make sure item is visible
				var sv = (ScrollViewer) FindName("scrollView");
				var ic = (ItemsControl) FindName("itemsCtrl");

				double h = 16;
				if (h * selectedIndex < sv.VerticalOffset)
					// Item is invisible above us
					sv.ScrollToVerticalOffset(h * selectedIndex);
				else if (h * (selectedIndex + 1) > sv.VerticalOffset + Height)
					// Item is invisible below us
					sv.ScrollToVerticalOffset(h * (selectedIndex + 1) - Height);

				if (SelectionChanged != null)
					SelectionChanged.Invoke(Items[selectedIndex], null);
			}
		}

		public object SelectedItem {
			get => selectedIndex == -1 ? null : Items[selectedIndex].Tag;
			set { SelectedIndex = Items.Select(x => x.Tag).ToList().IndexOf(value); }
		}

		private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			var tb = (TextBlock) sender;
			var item = (HaListItem) tb.DataContext;
			SelectedIndex = Items.IndexOf(item);
		}

		public void Scroll(int delta) {
			var sv = (ScrollViewer) FindName("scrollView");
			sv.ScrollToVerticalOffset(sv.VerticalOffset - delta);
		}

		public event SelectionChangedEventHandler SelectionChanged;
	}
}