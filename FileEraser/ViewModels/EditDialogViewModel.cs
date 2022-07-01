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
		public ReactivePropertySlim<bool> ChangeSelectorTypeEnable { get; } = new();
		public ReactivePropertySlim<ISelectorViewModel> SelectorPage { get; } = new();

		private readonly IFileSelector oldSelector;

		public EditDialogViewModel() : this(null) { }
		public EditDialogViewModel(IFileSelector selector)
		{
			oldSelector = selector;

			SelectorTypeListSelectedIndex.Subscribe(p => {
				SelectorPage.Value = p switch {
					(int)FileSelectorTypes.Name => new SelectorNameViewModel(oldSelector as FileSelectorName ),
					(int)FileSelectorTypes.Ext => new SelectorExtViewModel(oldSelector as FileSelectorExt),
					(int)FileSelectorTypes.Date => new SelectorDateViewModel(oldSelector as FileSelectorDate),
					(int)FileSelectorTypes.Contents => new SelectorContentsViewModel(oldSelector as FileSelectorContents),
					_ => new SelectorNameViewModel(null),
				};
			});
			ChangeSelectorTypeEnable.Value = selector == null;
		}

		// This method would be called from View, when ContentRendered event was raised.
		public void Initialize()
		{
			SelectorTypeListSelectedIndex.Value = (int)(oldSelector?.SelectorType ?? FileSelectorTypes.Name);
		}

		public IFileSelector GetOldSelector()
		{
			return oldSelector;
		}

		public IFileSelector GetNewSelector()
		{
			return SelectorPage.Value.GetFileSelector();
		}

	}
}
