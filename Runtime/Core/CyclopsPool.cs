// Cyclops Framework
// 
// Copyright 2010 - 2022 Mark Davis
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;

namespace Smonch.CyclopsFramework
{
    public class CyclopsPool<T> where T : class
    {
        private readonly ConcurrentDictionary<Type, ConcurrentBag<T>> _table;

        public CyclopsPool(int concurrencyLevel = 1, int initialCapacity = 256)
        {
            _table = new ConcurrentDictionary<Type, ConcurrentBag<T>>(concurrencyLevel, initialCapacity);
        }

        /// <summary>
        /// <para>This provides a thread-safe way to instantiate and reuse pooled objects of a particular type.</para>
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="valueFactory"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool Rent<TS>(Func<TS> valueFactory, out TS result) where TS : T
        {
            bool wasFound = true;

            if (!(_table.TryGetValue(typeof(TS), out var bag) && bag.TryTake(out T o)))
            {
                wasFound = false;
                o = valueFactory();
            }

            result = (TS)o;

            return wasFound;
        }

        public void Release<TS>(TS o) where TS : T
        {
            if (!_table.TryGetValue(typeof(TS), out var bag))
            {
                bag = new ConcurrentBag<T>();

                // Result is ignored because if it doesn't work, it's not a problem.
                _ = _table.TryAdd(typeof(TS), bag);
            }

            // Note: This is not a guarantee that the bag was actually added to the dictionary and that's fine.
            bag.Add(o);
        }
    }
}
