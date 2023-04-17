// Cyclops Framework
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public sealed class CyclopsEngine : CyclopsCommon, ICyclopsRoutineScheduler, IDisposable
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
        private Dictionary<string, double> _timers;
        private bool _nextAdditionIsImmediate = false;

        public float DeltaTime { get; private set; }
        public float Fps => Mathf.Round(1f / DeltaTime);

        public CyclopsNext Next => CyclopsNext.Rent(this);

        /// <summary>
        /// <para>Immediately allows a chained Add method (e.g. <see cref="Immediately"/>.Add(foo)) to be processed at the end of either the current or next ProcessRoutines call.</para>
        /// <para>If Immediately is used before the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the same frame.</para>
        /// <para>If Immediately is used after the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the next frame.</para>
        /// <para>Tip: Immediately can be use with other common methods that use Add internally such as Listen, Sleep, WaitUntil, etc.</para>
        /// </summary>
        public CyclopsNext Immediately
        {
            get
            {
                _nextAdditionIsImmediate = true;
                return Next;
            }
        }

        /// <summary>
        /// <para>This limits the nesting depth of CyclopsRoutines that are immediately enqueued within a Cyclopsroutine that was itself enqueued on the same frame.</para>
        /// <para>While nesting is perfectly safe and predictable, it should still be considered the exception to the rule.</para>
        /// <para>Please use <see cref="Immediately"/> only when required. Failure to provide a limit combined with erroneous code could result in an endless loop.</para>
        /// <para>To enable nesting, raise MaxNestingDepth to a value greater than 1.</para>
        /// </summary>
        public int MaxNestingDepth { get; set; } = 1;

        public CyclopsEngine()
        {
            // Note: Internally, GenericPool (and who knows what else) uses RemoveAt within a List and it looks like a design flaw.
            // It wastes a few cycles as a result, but should easily outperform memory allocation + GC hiccups.
            // And who knows... by the time you're reading this, it might be fixed.
            _registry = DictionaryPool<string, HashSet<ICyclopsTaggable>>.Get();
            _routines = GenericPool<Queue<CyclopsRoutine>>.Get();
            _finishedRoutines = GenericPool<Queue<CyclopsRoutine>>.Get();
            _additions = GenericPool<Queue<ICyclopsTaggable>>.Get();
            _removals = GenericPool<Queue<ICyclopsTaggable>>.Get();
            _stopsRequested = GenericPool<Queue<CyclopsStopRoutineRequest>>.Get();
            _messages = GenericPool<Queue<CyclopsMessage>>.Get();
            _pausesRequested = HashSetPool<string>.Get();
            _resumesRequested = HashSetPool<string>.Get();
            _blocksRequested = HashSetPool<string>.Get();
            _timers = DictionaryPool<string, double>.Get();
        }

        public void Dispose()
        {
            DictionaryPool<string, HashSet<ICyclopsTaggable>>.Release(_registry);
            GenericPool<Queue<CyclopsRoutine>>.Release(_routines);
            GenericPool<Queue<CyclopsRoutine>>.Release(_finishedRoutines);
            GenericPool<Queue<ICyclopsTaggable>>.Release(_additions);
            GenericPool<Queue<ICyclopsTaggable>>.Release(_removals);
            GenericPool<Queue<CyclopsStopRoutineRequest>>.Release(_stopsRequested);
            GenericPool<Queue<CyclopsMessage>>.Release(_messages);
            HashSetPool<string>.Release(_pausesRequested);
            HashSetPool<string>.Release(_resumesRequested);
            HashSetPool<string>.Release(_blocksRequested);
            DictionaryPool<string, double>.Release(_timers);
        }

        public void Reset()
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
            _timers.Clear();

            _nextAdditionIsImmediate = false;

            MaxNestingDepth = 1;
        }
        
        // Sequencing Additions

        T ICyclopsRoutineScheduler.Add<T>(T routine)
        {
            Assert.IsNotNull(routine);

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
        
        public void Remove(string tag, bool willStopChildren = true)
        {
            Assert.IsTrue(ValidateTag(tag, out var reason), reason);
            _stopsRequested.Enqueue(new CyclopsStopRoutineRequest(tag, willStopChildren));
        }

        public void Remove(IEnumerable<string> tags, bool willStopChildren = true)
        {
            foreach (var tag in tags)
                Remove(tag, willStopChildren);
        }

        public void Remove(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);
            
            if (taggedObject is CyclopsRoutine)
                ((CyclopsRoutine)taggedObject).Stop();
            else
                _removals.Enqueue(taggedObject);
        }

        // Registration & Housekeeping

        private void Register(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);

            void AddToTaggables(string tag)
            {
                if (_registry.TryGetValue(tag, out var taggables))
                {
                    taggables.Add(taggedObject);
                }
                else
                {
                    taggables = HashSetPool<ICyclopsTaggable>.Get();
                    taggables.Add(taggedObject);
                    _registry[tag] = taggables;
                }
            }

            AddToTaggables(Tag_All);

            foreach (var tag in taggedObject.Tags)
                AddToTaggables(tag);
        }

        private void Unregister(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out var reason), reason);

            void RemoveFromTaggables(string tag)
            {
                if (_registry.ContainsKey(tag))
                {
                    var taggables = _registry[tag];

                    taggables.Remove(taggedObject);

                    if (taggables.Count == 0)
                    {
                        HashSetPool<ICyclopsTaggable>.Release(taggables);
                        _registry.Remove(tag);
                    }
                }
            }

            RemoveFromTaggables(Tag_All);

            foreach (var tag in taggedObject.Tags)
                RemoveFromTaggables(tag);
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

        /// <summary>
        /// For debugging purposes only.
        /// </summary>
        /// <param name="results"></param>
        public void CopyTagStatusToList(List<CyclopsTagStatus> results)
        {
            results.Clear();

            CyclopsTagStatus status;

            foreach (var tag in _registry.Keys)
            {
                status.tag = tag;
                status.count = Count(tag);

                results.Add(status);
            }

            // Nobody wants to scan an unsorted list of tags.
            results.Sort();
        }

        /// <summary>
        /// For debugging purposes only.
        /// </summary>
        /// <param name="results"></param>
        public void CopyRoutinesToList(List<CyclopsRoutine> result)
        {
            result.Clear();

            for (int i = 0; i < _routines.Count; ++i)
            {
                var routine = _routines.Dequeue();

                _routines.Enqueue(routine);
                result.Add(routine);
            }
        }
        
        public bool TimerReady(double timeout, string tag, bool canRestart = false)
        {
            Assert.IsTrue(ValidateTimingValue(timeout, out var reason), reason);
            Assert.IsTrue(ValidateTag(tag, out reason), reason);

            if (_timers.TryGetValue(tag, out double secondsRemaining))
            {
                if (secondsRemaining <= 0d)
                {
                    _timers[tag] = timeout;

                    return true;
                }
                else if (canRestart)
                {
                    _timers[tag] = timeout;
                }

                return false;
            }
            else
            {
                _timers[tag] = timeout;

                return true;
            }
        }

        // Messaging

        public void Send(string receiverTag, string name, object sender = null, object data = null, CyclopsMessage.DeliveryStage stage = CyclopsMessage.DeliveryStage.AfterRoutines)
        {
            if (sender == null)
                sender = this;

            var msg = new CyclopsMessage
            {
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
            receiver?.InterceptMessage(msg);
        }

        // TODO: This should live outside the engine.
        //public void TrackAnalytics(string tag, float lingerPeriod = 0f)
        //{
        //    Assert.IsTrue(ValidateTag(tag, out var reason), reason);
        //    Assert.IsTrue(ValidateTimingValue(lingerPeriod, out reason), reason);

        //    Send(tag, Message_Analytics);

        //    if (lingerPeriod > 0f)
        //        Next.Sleep(tag: tag, period: lingerPeriod);
        //}

        // Updates

        private void ProcessRoutines(float deltaTime)
        {
            Assert.IsTrue(ValidateTimingValue(deltaTime, out var reason), reason);
            
            while (_routines.Count > 0)
            {
                var routine = _routines.Dequeue();
                
                _finishedRoutines.Enqueue(routine);

                // Note: If paused routines were removed from this list and added to a holding collection like say a HashSet,
                // they'd typically lose their position in the queue when later resumed.  Deterministic ordering is a nice thing to have.
                if (!routine.IsPaused)
                {
                    Context = routine;
                    routine.Update(deltaTime);

                    // The possibility of an infinite loop caused by nesting Immediately.Add() has passed
                    // unless someone goes out of their way to modify NestingDepth... don't do that.
                    routine.NestingDepth = 0;
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

                // When timers were implemented as CyclopsSleep routines, they were naturally removed as taggables in the registry.
                // That is no longer the case, so we're handling removal manually.
                _ = _timers.Remove(request.routineTag);

                if (_registry.ContainsKey(request.routineTag))
                {
                    foreach (var taggable in _registry[request.routineTag])
                    {
                        if (taggable is CyclopsRoutine routine)
                        {
                            routine.Stop();
                            
                            if (request.stopChildren)
                                routine.RemoveAllChildren();
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
                    ((ICyclopsDisposable)removal).Dispose();
            }

            // Process CyclopsRoutines removals here.

            var scheduler = (ICyclopsRoutineScheduler)this;
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

                                scheduler.Add(child);
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

                            scheduler.Add(child);
                        }
                    }

                    // Note: For CyclopsRoutines, disposing handles memory pooling cleanup.
                    ((ICyclopsDisposable)routine).Dispose();
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
                    if (skipPredicate())
                        return;

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

        // Q. Why aren't paused items removed from the update list for efficiency?
        // A. It would cause non-deterministic reinsertion.
        // Retaining initial order reduces complexity by removing the need to query state.
        // Fragmentation makes a run and placeholder solution unlikely.
        private void ProcessPauseRequests()
        {
            foreach (string tag in _pausesRequested)
                if (_registry.ContainsKey(tag))
                    foreach (ICyclopsTaggable pausationCandidate in _registry[tag])
                        if (pausationCandidate is ICyclopsPausable)
                            ((ICyclopsPausable)pausationCandidate).IsPaused = true;

            _pausesRequested.Clear();
        }

        private void ProcessTimers(double deltaTime)
        {
            foreach (string tag in _timers.Keys)
                _timers[tag] = Math.Max(0d, _timers[tag] - deltaTime);
        }

        public void Update(float deltaTime)
        {
            Assert.IsFalse(_nextAdditionIsImmediate, "CyclopsEngine should not currently be in immediate additions mode.  Add() should follow the use of Immediately.");
            _nextAdditionIsImmediate = false;

            Assert.IsTrue(ValidateTimingValue(deltaTime, out var reason), reason);
            
            DeltaTime = Mathf.Clamp(deltaTime, float.Epsilon, MaxDeltaTime);

            ProcessTimers(deltaTime);
            ProcessMessages(CyclopsMessage.DeliveryStage.BeforeRoutines);
            ProcessRoutines(deltaTime);
            // Delivering messages immediately after all updates are processed is the default behavior.
            // Messages aren't delivered during updates because that would introduce nondeterministic side effects.
            // Using AfterRoutines will keep complexity down.
            ProcessMessages(CyclopsMessage.DeliveryStage.AfterRoutines);
            // Stop, then remove, then add. Order matters.
            ProcessStopRequests();
            ProcessRemovals();
            ProcessAdditions();
            ProcessMessages(CyclopsMessage.DeliveryStage.SoonestPossible);
            // Pause and Resume act on new additions intentionally.
            // Paused routines are NOT removed from the queue.
            // See comments above ProcessPauseRequests() for an explanation.
            ProcessResumeRequests();
            ProcessPauseRequests();
            // Clear blocking tags that were used to filter out additions that might have otherwise been added this frame.
            _blocksRequested.Clear();
        }
    }
}
