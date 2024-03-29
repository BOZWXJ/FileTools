﻿using FileEraser.Models;
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
	public class SelectorDateViewModel : ViewModel, ISelectorViewModel
	{
		public ReactivePropertySlim<int> Days { get; } = new();
		public ReactivePropertySlim<string> Comparison { get; } = new();

		public SelectorDateViewModel(FileSelectorDate selector)
		{

		}

		public IFileSelector GetFileSelector()
		{
			return new FileSelectorDate();
		}
	}
}
