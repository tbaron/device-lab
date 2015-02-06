using System.Collections.Generic;

namespace InfoSpace.DeviceLab
{
    public static class LinqExtensions
    {
        /// <see>http://www.make-awesome.com/2010/08/batch-or-partition-a-collection-with-linq</see>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            List<T> nextbatch = new List<T>(batchSize);
            foreach (T item in collection)
            {
                nextbatch.Add(item);
                if (nextbatch.Count == batchSize)
                {
                    yield return nextbatch;
                    nextbatch = new List<T>(batchSize);
                }
            }
            if (nextbatch.Count > 0)
            {
                yield return nextbatch;
            }
        }
    }
}
