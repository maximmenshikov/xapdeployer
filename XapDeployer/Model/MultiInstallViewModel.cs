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
using System.Runtime.Serialization;

namespace XapDeployer
{
    [DataContract]
    public class MultiInstallViewModel : BaseViewModel
    {
        private List<XapReaderViewModelEx> _xaps = null;

        private bool _isLoadingList = false;

        public event EventHandler OnLoadingListStateChanged;

        [DataMember]
        public bool IsLoadingList
        {
            get
            {
                return _isLoadingList;
            }
            set
            {
                _isLoadingList = value;
                OnChange("IsLoadingList");
            }
        }

        [DataMember]
        public List<XapReaderViewModelEx> Xaps
        {
            get
            {
                return _xaps;
            }
            set
            {
                _xaps = value;
                OnChange("Xaps");
            }
        }

        public void Refresh()
        {
            OnChange("Xaps");
        }

        private void PreloadXapThread(object param)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                IsLoadingList = true;
                if (OnLoadingListStateChanged != null)
                    OnLoadingListStateChanged(this, new EventArgs());
            });
            var list = param as List<XapReaderViewModelEx>;
            foreach (var xap in list)
            {
                xap.Read();
            }
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                Xaps = list;
                IsLoadingList = false;
                if (OnLoadingListStateChanged != null)
                    OnLoadingListStateChanged(this, new EventArgs());
            });
        }

        
        public void Preload(List<XapReaderViewModelEx> list, bool sync = false)
        {
            if (sync == false)
            {
                var thread = new Thread(PreloadXapThread);
                thread.Start(list);
            }
            else
            {
                PreloadXapThread(list);
            }
        }
    }
}
