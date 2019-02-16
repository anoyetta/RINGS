using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using aframe.Updater;

namespace aframe
{
    public static class AssemblyExtensions
    {
        public static string GetLocationDirectory(
            this Assembly asm)
            => Path.GetDirectoryName(asm.Location);

        public static string GetTitle(
            this Assembly asm)
        {
            var att = asm.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            return att.Length == 0 ? string.Empty : ((AssemblyTitleAttribute)att[0]).Title;
        }

        public static string GetProduct(
            this Assembly asm)
        {
            var att = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            return att.Length == 0 ? string.Empty : ((AssemblyProductAttribute)att[0]).Product;
        }

        public static string GetDescription(
            this Assembly asm)
        {
            var att = asm.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            return att.Length == 0 ? string.Empty : ((AssemblyDescriptionAttribute)att[0]).Description;
        }

        public static string GetCopyright(
            this Assembly asm)

        {
            var att = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            return att.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)att[0]).Copyright;
        }

        public static string GetConfiguration(
            this Assembly asm)

        {
            var att = asm.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            return att.Length == 0 ? string.Empty : ((AssemblyConfigurationAttribute)att[0]).Configuration;
        }

        public static ReleaseChannels GetReleaseChannels(
            this Assembly asm)
        {
            var result = ReleaseChannels.Stable;
            var config = asm.GetConfiguration();
            Enum.TryParse(config, out result);
            return result;
        }

        public static Version GetVersion(
            this Assembly asm)
            => asm.GetName().Version;

        public static string Guid(
            this Assembly asm)
        {
            var att = asm.GetCustomAttributes(typeof(GuidAttribute), false);
            return att.Length == 0 ? string.Empty : ((GuidAttribute)att[0]).Value;
        }
    }
}
