using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace WinTerMul.Common.Logging
{
    internal class FileLogger : ILogger
    {
        private readonly static object Lock = new object();

        private readonly string _logPath;

        public FileLogger(IWinTerMulConfiguration configuration)
        {
            _logPath = configuration.LogPath;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Not supported.
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var stackFrameInfo = GetStackFrameInfo();
            var log = $"{timestamp}|{logLevel}|{stackFrameInfo}|{state}";

            if (exception != null)
            {
                var serializedException = JsonConvert.SerializeObject(
                    CreateLogFriendlyException(exception),
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })
                    .Replace(@"\\", @"\")
                    .Replace(@"\r\n", Environment.NewLine);

                log += "|" + serializedException;
            }

            using (var mutex = new Mutex(false, "WinTerMul.Common.Logging.FileLogger.Log"))
            {
                mutex.WaitOne();
                File.AppendAllText(_logPath, log + Environment.NewLine);
                mutex.ReleaseMutex();
            }
        }

        private dynamic CreateLogFriendlyException(Exception exception)
        {
            var innerException = exception.InnerException == null
                ? null
                : CreateLogFriendlyException(exception.InnerException);

            return new
            {
                exception.Message,
                ExceptionType = exception.GetType(),
                InnerException = innerException,
                StackTrace = Environment.NewLine + exception.StackTrace
            };
        }

        private string GetStackFrameInfo()
        {
            // Only support for stack frame when log is called from Microsoft.Extensions.Logging.LoggerExtensions.
            // Otherwise the stack frame info will be wrong.
            if (!IsCalledFromLoggerExtensions())
            {
                // Stack frame info not supported for this case.
                return "";
            }

            const int skipFrames = 5;
            var stackFrame = new StackFrame(skipFrames, true);
            var callerMethod = stackFrame.GetMethod();
            var fullClassName = callerMethod.ReflectedType.Namespace + "." + callerMethod.ReflectedType.Name;

            var stackFrameInfo = "";
            stackFrameInfo += $"{fullClassName}:";
            stackFrameInfo += $"{callerMethod.Name}():";
            stackFrameInfo += $"L{stackFrame.GetFileLineNumber()}:";
            stackFrameInfo += $"C{stackFrame.GetFileColumnNumber()}";
            return stackFrameInfo;
        }

        private bool IsCalledFromLoggerExtensions()
        {
            const int skipFrames = 4;
            var callerMethod = new StackFrame(skipFrames, true).GetMethod();
            var fullClassName = callerMethod.ReflectedType.Namespace + "." + callerMethod.ReflectedType.Name;
            return fullClassName == typeof(LoggerExtensions).FullName;
        }
    }
}
