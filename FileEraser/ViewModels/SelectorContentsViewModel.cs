using FileEraser.Models;
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
using System.IO;
using System.Linq;
using System.Text;

namespace FileEraser.ViewModels
{
	public class SelectorContentsViewModel : ViewModel, ISelectorViewModel
	{
		public ReactiveCommand FolderSelectCommand { get; } = new();
		public ReactivePropertySlim<string> TextBox1 { get; } = new();

		public SelectorContentsViewModel(FileSelectorContents selector)
		{
			FolderSelectCommand.Subscribe(() => {
				OpeningFileSelectionMessage msg = new() { MessageKey = "FolderSelect", Title = "ファイルを選択", Filter="すべてのファイル(*.*)|*.*", FileName = TextBox1.Value };
				Messenger.Raise(msg);
				if (msg.Response != null) {
					TextBox1.Value = msg.Response.FirstOrDefault();
				}
			});

			TextBox1.Value = selector?.FilePath;
		}

		public IFileSelector GetFileSelector()
		{
			if (File.Exists(TextBox1.Value)) {
				return new FileSelectorContents(TextBox1.Value);
			} else {
				return null;
			}
		}
	}
}
