using System;
using System.Collections.Generic;

namespace MandelbrotCSharp
{
    class ObjectPool<T, HashType>
    {
        private readonly Dictionary<HashType, List<T>> objects = new Dictionary<HashType, List<T>>();
        private readonly Func<HashType, T> creator;
        private readonly Func<T, HashType> hashGetter;

        public ObjectPool(Func<HashType, T> creator, Func<T, HashType> hashGetter)
        {
            this.creator = creator;
            this.hashGetter = hashGetter;
        }

        public T Get(HashType hash)
        {
            List<T> objs = null;
            lock (objects)
            {
                if (!objects.TryGetValue(hash, out objs))
                {
                    objs = new List<T>();
                    objects[hash] = objs;
                }
            }
            lock (objs)
            {
                T retval = default;
                if (objs.Count > 0)
                {
                    retval = objs[objs.Count - 1];
                    objs.RemoveAt(objs.Count - 1);
                }
                else
                {
                    retval = creator(hash);
                }
                return retval;
            }
        }

        public void Put(T obj)
        {
            List<T> objs = null;
            lock (objects)
            {
                var hash = hashGetter(obj);
                if (!objects.TryGetValue(hash, out objs))
                {
                    objs = new List<T>();
                    objects[hash] = objs;
                }
            }
            lock (objs)
            {
                objs.Add(obj);
            }
        }
    }
}
