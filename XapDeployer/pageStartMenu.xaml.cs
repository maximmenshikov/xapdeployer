/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * Terms of usage.
 * 1) Source code redistribution is prohibited.
 * 2) Source code usage is allowed only after getting permission from author.
 * 3) You can extend this project if you share modified code with author
 *    or post it in original thread.
 * 4) Application, as well as its derrivatives, cannot be sold unless opposite
 *    is allowed by author.
*/
using System;
using System.Windows;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace XapDeployer
{
    public partial class PageStartMenu : PhoneApplicationPage
    {
        public PageStartMenu()
        {
            InitializeComponent();
        }

        private const string xapDeployerClass = "xapdeployerfile";
        private const string ext = ".xap";

        private bool _associationState = false;

        private bool AssociationState
        {
            get
            {
                return _associationState;
            }
            set
            {
                _associationState = value;
                if (value == true)
                {
                    ApplicationBarMenuItem item = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
                    if (item != null)
                    {
                        item.Text = LocalizedResources.txtUnassociate;
                    }
                }
                else
                {
                    ApplicationBarMenuItem item = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
                    if (item != null)
                    {
                        item.Text = LocalizedResources.txtAssociate;
                    }
                }
            }
        }

        private void LoadState()
        {
            if (ApplicationApi.FileAssocation.GetClass(ext) == xapDeployerClass)
            {
                AssociationState = true;
            }
            else
            {
                AssociationState = false;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (InteropLib.HasRootAccess() == false)
            {
                // double check
                System.Threading.Thread.Sleep(1000);
                if (InteropLib.HasRootAccess() == false)
                {
                    MessageBox.Show(LocalizedResources.txtNoRootAccess, LocalizedResources.txtError, MessageBoxButton.OK);
                    throw new Exception("Quit");
                }
            }
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("LicenseShownOnce"))
            {
                NavigationService.Navigate(new Uri("/pageLicense.xaml", UriKind.Relative));
                return;
            }
            IDictionary<String, String> qs = this.NavigationContext.QueryString;
            string arg = null;
            if (qs.ContainsKey("file"))
                arg = qs["file"];

            if (arg != null)
            {
                var bytes = System.Text.Encoding.Unicode.GetBytes(arg);
                var uri = new Uri("/MainPage.xaml?file=base64" + Convert.ToBase64String(bytes) + "&cleanstack=true", UriKind.Relative);
                NavigationService.Navigate(uri);
                return;
            }
            LoadState();
            ShowGreetingAnimation.Begin();
            /*
            ApplicationBarIconButton button1 = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            if (button1 != null)
            {
                button1.Text = LocalizedResources.txtFileBrowser;
            }

            ApplicationBarMenuItem item = ApplicationBar.MenuItems[1] as ApplicationBarMenuItem;
            if (item != null)
            {
                item.Text = LocalizedResources.txtAbout;
            }
            */
        }

        private void txtAbout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageExplorer.xaml?cleanstack=false", UriKind.Relative));
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
        }

        private void btnAssociate_Click(object sender, EventArgs e)
        {
            string className = ApplicationApi.FileAssocation.GetClass(ext);
            if (AssociationState)
            {
                
                if (className == xapDeployerClass)
                {
                    string bkp = ApplicationApi.FileAssocation.GetBackupClass(ext);
                    if (bkp == null)
                        bkp = "";
                    ApplicationApi.FileAssocation.SetClass(ext, bkp);
                    ApplicationApi.FileAssocation.RemoveClass(xapDeployerClass);
                }
            }
            else
            {
                if (className != xapDeployerClass)
                {
                    if (ApplicationApi.FileAssocation.CreateClass(xapDeployerClass,
                        "app://5404fb98-1b79-4523-9955-9ea2c8bf9068/_default?file=%s") == true)
                    {
                        ApplicationApi.FileAssocation.BackupClass(ext);
                        ApplicationApi.FileAssocation.SetClass(ext, xapDeployerClass);
                    }
                }
            }
            LoadState();
        }
    }
}