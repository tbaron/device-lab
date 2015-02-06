using System.Threading;

namespace InfoSpace.DeviceLab
{
    /// <summary>
    /// A lock that remains locked after a single entry.
    /// </summary>
    public class SingleUseLock
    {
        private const int Unset = 0;
        private const int Set = 1;

        private int value;

        /// <summary>
        /// Try to enter the lock and close the lock if successful.
        /// </summary>
        /// <returns>True if lock successfully entered, false otherwise.</returns>
        public bool TryEnter()
        {
            return Interlocked.Exchange(ref value, Set) == Unset;
        }

        /// <summary>
        /// Reset the lock to an open state.
        /// </summary>
        public void Reset()
        {
            value = Unset;
        }
    }
}
