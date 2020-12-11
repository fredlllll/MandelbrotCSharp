using System.Collections.Generic;

namespace MandelSpeedTest
{
    class ArrayPool<T>
    {
        private readonly Dictionary<int, List<T[]>> arrays = new Dictionary<int, List<T[]>>();

        public T[] Get(int size)
        {
            List<T[]> arrs = null;
            lock (arrays)
            {
                if (!arrays.TryGetValue(size, out arrs))
                {
                    arrs = new List<T[]>();
                    arrays[size] = arrs;
                }
            }
            lock (arrs)
            {
                T[] retval = null;
                if(arrs.Count > 0)
                {
                    retval = arrs[arrs.Count - 1];
                    arrs.RemoveAt(arrs.Count - 1);
                }
                else
                {
                    retval = new T[size];
                }
                return retval;
            }
        }

        public void Put(T[] arr)
        {
            List<T[]> arrs = null;
            lock (arrays)
            {
                if (!arrays.TryGetValue(arr.Length, out arrs))
                {
                    arrs = new List<T[]>();
                    arrays[arr.Length] = arrs;
                }
            }
            lock (arrs)
            {
                arrs.Add(arr);
            }
        }
    }
}
