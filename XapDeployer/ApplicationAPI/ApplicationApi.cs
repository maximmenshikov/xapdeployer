/*
 * Application API
 * (C) ultrashot
 * 
 * See "ApplicationAPI - Readme.txt" for overview.
 * See "ApplicationAPI - Terms of usage.txt" for terms of usage.
*/
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Phone.InteropServices;

namespace ApplicationApi
{
    internal static class _ApplicationApiGlobals
    {
        /// <summary>
        /// MAKE SURE YOU CHANGE IT!
        /// <remarks>
        /// This is against of DLL-hell issue that still exists in WP7
        /// </remarks>
        /// </summary>
        public const string AppShortName = "XPD";

    }

    /// <summary>
    /// Application install state.
    /// <remarks>
    /// *Progress deliver progress as well.
    /// </remarks>
    /// </summary>
    public enum InstallationState : uint
    {
        AppInstallStarted = 0,
        AppInstallCompleted = 1,
        AppUpdateStarted = 2,
        AppUpdateCompleted = 3,
        AppRemoveStarted = 4,
        AppRemoveCompleted = 5,
        AppUpdateLicenseStarted = 6,
        AppUpdateLicenseCompleted = 7,
        AppDownloadStarted = 8,
        AppDownloadProgress = 9,
        AppDownloadCompleted = 10,
        AppInstallProgress = 11,
        AppUpdateProgress = 12
    };

    /// <summary>
    /// Application install event structure definition.
    /// </summary>
    public struct ApplicationEvent
    {
        public InstallationState state;
        public UInt32 progress;
        public UInt32 error;
    };

    /// <summary>
    /// Class representing application installation context
    /// </summary>
    public class ApplicationInstaller
    {

        private enum EventReceiveError : uint
        {
	        Success = 0,
	        Error_First = 0xFFFFFFFD,
	        Error_InvalidParameter = 0xFFFFFFFD,
	        Error_NoData = 0xFFFFFFFE,
	        Error_NoListener = 0xFFFFFFFF
        };

        private UInt32 _context = 0;

        /// <summary>
        /// Create new ApplicationInstaller object 
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInstaller(string fileName, string guid = null, string InstanceId = null, string offerId = null, bool uninstallDisabled = false, bool preInstall = false, byte[] license = null)
        {
            Internal.CheckInstance();
            int bLicenseLength = 0;
            IntPtr ptrLicense = IntPtr.Zero;
            if (license != null)
            {
                bLicenseLength = license.Length;
                ptrLicense = Internal.Instance.AllocMem((uint)bLicenseLength);
                Microsoft.Phone.InteropServices.Marshal.Copy(license, 0, ptrLicense, bLicenseLength);
            }
            _context = Internal.Instance.BeginDeploy(fileName, 
                                                     Internal.GuidToString(guid), 
                                                     Internal.GuidToString(InstanceId), 
                                                     Internal.GuidToString(offerId), 
                                                     uninstallDisabled, 
                                                     preInstall, 
                                                     ptrLicense, 
                                                     (uint)bLicenseLength);
            if (ptrLicense != IntPtr.Zero)
                Internal.Instance.FreeMem(ptrLicense);
        }

        /// <summary>
        /// Checks if current context is valid.
        /// </summary>
        /// <returns>TRUE if it context is valid, FALSE otherwise</returns>
        public bool IsValidContext()
        {
            return (_context > 0) ? true : false;
        }

        /// <summary>
        /// Checks if application is already installed.
        /// </summary>
        /// <returns>TRUE if installed, FALSE otherwise</returns>
        public bool IsInstalled()
        {
            Internal.CheckInstance();
            return Internal.Instance.IsInstalled(_context);
        }

        /// <summary>
        /// Starts installation.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Install(bool debugMode)
        {
            Internal.CheckInstance();
            uint res = Internal.Instance.Install(_context, false, debugMode);
            _throwError = res;
            return (res == 0) ? true : false;
        }

        /// <summary>
        /// Starts updating.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Update(bool debugMode)
        {
            Internal.CheckInstance();
            uint res = Internal.Instance.Install(_context, true, debugMode);
            _throwError = res;
            return (res == 0) ? true : false;
        }

        /// <summary>
        /// Starts uninstallation.
        /// </summary>
        /// <returns>TRUE on success, FALSE otherwise</returns>
        /// <remarks>Success means that you would be able to use WaitForEvent</remarks>
        public bool Uninstall()
        {
            Internal.CheckInstance();
            uint res = Internal.Instance.Uninstall(_context);
            _throwError = res;
            return (res == 0) ? true : false;
        }

        private UInt32 _throwError = 0;
 
        /// <summary>
        /// Waits for incoming notifications.
        /// </summary>
        /// <returns>Returns ApplicationEvent with current information about install/update process.
        /// Sets .error = true on failure.
        /// </returns>
        public ApplicationEvent WaitForEvent()
        {
            Internal.CheckInstance();
            ApplicationEvent evt = new ApplicationEvent();
            if (_throwError != 0)
            {
                evt.error = _throwError;
                evt.progress = 0;
                return evt;
            }
            InstallationState state;
            uint progress;
            uint hResult;
            UInt32 data = Internal.Instance.WaitForEvents(_context, 0xFFFFFFFF, out state, out progress, out hResult);

            if (data >= (uint)EventReceiveError.Error_First)
            {
                evt.error = data;
                return evt;
            }
            evt.state = (InstallationState)state;

            if (progress > 100)
                progress = 0;
            evt.progress = progress;
            evt.error = hResult;
            return evt;
        }


        ~ApplicationInstaller()
        {
            Internal.CheckInstance();
            if (_context != 0)
                Internal.Instance.EndDeploy(_context);
        }
    }

    /// <summary>
    /// Application Info class
    /// </summary>
    public class ApplicationInfo
    {

        private enum GuidIndex : uint
        {
            ProductID = 0,
            InstanceID = 1,
            OfferID = 2
        };

        private enum AppInfoIntegerIndex : uint
        {
            AppId = 0,
            IsNotified = 1,
            AppInstallType = 2,
            AppState = 3,
            IsRevoked = 4,
            IsUpdateAvailable = 5,
            IsUninstallable = 6,
            IsThemable = 7,
            Rating = 8,
            AppId2 = 9,
            Genre = 10
        };

        private enum AppInfoStringIndex : uint
        {
            DefaultTask = 0,
            Title = 1,
            ApplicationIcon = 2,
            InstallFolder = 3,
            DataFolder = 4,
            Genre = 5,
            Publisher = 6,
            Author = 7,
            Description = 8,
            Version = 9,
            ImagePath = 10
        };

        private class ValueCache
        {
            private object _value;
            public object Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                    Cached = true;
                }
            }

            public bool Cached = false;
        }

        private UInt32 _appInfo = 0;

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(string guid)
        {
            Internal.CheckInstance();
            _appInfo = Internal.Instance.GetApplicationInfo(guid);
        }

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(Guid guid)
        {
            Internal.CheckInstance();
            _appInfo = Internal.Instance.GetApplicationInfo(guid.ToString());
        }

        /// <summary>
        /// Creates a new ApplicationInfo object and fullfills it with an actual information.
        /// </summary>
        /// <param name="guid"></param>
        public ApplicationInfo(UInt32 nativeAppInfo)
        {
            Internal.CheckInstance();
            _appInfo = nativeAppInfo;
        }

        ~ApplicationInfo()
        {
            Internal.CheckInstance();
            if (_appInfo != 0)
                Internal.Instance.ReleaseApplicationInfo(_appInfo);
        }


        /// <summary>
        /// Returns true if application info handle is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (_appInfo != 0)
                return true;
            return false;
        }

        /// <summary>
        /// Returns invoke information.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="parameters"></param>
        /// <returns>True on success, false otherwise</returns>
        public bool GetInvocationInfo(out string uri, out string parameters)
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
            {
                uri = "";
                parameters = "";
                return false;
            }
            return Internal.Instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters);
        }

        /// <summary>
        /// Returns main application uri
        /// </summary>
        /// <returns></returns>
        public string GetUri()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return "";
            string uri = "", parameters = "";
            if (Internal.Instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters) == true)
            {
                if (uri == null)
                    uri = "";
                return uri;
            }
            return "";
        }

        public string GetParameters()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return "";
            string uri = "", parameters = "";
            if (Internal.Instance.ApplicationInfoGetInvocationInfo(_appInfo, out uri, out parameters) == true)
            {
                if (parameters == null)
                    parameters = "";
                return parameters;
            }
            return "";
        }
        /// <summary>
        /// Returns Product ID
        /// </summary>
        /// <returns></returns>
        public Guid ProductID()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = Internal.Instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.ProductID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Instance ID
        /// </summary>
        /// <returns></returns>
        public Guid InstanceID()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = Internal.Instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.InstanceID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Offer ID
        /// </summary>
        /// <returns></returns>
        public Guid OfferID()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return Guid.Empty;
            string guidString = Internal.Instance.ApplicationInfoGetGuid(_appInfo, (uint)GuidIndex.OfferID);
            if (guidString.Length == 0)
                return Guid.Empty;
            return new Guid(guidString);
        }

        /// <summary>
        /// Returns Application Id.
        /// </summary>
        /// <returns></returns>
        public UInt64 AppId()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return 0;
            UInt64 ull1 = (UInt64)Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppId);
            UInt64 ull2 = (UInt64)Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppId2);
            UInt64 result = (ull2 << 32) | ull1;
            return result;
        }

        /// <summary>
        /// Checks if application is notified
        /// </summary>
        /// <returns></returns>
        public UInt32 IsNotified()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return 0;
            return Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsNotified);
        }

        /// <summary>
        /// Application installation types.
        /// </summary>
        public enum AppInstallationType : uint
        {
            Marketplace = 0,
            System = 1,
            Oem = 2,
            External = 3,
            Last = 3,
            Unknown = 0xFFFFFFFF
        };

        /// <summary>
        /// Checks application installation type.
        /// </summary>
        /// <returns></returns>
        public AppInstallationType AppInstallType()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return 0;
            UInt32 type = Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppInstallType);
            if (type <= (uint)AppInstallationType.Last)
                return (AppInstallationType)type;
            return AppInstallationType.Unknown;
        }


        private ValueCache _appState = null;
        /// <summary>
        /// Checks application state.
        /// </summary>
        /// <returns></returns>
        public UInt32 AppState
        {
            get
            {
                if (_appState == null)
                    _appState = new ValueCache();
                if (_appState.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return 0;
                    _appState.Value = Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.AppState);
                }
                return (UInt32)_appState.Value;
            }
        }

        private ValueCache _isRevoked = null;
        /// <summary>
        /// Checks if application is revoked
        /// </summary>
        /// <returns></returns>
        public bool IsRevoked
        {
            get
            {
                if (_isRevoked == null)
                    _isRevoked = new ValueCache();
                if (_isRevoked.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return false;
                    _isRevoked.Value = (Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsRevoked) > 0) ? true : false;
                }
                return (bool)_isRevoked.Value;
            }
        }

        private ValueCache _isUpdateAvailable = null;
        /// <summary>
        /// Checks if an update is available.
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateAvailable
        {
            get
            {
                if (_isUpdateAvailable == null)
                    _isUpdateAvailable = new ValueCache();
                if (_isUpdateAvailable.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return false;
                    _isUpdateAvailable.Value = (Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsUpdateAvailable) > 0) ? true : false;
                }
                return (bool)_isUpdateAvailable.Value;
            }
        }

        private ValueCache _isUninstallable = null;
        /// <summary>
        /// Checks if application is uninstallable
        /// </summary>
        /// <returns></returns>
        public bool IsUninstallable
        {
            get
            {
                if (_isUninstallable == null)
                    _isUninstallable = new ValueCache();
                if (_isUninstallable.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return false;
                    _isUninstallable.Value = (Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsUninstallable) > 0) ? true : false;
                }
                return (bool)_isUninstallable.Value;
            }
        }

        private ValueCache _isThemable = null;
        /// <summary>
        /// Checks if application is themable
        /// </summary>
        /// <returns></returns>
        public bool IsThemable
        {
            get
            {
                if (_isThemable == null)
                    _isThemable = new ValueCache();
                if (_isThemable.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return false;
                    _isThemable.Value = (Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.IsThemable) > 0) ? true : false;
                }
                return (bool)_isThemable.Value;
            }
        }

        private ValueCache _Rating;
        /// <summary>
        /// Returns application rating on marketplace.
        /// </summary>
        /// <returns></returns>
        public UInt16 Rating
        {
            get
            {
                if (_Rating == null)
                    _Rating = new ValueCache();
                if (_Rating.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return 0;
                    _Rating.Value = Internal.Instance.ApplicationInfoGetInteger(_appInfo, (uint)AppInfoIntegerIndex.Rating);
                }
                return (UInt16)_Rating.Value;
            }
        }

        private ValueCache _Title = null;
        /// <summary>
        /// Returns correctly application title [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Title
        {
            get
            {
                if (_Title == null)
                    _Title = new ValueCache();
                if (_Title.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Title);
                    string result = Internal.Instance.ReinterpreteString(str);
                    _Title.Value = result;
                }
                return _Title.Value as string;
            }
        }

        private ValueCache _DefaultTask = null;
        /// <summary>
        /// Returns default task
        /// </summary>
        /// <returns></returns>
        public string DefaultTask
        {
            get
            {
                if (_DefaultTask == null)
                    _DefaultTask = new ValueCache();
                if (_DefaultTask.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.DefaultTask);
                    _DefaultTask.Value = str;
                }
                return _DefaultTask.Value as string;
            }
        }

        private ValueCache _ApplicationIcon = null;
        /// <summary>
        /// Returns application icon path [Reinterpreted]
        /// </summary>
        public string ApplicationIcon
        {
            get
            {
                if (_ApplicationIcon == null)
                    _ApplicationIcon = new ValueCache();
                if (_ApplicationIcon.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.ApplicationIcon);
                    string result = Internal.Instance.ReinterpreteString(str);
                    _ApplicationIcon.Value = result;
                }
                return _ApplicationIcon.Value as string;
            }
            set
            {
                Internal.CheckInstance();
                if (_appInfo == 0)
                    return;
                Internal.Instance.UpdateAppIconPath(ProductID().ToString(), value);
            }
        }

        private ValueCache _InstallFolder = null;
        /// <summary>
        /// Returns application's install folder
        /// </summary>
        /// <returns></returns>
        public string InstallFolder
        {
            get
            {
                if (_InstallFolder == null)
                    _InstallFolder = new ValueCache();
                if (_InstallFolder.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.InstallFolder);
                    _InstallFolder.Value = str;
                }
                return _InstallFolder.Value as string;
            }
        }

        private ValueCache _DataFolder = null;
        /// <summary>
        /// Returns application's data folder
        /// </summary>
        /// <returns></returns>
        public string DataFolder
        {
            get
            {
                if (_DataFolder == null)
                    _DataFolder = new ValueCache();
                if (_DataFolder.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.DataFolder);
                    _DataFolder.Value = str;
                }
                return _DataFolder.Value as string;
            }
        }

        private ValueCache _Genre = null;
        /// <summary>
        /// Returns application's genre
        /// </summary>
        /// <returns></returns>
        public UInt16 Genre
        {
            get
            {
                if (_Genre == null)
                    _Genre = new ValueCache();
                if (_Genre.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return 0;
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Genre);
                    _Genre.Value = str;
                }
                return (UInt16)_Genre.Value;
            }
        }

        private ValueCache _Publisher = null;
        /// <summary>
        /// Returns application's publisher [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Publisher
        {
            get
            {
                if (_Publisher == null)
                    _Publisher = new ValueCache();
                if (_Publisher.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Publisher);
                    string result = Internal.Instance.ReinterpreteString(str);
                    _Publisher.Value = result;
                }
                return _Publisher.Value as string;
            }
        }

        private ValueCache _Author = null;
        /// <summary>
        /// Returns application's author [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Author
        {
            get
            {
                if (_Author == null)
                    _Author = new ValueCache();
                if (_Author.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Author);
                    string result = Internal.Instance.ReinterpreteString(str);
                    _Author.Value = result;
                }
                return _Author.Value as string;
            }
        }

        private ValueCache _Description = null;
        /// <summary>
        /// Returns application's description [Reinterpreted]
        /// </summary>
        /// <returns></returns>
        public string Description
        {
            get
            {
                if (_Description == null)
                    _Description = new ValueCache();
                if (_Description.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Description);
                    string result = Internal.Instance.ReinterpreteString(str);
                    _Description.Value = result;
                }
                return _Description.Value as string;
            }
        }

        private ValueCache _Version = null;
        /// <summary>
        /// Returns application version 
        /// </summary>
        /// <returns></returns>
        public string Version
        {
            get
            {
                if (_Version == null)
                    _Version = new ValueCache();
                if (_Version.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.Version);
                    _Version.Value = str;
                }
                return _Version.Value as string;
            }
        }

        private ValueCache _ImagePath = null;
        /// <summary>
        /// Returns path to EXE that is used to run the application.
        /// </summary>
        /// <returns></returns>
        public string ImagePath
        {
            get
            {
                if (_ImagePath == null)
                    _ImagePath = new ValueCache();
                if (_ImagePath.Cached == false)
                {
                    Internal.CheckInstance();
                    if (_appInfo == 0)
                        return "";
                    string str = Internal.Instance.ApplicationInfoGetString(_appInfo, (uint)AppInfoStringIndex.ImagePath);
                    _ImagePath.Value = str;
                }
                return _ImagePath.Value as string;
            }
        }

        /// <summary>
        /// Terminates application processes
        /// </summary>
        public void Terminate()
        {
            Internal.CheckInstance();
            if (_appInfo == 0)
                return;
            Internal.Instance.TerminateApplicationProcesses(ProductID().ToString());
        }
    }

    /// <summary>
    /// Association management class.
    /// </summary>
    public static class FileAssocation
    {

        /* all about HKCR\.smth */

        /// <summary>
        /// Returns handler class of certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetClass(string extension)
        {
            Internal.CheckInstance();
            return Internal.Instance.GetAssocationClass(extension, 0);
        }

        /// <summary>
        /// Returns backup handler class of certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetBackupClass(string extension)
        {
            Internal.CheckInstance();
            return Internal.Instance.GetAssocationClass(extension, 1);
        }

        /// <summary>
        /// Sets new handler class for certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool SetClass(string extension, string className)
        {
            Internal.CheckInstance();
            return Internal.Instance.SetAssocationClass(extension, className);
        }

        /// <summary>
        /// Backups handler class for certain extension.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool BackupClass(string extension)
        {
            Internal.CheckInstance();
            return Internal.Instance.BackupAssocationClass(extension);
        }

        /* all about HKCR\SomeClass */

        /// <summary>
        /// Creates a new handler class
        /// </summary>
        /// <param name="className"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool CreateClass(string className, string command)
        {
            Internal.CheckInstance();
            return Internal.Instance.CreateAssocationClass(className, command);
        }

        /// <summary>
        /// Removes certain handler class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static bool RemoveClass(string className)
        {
            Internal.CheckInstance();
            return Internal.Instance.RemoveAssocationClass(className);
        }
    }

    /// <summary>
    /// Native EXE runner.
    /// </summary>
    public class NativeExe
    {
        private string _fileName;
        private UInt32 _handle;

        private bool _wasCopied = false;


        /// <summary>
        /// Returns true if file is EXE7
        /// </summary>
        private bool IsExe7
        {
            get
            {
                if (_fileName == null)
                    return false;
                if (_fileName.Contains("."))
                {
                    string ext = _fileName.Substring(_fileName.LastIndexOf(".")).ToLower();
                    if (ext == ".exe7")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// File name
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }

        public NativeExe()
        {
        }

        public NativeExe(string newFileName)
        {
            FileName = newFileName;
        }

        ~NativeExe()
        {
            Cleanup();
        }

        /// <summary>
        /// Run application
        /// </summary>
        /// <param name="arguments">Arguments that will be passed to main()</param>
        /// <param name="accountName">Name of account in which an app will run</param>
        /// <param name="getHandle">Save handle or not (required for Wait())</param>
        /// <returns>true on success, false otherwise</returns>
        public bool Run(string arguments, string accountName = "", bool getHandle = false)
        {
            if (_fileName == null)
                return false;

            Internal.CheckInstance();

            string realFileName = FileName;
            if (IsExe7 == true)
            {
                realFileName = FileName.Substring(0, FileName.Length - 1);
                Functions.CopyFile(FileName, realFileName);
                _wasCopied = true;
            }

            UInt32 handle;
            if (Internal.Instance.CreateProcess(realFileName, arguments, accountName, out handle) == true)
            {
                if (getHandle == false)
                {
                    Internal.Instance.CloseHandle7(handle);
                    _handle = 0;
                }
                else
                {
                    _handle = handle;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes EXE7->EXE intermediate file if it was created.
        /// </summary>
        public void Cleanup()
        {
            if (IsExe7 && _wasCopied && FileName != null)
            {
                string realFileName = FileName;
                realFileName = FileName.Substring(0, FileName.Length - 1);
                Functions.RemoveFile(realFileName);
                _wasCopied = false;
            }
        }

        public const UInt32 WaitingFailed = 0xFFFFFFFF;

        /// <summary>
        /// Waits for EXE to close.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>Returns 0 on success, WaitingFailed on failure</returns>
        public UInt32 Wait(UInt32 timeout = 0xFFFFFFFF)
        {
            Internal.CheckInstance();
            if (_handle > 0)
            {
                UInt32 result = Internal.Instance.WaitForSingleObject7(_handle, timeout);
                return result;
            }
            return WaitingFailed;
        }
    }

    /// <summary>
    /// Functions class contains definitions for some needed functions which don't belong to Installer or Info classes.
    /// </summary>
    public static class Functions
    {
        #region "Sessions, tasks, pages"

        /// <summary>
        /// Starts new session with specified uri
        /// </summary>
        /// <param name="uri">URI</param>
        public static void LaunchSession(string uri)
        {
            Internal.CheckInstance();
            Internal.Instance.LaunchSessionByUri(uri);
        }

        /// <summary>
        /// Starts new sesson with specified appId and token.
        /// </summary>
        /// <param name="appId">Application ID</param>
        /// <param name="token">Application token</param>
        public static void LaunchSession(UInt64 appId, string token)
        {
            Internal.CheckInstance();
            Internal.Instance.LaunchSession(appId, token);
        }

        /// <summary>
        /// Starts specified control panel applet.
        /// </summary>
        /// <param name="cpl">Canonical applet name OR GUID OR empty string.</param>
        public static void LaunchCplApplet(string cpl)
        {
            Internal.CheckInstance();
            Internal.Instance.LaunchCplApplet(cpl);
        }

        /// <summary>
        /// Starts specified uri with certain arguments
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="args">Arguments</param>
        public static void LaunchTask(string uri, string args)
        {
            Internal.CheckInstance();
            LaunchTask(uri, args);
        }

        /// <summary>
        /// Checks application running in background
        /// </summary>
        /// <param name="uri">URI of foreground application</param>
        /// <param name="pageId">PageId of foreground application</param>
        public static void GetForegroundPageUri(out string uri, out string pageId)
        {
            Internal.CheckInstance();
            Internal.Instance.GetForegroundPageUri(out uri, out pageId);
        }

        #endregion 
        #region "Application lists"

        /// <summary>
        /// Returns list of all applications
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetAllApplications()
        {
            Internal.CheckInstance();
            UInt32 iterator = Internal.Instance.GetAllApplicationsIterator();
            return CollectListItems(iterator);
        }

        /// <summary>
        /// Returns list of certain hub type applications
        /// </summary>
        /// <param name="hubType"></param>
        /// <returns></returns>
        public static List<ApplicationInfo> GetAllApplications(UInt32 hubType)
        {
            Internal.CheckInstance();
            UInt32 iterator = Internal.Instance.GetApplicationsOfHubTypeIterator(hubType);
            return CollectListItems(iterator);
        }

        /// <summary>
        /// Returns list of all visible applications
        /// </summary>
        /// <returns></returns>
        public static List<ApplicationInfo> GetAllVisibleApplications()
        {
            Internal.CheckInstance();
            UInt32 iterator = Internal.Instance.GetAllVisibleApplicationsIterator();
            return CollectListItems(iterator);
        }


        private static List<ApplicationInfo> CollectListItems(UInt32 iterator)
        {
            List<ApplicationInfo> list = new List<ApplicationInfo>();
            if (iterator != 0)
            {
                while (true)
                {
                    UInt32 appInfo = Internal.Instance.GetNextApplication(iterator);
                    if (appInfo == 0)
                        break;
                    ApplicationInfo info = new ApplicationInfo(appInfo);
                    list.Add(info);
                }
                Internal.Instance.ReleaseIterator(iterator);
            }
            return list;
        }

        #endregion
        #region "Important WINAPI functions"

        /// <summary>
        /// Gets current locale code.
        /// </summary>
        /// <returns></returns>
        public static UInt32 GetLocaleId()
        {
            Internal.CheckInstance();
            return Internal.Instance.GetLocale();
        }

        /// <summary>
        /// Copy certain file to a new location. Replaces any file that would be there.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool CopyFile(string src, string dest)
        {
            Internal.CheckInstance();
            return Internal.Instance.CopyFile(src, dest);
        }

        /// <summary>
        /// Remove certain file.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static bool RemoveFile(string src)
        {
            Internal.CheckInstance();
            return Internal.Instance.RemoveFile(src);
        }

        /// <summary>
        /// Returns file content as a byte array.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] ReadFile(string src)
        {
            Internal.CheckInstance();
            IntPtr ptrContent;
            uint ptrLength = 0;
            Internal.Instance.GetFileContent(src, out ptrContent, out ptrLength);
            if (ptrContent == IntPtr.Zero || ptrLength == 0)
                return null;
            byte[] bContent = new byte[ptrLength];
            Microsoft.Phone.InteropServices.Marshal.Copy(ptrContent, bContent, 0, (int)ptrLength);
            return bContent;
        }

        #endregion 
        #region "Resource code"
        /// <summary>
        /// Replaces string written in "@AppResLib.dll,-123"-like form with correct information.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReinterpreteString(string str)
        {
            Internal.CheckInstance();
            string result = Internal.Instance.ReinterpreteString(str);
            if (result == null)
                result = "";
            return result;
        }
        #endregion
    }

    #region "Internal code"
    internal static class Internal
    {
     
    /// <summary>
    /// Checking if Instance is up and running, creating new Instance if it isn't.
    /// </summary>
    public static void CheckInstance()
    {
        if (Instance == null)
        {
            if (Initialize() == false)
            {
                //MessageBox.Show("Cannot instanciate Application API", "Application API", MessageBoxButton.OK);
                throw new Exception("Cannot instanciate Application API");
            }
        }
    }

    public static IXapHandler Instance = null;

    /// <summary>
    /// Initializes Application API Instance
    /// </summary>
    /// <remarks>
    /// This function is better to be executed on application startup.
    /// </remarks>
    /// <returns></returns>
    public static bool Initialize()
    {
        if (Instance == null)
        {
            uint retval = 0;
            try
            {
                retval = Microsoft.Phone.InteropServices.ComBridge.RegisterComDll("ApplicationApiNative" + _ApplicationApiGlobals.AppShortName + ".dll", new Guid("7E6418C7-C93F-4B82-947E-83FEA7A757CC"));
            }
            catch (Exception ex)
            {
                return false;
            }
            Instance = (IXapHandler)new CXapHandler();
            return (Instance != null) ? true : false;
        }
        return true;
    }

    #region "Common used internal code"

    /// <summary>
    /// Converts Guid to string.
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static string GuidToString(Guid guid)
    {
        if (guid == null)
            return "";
        return "{" + guid.ToString() + "}";
    }

    /// <summary>
    /// Normalizes guid string representation
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static string GuidToString(string guid)
    {
        if (guid != null)
        {
            if (!guid.StartsWith("{"))
                guid = "{" + guid;
            if (!guid.EndsWith("}"))
                guid = guid + "}";
        }
        return guid;
    }
    
    #endregion 

    }

    [ComImport, Guid("55D492CE-1269-4102-8079-5FC729F93FA3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IXapHandler
    {
        UInt32 BeginDeploy([MarshalAs(UnmanagedType.BStr)]string path, 
                            [MarshalAs(UnmanagedType.BStr)]string productID, 
                            [MarshalAs(UnmanagedType.BStr)]string InstanceID, 
                            [MarshalAs(UnmanagedType.BStr)]string offerID, 
                            [MarshalAs(UnmanagedType.Bool)]bool uninstallDisabled, 
                            [MarshalAs(UnmanagedType.Bool)]bool preInstall,
                            IntPtr license, 
                            [MarshalAs(UnmanagedType.U4)] uint licenseLength);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool IsInstalled(UInt32 context);

        [return: MarshalAs(UnmanagedType.U4)]
        UInt32 Install(UInt32 context, [MarshalAs(UnmanagedType.Bool)]bool update, [MarshalAs(UnmanagedType.Bool)]bool debugMode);

        [return: MarshalAs(UnmanagedType.U4)]
        UInt32 Uninstall(UInt32 context);

        UInt32 WaitForEvents(UInt32 context, UInt32 timeout, [MarshalAs(UnmanagedType.U4)] out InstallationState state, [MarshalAs(UnmanagedType.U4)] out uint progress, [MarshalAs(UnmanagedType.U4)] out uint hResult);

        void DecodeState(UInt32 data, out UInt32 state, out UInt32 progress, out UInt32 error);

        UInt32 EncodeState(UInt32 state, UInt32 progress, UInt32 error);

        void EndDeploy(UInt32 context);

        void LaunchSessionByUri([MarshalAs(UnmanagedType.BStr)]string uri);
        void LaunchSession([MarshalAs(UnmanagedType.U8)] UInt64 ulAppId, [MarshalAs(UnmanagedType.BStr)]string pszTokenId);
        void LaunchCplApplet([MarshalAs(UnmanagedType.BStr)]string pszCpl);
        void LaunchTask([MarshalAs(UnmanagedType.BStr)]string pszTaskUri, [MarshalAs(UnmanagedType.BStr)]string pszCmdLineArguments);
        void GetForegroundPageUri([MarshalAs(UnmanagedType.BStr)] out string szTaskUri, [MarshalAs(UnmanagedType.BStr)] out  string szPageId);

        UInt32 GetApplicationInfo([MarshalAs(UnmanagedType.BStr)]string guidString);
        void ReleaseApplicationInfo(UInt32 appInfo);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ApplicationInfoGetGuid(UInt32 appInfo, UInt32 dwIndex);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ApplicationInfoGetString(UInt32 appInfo, UInt32 dwIndex);

        UInt32 ApplicationInfoGetInteger(UInt32 appInfo, UInt32 dwIndex);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ApplicationInfoGetInvocationInfo(UInt32 appInfo, [MarshalAs(UnmanagedType.BStr)] out string pszUri, [MarshalAs(UnmanagedType.BStr)] out string pszParameters);

        UInt32 GetAllApplicationsIterator();
        UInt32 GetAllVisibleApplicationsIterator();
        UInt32 GetApplicationsOfHubTypeIterator(UInt32 hubType);
        UInt32 GetNextApplication(UInt32 iterator);

        void ReleaseIterator(UInt32 iterator);

        [return: MarshalAs(UnmanagedType.BStr)]
        string ReinterpreteString([MarshalAs(UnmanagedType.BStr)] string oldString);

        void UpdateAppIconPath(string guid, [MarshalAs(UnmanagedType.BStr)]string path);
        void TerminateApplicationProcesses([MarshalAs(UnmanagedType.BStr)]string guid);

        UInt32 GetLocale();

        [return: MarshalAs(UnmanagedType.BStr)]
        string GetAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension, int type);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool SetAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension, [MarshalAs(UnmanagedType.BStr)]string className);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool BackupAssocationClass([MarshalAs(UnmanagedType.BStr)]string extension);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CreateAssocationClass([MarshalAs(UnmanagedType.BStr)]string className, [MarshalAs(UnmanagedType.BStr)]string openCommand);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveAssocationClass([MarshalAs(UnmanagedType.BStr)]string className);
        
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CopyFile([MarshalAs(UnmanagedType.BStr)] string sourceFile, [MarshalAs(UnmanagedType.BStr)] string destFile);
        [return: MarshalAs(UnmanagedType.Bool)]
        bool RemoveFile([MarshalAs(UnmanagedType.BStr)] string sourceFile);


        [return: MarshalAs(UnmanagedType.Bool)]
        bool CreateProcess([MarshalAs(UnmanagedType.BStr)] string path, [MarshalAs(UnmanagedType.BStr)] string arguments, [MarshalAs(UnmanagedType.BStr)] string accountName, out UInt32 handle);
        
        UInt32 WaitForSingleObject7(UInt32 handle, UInt32 dwMilliseconds);
        
        [return: MarshalAs(UnmanagedType.Bool)]
        bool CloseHandle7(UInt32 handle);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ProcessProvxmlFile([MarshalAs(UnmanagedType.BStr)]string filePath);

        [return: MarshalAs(UnmanagedType.Bool)]
        bool ProcessProvxmlPlainText([MarshalAs(UnmanagedType.BStr)]string filePath);

        void GetFileContent([MarshalAs(UnmanagedType.BStr)]string filePath, 
                            out IntPtr array, 
                            out uint dwLength);

        
        IntPtr AllocMem([MarshalAs(UnmanagedType.U4)] uint count);

        void FreeMem(IntPtr array);
    }


    [ComImport, ClassInterface(ClassInterfaceType.None), Guid("7E6418C7-C93F-4B82-947E-83FEA7A757CC")]
    public class CXapHandler
    {
    }

    #endregion 

}

