using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace aframe
{
    public static class WebBrowserHelper
    {
        public static void SetUseNewestWebBrowser()
        {
            var filename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            if (filename.Contains("vhost"))
            {
                filename = filename.Substring(0, filename.IndexOf('.') + 1) + "exe";
            }

            var key1 = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION");
            var key2 = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BEHAVIORS");
            key1?.SetValue(filename, 11001, RegistryValueKind.DWord);
            key2?.SetValue(filename, 11001, RegistryValueKind.DWord);
            key1?.Close();
            key2?.Close();
        }
    }
}
