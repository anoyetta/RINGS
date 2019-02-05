using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace aframe
{
    public static partial class AppLogger
    {
        public static void WriteApiErrorLog(
            ControllerBase controller,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            var caller = string.Empty;
            if (!string.IsNullOrEmpty(callerMemberName) &&
                !string.IsNullOrEmpty(callerFilePath))
            {
                caller = $"{Path.GetFileNameWithoutExtension(callerFilePath)} {callerMemberName}()";
            }

            var uri = default(string);
            var request = controller.Request;
            if (request != null)
            {
                uri = $"{request.Method} {request.Path}{request.QueryString}";
            }

            AppLogger.Error(
                uri != null ? $"[{caller}] {uri}" : $"[{caller}]",
                exception);
        }
    }
}
