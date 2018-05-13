using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Xamarin.Forms;

namespace HRmobile
{
	public partial class App : Application
	{
		public App (Stream[] assets)
		{
			InitializeComponent();

            MainPage = new HRmobile.MainPage(assets);
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
