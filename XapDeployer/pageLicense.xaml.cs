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
using System.IO.IsolatedStorage;

namespace XapDeployer
{
    public partial class pageLicense : PhoneApplicationPage
    {
        public pageLicense()
        {
            InitializeComponent();
            btnAgree.IsEnabled = !IsolatedStorageSettings.ApplicationSettings.Contains("LicenseShownOnce");
        }

        private void btnAgree_Click(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LicenseShownOnce"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("LicenseShownOnce", "true");
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }

        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LicenseShownOnce"))
            {
                Utils.CleanPageStack(this);
            }
        }
    }
}