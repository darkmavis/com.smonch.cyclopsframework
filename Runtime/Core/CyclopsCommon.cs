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
using System.Collections;
using System.Collections.Generic;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsCommon
    {
        public const float MaxDeltaTime = .25f;
        public const string MessagePrefix_Cyclops = "Cyclops";
        public const string Message_Analytics = MessagePrefix_Cyclops + "Analytics";
        public const string TagAttributeDelimiter = ":";
        public const string TagPrefix_Attribute = "@";
        public const string TagPrefix_Noncascading = "!";
        public const string TagPrefix_Cyclops = "!cf.";
        public const string Tag_All = "*";
        public const string Tag_Logging = TagPrefix_Cyclops + "Logging";
        public const string Tag_Nop = TagPrefix_Cyclops + "Nop";
        public const string Tag_Undefined = TagPrefix_Cyclops + "Undefined";
        public const string Tag_Loop = TagPrefix_Cyclops + "Loop";
        public const string Tag_LoopWhile = TagPrefix_Cyclops + "LoopWhile";
        
        /// <summary>
        /// This provides a reference to the CyclopsEngine host.
        /// </summary>
        internal CyclopsEngine Host { get; set; }

        public CyclopsRoutine Context { get; protected set; }

        public abstract T Add<T>(T routine) where T : CyclopsRoutine;

        public static Action<string> Logger;

        /// <summary>
        /// Validates that a Cyclops Framework tag is actually useful.
        /// A tag can't be null and must contain at least one non-whitespace character.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected bool ValidateTag(string tag, out string reason)
        {
            reason = null;

            if (string.IsNullOrWhiteSpace(tag))
            {
                if (tag == null)
                    reason = "Tag is null.";
                else
                    reason = "A tag must contain a least one non-whitespace character.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that a timing value which could be used for a timer or routine is actually useful.
        /// </summary>
        /// <param name="timingValue"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected bool ValidateTimingValue(double timingValue, out string reason)
        {
            bool result;

            if (double.IsInfinity(timingValue))
            {
                reason = "Timing value must be a finite.";
                result = false;
            }
            else if (double.IsNaN(timingValue))
            {
                reason = "Timing value must be a number.";
                result = false;
            }
            else if (timingValue < 0d)
            {
                reason = "Timing value must be positive.";
                result = false;
            }
            else if (timingValue == 0d)
            {
                reason = "Timing value must be greater than zero.";
                result = false;
            }
            else
            {
                reason = null;
                result = true;
            }
            
            return result;
        }

        /// <summary>
        /// Validates that a timing value which could be used for a timer or routine is actually useful.
        /// Zero is Ok.
        /// </summary>
        /// <param name="timingValue"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected bool ValidateTimingValueWhereZeroIsOk(double timingValue, out string reason)
        {
            bool result;

            if (double.IsInfinity(timingValue))
            {
                reason = "Timing value must be a finite.";
                result = false;
            }
            else if (double.IsNaN(timingValue))
            {
                reason = "Timing value must be a number.";
                result = false;
            }
            else if (timingValue < 0d)
            {
                reason = "Timing value must be positive.";
                result = false;
            }
            else
            {
                reason = null;
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Validates various aspects of ICyclopsTaggable and instantiates the appropriate reason as needed.<br/>
        /// If validation is successful, reason will be null.<br/>
        /// </summary>
        /// <param name="taggable"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected bool ValidateTaggable(ICyclopsTaggable o, out string reason)
        {
            reason = null;

            if (o == null)
            {
                reason = "ICyclopsTaggable.Tags must not be null.";
                return false;
            }

            if (o.Tags.Count == 0)
            {
                reason = "ICyclopsTaggable.Tags must contain at least 1 tag.";
                return false;
            }

            foreach (var tag in o.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    reason = $"Tags for {o.GetType()} can't be null and must contain non-whitespace characters.";
                    return false;
                }
            }

            return true;
        }
 
		public CyclopsRoutine Add(Action f)
        {
            return Add(new CyclopsLambda(f));
        }

        public CyclopsRoutine Add(string tag, Action f)
        {
            return Add(new CyclopsLambda(f))
                .AddTag(tag);
        }

        public CyclopsCoroutine Add(Func<IEnumerator> f)
        {
            return (CyclopsCoroutine)Add(new CyclopsCoroutine(f));
        }

        public CyclopsCoroutine Add(string tag, Func<IEnumerator> f)
        {
            return (CyclopsCoroutine)(Add(new CyclopsCoroutine(f))
                .AddTag(tag));
        }

        public CyclopsRoutine Add(string tag, CyclopsRoutine routine)
        {
            return Add(routine)
                .AddTag(tag);
        }

        private CyclopsRoutine AddSequence(IEnumerable<CyclopsRoutine> routines, bool returnHead)
        {
            CyclopsRoutine head = null;
            CyclopsRoutine tail = null;
            CyclopsRoutine currRoutine = null;

            foreach (var o in routines)
            {
                currRoutine = Add(o);

                if (currRoutine != null)
                {
                    if (head == null)
                        head = tail = currRoutine;
                    else if (head != null)
                        tail = tail.Add(o);
                }
            }

            return (returnHead ? head : tail);
        }

        public CyclopsRoutine AddSequenceReturnHead(IEnumerable<CyclopsRoutine> routines)
        {
            return AddSequence(routines, true);
        }

        public CyclopsRoutine AddSequenceReturnTail(IEnumerable<CyclopsRoutine> routines)
        {
            return AddSequence(routines, false);
        }

        public CyclopsRoutine Loop(Action f)
        {
            return Add(new CyclopsLambda(period: 0f, cycles: float.MaxValue, f))
                .AddTag(Tag_Loop);
        }

        public CyclopsRoutine Loop(float period, float cycles, Action f)
        {
            return Add(new CyclopsLambda(period, cycles, f))
                .AddTag(Tag_Loop);
        }

        public CyclopsRoutine LoopWhile(Func<bool> predicate, float period = 0f)
        {
            var routine = Add(new CyclopsLambda(period, float.MaxValue, () =>
            {
                if (!predicate())
                    Context.Stop();
            })).AddTag(Tag_LoopWhile);

            return routine;
        }

        public CyclopsRoutine LoopWhile(Func<bool> whilePredicate, Action whileBody, float period = 0f)
        {
            var routine = Add(new CyclopsLambda(period, float.MaxValue, () =>
            {
                if (whilePredicate())
                {
                    whileBody();

                    if (!whilePredicate())
                        Context.Stop();
                }
                else
                {
                    Context.Stop();
                }
            })).AddTag(Tag_LoopWhile);

            return routine;
        }

        public CyclopsRoutine Nop(string tag = null, int cycles=1)
        {
            var nop = new CyclopsRoutine(period: 0f, cycles: cycles, bias: null, tag: Tag_Nop);

            if (tag != null)
                nop.AddTag(tag);

            Add(nop);

            return nop;
        }
        
        public CyclopsRoutine SkipIf(Func<bool> predicate)
        {
            Context.SkipPredicate = predicate;
            return Context;
        }
        
        public CyclopsRoutine Sleep(float period, string tag=null)
        {
            if (tag == null)
                return Add(new CyclopsSleep(period));
            else
                return Add(new CyclopsSleep(period)).AddTag(tag);
        }

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName)
        {
            return Add(new CyclopsWaitForMessage(receiverTag, messageName));
        }

        public CyclopsWaitForMessage Listen(string receiverTag, string messageName, float timeout, float cycles = 1)
        {
            return Add(new CyclopsWaitForMessage(receiverTag, messageName, timeout, cycles));
        }
        
        public CyclopsRoutine WaitForTask(Action<CyclopsTask> f)
        {
            return Add(new CyclopsTask(f));
        }

        public CyclopsRoutine WaitUntil(Func<bool> condition)
        {
            return Add(new CyclopsWaitUntil(condition));
        }

        public CyclopsRoutine WaitUntil(Func<bool> condition, float timeout)
        {
            return Add(new CyclopsWaitUntil(condition, timeout));
        }

        public CyclopsRoutine When(Func<bool> condition, Action response = null, float timeout = float.MaxValue)
        {
            return Add(new CyclopsWhen(condition, response, timeout));
        }
        
        public CyclopsRoutine Lerp(float period, float cycles, Action<float> f)
        {
            return Add(new CyclopsUpdate(period, cycles, null, f));
        }

        public CyclopsRoutine Lerp(float period, Action<float> f)
        {
            return Add(new CyclopsUpdate(period, 1, null, f));
        }

        public CyclopsWaitForMessage ProcessAnalytics(string tag, Action<CyclopsMessage> f, int cycles = 1, float timeout = float.MaxValue)
        {
            var seq = Listen(tag, Message_Analytics, timeout, cycles);
            seq.OnSuccess(f);

            return seq;
        }

        public CyclopsWaitForMessage WaitForAnalytics(string tag, int cycles = 1, float timeout = float.MaxValue)
        {
            return Listen(tag, Message_Analytics, timeout, cycles);
        }

        public CyclopsWaitForMessage WaitForAnalytics(CyclopsRequirement requirement)
        {
            if (requirement.timeout <= 0f)
                requirement.timeout = float.MaxValue;

            requirement.count = Math.Max(1, requirement.count);

            return Listen(requirement.tag, Message_Analytics, requirement.timeout, requirement.count);
        }

        /// <summary>
        /// Logs non-deferred text via either CyclopsCommon.Logger or UnityEngine.Debug.Log.
        /// For lazy evaluation, please use: Log(Func<string> lazyPrinter)
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
                return Add(tag: Tag_Logging, f: () => UnityEngine.Debug.Log(text));
#else
                return Context;
#endif
            }
            else
            {
                return Add(tag: Tag_Logging, f: () => Logger?.Invoke(text));
            }
        }

        /// <summary>
        /// Logs via either CyclopsCommon.Logger or UnityEngine.Debug.Log using a deferred printing function.
        /// If CyclopsCommon.Logger is null, logging will default to UnityEngine.Debug.
        /// When using UnityEngine.Debug, logging is only available for debug builds.
        /// To log output without a Unity project reference, please ensure that CyclopsCommon.Logger refers to the desired logger.
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public CyclopsRoutine Log(Func<string> lazyPrinter)
        {
            if (Logger == null)
            {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
                return Add(tag: Tag_Logging, f: () => UnityEngine.Debug.Log(lazyPrinter()));
#else
                return Context;
#endif
            }
            else
            {
                return Add(tag: Tag_Logging, f: () => Logger?.Invoke(lazyPrinter()));
            }
        }

        /// <summary>
        /// Logs an Exception via UnityEngine.Debug.LogException.
        /// This is used internally by CyclopsEngine.
        /// To log output without a Unity project reference, please ensure that CyclopsCommon.Logger refers to the desired logger.
        /// </summary>
        /// <param name="e"></param>
        public void LogException(Exception e)
        {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            UnityEngine.Debug.LogException(e);
#else
            Logger?.Invoke(e.ToString());
#endif
        }
    }
}