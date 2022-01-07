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
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Smonch.CyclopsFramework
{
    public sealed class CyclopsEngine : CyclopsCommon
    {
        private Dictionary<string, HashSet<ICyclopsTaggable>> _registry;
        private Queue<ICyclopsTaggable> _additions;
        private Queue<ICyclopsTaggable> _removals;
        private Queue<CyclopsRoutine> _routines;
        private Queue<CyclopsRoutine> _finishedRoutines;
        private Queue<CyclopsStopRoutineRequest> _stopsRequested;
        private Queue<CyclopsMessage> _messages;
        private HashSet<string> _pausesRequested;
        private HashSet<string> _resumesRequested;
        private HashSet<string> _blocksRequested;
        private HashSet<string> _autotags;
        private Dictionary<string, int> _timers;
        private List<float> _timerTimes;
        private bool _nextAdditionIsImmediate = false;

        public enum RoutineExceptionScope
        {
            Dispose_ProcessRemovals,
            SkipPredicate_ProcessAdditions,
            Stop_ProcessRoutines,
            Stop_ProcessStopRequests,
            Stop_Remove,
            Update_ProcessRoutines
        }

        public event Action<CyclopsRoutine, RoutineExceptionScope, Exception> RoutineExceptionCaught;
        public event Action<CyclopsEngine, CyclopsMessage, Exception> MessageExceptionCaught;
        public event Action<CyclopsEngine, ICyclopsDisposable, Exception> DisposableExceptionCaught;

        public float DeltaTime { get; private set; }
        public float Fps => MathF.Round(1f / DeltaTime);

        /// <summary>
        /// <para>Immediately allows a chained Add method (e.g. Engine.Immediately.Add(foo)) to be processed at the end of either the current or next ProcessRoutines call.</para>
        /// <para>If Immediately is used before the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the same frame.</para>
        /// <para>If Immediately is used after the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the next frame.</para>
        /// <para>Tip: Immediately can be use with other common methods that use Add internally such as Listen, Sleep, WaitUntil, etc.</para>
        /// </summary>
        public CyclopsEngine Immediately
        {
            get
            {
                _nextAdditionIsImmediate = true;
                return this;
            }
        }

        /// <summary>
        /// <para>This limits the maximum number of times a CyclopsRoutine can immediately enqueue a new CyclopsRoutine to the active queue on the same frame.</para>
        /// <para>Failure to provide a limit combined with erroneous code could result in an endless loop.</para>
        /// <para>Regardless of this failsafe, good practice is to use Immediately.Add() only when required and with plenty of caution.</para>
        /// <para>Raise this limit to a value higher than 1 in order to enable nesting.</para>
        /// </summary>
        public int MaxNestingDepth { get; set; } = 1;

        public CyclopsEngine(int initialCapacity = 256)
        {
            _registry = new Dictionary<string, HashSet<ICyclopsTaggable>>(initialCapacity);
            _routines = new Queue<CyclopsRoutine>(initialCapacity);
            _finishedRoutines = new Queue<CyclopsRoutine>(initialCapacity);
            _additions = new Queue<ICyclopsTaggable>(initialCapacity);
            _removals = new Queue<ICyclopsTaggable>(initialCapacity);
            _stopsRequested = new Queue<CyclopsStopRoutineRequest>(initialCapacity);
            _messages = new Queue<CyclopsMessage>(initialCapacity);
            _pausesRequested = new HashSet<string>();
            _resumesRequested = new HashSet<string>();
            _blocksRequested = new HashSet<string>();
            _autotags = new HashSet<string>();
            _timers = new Dictionary<string, int>(initialCapacity);
            _timerTimes = new List<float>(initialCapacity);

            RoutineExceptionCaught += CyclopsEngine_RoutineExceptionCaught;
            MessageExceptionCaught += CyclopsEngine_MessageExceptionCaught;
            DisposableExceptionCaught += CyclopsEngine_DisposableExceptionCaught;

            BeginAutotag(Tag_All);
            Add(new CyclopsRoutine(period: float.MaxValue, cycles: 1f, bias: null, tag: Tag_Sentinel));
        }


        public void Dispose()
        {
            Remove(Tag_All, true);
            Block(Tag_All);
            ProcessStopRequests();
            ProcessRemovals();

            _registry.Clear();
            _routines.Clear();
            _finishedRoutines.Clear();
            _additions.Clear();
            _removals.Clear();
            _stopsRequested.Clear();
            _messages.Clear();
            _pausesRequested.Clear();
            _resumesRequested.Clear();
            _blocksRequested.Clear();
            _autotags.Clear();
            _timers.Clear();
            _timerTimes.Clear();

            RoutineExceptionCaught -= CyclopsEngine_RoutineExceptionCaught;
            MessageExceptionCaught -= CyclopsEngine_MessageExceptionCaught;
            DisposableExceptionCaught -= CyclopsEngine_DisposableExceptionCaught;
        }

        private void CyclopsEngine_DisposableExceptionCaught(CyclopsEngine engine, ICyclopsDisposable disposable, Exception e)
        {
            engine.LogException(e);
        }

        private void CyclopsEngine_MessageExceptionCaught(CyclopsEngine engine, CyclopsMessage msg, Exception e)
        {
            engine.LogException(e);
        }

        private void CyclopsEngine_RoutineExceptionCaught(CyclopsRoutine routine, RoutineExceptionScope scope, Exception e)
        {
            routine.LogException(e);
        }

        // Sequencing Tags

        public void BeginAutotag(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _autotags.Add(tag);
        }

        public void EndAutotag(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _autotags.Remove(tag);
        }

        private void ApplyAutotags(ICyclopsTaggable o)
        {
            Assert.IsNotNull(o);

            if (!o.Tags.Contains(Tag_Sentinel))
                foreach (var tag in _autotags)
                    o.Tags.Add(tag);
        }

        // Sequencing Additions

        public override T Add<T>(T routine)
        {
            Assert.IsNotNull(routine);

            ApplyAutotags(routine);
            routine.Host = this;

            if (_nextAdditionIsImmediate)
            {
                _nextAdditionIsImmediate = false;

                if (Context == null)
                    routine.NestingDepth = 1;
                else
                    routine.NestingDepth = Context.NestingDepth + 1;
                
                Assert.IsTrue(routine.NestingDepth <= MaxNestingDepth,
                    $"Couldn't add {nameof(routine)}:{routine.GetType()} because nesting depth was exceeded. MaxNestingDepth: {MaxNestingDepth} Actual: {routine.NestingDepth}");

                if (routine.NestingDepth <= MaxNestingDepth)
                    ProcessAddition(routine);
            }
            else
            {
                _additions.Enqueue(routine);
            }

            return routine;
        }

        public void AddTaggable(ICyclopsTaggable o)
        {
            Assert.IsTrue(ValidateTaggable(o, out var reason), reason);
            Register(o);
        }

        // Control Flow

        public void Pause(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _pausesRequested.Add(tag);
        }

        public void Pause(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
                Pause(tag);
        }

        public void Resume(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _resumesRequested.Add(tag);
        }

        public void Resume(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
                Resume(tag);
        }

        public void Block(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _blocksRequested.Add(tag);
        }

        public void Block(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
                Block(tag);
        }
        
        public void Remove(string tag, bool stopChildren = true)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _stopsRequested.Enqueue(new CyclopsStopRoutineRequest(tag, stopChildren));
        }

        public void Remove(IEnumerable<string> tags, bool stopChildren = true)
        {
            foreach (var tag in tags)
                Remove(tag, stopChildren);
        }

        public void Remove(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);

            // If something goes wrong here it's likely burried in a long running routine.
            // That's why these exceptions are being caught but the above situation can throw exceptions.
            if (taggedObject is CyclopsRoutine)
            {
                try
                {
                    ((CyclopsRoutine)taggedObject).Stop();
                }
                catch (Exception e)
                {
                    RoutineExceptionCaught((CyclopsRoutine)taggedObject, RoutineExceptionScope.Stop_Remove, e);
                }
            }
            else
            {
                _removals.Enqueue(taggedObject);
            }
        }

        // Registration & Housekeeping

        private void Register(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);

            foreach (var tag in taggedObject.Tags)
            {
                if (_registry.TryGetValue(tag, out var taggables))
                {
                    taggables.Add(taggedObject);
                }
                else
                {
                    taggables = new HashSet<ICyclopsTaggable>();
                    taggables.Add(taggedObject);
                    _registry[tag] = taggables;
                }
            }
        }

        private void Unregister(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);

            foreach (var tag in taggedObject.Tags)
            {
                if (_registry.ContainsKey(tag))
                    _registry[tag].Remove(taggedObject);
            }
        }

        public int Count(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);

            int result = 0;

            if (_registry.TryGetValue(tag, out var taggables))
                result = taggables.Count;

            return result;
        }

        public bool Exists(string tag)
        {
            return Count(tag) > 0;
        }

        public void CopyTagStatusToList(List<CyclopsTagStatus> results)
        {
            results.Clear();

            CyclopsTagStatus status;

            foreach (var tag in _registry.Keys)
            {
                if (tag == Tag_Sentinel)
                    continue;

                status.tag = tag;
                status.count = Count(tag);

                results.Add(status);
            }

            results.Sort();
        }

        // TODO: This is outdated now. Can be simplified.
        // Was designed to handle status reports from within at any time.
        public void CopyRoutinesToList(List<CyclopsRoutine> routines)
        {
            routines.Clear();

            bool alreadyEncounteredHead = false;

            for (int i = 0; i < _routines.Count; ++i)
            {
                var routine = _routines.Dequeue();
                _routines.Enqueue(routine);

                if (!alreadyEncounteredHead)
                {
                    if (routine.Tags.Contains(Tag_Sentinel))
                    {
                        alreadyEncounteredHead = true;

                        for (int j = 0; j < _routines.Count; ++j)
                        {
                            var orderedRoutine = _routines.Dequeue();
                            _routines.Enqueue(orderedRoutine);
                            
                            if (orderedRoutine.IsActive && !orderedRoutine.Tags.Contains(Tag_Sentinel))
                                routines.Add(orderedRoutine);
                        }
                    }
                }
            }
        }
        
        public bool TimerReady(float period, string tag, bool canRestart = false)
        {

            Assert.IsTrue(ValidateTimingValue(period, out var reason), reason);
            Assert.IsTrue(ValidateTag(tag, out reason), reason);

            int index;

            if (_timers.TryGetValue(tag, out index))
            {
                if (_timerTimes[index] <= 0f)
                {
                    _timerTimes[index] = period;

                    return true;
                }
                else if (canRestart)
                {
                    _timerTimes[index] = period;
                }

                return false;
            }
            else
            {
                index = _timerTimes.Count;
                _timerTimes.Add(period);

                _timers[tag] = index;

                return true;
            }
        }

        // Messaging

        public void Send(string receiverTag, string name, object sender = null, object data = null, CyclopsMessage.DeliveryStage stage = CyclopsMessage.DeliveryStage.AfterRoutines)
        {
            if (sender == null)
                sender = this;

            var msg = new CyclopsMessage {
                receiverTag = receiverTag,
                name = name,
                sender = sender,
                data = data,
                stage = stage,
                isBroadcast = false
            };

            Send(msg);
        }
        
        public void Send(CyclopsMessage msg)
        {
            Assert.IsNotNull(msg.sender, "Sender must not be null.");
            Assert.IsTrue(ValidateTag(msg.receiverTag, out var reason), reason);

            _messages.Enqueue(msg);
        }

        private void DeliverMessage(CyclopsMessage msg, ICyclopsMessageInterceptor receiver)
        {
            try
            {
                receiver?.InterceptMessage(msg);
            }
            catch (Exception e)
            {
                MessageExceptionCaught(this, msg, e);
            }
        }

        public void TrackAnalytics(string tag, float lingerPeriod = 0f)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            Assert.IsTrue(ValidateTimingValue(lingerPeriod, out reason), reason);

            Send(tag, Message_Analytics);

            if (lingerPeriod > 0f)
                Sleep(tag: tag, period: lingerPeriod);
        }

        // Updates

        private void ProcessRoutines(float deltaTime)
        {
            Assert.IsTrue(ValidateTimingValue(deltaTime, out var reason), reason);
            
            while (_routines.Count > 0)
            {
                var routine = _routines.Dequeue();

                Context = routine;
                _finishedRoutines.Enqueue(routine);
                
                try
                {
                    routine.Update(deltaTime);

                    // The possibility of an infinite loop caused by nesting Immediately.Add() has passed
                    // unless someone goes out of their way to modify NestingDepth... don't do that.
                    routine.NestingDepth = 0;
                }
                catch (Exception updateException)
                {
                    RoutineExceptionCaught(routine, RoutineExceptionScope.Update_ProcessRoutines, updateException);

                    try
                    {
                        routine.Stop();
                    }
                    catch (Exception stopException)
                    {
                        RoutineExceptionCaught(routine, RoutineExceptionScope.Stop_ProcessRoutines, stopException);
                    }
                }
            }

            Context = null;

            var tmp = _routines;
            _routines = _finishedRoutines;
            _finishedRoutines = tmp;
        }

        private void ProcessMessages(CyclopsMessage.DeliveryStage stage)
        {
            int msgCount = _messages.Count;

            for (int i = 0; i < msgCount; ++i)
            {
                var msg = _messages.Dequeue();

                if ((msg.stage != CyclopsMessage.DeliveryStage.SoonestPossible) && (msg.stage != stage))
                {
                    _messages.Enqueue(msg);
                    continue;
                }

                if (_registry.TryGetValue(msg.receiverTag, out var taggables))
                    foreach (var possibleReceiver in taggables)
                        if (possibleReceiver is ICyclopsMessageInterceptor)
                            DeliverMessage(msg, (ICyclopsMessageInterceptor)possibleReceiver);
            }
        }

        private void ProcessStopRequests()
        {
            for (int i = 0; i < _stopsRequested.Count; ++i)
            {
                var request = _stopsRequested.Dequeue();

                if (_registry.ContainsKey(request.routineTag))
                {
                    foreach (var taggable in _registry[request.routineTag])
                    {
                        if (taggable is CyclopsRoutine routine)
                        {
                            try
                            {
                                routine.Stop();
                            }
                            catch (Exception e)
                            {
                                RoutineExceptionCaught(routine, RoutineExceptionScope.Stop_ProcessStopRequests, e);
                            }

                            if (request.stopChildren)
                            {
                                routine.RemoveAllChildren();
                            }
                        }
                        else
                        {
                            Remove(taggable);
                        }
                    }
                }
            }
        }

        private void ProcessRemovals()
        {
            // CyclopsRoutines are excluded from the _removals list.
            // This is where everything else is unregistered and properly disposed of if required.

            while (_removals.Count > 0)
            {
                var removal = _removals.Dequeue();

                Unregister(removal);

                if (removal is ICyclopsDisposable)
                {
                    try
                    {
                        ((ICyclopsDisposable)removal).Dispose();
                    }
                    catch (Exception e)
                    {
                        DisposableExceptionCaught(this, (ICyclopsDisposable)removal, e);
                    }
                }
            }

            // Process CyclopsRoutines removals here.

            int routineCount = _routines.Count;

            for (int i = 0; i < routineCount; ++i)
            {
                var routine = _routines.Dequeue();

                if (routine.IsActive)
                {
                    _routines.Enqueue(routine);
                }
                else
                {
                    Unregister(routine);

                    // Before adding tags, check each child to see if any of its tags are actively being blocked.
                    // If we're in the clear, the child inherits the parents cascading tags and is added to the Additions queue.

                    if (_blocksRequested.Count > 0)
                    {
                        foreach (var child in routine.Children)
                        {
                            if (!_blocksRequested.Overlaps(child.Tags))
                            {
                                foreach (var tag in child.Tags)
                                    if (!tag.StartsWith(TagPrefix_Noncascading))
                                        child.AddTag(tag);

                                Add(child);
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in routine.Children)
                        {
                            foreach (var tag in child.Tags)
                                if (!tag.StartsWith(TagPrefix_Noncascading))
                                    child.AddTag(tag);

                            Add(child);
                        }
                    }

                    // For CyclopsRoutines, disposing handles memory pooling cleanup.

                    try
                    {
                        ((ICyclopsDisposable)routine).Dispose();
                    }
                    catch (Exception e)
                    {
                        RoutineExceptionCaught(routine, RoutineExceptionScope.Dispose_ProcessRemovals, e);
                    }
                }
            }
        }

        private void ProcessAdditions()
        {
            while (_additions.Count > 0)
                ProcessAddition(_additions.Dequeue());
        }

        private void ProcessAddition(ICyclopsTaggable additionCandidate)
        {
            // Check to see if a candidate has a tag that is actively being blocked.
            // If so, don't add it; instead, continue with the next candidate.

            if (_blocksRequested.Count > 0)
            {
                bool skipTag = false;

                foreach (string tag in additionCandidate.Tags)
                {
                    if (_blocksRequested.Contains(tag))
                    {
                        skipTag = true;
                        break;
                    }
                }

                if (skipTag)
                    return;
            }

            if (additionCandidate is CyclopsRoutine)
            {
                var skipPredicate = ((CyclopsRoutine)additionCandidate).SkipPredicate;

                if (skipPredicate != null)
                {
                    // We'll skip adding this routine if anything goes wrong.
                    // This may not be the best course of action, but there's no way to tell what would be.
                    bool result = true;

                    try
                    {
                        result = skipPredicate();
                    }
                    catch (Exception e)
                    {
                        RoutineExceptionCaught((CyclopsRoutine)additionCandidate, RoutineExceptionScope.SkipPredicate_ProcessAdditions, e);
                    }

                    if (result)
                        return;
                }

                _routines.Enqueue((CyclopsRoutine)additionCandidate);
            }

            Register(additionCandidate);
        }

        private void ProcessResumeRequests()
        {
            foreach (string tag in _resumesRequested)
                if (_registry.ContainsKey(tag))
                    foreach (var resumptionCandidate in _registry[tag])
                        if (resumptionCandidate is ICyclopsPausable)
                            ((ICyclopsPausable)resumptionCandidate).IsPaused = false;

            _resumesRequested.Clear();
        }

        private void ProcessPauseRequests()
        {
            foreach (string tag in _pausesRequested)
                if (_registry.ContainsKey(tag))
                    foreach (ICyclopsTaggable pausationCandidate in _registry[tag])
                        if (pausationCandidate is ICyclopsPausable)
                            ((ICyclopsPausable)pausationCandidate).IsPaused = true;

            _pausesRequested.Clear();
        }

        private void ProcessTimers(float deltaTime)
        {
            for (int i = 0; i < _timerTimes.Count; ++i)
            {
                float secondsRemaining = _timerTimes[i];

                if (secondsRemaining > 0f)
                {
                    secondsRemaining -= deltaTime;
                    secondsRemaining = Math.Max(0f, secondsRemaining);
                    _timerTimes[i] = secondsRemaining;
                }
            }
        }

        public void Update(float deltaTime)
        {
            Assert.IsFalse(_nextAdditionIsImmediate, "CyclopsEngine should not currently be in immediate mode.  Add() should follow the use of Immediately.");
            _nextAdditionIsImmediate = false;

            // Further timing checks after this one only apply to release builds.
            Assert.IsTrue(ValidateTimingValue(deltaTime, out var reason), reason);
            
            DeltaTime = Math.Clamp(deltaTime, float.Epsilon, MaxDeltaTime);

            ProcessTimers(deltaTime);
            ProcessMessages(CyclopsMessage.DeliveryStage.BeforeRoutines);
            ProcessRoutines(deltaTime);
            ProcessMessages(CyclopsMessage.DeliveryStage.AfterRoutines);
            ProcessStopRequests();
            ProcessRemovals();
            ProcessAdditions();
            ProcessMessages(CyclopsMessage.DeliveryStage.SoonestPossible);
            // Pause and Resume act on new additions intentionally.
            ProcessResumeRequests();
            ProcessPauseRequests();
            _blocksRequested.Clear();
        }
    }
}
