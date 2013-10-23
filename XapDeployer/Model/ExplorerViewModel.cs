/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace XapDeployer
{
    [DataContract]
    public class ExplorerViewModel : BaseViewModel
    {

        public ExplorerViewModel()
        {
        }

        public event EventHandler BusyStateChanged;

        public event EventHandler CurrentPathChanged;

        private string _currentPath = "\\";

        private Dictionary<string, double> _scrollPosBackStack = new Dictionary<string, double>();

        [DataMember]
        public Dictionary<string, double> ScrollPosBackStack
        {
            get
            {
                return _scrollPosBackStack;
            }
            set
            {
                _scrollPosBackStack = value;
                OnChange("ScrollPosBackStack");
            }
        }

        [DataMember]
        public string CurrentPath
        {
            get
            {
                return _currentPath;
            }
            set
            {
                if (_currentPath != value)
                {

                    _currentPath = value.TrimEnd('\\');
                    if (!_currentPath.StartsWith("\\"))
                        _currentPath = "\\" + _currentPath;
                    OnChange("CurrentPath");
                    if (CurrentPathChanged != null)
                    {
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            CurrentPathChanged(this, new EventArgs());
                        });
                    }
                }
            }
        }



        private bool _isLoadingList = false;

        [IgnoreDataMember]
        public bool IsLoadingList
        {
            get
            {
                return _isLoadingList;
            }
            private set
            {
                _isLoadingList = value;
                OnChange("IsLoadingList");
            }
        }

        #region "File list"

        public void Load(string path, bool sync = false)
        {
            CurrentPath = path;
            LoadItemsIntoCache(_currentPath, sync);
        }

        private static List<FileViewModel> _cache = null;

        private Object _loadItemListLock = null;
        private Object LoadItemListLock
        {
            get
            {
                if (_loadItemListLock == null)
                    _loadItemListLock = new Object();
                return _loadItemListLock;
            }
        }
        private void LoadItemsThread(object param)
        {
            lock (LoadItemListLock)
            {
                var path = param as string;

                var dispatcher = System.Windows.Deployment.Current.Dispatcher;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    IsLoadingList = true;
                    if (BusyStateChanged != null)
                        BusyStateChanged(this, new EventArgs());
                }));
                var list = GetActualItemList(path);
                _cache = list;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    IsLoadingList = false;
                    OnChange("Items");
                    if (BusyStateChanged != null)
                        BusyStateChanged(this, new EventArgs());
                }));
            }
        }

        /// <summary>
        /// Loads items asynchronously.
        /// </summary>
        public void LoadItemsIntoCache(string path, bool sync = false)
        {
            if (sync)
            {
                LoadItemsThread(path);
            }
            else
            {
                var thread = new Thread(LoadItemsThread);
                thread.Start(path);
            }
        }

        /// <summary>
        /// Item list
        /// </summary>
        [DataMember]
        public List<FileViewModel> Items
        {
            get
            {
                return _cache;
            }
            set
            {
                _cache = value;
                OnChange("Items");
            }
        }

        
        private static List<FileViewModel> GetActualItemList(string path)
        {
            
            var list = new List<FileViewModel>();

                bool isRoot = false;
                if (path == "\\" || path == "")
                {
                    isRoot = true;
                }

                if (!isRoot)
                {
                    var back = new FileViewModel();
                    back.Text = "..";
                    back.IsDirectory = true;
                    back.IconUri = "icons/back.png";
                    back.IsSpecial = true;
                    if (Utils.IsLightTheme())
                        back.IconUri = back.IconUri.Replace("icons/", "icons/light/");
                    list.Add(back);
                }

                string searchPattern = System.IO.Path.Combine(path, "*");
                var files = InteropLib.GetContent(searchPattern);
                foreach (var file in files)
                {
                    string loweredFileName = file.FileName.ToLower();
                    if (file.IsDirectory ||
                        loweredFileName.EndsWith(".xap") ||
                        loweredFileName.EndsWith(".exe7") ||
                        loweredFileName.EndsWith(".exe") ||
                        loweredFileName.EndsWith(".provxml"))
                    {

                        var lbd = new FileViewModel();
                        lbd.Text = file.FileName;
                        lbd.IsDirectory = file.IsDirectory;
                        if (file.IsDirectory)
                        {
                            lbd.IconUri = "icons/folder.png";
                        }
                        else
                        {
                            if (loweredFileName.EndsWith(".xap"))
                                lbd.IconUri = "icons/xap.png";
                            else if (loweredFileName.EndsWith(".exe"))
                                lbd.IconUri = "icons/exe.png";
                            else if (loweredFileName.EndsWith(".provxml"))
                                lbd.IconUri = "icons/provxml.png";
                            else
                                lbd.IconUri = "icons/exe7.png";
                        }
                        if (Utils.IsLightTheme())
                            lbd.IconUri = lbd.IconUri.Replace("icons/", "icons/light/");
                        list.Add(lbd);
                    }
                }
                list.Sort(new FileViewModelComparer());
            return list;
        }

        #endregion

    }
}
