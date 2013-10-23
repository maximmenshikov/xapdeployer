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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;

namespace XapDeployer
{
    public partial class pageExplorer : PhoneApplicationPage
    {
        private ExplorerViewModel viewModel
        {
            get
            {
                return this.DataContext as ExplorerViewModel;
            }
        }



        public static T FindChildOfType<T>(DependencyObject root) where T : class
        {
            var queue = new Queue<DependencyObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (int i = VisualTreeHelper.GetChildrenCount(current) - 1; 0 <= i; i--)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    var typedChild = child as T;
                    if (typedChild != null)
                    {
                        return typedChild;
                    }
                    queue.Enqueue(child);
                }
            }
            return null;
        }

        private ScrollViewer InternalScrollViewer = null;
        private void listBox1_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var scrollViewer = FindChildOfType<ScrollViewer>(element);
            this.InternalScrollViewer = scrollViewer;
        }

        private void viewModel_BusyStateChanged(object sender, EventArgs e)
        {
            if (SystemTray.ProgressIndicator == null)
                SystemTray.SetProgressIndicator(this, new ProgressIndicator());
            bool isLoading = viewModel.IsLoadingList;
            SystemTray.ProgressIndicator.IsIndeterminate = isLoading ? true : false;
            SystemTray.ProgressIndicator.IsVisible = isLoading ? true : false;
            if (isLoading)
            {
                VisualStateManager.GoToState(this, "Loading", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", true);
                if (InternalScrollViewer != null)
                {
                    double vertOffset = 0.0;
                    if (viewModel.ScrollPosBackStack.ContainsKey(viewModel.CurrentPath))
                    {
                        vertOffset = viewModel.ScrollPosBackStack[viewModel.CurrentPath];
                    }
                    InternalScrollViewer.ScrollToVerticalOffset(vertOffset);
                }
            }
        }

        public pageExplorer()
        {
            this.DataContext = App.ExplorerViewModel;
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private bool _loadedOnce = false;
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loadedOnce == false)
            {
                IDictionary<String, String> qs = this.NavigationContext.QueryString;
                if (qs.ContainsKey("cleanstack"))
                {
                    var val = qs["cleanstack"];
                    if (val == "true")
                        Utils.CleanPageStack(this);
                }
                viewModel.Load(viewModel.CurrentPath);
                /*
                ApplicationBarMenuItem item = ApplicationBar.MenuItems[0] as ApplicationBarMenuItem;
                if (item != null)
                {
                    item.Text = LocalizedResources.txtInstallAll;
                }*/
                _loadedOnce = true;
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            viewModel.BusyStateChanged += new EventHandler(viewModel_BusyStateChanged);
            viewModel.CurrentPathChanged += new EventHandler(viewModel_CurrentPathChanged);
        }

        void viewModel_CurrentPathChanged(object sender, EventArgs e)
        {
            scrollTitle.ScrollToHorizontalOffset(scrollTitle.ScrollableWidth);

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            viewModel.BusyStateChanged -= viewModel_BusyStateChanged;
            viewModel.CurrentPathChanged -= viewModel_CurrentPathChanged;
        }
        private Object _loadFilesSync = new Object();

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string CurrentFolder = viewModel.CurrentPath;
            if (CurrentFolder != "\\" && CurrentFolder != "")
            {
                e.Cancel = true;
                if (CurrentFolder.Contains("\\"))
                    CurrentFolder = CurrentFolder.Substring(0, CurrentFolder.LastIndexOf("\\") + 1);
                viewModel.Load(CurrentFolder);
            }
        }

        private void PageTitle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollTitle.ScrollToHorizontalOffset(scrollTitle.ScrollableWidth);
        }

        private void listBox1_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            FileViewModel lbd = listBox1.Items[listBox1.SelectedIndex] as FileViewModel;
            if (lbd.IsDirectory)
            {
                string CurrentFolder = viewModel.CurrentPath;
                if (lbd.Text == "..")
                {
                    if (CurrentFolder.Contains("\\"))
                        CurrentFolder = CurrentFolder.Substring(0, CurrentFolder.LastIndexOf("\\") + 1);
                    if (viewModel.ScrollPosBackStack.ContainsKey(viewModel.CurrentPath))
                        viewModel.ScrollPosBackStack.Remove(viewModel.CurrentPath);
                }
                else
                {
                    CurrentFolder = Path.Combine(CurrentFolder, lbd.Text);
                    if (!viewModel.ScrollPosBackStack.ContainsKey(viewModel.CurrentPath))
                        viewModel.ScrollPosBackStack.Add(viewModel.CurrentPath, 0.0);
                    viewModel.ScrollPosBackStack[viewModel.CurrentPath] = InternalScrollViewer.VerticalOffset;
                }
                viewModel.Load(CurrentFolder);
            }
            else
            {
                string path = Path.Combine(viewModel.CurrentPath, lbd.Text);
                if (lbd.Text.ToLower().EndsWith(".exe7") || lbd.Text.ToLower().EndsWith(".exe"))
                {
                    NavigationService.Navigate(new Uri("/pageRunExe.xaml?file=" + path, UriKind.Relative));
                }
                else if (lbd.Text.ToLower().EndsWith(".provxml"))
                {
                    string param = Provxml.GetMultiInstallParameter(path);
                    pageMultiInstall.Request = param;
                    NavigationService.Navigate(new Uri("/pageMultiInstall.xaml", UriKind.Relative));
                }
                else
                {
                    var bytes = System.Text.Encoding.Unicode.GetBytes(path);
                    var uri = new Uri("/MainPage.xaml?file=base64" + Convert.ToBase64String(bytes), UriKind.Relative);
                    NavigationService.Navigate(uri);
                }
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnInstallAll_Click(object sender, EventArgs e)
        {
            string request = "";
            int i = 1;
            foreach (var item in viewModel.Items)
            {
                if (!item.IsDirectory && item.Text.ToLower().EndsWith(".xap"))
                {
                    if (request != "")
                        request += "&";
                    request += "file" + i.ToString() + "=" + System.IO.Path.Combine(viewModel.CurrentPath, item.Text);
                    i++;
                }
            }
            pageMultiInstall.Request = request;
            NavigationService.Navigate(new Uri("/pageMultiInstall.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                ApplicationBar.BackgroundColor = Colors.Transparent;
            }
            else
            {
                ApplicationBar.BackgroundColor = (Color)App.Current.Resources["PhoneChromeColor"];
            }
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible || this.Orientation == PageOrientation.Portrait || this.Orientation == PageOrientation.PortraitUp)
                ApplicationBar.BackgroundColor = (Color)App.Current.Resources["PhoneChromeColor"];
            else
                ApplicationBar.BackgroundColor = Colors.Transparent;
        }


    }
}