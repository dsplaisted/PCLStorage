using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PCLStorage.TestFramework.Infrastructure;

namespace PCLStorage.Test.SL
{
	public partial class MainPage : UserControl
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private async void RunTestsButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var testRunner = new TestRunner(typeof(FileTests).Assembly);
				await testRunner.RunTestsAsync();
				ResultsTextBox.Text = testRunner.Log;
			}
			catch (Exception ex)
			{
				ResultsTextBox.Text = ex.ToString();
			}
		}
	}
}
