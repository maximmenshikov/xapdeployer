/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Windows;
using ApplicationApi;
using System.Runtime.Serialization;

namespace XapDeployer
{
    [DataContract]
    public class XapReaderViewModelEx : XapReaderViewModel
    {

        public XapReaderViewModelEx() : base()
        {
        }

        public XapReaderViewModelEx(string fileName) : base(fileName)
        {
        }

        #region "Properties useful for views only"

        private bool _isInstalling = false;
        [DataMember]
        public bool IsInstalling
        {
            get
            {
                return _isInstalling;
            }
            set
            {
                _isInstalling = value;
                OnChange("IsInstalling");
                OnChange("IsInstallingVisibility");
            }
        }
        [IgnoreDataMember]
        public Visibility IsInstallingVisibility
        {
            get
            {
                return _isInstalling ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool _switchEnabled = true;
        [DataMember]
        public bool SwitchEnabled
        {
            get
            {
                return _switchEnabled;
            }
            set
            {
                _switchEnabled = value;
                OnChange("SwitchEnabled");
            }
        }

        private bool? _checkedState = true;
        [DataMember]
        public bool? CheckedState
        {
            get
            {
                return _checkedState;
            }
            set
            {
                _checkedState = value;
                OnChange("CheckedState");
            }
        }

        private int _progress;
        [IgnoreDataMember]
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnChange("Progress");
            }
        }

        private Visibility _installationCompletedVisibility = Visibility.Collapsed;
        [DataMember]
        public Visibility InstallationCompletedVisibility
        {
            get
            {
                return _installationCompletedVisibility;
            }
            set
            {
                _installationCompletedVisibility = value;
                OnChange("InstallationCompletedVisibility");
            }
        }

        private string _state;
        
        [DataMember]
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnChange("State");
            }
        }

        #endregion

        #region "User data"

        private XapAdditionData _userData = null;
        [DataMember]
        public XapAdditionData UserData
        {
            get
            {
                return _userData;
            }
            set
            {
                _userData = value;
                OnChange("UserData");
            }
        }

        #endregion

    }

    [DataContract]
    public class XapAdditionData
    {
        [DataMember]
        public string LicenseFile { get; set; }

        [DataMember]
        public string InstanceID { get; set; }

        [DataMember]
        public string OfferID { get; set; }
    }


}
