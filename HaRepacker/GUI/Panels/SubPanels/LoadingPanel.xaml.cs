using System.ComponentModel;
using System.Windows;
using WpfAnimatedGif;
using UserControl = System.Windows.Controls.UserControl;

namespace HaRepacker.GUI.Panels.SubPanels {
	/// <summary>
	/// Interaction logic for LoadingPanel.xaml
	/// </summary>
	public partial class LoadingPanel : UserControl, INotifyPropertyChanged {
		private ImageAnimationController imageController;

		public LoadingPanel() {
			InitializeComponent();

			DataContext = this; // set data binding to self.
		}


		/// <summary>
		/// Loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ImageLoadingGif_AnimationLoaded(object sender, RoutedEventArgs e) {
			imageController = ImageBehavior.GetAnimationController(imageLoadingGif);
		}

		/// <summary>
		/// 
		/// </summary>
		public void OnStartAnimate() {
			//imageController.Play(); // doesnt animate when Visibility is collapsed anyway.
		}

		/// <summary>
		/// 
		/// </summary>
		public void OnPauseAnimate() {
			//imageController.Pause();
		}

		/// <summary>
		/// Sets the visibility for the stats on wzIv bruteforcing
		/// </summary>
		/// <param name="visibility"></param>
		public void SetWzIvBruteforceStackpanelVisiblity(Visibility visibility) {
			stackPanel_wzIvBruteforceStat.Visibility = visibility;
		}

		#region Exported Fields

		private ulong _WzIvKeyTries;

		public ulong WzIvKeyTries {
			get => _WzIvKeyTries;
			set {
				_WzIvKeyTries = value;
				OnPropertyChanged("WzIvKeyTries");
			}
		}

		private long _WzIvKeyDuration; // number of ticks

		public long WzIvKeyDuration {
			get => _WzIvKeyDuration;
			set {
				_WzIvKeyDuration = value;
				OnPropertyChanged("WzIvKeyDuration");
			}
		}

		#endregion


		#region PropertyChanged

		/// <summary>
		/// Property changed event handler to trigger update UI
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) {
			var handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}