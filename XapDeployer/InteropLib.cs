/*
 * Interop Library by ultrashot.
 * Squeezed version.
 * 
 * Isn't for usage in other projects.
*/
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace XapDeployer
{
    public class InteropLib
    {

        /// <summary>
        /// MAKE SURE YOU CHANGE IT!
        /// <remarks>
        /// This is against of DLL-hell issue that still exists in WP7
        /// </remarks>
        /// </summary>
        private const string AppShortName = "XPD";


        private static IInteropClass instance = null;

        static InteropLib()
        {
            if (_checkInstance() == false)
            {

            }
        }

        /// <summary>
        /// Returns TRUE if application has root permissions
        /// </summary>
        /// <returns></returns>
        public static bool HasRootAccess()
        {
            if (_checkInstance() == false)
            {
                return false;
            }
            return instance.HasRootAccess();
        }

        /// <summary>
        /// Checking if instance is up and running, creating new instance if it isn't.
        /// </summary>
        private static bool _checkInstance()
        {
            if (instance == null)
            {
                if (Initialize() == false)
                    return false;
            }
            return true;
        }

        public class FileEntry
        {
            private string _fileName = "";
            private bool _isDirectory = false;

            public string FileName 
            { 
                get 
                { 
                    if (_fileName == null) 
                        return ""; 
                return _fileName; 
                } 
                set 
                { 
                    _fileName = value; 
                } 
            }
            public bool IsDirectory 
            { 
                get 
                { 
                    return _isDirectory; 
                } 
                set 
                {
                    _isDirectory = value;
                } 
            }
        }

        public class FileEntryComparer : IComparer<FileEntry>
        {

            public int Compare(FileEntry x, FileEntry y)
            {
                if (x.IsDirectory == true && y.IsDirectory == false)
                    return -1;
                else if (x.IsDirectory == false && y.IsDirectory == true)
                    return 1;
                else 
                {
                    return x.FileName.CompareTo(y.FileName);
                }
            }
        }

        private static Object getContentLock = new Object();

        /// <summary>
        /// Returns folder content.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static List<FileEntry> GetContent(string folder)
        {
            lock (getContentLock)
            {
                if (instance == null)
                {
                    return null;
                }
                var list = new List<FileEntry>();

                string n = instance.GetFolder(folder);
                if (n != null)
                {
                    if (n.Length > 0)
                    {
                        string[] fnames = n.Split('|');
                        foreach (var fname in fnames)
                        {
                            var fe = new FileEntry();
                            fe.FileName = fname.Substring(0, fname.IndexOf(":"));
                            fe.IsDirectory = (fname.Substring(fname.IndexOf(":") + 1) == "dir") ? true : false;
                            list.Add(fe);
                        }
                    }
                }
                list.Sort(new FileEntryComparer());
                return list;
            }
        }

        /// <summary>
        /// Initializes Application API instance
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            uint retval = 0;
            try
            {
                retval = Microsoft.Phone.InteropServices.ComBridge.RegisterComDll("InteropLib" + AppShortName + ".dll", new Guid("070E61BC-473F-4215-8A31-02206D9D15F2"));
            }
            catch (Exception ex)
            {
                instance = null;
                return false;
            }
            instance = (IInteropClass)new CInteropClass();
            return (instance != null) ? true : false;
        }

        [ComImport, ClassInterface(ClassInterfaceType.None), Guid("070E61BC-473F-4215-8A31-02206D9D15F2")]
        public class CInteropClass
        {
        }

        [ComImport, Guid("3B57C022-D53F-4CCA-8DAD-3E12235BE8AE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IInteropClass
        {
            [return: MarshalAs(UnmanagedType.Bool)]
           	bool HasRootAccess();

            [return: MarshalAs(UnmanagedType.BStr)]
            string GetFolder([MarshalAs(UnmanagedType.BStr)] string path); 
        }
    }
}
