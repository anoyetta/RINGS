using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace aframe
{
    public static partial class AppLogger
    {
        public delegate bool ExceptionHandler(string methodName, Exception catchedException);

        public static Action Wrap(
            Func<Task> action,
            ExceptionHandler exceptionHandler = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
            => new Action(async () =>
            {
                var caller = string.Empty;
                if (!string.IsNullOrEmpty(callerMemberName) &&
                    !string.IsNullOrEmpty(callerFilePath))
                {
                    caller = $"{Path.GetFileNameWithoutExtension(callerFilePath)} {callerMemberName}()";
                }

                try
                {
                    AppLogger.Write($"{caller} begin.");
                    await action();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{caller} catched exception.", ex);

                    var handled = false;
                    if (exceptionHandler != null)
                    {
                        handled = exceptionHandler.Invoke(caller, ex);
                    }

                    if (!handled)
                    {
                        throw;
                    }
                }
                finally
                {
                    AppLogger.Write($"{caller} end.");
                }
            });

        public static Action Wrap(
            Action action,
            ExceptionHandler exceptionHandler = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
            => new Action(() =>
            {
                var caller = string.Empty;
                if (!string.IsNullOrEmpty(callerMemberName) &&
                    !string.IsNullOrEmpty(callerFilePath))
                {
                    caller = $"{Path.GetFileNameWithoutExtension(callerFilePath)} {callerMemberName}()";
                }

                try
                {
                    AppLogger.Write($"{caller} begin.");
                    action();
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{caller} catched exception.", ex);

                    var handled = false;
                    if (exceptionHandler != null)
                    {
                        handled = exceptionHandler.Invoke(caller, ex);
                    }

                    if (!handled)
                    {
                        throw;
                    }
                }
                finally
                {
                    AppLogger.Write($"{caller} end.");
                }
            });

        public static void RunWrap(
            Func<Task> action,
            ExceptionHandler exceptionHandler = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
            => Wrap(action, exceptionHandler, callerMemberName, callerFilePath).Invoke();

        public static void RunWrap(
            Action action,
            ExceptionHandler exceptionHandler = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
            => Wrap(action, exceptionHandler, callerMemberName, callerFilePath).Invoke();
    }
}
