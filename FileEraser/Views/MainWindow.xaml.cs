using FileEraser.Properties;
using FileEraser.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileEraser.Views
{
	/* 
	 * If some events were receive from ViewModel, then please use PropertyChangedWeakEventListener and CollectionChangedWeakEventListener.
	 * If you want to subscribe custome events, then you can use LivetWeakEventListener.
	 * When window closing and any timing, Dispose method of LivetCompositeDisposable is useful to release subscribing events.
	 *
	 * Those events are managed using WeakEventListener, so it is not occurred memory leak, but you should release explicitly.
	 */
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			LoadLocation();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			SaveLocation();
			base.OnClosing(e);
		}

		private void LoadLocation()
		{
			if (Settings.Default.WindowLocationLeft != 0 && Settings.Default.WindowLocationTop != 0 && Settings.Default.WindowLocationWidth != 0 && Settings.Default.WindowLocationHeight != 0) {
				Left = Settings.Default.WindowLocationLeft;
				Top = Settings.Default.WindowLocationTop;
				Width = Settings.Default.WindowLocationWidth;
				Height = Settings.Default.WindowLocationHeight;
			}
		}

		private void SaveLocation()
		{
			Settings.Default.WindowLocationLeft = (int)Left;
			Settings.Default.WindowLocationTop = (int)Top;
			Settings.Default.WindowLocationWidth = (int)Width;
			Settings.Default.WindowLocationHeight = (int)Height;
			Settings.Default.Save();
		}

		private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
		{
			((MainWindowViewModel)DataContext).DragOverCommand.Execute(e);
		}

		private void TextBox_PreviewDrop(object sender, DragEventArgs e)
		{
			((MainWindowViewModel)DataContext).DropCommand.Execute(e);
		}
	}
}
