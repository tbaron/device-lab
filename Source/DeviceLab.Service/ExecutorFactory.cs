using System;

namespace InfoSpace.DeviceLab.Service
{
    public class ExecutorFactory
    {
        private readonly static bool isWindows = IsRunningOnWindows();

        public IExecutor CreateExecutor()
        {
            return isWindows
                ? (IExecutor)new WindowsExecutor()
                : (IExecutor)new UnixExecutor();
        }

        private static bool IsRunningOnWindows()
        {
            switch (Environment.OSVersion.Platform)
            {
                case (PlatformID.MacOSX):
                case (PlatformID.Unix):
                    return false;
                default:
                    return true;
            }
        }
    }
}

