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
using Microsoft.Phone.Controls;
using PCLTesting.Infrastructure;

namespace PCLStorage.Test.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private async void RunTestsButton_Click(object sender, RoutedEventArgs e)
        {
            RunTestsButton.IsEnabled = false;
            ClearIsoStoreButton.IsEnabled = false;

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

            RunTestsButton.IsEnabled = true;
            ClearIsoStoreButton.IsEnabled = true;
        }

        private async void ClearIsoStoreButton_Click(object sender, RoutedEventArgs e)
        {
            RunTestsButton.IsEnabled = false;
            ClearIsoStoreButton.IsEnabled = false;

            var isoStore = FileSystem.Current.LocalStorage;
            {
                foreach (var file in await isoStore.GetFilesAsync())
                {
                    await file.DeleteAsync();
                }
                foreach (var directory in await isoStore.GetFoldersAsync())
                {
                    await directory.DeleteAsync();
                }
            }

            RunTestsButton.IsEnabled = true;
            ClearIsoStoreButton.IsEnabled = true;
        }
    }
}