using System.Windows;

namespace aframe
{
    public class BooleanToHiddenConverter :
        BooleanConverter<Visibility>
    {
        public BooleanToHiddenConverter() :
            base(Visibility.Visible, Visibility.Hidden)
        {
        }
    }
}
