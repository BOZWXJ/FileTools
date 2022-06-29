using FileEraser.Models;
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
using System.Text;

namespace FileEraser.ViewModels
{
	public class FileSelectorListItemViewModel : ViewModel
	{
		public ReadOnlyReactivePropertySlim<string> Description { get; }

		public FileSelectorListItemViewModel() { }
		public FileSelectorListItemViewModel(IFileSelector item)
		{
			Description = item.ObserveProperty(p => p.Description).ToReadOnlyReactivePropertySlim();
		}
	}
}
