using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using PdfImageExtracter.Models;
using PdfImageExtracter.Properties;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfImageExtracter.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		readonly PdfImageExtracterModel model = new();

		public ReadOnlyReactiveProperty<bool> CanExecute { get; }
		public ReadOnlyReactiveProperty<bool> ResizeEnable { get; }

		// Drag & Drop
		public ReactiveCommand<DragEventArgs> DragOverCommand { get; } = new();
		public AsyncReactiveCommand<DragEventArgs> DropCommand { get; }

		// 設定
		public ReactiveCommand FolderSelectCommand { get; } = new();
		public ReactivePropertySlim<string> OutputFolder { get; } = new();
		public ReactivePropertySlim<bool> MakeSubFolder { get; } = new();
		public ReactivePropertySlim<bool> ResizeImage { get; } = new();
		public ReactivePropertySlim<int> ResizeMode { get; } = new();
		public ReactivePropertySlim<int> ResizePixel { get; } = new();

		// StatusBar
		public ReadOnlyReactiveProperty<string> StatusMessage { get; }
		public ReadOnlyReactiveProperty<int> ProgressMax { get; }
		public ReadOnlyReactiveProperty<int> Progress { get; }
		public ReadOnlyReactivePropertySlim<Visibility> ProgressBarVisibility { get; }

		// 終了処理
		public ReactiveProperty<bool> CanClose { get; } = new(false);
		public AsyncReactiveCommand CloseCanceledCallbackCommand { get; } = new();

		public MainWindowViewModel()
		{
			CanExecute = model.CanExecute.ToReadOnlyReactiveProperty();
			ResizeEnable = Observable.CombineLatest(CanExecute, ResizeImage, (x, y) => x & y).ToReadOnlyReactiveProperty();

			DragOverCommand.Subscribe(e => {
				if (CanExecute.Value && e.Data.GetDataPresent(DataFormats.FileDrop)) {
					e.Effects = DragDropEffects.Copy;
				} else {
					e.Effects = DragDropEffects.None;
				}
				e.Handled = true;
			});

			DropCommand = CanExecute.ToAsyncReactiveCommand<DragEventArgs>().WithSubscribe(async e => {
				if (e.Data.GetData(DataFormats.FileDrop) is not string[] list || list.Length == 0) {
					return;
				}
				PdfImageExtracterModel.ResizeMode mode = PdfImageExtracterModel.ResizeMode.None;
				if (ResizeImage.Value) {
					mode = (ResizeMode.Value == 0) ? PdfImageExtracterModel.ResizeMode.Height : PdfImageExtracterModel.ResizeMode.Width;
				}
				await Task.Run(() => model.Method(list, OutputFolder.Value, MakeSubFolder.Value, mode, ResizePixel.Value));
			});

			FolderSelectCommand.Subscribe(() => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "保存先フォルダの選択", Response = new string[] { OutputFolder.Value } };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					OutputFolder.Value = msg.Response.FirstOrDefault();
				}
			});

			StatusMessage = model.StatusMessage.ToReadOnlyReactiveProperty();
			ProgressMax = model.ProgressMax.ToReadOnlyReactiveProperty();
			Progress = model.Progress.ToReadOnlyReactiveProperty();
			ProgressBarVisibility = CanExecute.Select(p => p ? Visibility.Collapsed : Visibility.Visible).ToReadOnlyReactivePropertySlim();

			CloseCanceledCallbackCommand.Subscribe(async () => {
				Settings.Default.OutputFolder = OutputFolder.Value;
				Settings.Default.MakeSubFolder = MakeSubFolder.Value;
				Settings.Default.ResizeImage = ResizeImage.Value;
				Settings.Default.ResizeMode = ResizeMode.Value;
				Settings.Default.ResizePixel = ResizePixel.Value;
				Settings.Default.Save();
				CanClose.Value = true;
				await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, "WindowAction"));
			});
		}

		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
		public void Initialize()
		{
			if (Directory.Exists(Settings.Default.OutputFolder)) {
				OutputFolder.Value = Settings.Default.OutputFolder;
			} else {
				OutputFolder.Value = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			}
			MakeSubFolder.Value = Settings.Default.MakeSubFolder;
			ResizeImage.Value = Settings.Default.ResizeImage;
			ResizeMode.Value = Settings.Default.ResizeMode switch {
				1 => 1,
				_ => 0,
			};
			ResizePixel.Value = Settings.Default.ResizePixel;
		}
	}
}
