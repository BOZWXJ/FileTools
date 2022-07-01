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
	public class SelectorNameViewModel : ViewModel, ISelectorViewModel
	{
		public ReactivePropertySlim<string> TextBox1 { get; } = new();

		public SelectorNameViewModel(FileSelectorName fileSelector)
		{
			TextBox1.Value = fileSelector?.Pattern;
		}

		public IFileSelector GetFileSelector()
		{
			if (!string.IsNullOrEmpty(TextBox1.Value)) {
				return new FileSelectorName(TextBox1.Value);
			} else {
				return null;
			}
		}
	}
}
