﻿using Livet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace FileOrganizer
{
	public partial class App : Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
			DispatcherHelper.UIDispatcher = Dispatcher;
			//AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
		}

		// Application level error handling
		//private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		//{
		//    //TODO: Logging
		//    MessageBox.Show(
		//        "Something errors were occurred.",
		//        "Error",
		//        MessageBoxButton.OK,
		//        MessageBoxImage.Error);
		//
		//    Environment.Exit(1);
		//}
	}
}
