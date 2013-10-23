/*
 * XAP Deployer project
 * (C) ultrashot 2011-2013
 * 
 * See "XAPDeployer - Terms of usage.txt".
*/
using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using System.Windows;

namespace XapDeployer
{
    public static class Utils
    {
        public static void CleanPageStack(PhoneApplicationPage page)
        {
                while (true)
                {
                    if (page.NavigationService.CanGoBack)
                        page.NavigationService.RemoveBackEntry();
                    else
                        break;
                }
        }

        public static void CleanPageStack(NavigationService srv)
        {
            while (true)
            {
                if (srv.CanGoBack)
                    srv.RemoveBackEntry();
                else
                    break;
            }
        }


        #region "Light theme handling"

        private static uint lightThemeChecked = 0;
        public static bool IsLightTheme()
        {
            if (lightThemeChecked == 0)
            {
                var visibility = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"];
                if (visibility == Visibility.Visible)
                    lightThemeChecked = 1;
                else
                    lightThemeChecked = 2;
            }
            return (lightThemeChecked == 1) ? true : false;
        }

        #endregion

    }
}
