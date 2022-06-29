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
using System.Linq;
using System.Text;

namespace FileEraser.ViewModels
{
	public class EditDialogViewModel : ViewModel
	{
		public string[] SelectorTypeList { get; } = new[] { "ファイル名", "拡張子", "更新日時", "ファイルと一致" };
		public ReactivePropertySlim<int> SelectorTypeListSelectedIndex { get; } = new();
		public ReactivePropertySlim<Uri> SelectorPage { get; } = new();

		public IFileSelector Selector { get; }

		public EditDialogViewModel() : this(new FileSelectorName()) { }
		public EditDialogViewModel(IFileSelector selector)
		{
			this.Selector = selector;

			SelectorTypeListSelectedIndex.Subscribe(p => {
				string page = p switch {
					0 => "/Views/SelectorNamePage.xaml",
					1 => "/Views/SelectorExtPage.xaml",
					2 => "/Views/SelectorDatePage.xaml",
					3 => "/Views/SelectorContentsPage.xaml",
					_ => "/Views/SelectorNamePage.xaml",
				};
				SelectorPage.Value = new Uri(page, UriKind.Relative);
			});

		}

		// This method would be called from View, when ContentRendered event was raised.
		public void Initialize()
		{
			SelectorTypeListSelectedIndex.Value = Selector.SelectorType switch {
				FileSelectorTypes.Name => 0,
				FileSelectorTypes.Ext => 1,
				FileSelectorTypes.Date => 2,
				FileSelectorTypes.Contents => 3,
				_ => 0,
			};
		}

	}
}
