namespace InfoSpace.DeviceLab.Status
{
    public interface IStatusConsumer
    {
        void ReportStatus(ServiceStatus status);
    }
}
