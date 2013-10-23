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
using System.Threading;
using Microsoft.Phone.Shell;
using System.Runtime.Serialization;

namespace XapDeployer
{
    public partial class pageMultiInstall : PhoneApplicationPage
    {
        public pageMultiInstall()
        {
            InitializeComponent();
            this.DataContext = App.MultiInstallViewModel;
            
        }

        void ViewModel_OnLoadingListStateChanged(object sender, EventArgs e)
        {
            if (SystemTray.ProgressIndicator == null)
                SystemTray.SetProgressIndicator(this, new ProgressIndicator());

            SystemTray.ProgressIndicator.IsVisible = ViewModel.IsLoadingList;
            SystemTray.ProgressIndicator.IsIndeterminate = ViewModel.IsLoadingList;

            btnInstall.IsEnabled = !ViewModel.IsLoadingList;
        }
        public MultiInstallViewModel ViewModel
        {
            get
            {
                return this.DataContext as MultiInstallViewModel;
            }
        }


        public static string Request = null;
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoadingListStateChanged += new EventHandler(ViewModel_OnLoadingListStateChanged);
            //IDictionary<String, String> qs = this.NavigationContext.QueryString;
            if (Request != null)
            {
                var qs = new Dictionary<string, string>();
                var lines = Request.Split('&');
                foreach (var line in lines)
                {
                    if (line.IndexOf("=") != -1)
                    {
                        qs.Add(line.Substring(0, line.IndexOf("=")), line.Substring(line.IndexOf("=") + 1));
                    }
                }
                var list = new List<XapReaderViewModelEx>();
                int i = 1;
                while (true)
                {
                    string keyName = "file" + i.ToString();
                    if (qs.ContainsKey(keyName))
                    {
                        var xap = new XapReaderViewModelEx(qs[keyName]);
                        var addon = new XapAdditionData();
                        if (qs.ContainsKey("license" + i.ToString()))
                            addon.LicenseFile = qs["license" + i.ToString()];
                        if (qs.ContainsKey("instance" + i.ToString()))
                            addon.InstanceID = qs["instance" + i.ToString()];
                        if (qs.ContainsKey("offer" + i.ToString()))
                            addon.OfferID = qs["offer" + i.ToString()];
                        xap.UserData = addon;
                        xap.State = LocalizedResources.txtStateInstallationPending;
                        list.Add(xap);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                ViewModel.Preload(list);
            }
        }

        private Object InstallSyncObject = new Object();

        private bool isBusy = false;

        void InstallThread(object param)
        {
            lock (InstallSyncObject)
            {
                isBusy = true;
                int installed = 0, skipped = 0, failed = 0;
                var vm = param as MultiInstallViewModel;
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    foreach (var xap in vm.Xaps)
                    {
                        xap.Progress = 1;
                        xap.InstallationCompletedVisibility = Visibility.Collapsed;
                        xap.IsInstalling = (xap.CheckedState == true);
                        xap.SwitchEnabled = false;
                    }
                });
                for (int x = 0; x < vm.Xaps.Count; x++)
                {
                    var xap = vm.Xaps[x];
                    if (xap.CheckedState == true)
                    {
                        int lowBoundary = 0;
                        var addon = (XapAdditionData)xap.UserData;
                        byte[] bLicense = null;
                        if (addon.LicenseFile != null)
                        {
                            bLicense = ApplicationApi.Functions.ReadFile(addon.LicenseFile);
                        }

                        xap.CreateInstallationContext(xap.ProductID, addon.InstanceID, addon.OfferID, license: bLicense, preInstall: true);
                        if (!xap.Context.IsInstalled())
                        {
                            if (xap.Install(false, false))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                {
                                    xap.State = LocalizedResources.txtStateInstalling;
                                });
                                while (true)
                                {
                                    var evt = xap.Context.WaitForEvent();
                                    if (evt.error > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine(evt.error.ToString());
                                        failed++;
                                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                        {
                                            xap.State = LocalizedResources.txtStateFailed;
                                        });
                                        break;
                                    }
                                    bool breakWhile = false;
                                    switch (evt.state)
                                    {
                                        case ApplicationApi.InstallationState.AppUpdateProgress:
                                        case ApplicationApi.InstallationState.AppInstallProgress:
                                            if (lowBoundary == 0)
                                                lowBoundary = (int)evt.progress;

                                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                            {
                                                var p = Math.Floor((double)(evt.progress - lowBoundary) / (double)(100 - lowBoundary) * 100);
                                                xap.Progress = (int)p;
                                            });
                                            break;
                                        case ApplicationApi.InstallationState.AppUpdateCompleted:
                                        case ApplicationApi.InstallationState.AppInstallCompleted:
                                            installed++;
                                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                            {
                                                xap.Progress = 100;
                                                xap.InstallationCompletedVisibility = Visibility.Visible;
                                                xap.State = LocalizedResources.txtStateInstalled;
                                            });
                                            breakWhile = true;
                                            break;
                                    }
                                    if (breakWhile)
                                        break;
                                }
                            }
                            else
                            {
                                failed++;
                                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                                {
                                    xap.State = LocalizedResources.txtStateFailed;
                                });
                            }
                        }
                        else
                        {
                            skipped++;
                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                            {
                                xap.State = LocalizedResources.txtStateSkipped; 
                            });
                        }
                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            xap.Progress = 100;
                            xap.InstallationCompletedVisibility = Visibility.Visible;
                            xap.IsInstalling = false;
                        });
                    }

                }
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    foreach (var xap in vm.Xaps)
                    {
                        xap.SwitchEnabled = true;
                    }
                    btnInstall.IsEnabled = true;
                    MessageBox.Show(String.Format(LocalizedResources.txtInstallationCompletedReport, installed, skipped, failed), LocalizedResources.txtInstallationCompleted, MessageBoxButton.OK);
                });
                isBusy = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isBusy)
            {
                var thread = new Thread(InstallThread);
                thread.Start(ViewModel);
                btnInstall.IsEnabled = false;
            }
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoadingListStateChanged -= ViewModel_OnLoadingListStateChanged;
        }
    }
}