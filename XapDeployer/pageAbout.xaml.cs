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
using Microsoft.Phone.Controls;

namespace XapDeployer
{
    public partial class pageAbout : PhoneApplicationPage
    {
        public pageAbout()
        {
            InitializeComponent();
        }

        private void GestureListener_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/pageLicense.xaml", UriKind.Relative));
        }
    }
}