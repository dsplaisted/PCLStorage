using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using PCLTesting.Infrastructure;

namespace PCLStorage.Test.Android
{
    [Activity(Label = "PCLStorage.Test.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        //int count = 1;

        Button _runTestsButton;
        Button _clearStorageButton;

        TextView _resultsTextView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //// Get our button from the layout resource,
            //// and attach an event to it
            _runTestsButton = FindViewById<Button>(Resource.Id.RunTests);
            _runTestsButton.Click += runTestsButton_Click;

            _clearStorageButton = FindViewById<Button>(Resource.Id.ClearStorage);
            _clearStorageButton.Click += _clearStorageButton_Click;

            _resultsTextView = FindViewById<TextView>(Resource.Id.ResultsTextView);
        }

        async void runTestsButton_Click(object sender, EventArgs e)
        {
            _runTestsButton.Enabled = false;
            _clearStorageButton.Enabled = false;

            try
            {
                var testRunner = new TestRunner(typeof(FileTests).Assembly);
                await testRunner.RunTestsAsync();
                _resultsTextView.Text = testRunner.Log;
            }
            catch (Exception ex)
            {
                _resultsTextView.Text = ex.ToString();
            }

            _runTestsButton.Enabled = true;
            _clearStorageButton.Enabled = true;
        }

        async void _clearStorageButton_Click(object sender, EventArgs e)
        {
            _runTestsButton.Enabled = false;
            _clearStorageButton.Enabled = false;

            try
            {
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
                _resultsTextView.Text = "Success";
            }
            catch (Exception ex)
            {
                _resultsTextView.Text = ex.ToString();
            }

            _runTestsButton.Enabled = true;
            _clearStorageButton.Enabled = true;
        }
    }
}

