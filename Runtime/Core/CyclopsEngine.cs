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
        private readonly Dictionary<string, HashSet<ICyclopsTaggable>> _registry;
        private readonly Queue<ICyclopsTaggable> _additions;
        private readonly Queue<ICyclopsTaggable> _removals;
        private Queue<CyclopsRoutine> _routines;
        private Queue<CyclopsRoutine> _finishedRoutines;
        private readonly Queue<CyclopsStopRoutineRequest> _stopsRequested;
        private readonly Queue<CyclopsMessage> _messages;
        private readonly HashSet<string> _pausesRequested;
        private readonly HashSet<string> _resumesRequested;
        private readonly HashSet<string> _blocksRequested;
        private bool _nextAdditionIsImmediate;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public float DeltaTime { get; private set; }
        public float Fps => 1f / DeltaTime;
        // ReSharper disable once MemberCanBePrivate.Global
        public CyclopsNext NextFrame => CyclopsNext.Rent(this);
        
        [Obsolete("Use NextFrame for clarity.")]
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
                return NextFrame;
            }
        }
        
        /// <summary>
        /// <para>This limits the nesting depth of CyclopsRoutines that are immediately enqueued within a CyclopsRoutine that was itself enqueued on the same frame.</para>
        /// <para>While nesting is perfectly safe and predictable, it should still be considered the exception to the rule.</para>
        /// <para>Please use <see cref="Immediately"/> only when required. Failure to provide a limit combined with erroneous code could result in an endless loop.</para>
        /// <para>To enable nesting, raise MaxNestingDepth to a value greater than 1.</para>
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
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
        }

        public void Reset()
        {
            Remove(TagAll);
            Block(TagAll);
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

                if (Context is null)
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
            Assert.IsTrue(ValidateTaggable(o, out string reason), reason);
            Register(o);
        }

        // Control Flow
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Pause(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);
            _pausesRequested.Add(tag);
        }

        public void Pause(IEnumerable<string> tags)
        {
            foreach (string tag in tags)
                Pause(tag);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Resume(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);
            _resumesRequested.Add(tag);
        }
        
        public void Resume(IEnumerable<string> tags)
        {
            foreach (string tag in tags)
                Resume(tag);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Block(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);
            _blocksRequested.Add(tag);
        }

        public void Block(IEnumerable<string> tags)
        {
            foreach (string tag in tags)
                Block(tag);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Remove(string tag, bool willStopChildren = true)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);
            _stopsRequested.Enqueue(new CyclopsStopRoutineRequest(tag, willStopChildren));
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Remove(IEnumerable<string> tags, bool willStopChildren = true)
        {
            foreach (string tag in tags)
                Remove(tag, willStopChildren);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Remove(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out string reason), reason);
            
            if (taggedObject is CyclopsRoutine routine)
                routine.Stop();
            else
                _removals.Enqueue(taggedObject);
        }

        // Registration & Housekeeping

        private void Register(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out string reason), reason);

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

            AddToTaggables(TagAll);

            foreach (var tag in taggedObject.Tags)
                AddToTaggables(tag);
        }

        private void Unregister(ICyclopsTaggable taggedObject)
        {
            Assert.IsTrue(ValidateTaggable(taggedObject, out string reason), reason);

            void RemoveFromTaggables(string tag)
            {
                if (!_registry.ContainsKey(tag))
                    return;
                
                var taggables = _registry[tag];

                taggables.Remove(taggedObject);

                if (taggables.Count != 0)
                    return;
                
                HashSetPool<ICyclopsTaggable>.Release(taggables);
                _registry.Remove(tag);
            }

            RemoveFromTaggables(TagAll);

            foreach (string tag in taggedObject.Tags)
                RemoveFromTaggables(tag);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public int Count(string tag)
        {
            Assert.IsTrue(ValidateTag(tag, out string reason), reason);

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
            
            foreach (string tag in _registry.Keys)
            {
                CyclopsTagStatus status;
                
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
        /// <param name="result"></param>
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
        
        // Messaging

        public void Send(string receiverTag, string name, object sender = null, object data = null,
            CyclopsMessage.DeliveryStage stage = CyclopsMessage.DeliveryStage.AfterRoutines)
        {
            sender ??= this;

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
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Send(CyclopsMessage msg)
        {
            Assert.IsNotNull(msg.sender, "Sender must not be null.");
            Assert.IsTrue(ValidateTag(msg.receiverTag, out string reason), reason);

            _messages.Enqueue(msg);
        }

        private static void DeliverMessage(CyclopsMessage msg, ICyclopsMessageInterceptor receiver)
            => receiver?.InterceptMessage(msg);
        
        // Updates

        private void ProcessRoutines(float deltaTime)
        {
            Assert.IsTrue(ValidateTimingValue(deltaTime, out string reason), reason);
            
            while (_routines.Count > 0)
            {
                CyclopsRoutine routine = _routines.Dequeue();
                
                _finishedRoutines.Enqueue(routine);

                // Note: If paused routines were removed from this list and added to a holding collection like say a HashSet,
                // they'd typically lose their position in the queue when later resumed.  Deterministic ordering is a nice thing to have.
                if (routine.IsPaused)
                    continue;
                
                Context = routine;
                routine.Update(deltaTime);

                // The possibility of an infinite loop caused by nesting Immediately.Add() has passed
                // unless someone goes out of their way to modify NestingDepth... don't do that.
                routine.NestingDepth = 0;
            }

            Context = null;
            (_routines, _finishedRoutines) = (_finishedRoutines, _routines);
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

                if (!_registry.TryGetValue(msg.receiverTag, out var taggables))
                    continue;
                
                foreach (ICyclopsTaggable possibleReceiver in taggables)
                {
                    if (possibleReceiver is ICyclopsMessageInterceptor receiver)
                        DeliverMessage(msg, receiver);
                }
            }
        }

        private void ProcessStopRequests()
        {
            for (int i = 0; i < _stopsRequested.Count; ++i)
            {
                CyclopsStopRoutineRequest request = _stopsRequested.Dequeue();
                
                if (!_registry.ContainsKey(request.routineTag))
                    continue;
                
                foreach (ICyclopsTaggable taggable in _registry[request.routineTag])
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

        private void ProcessRemovals()
        {
            // CyclopsRoutines are excluded from the _removals list.
            // This is where everything else is unregistered and properly disposed of if required.

            while (_removals.Count > 0)
            {
                var removal = _removals.Dequeue();

                Unregister(removal);

                if (removal is ICyclopsDisposable disposable)
                    disposable.Dispose();
            }

            // Process CyclopsRoutines removals here.

            var scheduler = (ICyclopsRoutineScheduler)this;
            int routineCount = _routines.Count;

            for (int i = 0; i < routineCount; ++i)
            {
                CyclopsRoutine routine = _routines.Dequeue();

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
                        foreach (CyclopsRoutine child in routine.Children)
                        {
                            if (_blocksRequested.Overlaps(child.Tags))
                                continue;
                            
                            foreach (string tag in routine.Tags)
                                if (!tag.StartsWith(TagPrefixNoncascading))
                                    child.AddTag(tag);

                            scheduler.Add(child);
                        }
                    }
                    else
                    {
                        foreach (CyclopsRoutine child in routine.Children)
                        {
                            foreach (string tag in routine.Tags)
                                if (!tag.StartsWith(TagPrefixNoncascading))
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
            if (_blocksRequested.Overlaps(additionCandidate.Tags))
                return;
            
            if (additionCandidate is CyclopsRoutine addition)
            {
                var skipPredicate = addition.SkipPredicate;
                
                if (skipPredicate is not null)
                    if (skipPredicate())
                        return;

                _routines.Enqueue(addition);
            }

            Register(additionCandidate);
        }

        private void ProcessResumeRequests()
        {
            foreach (string tag in _resumesRequested)
                if (_registry.TryGetValue(tag, out var resumptionCandidates))
                    foreach (ICyclopsTaggable candidate in resumptionCandidates)
                        if (candidate is ICyclopsPausable pausable)
                            pausable.IsPaused = false;

            _resumesRequested.Clear();
        }

        // Q. Why aren't paused items removed from the update list for efficiency?
        // A. It would cause non-deterministic reinsertion.
        // Retaining initial order reduces complexity by removing the need to query state.
        // Fragmentation makes a placeholder solution unlikely.
        private void ProcessPauseRequests()
        {
            foreach (string tag in _pausesRequested)
                if (_registry.TryGetValue(tag, out var pausationCandidates))
                    foreach (ICyclopsTaggable candidate in pausationCandidates)
                        if (candidate is ICyclopsPausable pausable)
                            pausable.IsPaused = true;

            _pausesRequested.Clear();
        }
        
        public void Update(float deltaTime)
        {
            Assert.IsFalse(_nextAdditionIsImmediate, "CyclopsEngine should not currently be in immediate additions mode.  Add() should follow the use of Immediately.");
            _nextAdditionIsImmediate = false;

            Assert.IsTrue(ValidateTimingValue(deltaTime, out string reason), reason);
            
            DeltaTime = Mathf.Clamp(deltaTime, float.Epsilon, MaxDeltaTime);
            
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
