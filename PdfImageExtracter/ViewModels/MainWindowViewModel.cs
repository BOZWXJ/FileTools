using PdfImageExtracter.Models;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive.Linq;
using PdfImageExtracter.Properties;
using System.IO;

namespace PdfImageExtracter.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		readonly FileToolsModel model = new();

		public ReactivePropertySlim<string> OutputFolder { get; } = new();
		public ReactiveCommand FolderSelectCommand { get; } = new();

		public ReactivePropertySlim<bool> MakeSubFolder { get; } = new();
		public ReactivePropertySlim<bool> ResizeImage { get; } = new();
		public ReactivePropertySlim<int> ResizeMode { get; } = new();
		public ReactivePropertySlim<int> ResizePixel { get; } = new();

		public ReactivePropertySlim<bool> CanExecute = new(true);
		public AsyncReactiveCommand ExtractImageCommand { get; } = new();

		// StatusBar
		public ReadOnlyReactiveProperty<string> StatusMessage { get; }
		public ReactivePropertySlim<int> ProgressMax { get; } = new();
		public ReactivePropertySlim<int> Progress { get; } = new();
		public ReadOnlyReactivePropertySlim<Visibility> ProgressBarVisibility { get; }

		public ReactiveProperty<bool> CanClose { get; } = new(false);
		public AsyncReactiveCommand CloseCanceledCallbackCommand { get; } = new();

		public MainWindowViewModel()
		{
			FolderSelectCommand.Subscribe(() => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "保存先フォルダの選択", Response = new string[] { OutputFolder.Value } };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					OutputFolder.Value = msg.Response.FirstOrDefault();
				}
			});

			ExtractImageCommand = CanExecute.ToAsyncReactiveCommand().WithSubscribe(async () => {

				string[] list = new[] { @"D:\Users\mitamura\Pictures\tmp\Test.pdf" };

				ProgressMax.Value = list.Length;
				Progress.Value = 0;
				foreach (var file in list) {
					int resize = 0;
					if (ResizeImage.Value) {
						resize = ResizePixel.Value * (ResizeMode.Value == 0 ? 1 : -1);
					}
					await Task.Run(() => model.ExtractImage(file, OutputFolder.Value, MakeSubFolder.Value, resize));
					Progress.Value++;
				}
				System.Diagnostics.Debug.WriteLine(Path.GetTempFileName());

			});

			ProgressBarVisibility = CanExecute.Select(p => p ? Visibility.Collapsed : Visibility.Visible).ToReadOnlyReactivePropertySlim();
			StatusMessage = model.StatusMessage.ToReadOnlyReactiveProperty();

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
