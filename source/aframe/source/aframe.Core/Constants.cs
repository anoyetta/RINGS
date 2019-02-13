namespace aframe
{
    public static class Constants
    {
        /// <summary>
        /// フォントサイズの拡大基本レート
        /// </summary>
        public const double FontSizeScalingRate = 0.1;

        public static double ScalingL => 1.0 + (Constants.FontSizeScalingRate * 1.0);

        public static double ScalingXL => 1.0 + (Constants.FontSizeScalingRate * 2.0);

        public static double ScalingS => 1.0 - (Constants.FontSizeScalingRate * 1.0);

        public static double ScalingXS => 1.0 - (Constants.FontSizeScalingRate * 2.0);
    }
}
