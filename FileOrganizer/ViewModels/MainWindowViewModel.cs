using FileOrganizer.Models;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FileOrganizer.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		private readonly Progress<ProgressInfo> _Progress;
		private CancellationTokenSource cts;
		// 処理コマンド
		public ReactivePropertySlim<bool> CanExecute { get; } = new(true);
		public AsyncReactiveCommand RenameCommand { get; }
		public AsyncReactiveCommand CollectFilesCommand { get; }
		public AsyncReactiveCommand ZipCompressCommand { get; }

		// Cancel
		public ReadOnlyReactivePropertySlim<bool> CanCancell { get; }
		public ReactiveCommand CancellCommand { get; }
		// Log
		public ReadOnlyReactiveProperty<string> LogText { get; }
		// StatusBar
		public ReactivePropertySlim<string> StatusMessage { get; } = new();
		public ReactivePropertySlim<int> ProgressMax { get; } = new();
		public ReactivePropertySlim<int> Progress { get; } = new();
		public ReadOnlyReactivePropertySlim<Visibility> ProgressBarVisibility { get; }

		public MainWindowViewModel()
		{
			_Progress = new(p => {
				StatusMessage.Value = p.Message;
				ProgressMax.Value = p.ValueMax;
				Progress.Value = p.Value;
			});

			// ファイル名修正
			RenameCommand = CanExecute.ToAsyncReactiveCommand().WithSubscribe(async () => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "処理フォルダの選択", SelectedPath = Rename.SelectedPath };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					cts = new();
					await Task.Run(() => Rename.Method(msg.Response.First(), _Progress, cts.Token));
				}
			});
			// 各サブフォルダに集める
			CollectFilesCommand = CanExecute.ToAsyncReactiveCommand().WithSubscribe(async () => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "処理フォルダの選択", SelectedPath = CollectFiles.SelectedPath };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					cts = new();
					await Task.Run(() => CollectFiles.Method(msg.Response.First(), _Progress, cts.Token));
				}
			});
			// フォルダ個別 zip 作成
			ZipCompressCommand = CanExecute.ToAsyncReactiveCommand().WithSubscribe(async () => {
				FolderSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "処理フォルダの選択", SelectedPath = ZipCompress.SelectedPath };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					cts = new();
					await Task.Run(() => ZipCompress.Method(msg.Response.First(), _Progress, cts.Token));
				}
			});

			// Cancel
			CanCancell = CanExecute.Select(p => !p).ToReadOnlyReactivePropertySlim().AddTo(CompositeDisposable);
			CancellCommand = CanCancell.ToReactiveCommand().WithSubscribe(() => {
				cts.Cancel();
			});
			// Log
			LogText = Log.Text.ToReadOnlyReactiveProperty().AddTo(CompositeDisposable);
			// StatusBar
			ProgressBarVisibility = CanExecute.Select(p => p ? Visibility.Collapsed : Visibility.Visible).ToReadOnlyReactivePropertySlim();
		}

		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
		public void Initialize()
		{
		}
	}
}
