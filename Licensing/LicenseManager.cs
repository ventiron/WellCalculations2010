using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Media.Imaging;

namespace WellCalculations2010.Licensing
{
    internal class LicenseManager
    {
        private string _copyKeyPattern = "[a-z A-Z 0-9]{4}-[a-z A-Z 0-9]{6}:[a-z A-Z 0-9]{8}";
        private string _fullVersionKeyPattern = "";

        public LicenseManager() 
        {

        }

        public void CheckAndCreateKeyInRegistry()
        {
            //RegistryKey helloKey = Registry.LocalMachine.OpenSubKey("SOFTWARE", true);
            //helloKey.CreateSubKey("InenTiron");
            //helloKey = helloKey.OpenSubKey("InenTiron", true);
            //helloKey.SetValue("login", "admin");
            //helloKey.SetValue("password", "12345");
            //helloKey.Close();

            //RegistryKey myKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            //myKey = myKey.OpenSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);

            //if (myKey != null)
            //{
            //    myKey.SetValue("DefaultPrinterId", ldiPrinters[e.RowIndex].id, RegistryValueKind.String);
            //    myKey.Close();
            //}
        }

        public LicenseKey CreateKey()
        {
            return new LicenseKey();
        }
    }
}
