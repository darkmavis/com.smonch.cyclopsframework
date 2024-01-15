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
}