﻿// Cyclops Framework
// 
// Copyright 2010 - 2023 Mark Davis
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
        {
            return Add(CyclopsLambda.Instantiate(f));
        }

        public CyclopsLambda Add(string tag, Action f)
        {
            return (CyclopsLambda)Add(CyclopsLambda.Instantiate(f))
                .AddTag(tag);
        }

        public CyclopsRoutine Add(string tag, CyclopsRoutine routine)
        {
            return Add(routine)
                .AddTag(tag);
        }

        public CyclopsLambda Loop(Action f)
            => Add(CyclopsLambda.Instantiate(period: 0f, maxCycles: float.MaxValue, f));
        
        public CyclopsLambda Loop(float period, float maxCycles, Action f)
            => (CyclopsLambda)Add(CyclopsLambda.Instantiate(period, maxCycles, f));

        public CyclopsLambda LoopWhile(Func<bool> predicate, float period = 0f)
        {
            CyclopsLambda context = null;
            
            var routine = context = Add(CyclopsLambda.Instantiate(period, float.MaxValue, () =>
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

        public CyclopsRoutine Nop(string tag = null, int maxCycles = 1)
        {
            CyclopsNop nop = CyclopsNop.Instantiate(maxCycles);

            if (tag != null)
                nop.AddTag(tag);

            Add(nop);

            return nop;
        }

        public CyclopsSleep Sleep(double period)
            => Add(CyclopsSleep.Instantiate(period));

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName)
            => Add(CyclopsWaitForMessage.Instantiate(receiverTag, messageName));

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName, double timeout, double maxCycles = 1)
            => Add(CyclopsWaitForMessage.Instantiate(receiverTag, messageName, timeout, maxCycles));

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
        
        /// <summary>
        /// Logs non-deferred text via either CyclopsCommon.Logger or UnityEngine.Debug.Log.
        /// For lazy evaluation, please use: Log(lazyPrinter)
        /// If CyclopsCommon.Logger is null, logging will default to UnityEngine.Debug.
        /// When using UnityEngine.Debug, logging is only available for debug builds.
        /// To log output without a Unity project reference, please ensure that CyclopsCommon.Logger refers to the desired logger.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
		public CyclopsRoutine Log(string text)
        {
            if (Logger == null)
            {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                return Add(tag: TagLog, f: () => UnityEngine.Debug.Log(text));
#else
                return Context;
#endif
            }
            else
            {
                return Add(tag: TagLog, f: () => Logger?.Invoke(text));
            }
        }

        /// <summary>
        /// Logs via either CyclopsCommon.Logger or UnityEngine.Debug.Log using a deferred printing function.
        /// If CyclopsCommon.Logger is null, logging will default to UnityEngine.Debug.
        /// When using UnityEngine.Debug, logging is only available for debug builds.
        /// To log output without a Unity project reference, please ensure that CyclopsCommon.Logger refers to the desired logger.
        /// </summary>
        /// <param name="lazyPrinter"></param>
        /// <returns></returns>
        public CyclopsRoutine Log(Func<string> lazyPrinter)
        {
            if (Logger == null)
            {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                return Add(tag: TagLog, f: () => UnityEngine.Debug.Log(lazyPrinter()));
#else
                return Context;
#endif
            }
            else
            {
                return Add(tag: TagLog, f: () => Logger?.Invoke(lazyPrinter()));
            }
        }
    }
}
