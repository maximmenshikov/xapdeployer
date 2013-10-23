/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XapDeployer
{
    public class FileViewModel : BaseViewModel
    {
            private static BitmapImage xapImage = null;
            private static BitmapImage folderImage = null;
            private static BitmapImage backImage = null;
            private static BitmapImage exeImage = null;
            private static BitmapImage exe7Image = null;

            private string _text = "";
            private string _iconUri = null;
            private bool _isDir = false;

            public string Text
            {
                get
                {
                    return _text;
                }
                set
                {
                    _text = value;
                    OnChange("Text");
                }
            }

            private BitmapImage GetImage(ref BitmapImage bitmapImage, string _iconUri)
            {
                if (bitmapImage == null)
                    bitmapImage = new BitmapImage(new Uri(_iconUri, UriKind.Relative));
                return bitmapImage;
            }

            public BitmapImage Icon
            {
                get
                {
                    if (_iconUri == null)
                        return null;
                    string tempUri = _iconUri.Replace("/light", "");
                    if (tempUri == "icons/xap.png")
                    {
                        return GetImage(ref xapImage, _iconUri);
                    }
                    else if (tempUri == "icons/folder.png")
                    {
                        return GetImage(ref folderImage, _iconUri);
                    }
                    else if (tempUri == "icons/back.png")
                    {
                        return GetImage(ref backImage, _iconUri);
                    }
                    else if (tempUri == "icons/exe7.png")
                    {
                        return GetImage(ref exe7Image, _iconUri);
                    }
                    else if (tempUri == "icons/exe.png")
                    {
                        return GetImage(ref exeImage, _iconUri);
                    }
                    return new BitmapImage(new Uri(_iconUri, UriKind.Relative));
                }
            }

            public string IconUri
            {
                get
                {
                    return _iconUri;
                }
                set
                {
                    _iconUri = value;
                    OnChange("Icon");
                }
            }

            public FontFamily FontFamily
            {
                get
                {
                    return new FontFamily(IsDirectory ? "Segoe WP Semilight" : "Segoe WP Light");
                }
            }

            public bool IsDirectory
            {
                get
                {
                    return _isDir;
                }
                set
                {
                    _isDir = value;
                    OnChange("IsDirectory");
                    OnChange("FontFamily");
                }
            }

            private bool _isSpecial = false;
            public bool IsSpecial
            {
                get
                {
                    return _isSpecial;
                }
                set
                {
                    if (_isSpecial != value)
                    {
                        _isSpecial = value;
                        OnChange("IsSpecial");
                    }
                }
            }
        
    }


    public class FileViewModelComparer : IComparer<FileViewModel>
    {
        public int Compare(FileViewModel x, FileViewModel y)
        {
            try
            {
                if (x.IsSpecial && !y.IsSpecial)
                    return -1;
                else if (!x.IsSpecial && y.IsSpecial)
                    return 1;
                if (x.IsDirectory && !y.IsDirectory)
                    return -1;
                else if (!x.IsDirectory && y.IsDirectory)
                    return 1;
                return x.Text.CompareTo(y.Text);
            }
            catch (Exception ex)
            {
            }
            return 0;
        }
    }

}
