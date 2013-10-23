/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using ApplicationApi;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace XapDeployer
{
    public partial class MainPage : PhoneApplicationPage
    {

        public MainPage()
        {
            this.DataContext = App.CurrentModel;
            InitializeComponent();
            VisualStateManager.GoToState(this, "Normal", false);
        }

        private MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        private XapReaderViewModel xapReader = null;

        /// <summary>
        /// Current XAP Reader.
        /// </summary>
        public XapReaderViewModel MyXapReader
        {
            get { return xapReader; }
            set { xapReader = value; }
        }

        /// <summary>
        /// UI states
        /// </summary>
        private enum UiInstallationState
        {
            NothingToDo = 0,
            Preloading = 1,
            AllowInstall = 2,
            AllowUpdate = 3,
            InstallInProgress = 4,
            UpdateInProgress = 5,
            UninstallInProgress = 6
        }

#region "UI"

        /// <summary>
        /// Internal xap deployer has progress starting from ~75. We save the lowest boundary
        /// for progressbar.
        /// </summary>
        uint lowBoundary = 0;

        /// <summary>
        /// State changed event handler running in UI thread.
        /// </summary>
        /// <param name="e"></param>
        private void XapInstallStateChanged(object sender, XapReaderViewModel.XapInstallEventArgs e)
        {
            var error = e.error;
            double progress;

            if (e.error > 0)
            {
                MessageBox.Show(LocalizedResources.txtInstallError, LocalizedResources.txtError, MessageBoxButton.OK);
                VisualStateManager.GoToState(this, "ShowInformation", true);
                SetProgress(false);
                viewModel.IsInProgress = false;
                return;
            }
            if (e.progress > 0)
            {
                if (lowBoundary == 0)
                {
                    lowBoundary = e.progress;
                }
            }
            progress = Math.Floor((double)(e.progress - lowBoundary) / (double)(100 - lowBoundary) * 100);
            if (lowBoundary == 100)
                progress = 100;

            string text = "";
            switch (e.state)
            {
                case InstallationState.AppInstallStarted:
                    progress = 0;
                    text = LocalizedResources.txtInstalling + " 0%";
                    break;
                case InstallationState.AppInstallProgress:
                    text = LocalizedResources.txtInstalling + " " + progress.ToString() + "%";
                    break;
                case InstallationState.AppInstallCompleted:
                    text = LocalizedResources.txtInstalling + " 100%";
                    break;
                case InstallationState.AppUpdateStarted:
                    progress = 0;
                    text = LocalizedResources.txtUpdating + " 0%";
                    break;
                case InstallationState.AppUpdateProgress:
                    text = LocalizedResources.txtUpdating + " " + progress.ToString() + "%";
                    break;
                case InstallationState.AppUpdateCompleted:
                    text = LocalizedResources.txtUpdating + " 100%";
                    break;
                case InstallationState.AppRemoveStarted:
                    progress = 0;
                    text = LocalizedResources.txtUninstalling + " 0%";
                    break;
                case InstallationState.AppRemoveCompleted:
                    text = LocalizedResources.txtUninstalling + " 100%";
                    break;
            }
            bool bShow = true;
            if (e.state == InstallationState.AppInstallCompleted ||
                    e.state == InstallationState.AppUpdateCompleted ||
                    e.state == InstallationState.AppRemoveCompleted)
            {
                bShow = false;
                progress = 100;
                viewModel.IsInProgress = false;
            }
            else
            {
                viewModel.IsInProgress = true;
            }
            SetProgress(bShow, text, progress);
            if (bShow == false)
                VisualStateManager.GoToState(this, "ShowInformation", true);
                //HideProgressBarAnimation.Begin();
            if (e.state == InstallationState.AppInstallCompleted ||
                e.state == InstallationState.AppUpdateCompleted)
            {
                viewModel.IsInstalled = true;
            }
            else if (e.state == InstallationState.AppRemoveCompleted)
            {
                viewModel.IsInstalled = false;
            }
        }

        /// <summary>
        /// Set progress bar info.
        /// </summary>
        /// <param name="show"></param>
        /// <param name="text"></param>
        /// <param name="val"></param>
        private void SetProgress(bool show, string text = "", double val = 0.0)
        {
            if (SystemTray.ProgressIndicator == null)
                SystemTray.SetProgressIndicator(this, new ProgressIndicator());
            if (SystemTray.ProgressIndicator != null)
            {
                SystemTray.ProgressIndicator.Text = text;
                SystemTray.ProgressIndicator.IsVisible = show;
                SystemTray.ProgressIndicator.Value = val / 100;
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            if (MyXapReader == null)
                return;
            if (MyXapReader.IsSigned == true)
            {
                MessageBox.Show(LocalizedResources.txtFileIsSigned, MyXapReader.Title, MessageBoxButton.OK);
                return;
            }
            if (MyXapReader.Context.IsInstalled() == true)
            {
                ApplicationInfo inf = new ApplicationInfo(MyXapReader.ProductID);
                string currentVersion = inf.Version;
                string newVersion = MyXapReader.Version;
                if (MessageBox.Show(LocalizedResources.txtAlreadyInstalled + "\n\n" + LocalizedResources.txtCurrentVersion + ": " + currentVersion + "\n" + LocalizedResources.txtNewVersion + ": " + newVersion + "\n", MyXapReader.Title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SetProgress(true, LocalizedResources.txtUpdating, 0);
                    MyXapReader.Update();
                }
            }
            else
            {
                //state = UiInstallationState.InstallInProgress;
                SetProgress(true, LocalizedResources.txtInstalling, 0);
                
                MyXapReader.Install();
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageAbout.xaml", UriKind.Relative));
        }

        private void picApplicationIcon_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            StackPanelBump.Begin();
        }

#endregion

        private void PreloadXapThread(object parameter)
        {
            string path = parameter as string;

            MyXapReader = new XapReaderViewModel(path);

            
            MyXapReader.Read();
            MyXapReader.StateChanged += XapInstallStateChanged;

            this.Dispatcher.BeginInvoke(delegate()
            {
                
                viewModel.XapReaderContext = MyXapReader;

                picApplicationIcon.Source = MyXapReader.Icon;
                MyXapReader.CreateInstallationContext(MyXapReader.ProductID);

                viewModel.RefreshInstalledState();
                VisualStateManager.GoToState(this, "ShowInformation", true);
            }
            );
        }

        /// <summary>
        /// Preloads XAP and initializes UI (ASYNC)
        /// </summary>
        /// <param name="path"></param>
        private void PreloadXap(string path)
        {
            SetProgress(false);
            picApplicationIcon.Source = null;
            VisualStateManager.GoToState(this, "Load", true);

            string shortName = path;
            if (shortName.Contains("\\"))
                shortName = shortName.Substring(shortName.LastIndexOf("\\") + 1);

            viewModel.FileName = shortName;
            var thread = new Thread(PreloadXapThread);
            thread.Start(path);
        }

        private bool _preloaded = false;
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Internal.Initialize();
            viewModel.IsInProgressChanged += new EventHandler(viewModel_IsInProgressChanged);
            viewModel.InstalledStateChanged += new EventHandler(viewModel_InstalledStateChanged);
            IDictionary<String, String> qs = this.NavigationContext.QueryString;
            string arg = null;
            if (qs.ContainsKey("file"))
                arg = qs["file"];
            
            if (arg != null)
            {
                if (arg.StartsWith("base64"))
                {
                    var bytes = System.Convert.FromBase64String(arg.Replace("base64", ""));
                    arg = System.Text.Encoding.Unicode.GetString(bytes, 0, bytes.Length);
                }
            }
            if (qs.ContainsKey("cleanstack") && qs["cleanstack"].ToLower() == "true")
            {
                Utils.CleanPageStack(this);
            }
            if (_preloaded == false)
                PreloadXap(arg);

            _preloaded = true;
            return;
        }

        void AdjustUi()
        {
            if (viewModel.IsInProgress &&
                BasicStates.CurrentState.Name == "ShowInformation")
            {
                VisualStateManager.GoToState(this, "Installing", true);
            }
            else if (!viewModel.IsInProgress &&
                BasicStates.CurrentState.Name == "Installing")
            {
                VisualStateManager.GoToState(this, "ShowInformation", true);
            }
        }

        void viewModel_InstalledStateChanged(object sender, EventArgs e)
        {
            
        }

        void viewModel_IsInProgressChanged(object sender, EventArgs e)
        {
            AdjustUi();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (BasicStates.CurrentState.Name != "ShowInformation")
            {
                e.Cancel = true;
            }
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (MyXapReader == null)
                return;
            if (MyXapReader.Context.IsInstalled() == true)
            {
                if (MessageBox.Show(LocalizedResources.txtUninstallWarning, MyXapReader.Title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SetProgress(true, LocalizedResources.txtUninstalling, 0);
                    MyXapReader.Uninstall();
                }
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (MyXapReader != null && MyXapReader.Context == null)
                MyXapReader.CreateInstallationContext(MyXapReader.ProductID);
            viewModel.RefreshInstalledState();
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            viewModel.IsInProgressChanged -= viewModel_IsInProgressChanged;
            viewModel.InstalledStateChanged -= viewModel_InstalledStateChanged;
        }

    }
}