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

namespace XapDeployer
{
    public partial class pageRunExe : PhoneApplicationPage
    {

        private string _fileName = null;

        public pageRunExe()
        {
            InitializeComponent();
        }

        private void ShowResult(bool error = false)
        {
            string shortName = _fileName;
            if (shortName.Contains("\\"))
                shortName = _fileName.Substring(_fileName.LastIndexOf("\\") + 1);
            if (error == false)
            {
                MessageBox.Show(LocalizedResources.txtRunExeOk, shortName, MessageBoxButton.OK);
        
            }
            else
            {
                MessageBox.Show(LocalizedResources.txtRunExeError, shortName, MessageBoxButton.OK);
            }
        }

        // BUGBUG, not synchronized
        private bool _stopThread = false;

        private bool StopThread { set { _stopThread = value; } }

        private void RunExeThread(object parameter)
        {
            bool result = false;
            string fileName = parameter as string;
            ApplicationApi.NativeExe ne = new ApplicationApi.NativeExe(fileName);
            if (ne.Run("", "", true) == true)
            {
                result = true;
                while (_stopThread == false)
                {
                    UInt32 res = ne.Wait(1000);
                    if (res == ApplicationApi.NativeExe.WaitingFailed || res == 0)
                    {
                        break;
                    }
                }
            }

            ne = null;
            GC.Collect();

            
            Dispatcher.BeginInvoke(delegate() 
            {
                VisualStateManager.GoToState(this, "Normal", true);
                ShowResult(!result);
            });

        }
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            StopThread = false;
            VisualStateManager.GoToState(this, "Running", true);
            var thread = new Thread(RunExeThread);
            thread.Start(_fileName);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
            string arg = null;
            IDictionary<String, String> qs = this.NavigationContext.QueryString;
            if (qs.ContainsKey("file"))
                arg = qs["file"];
            if (arg == null)
            {
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }

            _fileName = arg;

            string shortName = arg;
            if (shortName.Contains("\\"))
                shortName = arg.Substring(arg.LastIndexOf("\\") + 1);

            PageTitle.Text = shortName;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (BasicStates.CurrentState.Name == "Running")
            {
                StopThread = true;
                e.Cancel = true;
            }
        }

    }
}