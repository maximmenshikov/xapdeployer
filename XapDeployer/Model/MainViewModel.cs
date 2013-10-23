/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Windows;
using ApplicationApi;

namespace XapDeployer
{
        public class MainViewModel : BaseViewModel
        {
            public MainViewModel()
            {
            }

            private XapReaderViewModel _xapReader = null;

            public XapReaderViewModel XapReaderContext
            {
                get { return _xapReader; }
                set
                {
                    _xapReader = value;
                    OnChange("Title");
                    OnChange("Author");
                    OnChange("Publisher");
                    OnChange("Genre");
                    OnChange("Version");
                    OnChange("Capabilities");
                    OnChange("Signature");
                    OnChange("FileSize");
                }
            }

            public string Title
            {
                get
                {
                    if (_xapReader != null)
                        return _xapReader.Title;
                    else
                        return LocalizedResources.txtNotStated;
                }
            }

            public string Author
            {
                get
                {
                    if (_xapReader != null)
                        return _xapReader.Author;
                    else
                        return LocalizedResources.txtNotStated;
                }
            }

            public string Publisher
            {
                get
                {
                    if (_xapReader != null)
                        return _xapReader.Publisher;
                    else
                        return LocalizedResources.txtNotStated;
                }
            }

            public string ProductID
            {
                get
                {
                    if (_xapReader != null)
                        return _xapReader.ProductID;
                    return "";
                }
            }

            /// <summary>
            /// Translates genre to current language.
            /// </summary>
            /// <param name="genre"></param>
            /// <returns></returns>
            private string GenreToTranslation(string genre)
            {
                switch (genre)
                {
                    case "apps.normal":
                        return LocalizedResources.GenreApp;
                    case "apps.games":
                        return LocalizedResources.GenreGame;

                }
                return "Unknown";
            }

            public string Genre
            {
                get
                {
                    if (_xapReader != null)
                        return GenreToTranslation(_xapReader.Genre);
                    else
                        return LocalizedResources.txtNotStated;
                }
            }

            public string Version
            {
                get
                {
                    if (_xapReader != null)
                        return _xapReader.Version;
                    else
                        return LocalizedResources.txtNotStated;
                }
            }

            /// <summary>
            /// Translation capability to current language.
            /// </summary>
            /// <param name="cap"></param>
            /// <returns></returns>
            private string CapabilityToTranslation(string cap)
            {
                try
                {
                    return LocalizedResources.ResourceManager.GetString(cap);
                }
                catch (Exception ex)
                {
                    return cap;
                }
            }


            public string Capabilities
            {
                get
                {
                    if (_xapReader == null)
                        return LocalizedResources.txtNotStated;
                    string caps = "";
                    foreach (var cap in _xapReader.Capabilities)
                    {
                        caps += CapabilityToTranslation(cap) + "\n";
                    }
                    if (caps.EndsWith("\n"))
                        caps = caps.Substring(0, caps.Length - 1);
                    return caps;
                }
            }

            public string Signature
            {
                get
                {
                    if (_xapReader == null)
                        return LocalizedResources.txtNotStated;
                    return (_xapReader.IsSigned == true) ? LocalizedResources.txtSignaturePresent : LocalizedResources.txtSignatureMissed;
                }
            }

            public string FileSize
            {
                get
                {
                    if (_xapReader == null)
                        return LocalizedResources.txtNotStated;
                    double size = (double)_xapReader.FileSize / 1024 / 1024;
                    size = Math.Round(size, 2);
                    return size.ToString();
                }
            }

            public event EventHandler InstalledStateChanged;

            private bool _isInstalled;
            public bool IsInstalled
            {
                get
                {
                    return _isInstalled;
                }
                set
                {
                    _isInstalled = value;
                    OnChange("IsInstalled");
                    OnChange("IsInstalledText");
                    if (InstalledStateChanged != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            InstalledStateChanged(this, new EventArgs());
                        });
                    }
                }
            }

            public string IsInstalledText
            {
                get
                {
                    if (_xapReader == null)
                        return LocalizedResources.txtNotStated;
                    return _isInstalled ? LocalizedResources.txtYes : LocalizedResources.txtNo;
                }
            }

            public void RefreshInstalledState()
            {
                if (!String.IsNullOrEmpty(ProductID))
                {
                    bool result = false;
                    var appInfo = new ApplicationApi.ApplicationInfo(ProductID);
                    if (appInfo.IsValid())
                        result = true;
                    appInfo = null;
                    IsInstalled = result;
                }
            }

            public event EventHandler IsInProgressChanged;

            private bool _isInProgress = false;
            public bool IsInProgress
            {
                get
                {
                    return _isInProgress;
                }
                set
                {
                    if (_isInProgress != value)
                    {
                        _isInProgress = value;
                        OnChange("IsInProgress");
                        if (IsInProgressChanged != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(delegate()
                            {
                                IsInProgressChanged(this, new EventArgs());
                            });
                        }
                    }
                }
            }

       
            private string _fileName;

            public string FileName
            {
                get
                {
                    if (_fileName == null)
                        return LocalizedResources.txtNotStated;
                    return _fileName;
                }
                set
                {
                    _fileName = value;
                    OnChange("FileName");
                }
            }
        }
}
