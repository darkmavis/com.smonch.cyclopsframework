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
using UnityEngine;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public class CyclopsNext : CyclopsCommon
    {
        internal ICyclopsRoutineScheduler Scheduler { get; set; }

        internal static CyclopsNext Rent(ICyclopsRoutineScheduler scheduler)
        {
            CyclopsNext next = GenericPool<CyclopsNext>.Get();

            next.Scheduler = scheduler;

            return next;
        }

        // Releasing always occurs ahead of returning a result.
        // This is not an oversight.
        private void Release() => GenericPool<CyclopsNext>.Release(this);

        public T Add<T>(T routine) where T : CyclopsRoutine
        {
            Release();

            return Scheduler.Add(routine);
        }

        public CyclopsLambda Add(Action f)
            => Add(CyclopsLambda.Instantiate(f));
        
        public CyclopsRoutine Log(string text)
            => Debug.isDebugBuild ? Add(CyclopsLambda.Instantiate(() => Debug.Log(text))) : Context;
        
        public CyclopsLambda Loop(Action f)
            => Add(CyclopsLambda.Instantiate(period: 0f, maxCycles: float.MaxValue, f));
        
        public CyclopsLambda Loop(float period, float maxCycles, Action f)
            => (CyclopsLambda)Add(CyclopsLambda.Instantiate(period, maxCycles, f));

        public CyclopsLambda LoopWhile(Func<bool> predicate, float period = 0f)
        {
            CyclopsLambda context = null;
            
            CyclopsLambda routine = context = Add(CyclopsLambda.Instantiate(period, float.MaxValue, () =>
            {
                if (!predicate())
                    context!.Stop();
            }));

            return routine;
        }

        public CyclopsLambda LoopWhile(Func<bool> whilePredicate, Action whileBody, float period = 0f)
        {
            CyclopsLambda context = null;
            
            CyclopsLambda routine = context = Add(CyclopsLambda.Instantiate(period, float.MaxValue, () =>
            {
                if (whilePredicate())
                {
                    whileBody();

                    if (!whilePredicate())
                        context!.Stop();
                }
                else
                {
                    context!.Stop();
                }
            }));

            return routine;
        }

        public CyclopsRoutine Nop(int maxCycles = 1)
            => Add(CyclopsNop.Instantiate(maxCycles));

        public CyclopsSleep Sleep(double period)
            => Add(CyclopsSleep.Instantiate(period));

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName)
            => Add(CyclopsWaitForMessage.Instantiate(receiverTag, messageName));

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName, double timeout, double maxCycles = 1)
            => Add(CyclopsWaitForMessage.Instantiate(receiverTag, messageName, timeout, maxCycles));
        
        public CyclopsWaitForEvent WaitForEvent(ref Action multicastDelegate,
            double timeout = double.MaxValue, double maxCycles = 1.0, Action handler = null)
            => Add(CyclopsWaitForEvent.Instantiate(ref multicastDelegate, timeout, maxCycles, handler));
        
        public CyclopsWaitForEvent<T> WaitForEvent<T>(ref Action<T> multicastDelegate,
            double timeout = double.MaxValue, double maxCycles = 1.0, Action<T> handler = null)
            => Add(CyclopsWaitForEvent<T>.Instantiate(ref multicastDelegate, timeout, maxCycles, handler));
        
        public CyclopsWaitForEvent<T1, T2> WaitForEvent<T1, T2>(ref Action<T1, T2> multicastDelegate,
            double timeout = double.MaxValue, double maxCycles = 1.0, Action<T1, T2> handler = null)
            => Add(CyclopsWaitForEvent<T1, T2>.Instantiate(ref multicastDelegate, timeout, maxCycles, handler));
        
        public CyclopsWaitForEvent<T1, T2, T3> WaitForEvent<T1, T2, T3>(ref Action<T1, T2, T3> multicastDelegate,
            double timeout = double.MaxValue, double maxCycles = 1.0, Action<T1, T2, T3> handler = null)
            => Add(CyclopsWaitForEvent<T1, T2, T3>.Instantiate(ref multicastDelegate, timeout, maxCycles, handler));
        
        public CyclopsWaitForEvent<T1, T2, T3, T4> WaitForEvent<T1, T2, T3, T4>(ref Action<T1, T2, T3, T4> multicastDelegate,
            double timeout = double.MaxValue, double maxCycles = 1.0, Action<T1, T2, T3, T4> handler = null)
            => Add(CyclopsWaitForEvent<T1, T2, T3, T4>.Instantiate(ref multicastDelegate, timeout, maxCycles, handler));
        
        public CyclopsTask WaitForTask(Action<CyclopsTask> f)
            => Add(CyclopsTask.Instantiate(f));

        public CyclopsWaitUntil WaitUntil(Func<bool> condition)
            => Add(CyclopsWaitUntil.Instantiate(condition));
        
        public CyclopsWaitUntil WaitUntil(Func<bool> condition, double timeout)
            => Add(CyclopsWaitUntil.Instantiate(condition, timeout));

        public CyclopsWhen When(Func<bool> condition, Action response = null, double timeout = double.MaxValue)
            => Add(CyclopsWhen.Instantiate(condition, response, timeout));

        public CyclopsUpdate Lerp(double period, double maxCycles, Action<float> f, Func<float, float> bias = null)
            => Add(CyclopsUpdate.Instantiate(period, maxCycles, bias, f));
    }
}
