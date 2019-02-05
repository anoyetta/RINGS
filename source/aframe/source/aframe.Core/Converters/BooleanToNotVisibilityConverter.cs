using System.Windows;

namespace aframe
{
    public class BooleanToNotVisibilityConverter :
        BooleanConverter<Visibility>
    {
        public BooleanToNotVisibilityConverter() :
            base(Visibility.Collapsed, Visibility.Visible)
        {
        }
    }
}
