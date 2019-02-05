namespace aframe
{
    public class BooleanToIntConverter :
        BooleanConverter<int>
    {
        public BooleanToIntConverter() :
            base(1, 0)
        {
        }
    }
}
