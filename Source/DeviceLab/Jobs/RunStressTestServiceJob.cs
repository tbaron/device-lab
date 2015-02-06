namespace InfoSpace.DeviceLab.Jobs
{
    public class RunStressTestServiceJob : ServiceJob
    {
        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                obj is RunStressTestServiceJob;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
