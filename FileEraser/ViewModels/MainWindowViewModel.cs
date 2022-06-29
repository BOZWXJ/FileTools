using FileEraser.Models;
using FileEraser.Properties;
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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FileEraser.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		private readonly FileEraserModel model;

		// 削除処理
		public ReadOnlyReactiveProperty<bool> CanExecute { get; }
		public AsyncReactiveCommand FileDeleteCommand { get; }

		// Drag & Drop
		public ReactiveCommand<DragEventArgs> DragOverCommand { get; } = new();
		public ReactiveCommand<DragEventArgs> DropCommand { get; }

		// 設定
		public ReactiveCommand FolderSelectCommand { get; } = new();
		public ReactivePropertySlim<string> EraseFolder { get; } = new();
		public ReactivePropertySlim<bool> SearchSubFolder { get; } = new();
		public ReadOnlyReactiveCollection<FileSelectorListItemViewModel> FileSelectorList { get; }
		public ReactivePropertySlim<int> FileSelectorListSelectedIndex { get; } = new();
		public ReadOnlyReactivePropertySlim<bool> CanEditFileSelectorItem { get; }
		public ReactiveCommand AddFileSelectorItemCommand { get; } = new();
		public ReactiveCommand EditFileSelectorItemCommand { get; } = new();
		public ReactiveCommand DeleteFileSelectorItemCommand { get; } = new();

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
			model = new();

			CanExecute = model.CanExecute.ToReadOnlyReactiveProperty();
			FileDeleteCommand = CanExecute.ToAsyncReactiveCommand().WithSubscribe(async () => {
				await Task.Run(() => model.Method(EraseFolder.Value, SearchSubFolder.Value));
			});

			// Drag & Drop
			DragOverCommand.Subscribe(e => {
				e.Handled = true;
				if (CanExecute.Value && e.Data.GetDataPresent(DataFormats.FileDrop)) {
					e.Effects = DragDropEffects.Copy;
				} else {
					e.Effects = DragDropEffects.None;
				}
			});

			DropCommand = CanExecute.ToReactiveCommand<DragEventArgs>().WithSubscribe(e => {
				e.Handled = true;
				if (e.Data.GetData(DataFormats.FileDrop) is not string[] list || list.Length != 1 || !Directory.Exists(list[0])) {
					return;
				}
				EraseFolder.Value = list[0];
			});

			// 設定
			FolderSelectCommand.Subscribe(() => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "削除処理フォルダの選択", Response = new string[] { EraseFolder.Value } };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					EraseFolder.Value = msg.Response.FirstOrDefault();
				}
			});

			FileSelectorList = model.FileSelectorList.ToReadOnlyReactiveCollection(p => new FileSelectorListItemViewModel(p));
			CanEditFileSelectorItem = FileSelectorListSelectedIndex.Select(p => p >= 0).ToReadOnlyReactivePropertySlim();
			AddFileSelectorItemCommand.Subscribe(() => {
				// todo: 新規
				Messenger.Raise(new TransitionMessage(new EditDialogViewModel(), "ShowEditDialog"));
			});
			EditFileSelectorItemCommand = CanEditFileSelectorItem.ToReactiveCommand().WithSubscribe(() => {
				// todo: 編集
				Messenger.Raise(new TransitionMessage(new EditDialogViewModel(), "ShowEditDialog"));
			});
			DeleteFileSelectorItemCommand = CanEditFileSelectorItem.ToReactiveCommand().WithSubscribe(() => {
				model.DeleteFileSelectorItem(FileSelectorListSelectedIndex.Value);
			});

			// StatusBar
			StatusMessage = model.StatusMessage.ToReadOnlyReactiveProperty();
			ProgressMax = model.ProgressMax.ToReadOnlyReactiveProperty();
			Progress = model.Progress.ToReadOnlyReactiveProperty();
			ProgressBarVisibility = CanExecute.Select(p => p ? Visibility.Collapsed : Visibility.Visible).ToReadOnlyReactivePropertySlim();

			// 終了処理
			CloseCanceledCallbackCommand.Subscribe(async () => {
				if (!CanExecute.Value) {
					return;
				}
				Settings.Default.EraseFolder = EraseFolder.Value;
				Settings.Default.SearchSubFolder = SearchSubFolder.Value;
				model.SaveSettingList();

				Settings.Default.Save();
				CanClose.Value = true;
				await Messenger.RaiseAsync(new WindowActionMessage(WindowAction.Close, "WindowAction"));
			});
		}

		public void Initialize()
		{
			if (Directory.Exists(Settings.Default.EraseFolder)) {
				EraseFolder.Value = Settings.Default.EraseFolder;
			} else {
				EraseFolder.Value = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			}
			SearchSubFolder.Value = Settings.Default.SearchSubFolder;
			model.LoadSettingList();
		}

	}
}
