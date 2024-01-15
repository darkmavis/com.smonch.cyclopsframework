// Cyclops Framework
// 
// Copyright 2010 - 2024 Mark Davis
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
using NUnit.Framework;

namespace Smonch.CyclopsFramework
{
    public class CyclopsWaitForEvent : CyclopsRoutine
    {
        private Action _multicastDelegate;
        private Action _handler;
        
        public static CyclopsWaitForEvent Instantiate(
            ref Action multicastDelegate,
            double timeout = double.MaxValue,
            double maxCycles = 1.0,
            Action handler = null)
        {
            var result = InstantiateFromPool<CyclopsWaitForEvent>(timeout, maxCycles);
            multicastDelegate += result.OnEvent;
            result._multicastDelegate = multicastDelegate;
            result._handler = handler;
            Assert.IsNotNull(result._multicastDelegate, "Multicast delegate must not be null.");
            return result;
        }
        
        private void OnEvent()
        {
            if (!WasEntered)
                return;
            
            _handler?.Invoke();
            StepForward();
        }
        
        protected override void OnRecycle()
        {
            _multicastDelegate = null;
            _handler = null;
        }
    }
    
    public class CyclopsWaitForEvent<T> : CyclopsRoutine
    {
        private Action<T> _multicastDelegate;
        private Action<T> _handler;
        
        public static CyclopsWaitForEvent<T> Instantiate(
            ref Action<T> multicastDelegate,
            double timeout = double.MaxValue,
            double maxCycles = 1.0,
            Action<T> handler = null)
        {
            var result = InstantiateFromPool<CyclopsWaitForEvent<T>>(timeout, maxCycles);
            multicastDelegate += result.OnEvent;
            result._multicastDelegate = multicastDelegate;
            result._handler = handler;
            Assert.IsNotNull(result._multicastDelegate, "Multicast delegate must not be null.");
            return result;
        }
        
        private void OnEvent(T x)
        {
            if (!WasEntered)
                return;
            
            _handler?.Invoke(x);
            StepForward();
        }

        protected override void OnRecycle()
        {
            _multicastDelegate = null;
            _handler = null;
        }
    }
    
    public class CyclopsWaitForEvent<T1, T2> : CyclopsRoutine
    {
        private Action<T1, T2> _multicastDelegate;
        private Action<T1, T2> _handler;
        
        public static CyclopsWaitForEvent<T1, T2> Instantiate(
            ref Action<T1, T2> multicastDelegate,
            double timeout = double.MaxValue,
            double maxCycles = 1.0,
            Action<T1, T2> handler = null)
        {
            var result = InstantiateFromPool<CyclopsWaitForEvent<T1, T2>>(timeout, maxCycles);
            multicastDelegate += result.OnEvent;
            result._multicastDelegate = multicastDelegate;
            result._handler = handler;
            Assert.IsNotNull(result._multicastDelegate, "Multicast delegate must not be null.");
            return result;
        }
        
        private void OnEvent(T1 x, T2 y)
        {
            if (!WasEntered)
                return;
            
            _handler?.Invoke(x, y);
            StepForward();
        }

        protected override void OnRecycle()
        {
            _multicastDelegate = null;
            _handler = null;
        }
    }

    public class CyclopsWaitForEvent<T1, T2, T3> : CyclopsRoutine
    {
        private Action<T1, T2, T3> _multicastDelegate;
        private Action<T1, T2, T3> _handler;

        public static CyclopsWaitForEvent<T1, T2, T3> Instantiate(
            ref Action<T1, T2, T3> multicastDelegate,
            double timeout = double.MaxValue,
            double maxCycles = 1.0,
            Action<T1, T2, T3> handler = null)
        {
            var result = InstantiateFromPool<CyclopsWaitForEvent<T1, T2, T3>>(timeout, maxCycles);
            multicastDelegate += result.OnEvent;
            result._multicastDelegate = multicastDelegate;
            result._handler = handler;
            Assert.IsNotNull(result._multicastDelegate, "Multicast delegate must not be null.");
            return result;
        }

        private void OnEvent(T1 x, T2 y, T3 z)
        {
            if (!WasEntered)
                return;

            _handler?.Invoke(x, y, z);
            StepForward();
        }

        protected override void OnRecycle()
        {
            _multicastDelegate = null;
            _handler = null;
        }
    }
    
    public class CyclopsWaitForEvent<T1, T2, T3, T4> : CyclopsRoutine
    {
        private Action<T1, T2, T3, T4> _multicastDelegate;
        private Action<T1, T2, T3, T4> _handler;
        
        public static CyclopsWaitForEvent<T1, T2, T3, T4> Instantiate(
            ref Action<T1, T2, T3, T4> multicastDelegate,
            double timeout = double.MaxValue,
            double maxCycles = 1.0,
            Action<T1, T2, T3, T4> handler = null)
        {
            var result = InstantiateFromPool<CyclopsWaitForEvent<T1, T2, T3, T4>>(timeout, maxCycles);
            multicastDelegate += result.OnEvent;
            result._multicastDelegate = multicastDelegate;
            result._handler = handler;
            Assert.IsNotNull(result._multicastDelegate, "Multicast delegate must not be null.");
            return result;
        }
        
        private void OnEvent(T1 x, T2 y, T3 z, T4 w)
        {
            if (!WasEntered)
                return;
            
            _handler?.Invoke(x, y, z, w);
            StepForward();
        }

        protected override void OnRecycle()
        {
            _multicastDelegate = null;
            _handler = null;
        }
    }
}