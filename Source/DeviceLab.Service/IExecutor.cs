using System;
using System.Threading.Tasks;

namespace InfoSpace.DeviceLab.Service
{
    public interface IExecutor
    {
        string Output { get; }

        string Error { get; }

        string Command { get; }

        Task Execute(string command);
    }
}
