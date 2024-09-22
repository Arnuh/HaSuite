using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MapleLib.WzLib;
using Microsoft.Win32;

namespace CanvasHelper {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void BrowseFolder(object sender, RoutedEventArgs e) {
			var folderDialog = new OpenFolderDialog();

			if (folderDialog.ShowDialog() != true) {
				return;
			}

			if (sender is not Button button) {
				return;
			}

			if (button.Name.Equals("InputFolderButton")) {
				InputFolder.Text = folderDialog.FolderName;
			} else {
				OutputFolder.Text = folderDialog.FolderName;
			}
		}

		private void FixCanvas(object sender, RoutedEventArgs e) {
			var outPath = OutputFolder.Text;
			Directory.CreateDirectory(outPath);

			var canvasFiles = CanvasHelp.GetCanvasFiles(InputFolder.Text);
			if (canvasFiles.Count == 0) {
				MessageBox.Show("No canvas files found in the input folder");
				return;
			}

			var mainFiles = CanvasHelp.GetMainFiles(Directory.GetParent(InputFolder.Text).FullName);

			var bw = new BackgroundWorker();

			// this allows our worker to report progress during work
			bw.WorkerReportsProgress = true;

			// what to do in the background thread
			bw.DoWork += delegate(object o, DoWorkEventArgs args) {
				var b = o as BackgroundWorker;

				var index = 0;
				try {
					foreach (var canvas in canvasFiles) {
						var newFile = new WzFile(canvas.Version, canvas.MapleVersion);
						foreach (var canvasImg in canvas.WzDirectory.WzImages) {
							var newImg = new WzImage(canvasImg.Name);
							foreach (var orgImg in mainFiles.SelectMany(file => file.WzDirectory.WzImages)) {
								if (canvasImg.Name == orgImg.Name) {
									CanvasHelp.MergeImage(canvasFiles, newImg, orgImg);
								}
							}

							newFile.WzDirectory.AddImage(newImg);
						}

						newFile.SaveToDisk(Path.Combine(outPath, canvas.Name));
						b.ReportProgress(++index);
					}
				} catch (Exception ex) {
					Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
					MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}");
				}
			};

			bw.ProgressChanged += delegate(object o, ProgressChangedEventArgs args) { Status.Text = $"{args.ProgressPercentage}/{canvasFiles.Count}"; };

			bw.RunWorkerCompleted += delegate { Status.Text = "Done"; };

			bw.RunWorkerAsync();
		}
	}
}