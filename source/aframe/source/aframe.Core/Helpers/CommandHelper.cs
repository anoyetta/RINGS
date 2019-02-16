using System;
using System.Windows;
using System.Windows.Media;

namespace aframe
{
    public static class CommandHelper
    {
        public static async void ExecuteChangeColor(
            Func<Color> getter,
            Action<Color> setter,
            bool ignoreAlpha = true,
            Window owner = null)
        {
            ColorDialog.Color = getter();
            ColorDialog.IgnoreAlpha = ignoreAlpha;

            if (owner == null)
            {
                owner = WPFHelper.MainWindow;
            }

            var result = await WPFHelper.Dispatcher.InvokeAsync(() =>
                ColorDialog.ShowDialog(owner) ?? false);

            if (result)
            {
                setter(ColorDialog.Color);
            }
        }

        public static async void ExecuteChangeFont(
            Func<FontInfo> getter,
            Action<FontInfo> setter,
            Window owner = null)
        {
            FontDialog.Font = getter();

            if (owner == null)
            {
                owner = WPFHelper.MainWindow;
            }

            var result = await WPFHelper.Dispatcher.InvokeAsync(() =>
                FontDialog.ShowDialog(owner) ?? false);

            if (result)
            {
                setter(FontDialog.Font);
            }
        }
    }
}
