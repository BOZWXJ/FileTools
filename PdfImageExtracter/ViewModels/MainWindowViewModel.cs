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

namespace PdfImageExtracter.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		readonly FileToolsModel model = new();

		public ReactivePropertySlim<string> OutputFolder { get; } = new();

		public ReactiveCommand ExtractImageCommand { get; } = new();

		// Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
		public void Initialize()
		{
			ExtractImageCommand.Subscribe(async _ => await Task.Run(model.ExtractImage));

		}
	}
}
