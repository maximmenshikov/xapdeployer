/*
 * Application API  -  Xap Reader View Model class
 * (C) ultrashot
 * 
 * See "ApplicationAPI - Readme.txt" for overview.
 * See "ApplicationAPI - Terms of usage.txt" for terms of usage.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Xml.Linq;

namespace ApplicationApi
{
    [DataContract]
    public class XRBaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnChange(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    [DataContract]
    public class XapReaderViewModel : XRBaseViewModel
    {

        static XapReaderViewModel()
        {
            Cleanup();
        }

        private string TempFolderPath =
            "\\Applications\\Data\\" + GetProductId() + "\\Data\\IsolatedStore\\Temp";

        /// <summary>
        /// Returns application product ID as string
        /// </summary>
        /// <returns></returns>
        private static string GetProductId()
        {
            Guid guid = Guid.Empty;

            var productId = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("ProductID");

            if (productId != null && !string.IsNullOrEmpty(productId.Value))
            {
                string result = productId.Value.Replace("{", "").Replace("}", "");
                return result;
            }
            // return dummy otherwise...
            return "0e565ec0-551e-4cb9-bf06-77f191d48f31";
        }


        private string _fileName = null;

        private string _productId, _title,
                        _version, _author,
                        _description, _publisher,
                        _genre;

        private string _applicationIcon;

        private bool _isSigned = false;

        private long _fileSize = 0;
        private List<string> _capabilities = new List<string>();

        private string _isoTempFolderPath = "Temp";
        [DataMember]
        public string IsoTempFolderPath { get { return _isoTempFolderPath; } set { _isoTempFolderPath = value; } }

        /// <summary>
        /// Possible errors during XAP reading.
        /// </summary>
        public enum XapReadError : uint
        {
            Success = 0,
            NotRead = 1,
            OpenError = 2,
            NoAppElement = 3,
            NoProductId = 4,
            InvalidEncoding = 5,
            CouldNotCopyFile = 6,
            CouldNotOpenWMAppManifest = 7
        }

        [DataMember]
        /// <summary>
        /// Returns TRUE if input XAP file is signed.
        /// </summary>
        public bool IsSigned
        {
            get
            {
                return _isSigned;
            }
            set
            {
                _isSigned = value;
                OnChange("IsSigned");
            }
        }

        
        [DataMember]
        /// <summary>
        /// Returns XAP file size.
        /// </summary>
        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                _fileSize = value;
                OnChange("FileSize");
            }
        }

        
        [DataMember]
        /// <summary>
        /// Returns list of capabilities.
        /// </summary>
        public List<string> Capabilities
        {
            get
            {
                return _capabilities;
            }
            set
            {
                _capabilities = value;
                OnChange("Capabilities");
            }
        }

        
        [DataMember]
        /// <summary>
        /// Returns icon path
        /// </summary>
        public string IconPath
        {
            get
            {
                return _applicationIcon;
            }
            set
            {
                _applicationIcon = value;
                OnChange("IconPath");
            }
        }


        private BitmapImage _iconSourceCached = null;

        [IgnoreDataMember]
        /// <summary>
        /// Application icon
        /// </summary>
        public BitmapImage Icon
        {
            get
            {
                if (ShowDummy || error != XapReadError.Success)
                    return null;
                if (_iconSourceCached == null)
                {
                    BitmapImage image = new BitmapImage();
                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                    try
                    {
                        var fs = new IsolatedStorageFileStream(IsoTempFolderPath + "\\" + IconPath, FileMode.Open, isf);
                        if (fs != null)
                        {
                            if (fs.Length >= 3)
                            {
                                try
                                {
                                    if (fs.Length > 3)
                                    {
                                        var testBytes = new byte[3];
                                        fs.Read(testBytes, 0, 3);

                                        fs.Seek(0, SeekOrigin.Begin);
                                        if (testBytes[0] == 0x47 && testBytes[1] == 0x49 && testBytes[2] == 0x46)
                                        {
                                            // GIF found: it needs decoding.
                                            var img1 = new ImageTools.Image();
                                            var gifDecoder = new ImageTools.IO.Gif.GifDecoder();
                                            gifDecoder.Decode(img1, fs);

                                            _iconSourceCached = (ImageConverter.ToBitmap(img1) as ImageSource) as BitmapImage;
                                        }
                                        else
                                        {
                                            image.SetSource(fs);
                                            _iconSourceCached = image as BitmapImage;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Image isn't really important, so we skip errors.
                                }
                            }
                            fs.Close();
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                   
                }
                return _iconSourceCached as BitmapImage;
            }
            set
            {
                _iconSourceCached = value;
                OnChange("Icon");
            }
        }

        [DataMember]
        /// <summary>
        /// Returns application Product ID
        /// </summary>
        public string ProductID
        {
            get
            {
                return _productId;
            }
            set
            {
                _productId = value;
                OnChange("ProductID");
            }
        }

        
        [DataMember]
        /// <summary>
        /// Returns application title
        /// </summary>
        public string Title
        {
            get
            {
                if (ShowDummy || error != XapReadError.Success)
                    return _fileName;
                return _title;
            }
            set
            {
                _title = value;
                OnChange("Title");
            }
        }

        
        [DataMember]
        /// <summary>
        /// Returns author's name
        /// </summary>
        public string Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
                OnChange("Author");
            }
        }

        
        /// <summary>
        /// Returns application version
        /// </summary>
        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
                OnChange("Version");
            }
        }

        
        /// <summary>
        /// Returns application publisher
        /// </summary>
        public string Publisher
        {
            get
            {
                return _publisher;
            }
            set
            {
                _publisher = value;
                OnChange("Publisher");
            }
        }

        
        /// <summary>
        /// Returns application genre.
        /// </summary>
        public string Genre
        {
            get
            {
                return _genre;
            }

            set
            {
                _genre = value;
                OnChange("Genre");
            }
        }

        private XapReadError error = XapReadError.NotRead;

        /// <summary>
        /// Returns TRUE if read process went well
        /// </summary>
        /// <returns></returns>
        public bool IsReady()
        {
            if (error == XapReadError.Success)
                return true;
            return false;
        }

        [DataMember]
        /// <summary>
        /// XAP file name
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnChange("FileName");
            }
        }

        private void GenerateTempFolder()
        {
            string tempGuid = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "");
            IsoTempFolderPath = "Temp\\" + tempGuid;
            TempFolderPath = "\\Applications\\Data\\" + GetProductId() + "\\Data\\IsolatedStore\\Temp\\" + tempGuid;
        }

        /// <summary>
        /// Creates new Xap Reader object
        /// </summary>
        public XapReaderViewModel()
        {
            _fileName = null;
            GenerateTempFolder();
        }

        /// <summary>
        /// Creates new Xap Reader object.
        /// </summary>
        /// <param name="fileName"></param>
        public XapReaderViewModel(string fileName)
        {
            _fileName = fileName;
            GenerateTempFolder();
        }

        private static string ShortenFileName(string fileName)
        {
            if (fileName.Contains("\\"))
                fileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);

            if (fileName.Contains("/"))
                fileName = fileName.Substring(fileName.LastIndexOf("/") + 1);
            return fileName;
        }

        private bool ExtractFile(StreamResourceInfo xapStream, IsolatedStorageFile isf, string fileName)
        {
            try
            {
                if (!isf.DirectoryExists("Temp"))
                    isf.CreateDirectory("Temp");
                if (!isf.DirectoryExists(IsoTempFolderPath))
                    isf.CreateDirectory(IsoTempFolderPath);

                var streamResource = Application.GetResourceStream(xapStream, new Uri(fileName, UriKind.Relative));

                if (streamResource == null)
                    return false;

                string shortFileName = ShortenFileName(fileName);

                var fs = new IsolatedStorageFileStream(IsoTempFolderPath + "\\" + shortFileName, FileMode.Create, isf);

                Byte[] bytes = new Byte[streamResource.Stream.Length];
                streamResource.Stream.Read(bytes, 0, bytes.Length);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();

                streamResource.Stream.Close();
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private static void DeleteFolder(string src)
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.DirectoryExists(src))
            {
                var fnames = isf.GetFileNames(src + "\\*");
                foreach (var fname in fnames)
                {
                    isf.DeleteFile(src + "\\" + fname);
                }
                var dirs = isf.GetDirectoryNames(src + "\\*");
                foreach (var dirname in dirs)
                {
                    DeleteFolder(src + "\\" + dirname);
                }
                isf.DeleteDirectory(src);
            }
        }

        private static void Cleanup()
        {
            DeleteFolder("Temp");
        }

        private string ExtractResourceString(StreamResourceInfo xapStream, string resString)
        {
            string result = resString;
            if (resString.Length > 4)
            {
                UInt32 locale = Functions.GetLocaleId();

                if (resString[0] == '@' && resString.Contains(".dll"))
                {
                    resString = resString.Replace('/', '\\');
                    resString = resString.ToLower();
                    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                    string fname = resString.Substring(1, resString.IndexOf(".dll") - 1) + ".dll";
                    
                    bool dllExists = ExtractFile(xapStream, isf, fname);

                    string newDllPath = null;
                    string realString = null;

                    string mui = fname + "." + locale.ToString("X").PadLeft(4, '0') + ".mui";
                    bool muiExists = ExtractFile(xapStream, isf, mui);

                    /*
                    // don't really need to load MUI, ComXapHandler will do it itself. 
                    
                    string newMuiPath = null;
                    if (muiExists)
                    {
                        string shortFileName = ShortenFileName(mui);
                        newMuiPath = TempFolderPath + "\\" + shortFileName;

                        string outputStr = resString.Replace(fname, newMuiPath);
                        realString = ReinterpreteString(outputStr);
                    }
                    */
                    if (dllExists)
                    {
                        string shortFileName = ShortenFileName(fname);
                        newDllPath = TempFolderPath + "\\" + shortFileName;

                        string outputStr = resString.Replace(fname, newDllPath);
                        realString = Functions.ReinterpreteString(outputStr);
                    }
                    if (realString == null || realString == "")
                        realString = resString;
                    return realString;
                }

            }
            return result;
        }

        private void UpdateView()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                OnChange("Author");
                OnChange("Capabilities");
                OnChange("FileSize");
                OnChange("Genre");
                OnChange("IconPath");
                OnChange("Icon");
                OnChange("IsSigned");
                OnChange("ProductID");
                OnChange("Publisher");
                OnChange("Title");
                OnChange("Version");
            });
        }

        private bool _showDummy = false;

        public bool ShowDummy
        {
            get
            {
                return _showDummy;
            }
            set
            {
                _showDummy = value;
                OnChange("ShowDummy");
            }
        }
        /// <summary>
        /// Reads information from WMAppManifest.xml.
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            if (_fileName == null)
                throw new Exception("File name is empty");
            GenerateTempFolder();
            UpdateView();
            //Cleanup();

            error = XapReadError.NotRead;

            string TempFileName = _fileName;
            bool deleteFile = false;
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            
            XDocument reader = null;

            StreamResourceInfo xapStream = null;
            try
            {
                if (TempFileName.Contains("\\"))
                {
                    string shortname = _fileName.Substring(_fileName.LastIndexOf("\\") + 1).Replace("%20", " ");
                    if (shortname.Contains(" "))
                    {
                        bool res = Functions.CopyFile(_fileName, "\\Temp\\temp.xap");
                        if (res == false)
                        {
                            error = XapReadError.CouldNotCopyFile;
                            throw new Exception("Could not copy file");
                        }
                        TempFileName = "\\Temp\\temp.xap";
                        deleteFile = true;
                    }
                }
                var uri = new Uri(TempFileName, UriKind.Relative);
                xapStream = Application.GetResourceStream(uri);
                if (xapStream == null)
                {
                    error = XapReadError.OpenError;
                    throw new Exception("Couldn't read XAP");
                }
                _fileSize = xapStream.Stream.Length;

                // Input WMAppManifest not always has valid encoding. Let's try different ones to find the best.
                var stream = Application.GetResourceStream(xapStream, new Uri("WMAppManifest.xml", UriKind.Relative)).Stream;
                if (stream == null)
                {
                    error = XapReadError.CouldNotOpenWMAppManifest;
                    throw new Exception("Couldn't open WMAppManifest");
                }
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Close();
                stream = null;
                var encodings = new System.Text.Encoding[3] { Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode };
                bool correctEncodingFound = false;
                for (int i = 0; i < encodings.Length; ++i)
                {
                    try
                    {
                        Encoding enc = encodings[i];
                        string text = enc.GetString(bytes, 0, bytes.Length);
                        text = text.Replace("utf-16", "utf-8");
                        byte[] newBytes = Encoding.UTF8.GetBytes(text);

                        var memStream = new MemoryStream(newBytes);
                        reader = XDocument.Load(memStream);
                        memStream.Close();
                        memStream = null;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    correctEncodingFound = true;
                    break;
                }
                if (!correctEncodingFound)
                {
                    error = XapReadError.InvalidEncoding;
                    throw new Exception("Invalid WMAppManifest.xml encoding");
                }
            }
            catch (Exception ex)
            {
                if (error == XapReadError.NotRead)
                    error = XapReadError.OpenError;
                if (deleteFile == true)
                    Functions.RemoveFile(TempFileName);
                UpdateView();
                return false;

            }
            _isSigned = false;
            try
            {
                var rs = Application.GetResourceStream(xapStream, new Uri("WMAppPRHeader.xml", UriKind.Relative));
                if (rs != null)
                {
                    _isSigned = true;
                    rs.Stream.Close();
                }
            }
            catch (Exception ex)
            {
                _isSigned = false;
            }
            var nodes = reader.Descendants().Descendants<XElement>();
            bool AppNodeFound = false;
            bool ProductIdFound = false;
            foreach (var node in nodes)
            {
                string uri = node.BaseUri;
                if (node.Name.LocalName == "App")
                {
                    AppNodeFound = true;

                    var attrs = node.Attributes();
                    foreach (var attr in attrs)
                    {
                        switch (attr.Name.LocalName)
                        {
                            case "ProductID":
                                _productId = attr.Value;
                                ProductIdFound = true;
                                break;
                            case "Title":
                                _title = attr.Value;
                                break;
                            case "Version":
                                _version = attr.Value;
                                break;
                            case "Author":
                                _author = attr.Value;
                                break;
                            case "Description":
                                _description = attr.Value;
                                break;
                            case "Publisher":
                                _publisher = attr.Value;
                                break;
                            case "Genre":
                                _genre = attr.Value;
                                break;
                        }
                    }

                    _title = ExtractResourceString(xapStream, _title);
                    _version = ExtractResourceString(xapStream, _version);
                    _author = ExtractResourceString(xapStream, _author);
                    _description = ExtractResourceString(xapStream, _description);
                    _publisher = ExtractResourceString(xapStream, _publisher);

                    var elems = node.Elements();
                    foreach (var elem in elems)
                    {
                        switch (elem.Name.LocalName)
                        {
                            case "IconPath":
                                _applicationIcon = elem.Value;
                                if (ExtractFile(xapStream, isf, _applicationIcon) == true)
                                {
                                    string shortFileName = ShortenFileName(_applicationIcon);
                                    _applicationIcon = shortFileName;
                                }
                                break;
                            case "Capabilities":
                                var caps = elem.Elements();
                                foreach (var cap in caps)
                                {
                                    Capabilities.Add(cap.Attribute("Name").Value);
                                }
                                break;
                        }
                    }
                }
            }
            xapStream.Stream.Close();
            xapStream.Stream.Dispose();
            xapStream = null;
            reader = null;


            if (!AppNodeFound)
                error = XapReadError.NoAppElement;
            else if (!ProductIdFound)
                error = XapReadError.NoProductId;
            if (error == XapReadError.NotRead)
                error = XapReadError.Success;

            if (deleteFile == true)
            {
                Functions.RemoveFile(TempFileName);
            }
            UpdateView();
            return true;
        }

        private ApplicationInstaller _context = null;
        private bool license = false;
        /// <summary>
        /// Creates new installation context.
        /// </summary>
        public void CreateInstallationContext(string ProductID, string InstanceID = null, string OfferID = null, bool noUninstall = false, bool preInstall = false, byte[] license = null)
        {
            if (OfferID != null || license != null)
                this.license = true;
            _context = new ApplicationInstaller(_fileName, ProductID, InstanceID, OfferID, noUninstall, preInstall, license);
        }

        
        /// <summary>
        /// Returns installation context.
        /// </summary>
        public ApplicationInstaller Context
        {
            get
            {
                if (_context == null)
                    CreateInstallationContext(_productId);
                return _context;
            }
        }

        /// <summary>
        /// Installs current XAP.
        /// </summary>
        public bool Install(bool report = true, bool async = true)
        {
            if (_context != null)
            {
                InstallThreadParam param = new InstallThreadParam();
                param.fileName = _fileName;
                param.productId = _productId;
                param.operation = XapInstallOperationType.Install;
                param.report = report;
                if (async)
                {
                    var thread = new Thread(InstallThread);
                    thread.Start(param);
                    return true;
                }
                else
                {
                    return Install(param);
                }
            }
            return false;
        }

        /// <summary>
        /// Updates current XAP with application data saving.
        /// </summary>
        public void Update(bool report = true, bool async = true)
        {
            if (_context != null)
            {
                InstallThreadParam param = new InstallThreadParam();
                param.fileName = _fileName;
                param.productId = _productId;
                param.operation = XapInstallOperationType.Update;
                param.report = report;
                if (async)
                {
                    var thread = new Thread(InstallThread);
                    thread.Start(param);
                }
                else
                {
                    InstallThread(param);
                }
            }
        }

        /// <summary>
        /// Uninstalls application.
        /// </summary>
        public void Uninstall(bool report = true)
        {
            if (_context != null)
            {
                InstallThreadParam param = new InstallThreadParam();
                param.fileName = _fileName;
                param.productId = _productId;
                param.operation = XapInstallOperationType.Uninstall;
                param.report = report;
                var thread = new Thread(InstallThread);
                thread.Start(param);
            }
        }

        /// <summary>
        /// Xap installer notification event args.
        /// </summary>
        public class XapInstallEventArgs
        {
            public ApplicationInstaller application;
            public string fileName;
            public InstallationState state;
            public UInt32 progress;
            public UInt32 error;
        }

       
        public delegate void XapInstallEventHandler(object sender, XapInstallEventArgs e);

        public event XapInstallEventHandler StateChanged;

        private enum XapInstallOperationType
        {
            Install = 0,
            Uninstall = 1,
            Update = 2
        };

        private struct InstallThreadParam
        {
            public string fileName;
            public string productId;
            public XapInstallOperationType operation;
            public bool report;
        };

        private bool Install(InstallThreadParam param)
        {
            if (_context.IsValidContext() == true)
            {
                bool result = false;
                bool debugMode = true;
                if (license)
                    debugMode = false;
                if (param.operation == XapInstallOperationType.Install)
                    result = _context.Install(debugMode);
                else if (param.operation == XapInstallOperationType.Update)
                    result = _context.Update(debugMode);
                else
                    result = _context.Uninstall();

                if (result && param.report)
                {
                    while (true)
                    {
                        ApplicationEvent evt = _context.WaitForEvent();
                        if (StateChanged != null)
                        {
                            XapInstallEventArgs e = new XapInstallEventArgs();
                            e.application = _context;
                            e.fileName = param.fileName;
                            e.progress = evt.progress;
                            e.state = evt.state;
                            e.error = evt.error;
                            Deployment.Current.Dispatcher.BeginInvoke(delegate() { StateChanged(this, e); });
                        }
                        if ((evt.state == InstallationState.AppInstallCompleted && param.operation == XapInstallOperationType.Install) ||
                            (evt.state == InstallationState.AppUpdateCompleted && param.operation == XapInstallOperationType.Update) ||
                            (evt.state == InstallationState.AppRemoveCompleted && param.operation == XapInstallOperationType.Uninstall))
                        {
                            break;
                        }
                    }
                }
                return result;
            }
            return false;
        }
        private void InstallThread(Object obj)
        {
            Install((InstallThreadParam)obj);
        }



    }

}
