using System.Threading.Tasks;
using InfoSpace.DeviceLab.Jobs;

namespace InfoSpace.DeviceLab
{
    public interface ICoreFeatures
    {
        Task LaunchApk(RunAppServiceJob job);

        Task LaunchStressTest(RunStressTestServiceJob job);

        Task LaunchUrl(RunUrlServiceJob job);
    }
}
