//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Cognitive Services (formerly Project Oxford): https://www.microsoft.com/cognitive-services
//
// Microsoft Cognitive Services (formerly Project Oxford) GitHub:
// https://github.com/Microsoft/Cognitive-Common-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SampleUserControlLibrary
{
    /// <summary>
    /// Interaction logic for SubscriptionKeyPage.xaml
    /// </summary>
    public partial class SubscriptionKeyPage : Page, INotifyPropertyChanged
    {
        private readonly string _isolatedStorageSubscriptionKeyFileName = "Subscription.txt";
        private readonly string _isolatedStorageSubscriptionEndpointFileName = "SubscriptionEndpoint.txt";

        public const string _defaultSubscriptionKeyPromptMessage = "Paste your subscription key here firstly";
        public const string _defaultSubscriptionEndpointPromptMessage = "Paste your endpoint here firstly";

        private static string s_subscriptionKey;
        private static string s_subscriptionEndpoint;

        private SampleScenarios _sampleScenarios;

        public SubscriptionKeyPage(SampleScenarios sampleScenarios)
        {
            InitializeComponent();
            _sampleScenarios = sampleScenarios;

            DataContext = this;
            SubscriptionKey = GetSubscriptionKeyFromIsolatedStorage();
            SubscriptionEndpoint = GetSubscriptionEndpointFromIsolatedStorage();
        }

        /// <summary>
        /// Gets or sets subscription key
        /// </summary>
        public string SubscriptionKey
        {
            get
            {
                return s_subscriptionKey;
            }

            set
            {
                s_subscriptionKey = string.IsNullOrEmpty(value) ? _defaultSubscriptionKeyPromptMessage : value;
                OnPropertyChanged<string>();
                _sampleScenarios.SubscriptionKey = s_subscriptionKey;
            }
        }

        /// <summary>
        /// Gets or sets subscription endpoint
        /// </summary>
        public string SubscriptionEndpoint
        {
            get
            {
                return s_subscriptionEndpoint;
            }

            set
            {
                s_subscriptionEndpoint = string.IsNullOrEmpty(value) ? _defaultSubscriptionEndpointPromptMessage : value;
                OnPropertyChanged<string>();
                _sampleScenarios.SubscriptionEndpoint = s_subscriptionEndpoint;
            }
        }

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper function for INotifyPropertyChanged interface
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="caller">Property name</param>
        private void OnPropertyChanged<T>([CallerMemberName]string caller = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// Gets the subscription key from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionKeyFromIsolatedStorage()
        {
            string subscriptionKey = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Open, isoStore))
                    {
                        using (var reader = new StreamReader(iStream))
                        {
                            subscriptionKey = reader.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionKey = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionKey))
            {
                subscriptionKey = _defaultSubscriptionKeyPromptMessage;
            }
            return subscriptionKey;
        }

        /// <summary>
        /// Gets the subscription endpoint from isolated storage.
        /// </summary>
        /// <returns></returns>
        private string GetSubscriptionEndpointFromIsolatedStorage()
        {
            string subscriptionEndpoint = null;

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                try
                {
                    using (var iStreamForEndpoint = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndpointFileName, FileMode.Open, isoStore))
                    {
                        using (var readerForEndpoint = new StreamReader(iStreamForEndpoint))
                        {
                            subscriptionEndpoint = readerForEndpoint.ReadLine();
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    subscriptionEndpoint = null;
                }
            }
            if (string.IsNullOrEmpty(subscriptionEndpoint))
            {
                subscriptionEndpoint = _defaultSubscriptionEndpointPromptMessage;
            }
            return subscriptionEndpoint;
        }


        /// <summary>
        /// Saves the subscription key to isolated storage.
        /// </summary>
        /// <param name="subscriptionKey">The subscription key.</param>
        private void SaveSubscriptionKeyToIsolatedStorage(string subscriptionKey)
        {
            if (subscriptionKey.Equals(_defaultSubscriptionKeyPromptMessage))
            {
                subscriptionKey = "";
            }

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionKeyFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionKey);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the subscription endpoint to isolated storage.
        /// </summary>
        /// <param name="subscriptionEndpoint">The subscription endpoint.</param>
        private void SaveSubscriptionEndpointToIsolatedStorage(string subscriptionEndpoint)
        {
            if (subscriptionEndpoint == _defaultSubscriptionEndpointPromptMessage)
            {
                subscriptionEndpoint = "";
            }

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null))
            {
                using (var oStream = new IsolatedStorageFileStream(_isolatedStorageSubscriptionEndpointFileName, FileMode.Create, isoStore))
                {
                    using (var writer = new StreamWriter(oStream))
                    {
                        writer.WriteLine(subscriptionEndpoint);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the saveSetting key save button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSubscriptionKeyToIsolatedStorage(SubscriptionKey);
                SaveSubscriptionEndpointToIsolatedStorage(SubscriptionEndpoint);
                MessageBox.Show("Subscription key and endpoint are saved in your disk.\nYou do not need to paste the key next time.", "Subscription Setting");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to save subscription key & endpoint. Error message: " + exception.Message,
                    "Subscription Setting", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SubscriptionKey = _defaultSubscriptionKeyPromptMessage;
                SubscriptionEndpoint = _defaultSubscriptionEndpointPromptMessage;
                SaveSubscriptionEndpointToIsolatedStorage("");
                SaveSubscriptionKeyToIsolatedStorage("");
                MessageBox.Show("Subscription setting is deleted from your disk.", "Subscription Setting");
            }
            catch (System.Exception exception)
            {
                MessageBox.Show("Fail to delete subscription setting. Error message: " + exception.Message,
                    "Subscription Setting", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.microsoft.com/cognitive-services/en-us/sign-up");
        }
    }
}
