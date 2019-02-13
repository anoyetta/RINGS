using System.Windows;
using System.Windows.Documents;

namespace aframe
{
    public static class DocumentHelper
    {
        public static readonly DependencyProperty VisibleProperty =
            DependencyProperty.RegisterAttached(
                "Visible",
                typeof(bool),
                typeof(DocumentHelper),
                new FrameworkPropertyMetadata(
                    true,
                    new PropertyChangedCallback(OnVisibilityChanged)));

        public static bool GetVisible(Run d)
            => (bool)d.GetValue(VisibleProperty);

        public static void SetVisible(Run d, bool value)
            => d.SetValue(VisibleProperty, value);

        private static void OnVisibilityChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is Run run)
            {
                if ((bool)e.NewValue == true)
                {
                    if (run.Tag != null)
                    {
                        run.FontSize = (double)run.Tag;
                    }
                }
                else
                {
                    run.Tag = run.FontSize;
                    run.FontSize = 0.004;
                }
            }
        }
    }
}
