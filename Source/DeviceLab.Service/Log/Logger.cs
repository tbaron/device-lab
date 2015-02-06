using System;

namespace InfoSpace.DeviceLab.Service.Log
{
    public static class Logger
    {
        private static LogLevel logLevel;

        public static void SetLogLevel(LogLevel level)
        {
            logLevel = level;
        }

        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public static void Debug(Func<string> messageFactory)
        {
            Log(LogLevel.Debug, messageFactory);
        }

        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public static void Error(Func<string> messageFactory)
        {
            Log(LogLevel.Error, messageFactory);
        }

        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, format, args);
        }

        public static void Info(Func<string> messageFactory)
        {
            Log(LogLevel.Info, messageFactory);
        }

        private static void Log(LogLevel level, string message)
        {
            if (IsValidLevel(level))
            {
                WriteToConsole(level, message);
            }
        }

        private static void Log(LogLevel level, string format, params object[] args)
        {
            if (IsValidLevel(level))
            {
                WriteToConsole(level, String.Format(format, args));
            }
        }

        private static void Log(LogLevel level, Func<string> messageFactory)
        {
            if (IsValidLevel(level))
            {
                WriteToConsole(logLevel, messageFactory());
            }
        }

        private static bool IsValidLevel(LogLevel level)
        {
            if (level <= LogLevel.None)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            return level <= logLevel;
        }

        private static void WriteToConsole(LogLevel level, string message)
        {
            message = String.Format("[{0}; {1:yyyy-MM-dd-HHmmss}] {2}",
                level,
                DateTime.Now,
                message);

            Console.WriteLine(message);
        }
    }
}
