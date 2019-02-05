using System;
using System.Text;

namespace aframe
{
    public static class ExceptionExtensions
    {
        public static string ToFormat(
            this Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine(ex.GetType().ToString() + " :");
            sb.AppendLine(ex.Message);

            if (ex.StackTrace != null)
            {
                sb.AppendLine();
                sb.AppendLine("Stack Trace :");
                sb.AppendLine(ex.StackTrace);
            }

            if (ex.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner Excepsion :");
                sb.AppendLine(ex.InnerException.ToFormat());
            }

            return sb.ToString();
        }
    }
}
