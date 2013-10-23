/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System.ComponentModel;
using System.Runtime.Serialization;

namespace XapDeployer
{
    [DataContract]
    public class BaseViewModel : INotifyPropertyChanged
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
}
