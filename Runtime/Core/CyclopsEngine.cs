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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public sealed class CyclopsEngine : CyclopsCommon, ICyclopsRoutineScheduler, IDisposable
    {
        /// The registry stores the relationships between tags and tagged objects.
        private readonly Dictionary<string, HashSet<ICyclopsTaggable>> _registry;
        
        /// This contains all actions to be added on a given frame after general processing is complete.
        private readonly Queue<ICyclopsTaggable> _additions;
        
        /// This contains all actions to be removed on a given frame after general processing is complete.
        private readonly Queue<ICyclopsTaggable> _removals;
        
        //// This contains all routines to be processed at the beginning of a frame.
        private Queue<CyclopsRoutine> _routines;
        
        /// This exists for double buffering purposes.
        private Queue<CyclopsRoutine> _finishedRoutines;
        
        /// This contains stop requests (by tag) for tagged objects.
        /// Any object with a requested tag will be stopped.
        /// If multiple objects contain the same tag, they will all be stopped.
        private readonly Queue<CyclopsStopRoutineRequest> _stopRequests;
        
        /// This contains messages to be delivered to tagged objects.
        private readonly Queue<CyclopsMessage> _messages;
        
        /// <p>This contains pause requests (by tag) for tagged objects.
        /// Any object with a requested tag will be paused.
        /// If multiple objects contain the same tag, they will all be paused.</p>
        /// <p>Please note that pause requests are processed after resume requests.
        /// Items that have just been resumed are still eligible to be paused.</p>
        private readonly HashSet<string> _pauseRequests;
        
        /// <p>This contains resume requests (by tag) for currently paused tagged objects.
        /// Any object with a requested tag will be resumed if already paused.
        /// If multiple objects contain the same tag, they will all be resumed.</p>
        /// <p>Please note that pause requests are processed after resume requests.
        /// Items that have just been resumed are still eligible to be paused.</p>
        private readonly HashSet<string> _resumeRequests;
        
        /// <p>This contains block requests (by tag) for tagged objects.
        /// Block requests prevent a tagged object from being added at anytime
        /// during the current frame if it contains a specified tag.</p>
        /// <p>This prevent sometimes unforeseen concurrency issues such as that where
        /// a message delivery might cause a tagged object to be added after removals are complete.</p>
        private readonly HashSet<string> _blockingRequests;
        
        private bool _isNextAdditionImmediate;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public float DeltaTime { get; private set; }
        public float Fps => 1f / DeltaTime;
        
        /// <summary>
        /// <para><b>Always prefer <see cref="NextFrame"/> to <see cref="Immediately"/> unless</b> there is a compelling reason to make something happen at the end of this frame.</para>
        /// <para>Starting new routines on the next frame improves determinism and as such will make everything easier to reason about. This advice applies to more than just Cyclops Framework.</para>
        /// <para><see cref="NextFrame"/> allows a chained Add method (e.g. <see cref="NextFrame"/>.Add(foo)) to be processed during the next <see cref="ProcessRoutines"/> call.
        /// Routines are processed in the order they are added. If a routine is paused, it will keep it's place in the queue in order to keep things easier to reason about.</para>
        /// <para>Tip: <see cref="NextFrame"/> can be use with other common methods that use <see cref="CyclopsNext.Add{T}"/> internally such as <see cref="CyclopsNext.Listen(string,string)"/>,
        /// <see cref="CyclopsNext.Sleep"/>, <see cref="CyclopsNext.WaitUntil(System.Func{bool})"/>, etc.</para>
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public CyclopsNext NextFrame => CyclopsNext.Rent(this);
        
        /// <summary>
        /// <para><b>Always prefer <see cref="NextFrame"/> to <see cref="Immediately"/> unless</b> there is a compelling reason to make something happen at the end of this frame.</para>
        /// <para>Immediately allows a chained Add method (e.g. <see cref="Immediately"/>.Add(foo)) to be processed at the end of either the current or next ProcessRoutines call.</para>
        /// <para>If Immediately is used before the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the same frame.</para>
        /// <para>If Immediately is used after the end of the current frame's ProcessRoutines call, the addition will be enqueued and processed on the next frame.</para>
        /// <para>Tip: Immediately can be use with other common methods that use <see cref="CyclopsNext.Add{T}"/> internally such as <see cref="CyclopsNext.Listen(string,string)"/>,
        /// <see cref="CyclopsNext.Sleep"/>, <see cref="CyclopsNext.WaitUntil(System.Func{bool})"/>, etc.</para>
        /// </summary>
        public CyclopsNext Immediately
        {
            get
            {
                _isNextAdditionImmediate = true;
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
        
        /// <summary>
        /// <p><see cref="CyclopsEngine"/> is the heart of the Cyclops Framework but stands alone as well.
        /// An <see cref="CyclopsEngine"/> controls sequencing, timing, tagging, messaging, and queries.</p>
        /// <p>Feel free to create as many <see cref="CyclopsEngine"/>s as required throughout your project.</p>
        /// </summary>
        public CyclopsEngine()
        {
            _registry = DictionaryPool<string, HashSet<ICyclopsTaggable>>.Get();
            _routines = GenericPool<Queue<CyclopsRoutine>>.Get();
            _finishedRoutines = GenericPool<Queue<CyclopsRoutine>>.Get();
            _additions = GenericPool<Queue<ICyclopsTaggable>>.Get();
            _removals = GenericPool<Queue<ICyclopsTaggable>>.Get();
            _stopRequests = GenericPool<Queue<CyclopsStopRoutineRequest>>.Get();
            _messages = GenericPool<Queue<CyclopsMessage>>.Get();
            _pauseRequests = HashSetPool<string>.Get();
            _resumeRequests = HashSetPool<string>.Get();
            _blockingRequests = HashSetPool<string>.Get();
        }
        
        /// <summary>
        /// Dispose of the <see cref="CyclopsEngine"/> and all of its pooled resources.
        /// </summary>
        public void Dispose()
        {
            DictionaryPool<string, HashSet<ICyclopsTaggable>>.Release(_registry);
            GenericPool<Queue<CyclopsRoutine>>.Release(_routines);
            GenericPool<Queue<CyclopsRoutine>>.Release(_finishedRoutines);
            GenericPool<Queue<ICyclopsTaggable>>.Release(_additions);
            GenericPool<Queue<ICyclopsTaggable>>.Release(_removals);
            GenericPool<Queue<CyclopsStopRoutineRequest>>.Release(_stopRequests);
            GenericPool<Queue<CyclopsMessage>>.Release(_messages);
            HashSetPool<string>.Release(_pauseRequests);
            HashSetPool<string>.Release(_resumeRequests);
            HashSetPool<string>.Release(_blockingRequests);
        }

        /// <summary>
        /// Reset the Engine for disposal or reuse.
        /// </summary>
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
            _stopRequests.Clear();
            _messages.Clear();
            _pauseRequests.Clear();
            _resumeRequests.Clear();
            _blockingRequests.Clear();

            _isNextAdditionImmediate = false;

            MaxNestingDepth = 1;
        }
        
        // Sequencing Additions

        /// <summary>
        /// This adds <see cref="CyclopsRoutine"/> to the engine for use on the next frame.
        /// It is possibly the most commonly used method in the entire framework.
        /// </summary>
        T ICyclopsRoutineScheduler.Add<T>(T routine)
        {
            Debug.Assert(routine is not null);

            routine.Host = this;

            if (_isNextAdditionImmediate)
            {
                _isNextAdditionImmediate = false;

                if (Context is null)
                    routine.NestingDepth = 1;
                else
                    routine.NestingDepth = Context.NestingDepth + 1;
                
                Debug.Assert(routine.NestingDepth <= MaxNestingDepth,
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

        /// <summary>
        /// This registers <see cref="ICyclopsTaggable"/> objects with the engine.
        /// Use <see cref="Add"/> for <see cref="CyclopsRoutine"/>s. 
        /// </summary>
        /// <param name="taggable">target</param>
        public void AddTaggable(ICyclopsTaggable taggable)
        {
            Debug.Assert(ValidateTaggable(taggable, out string reason), reason);
            Register(taggable);
        }

        // Control Flow
        
        /// <summary>
        /// Pause any routines with the specified tag.
        /// Please see the order of operations in <see cref="Update"/> for more information.
        /// </summary>
        /// <param name="tag">registered tag</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Pause(string tag)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);
            _pauseRequests.Add(tag);
        }
        
        /// <summary>
        /// Resume any routines with the specified tag that were previously paused.
        /// Please see the order of operations in <see cref="Update"/> for more information.
        /// </summary>
        /// <param name="tag">registered tag</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Resume(string tag)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);
            _resumeRequests.Add(tag);
        }
        
        /// <summary>
        /// Prevent any new additions with the specified tag from being added this frame.
        /// </summary>
        /// <param name="tag">registered tag</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Block(string tag)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);
            _blockingRequests.Add(tag);
        }
        
        /// <summary>
        /// Remove a CyclopsRoutine or other ICyclopsTaggable by tag.
        /// </summary>
        /// <param name="tag">registered tag</param>
        /// <param name="willStopChildren">If this is a CyclopsRoutine, will children be skipped?</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Remove(string tag, bool willStopChildren = true)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);
            _stopRequests.Enqueue(new CyclopsStopRoutineRequest(tag, willStopChildren));
        }
        
        /// <summary>
        /// Remove a CyclopsRoutine or other ICyclopsTaggable by reference.
        /// </summary>
        /// <param name="taggedObject">ICyclopsTaggable such as CyclopsRoutine</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Remove(ICyclopsTaggable taggedObject)
        {
            Debug.Assert(ValidateTaggable(taggedObject, out string reason), reason);
            
            if (taggedObject is CyclopsRoutine routine)
                routine.Stop();
            else
                _removals.Enqueue(taggedObject);
        }

        // Registration & Housekeeping

        private void Register(ICyclopsTaggable taggedObject)
        {
            Debug.Assert(ValidateTaggable(taggedObject, out string reason), reason);

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
            Debug.Assert(ValidateTaggable(taggedObject, out string reason), reason);

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
        
        /// <summary>
        /// Query the number of objects currently registered with the specified tag.
        /// </summary>
        /// <param name="tag">registered tag</param>
        /// <returns>Number of objets found.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public int Count(string tag)
        {
            Debug.Assert(ValidateTag(tag, out string reason), reason);

            int result = 0;

            if (_registry.TryGetValue(tag, out var taggables))
                result = taggables.Count;

            return result;
        }

        /// <summary>
        /// Check whether anything is currently registered with the specified tag.
        /// </summary>
        /// <param name="tag">registered tag</param>
        /// <returns>Result</returns>
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
                
                status.Tag = tag;
                status.Count = Count(tag);

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

        /// <summary>
        /// Send a <see cref="CyclopsMessage"/> to all <see cref="ICyclopsMessageInterceptor"/> objects registered with a given tag.
        /// </summary>
        public void Send(string receiverTag, string name, object sender = null, object data = null,
            CyclopsMessage.DeliveryStage stage = CyclopsMessage.DeliveryStage.AfterRoutines)
        {
            sender ??= this;

            var msg = new CyclopsMessage
            {
                ReceiverTag = receiverTag,
                Name = name,
                Sender = sender,
                Data = data,
                Stage = stage,
                IsBroadcast = false
            };

            Send(msg);
        }
        
        /// <summary>
        /// Send a <see cref="CyclopsMessage"/> to all <see cref="ICyclopsMessageInterceptor"/> objects registered with a given tag.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void Send(CyclopsMessage msg)
        {
            Debug.Assert(msg.Sender is not null, "Sender must not be null.");
            Debug.Assert(ValidateTag(msg.ReceiverTag, out string reason), reason);

            _messages.Enqueue(msg);
        }

        private static void DeliverMessage(CyclopsMessage msg, ICyclopsMessageInterceptor receiver)
            => receiver?.InterceptMessage(msg);
        
        // Updates

        private void ProcessRoutines(float deltaTime)
        {
            Debug.Assert(ValidateTimingValue(deltaTime, out string reason), reason);
            
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

                if ((msg.Stage != CyclopsMessage.DeliveryStage.SoonestPossible) && (msg.Stage != stage))
                {
                    _messages.Enqueue(msg);
                    continue;
                }

                if (!_registry.TryGetValue(msg.ReceiverTag, out var taggables))
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
            for (int i = 0; i < _stopRequests.Count; ++i)
            {
                CyclopsStopRoutineRequest request = _stopRequests.Dequeue();
                
                if (!_registry.ContainsKey(request.RoutineTag))
                    continue;
                
                foreach (ICyclopsTaggable taggable in _registry[request.RoutineTag])
                {
                    if (taggable is CyclopsRoutine routine)
                    {
                        routine.Stop();
                            
                        if (request.StopChildren)
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

                    if (_blockingRequests.Count > 0)
                    {
                        foreach (CyclopsRoutine child in routine.Children)
                        {
                            if (_blockingRequests.Overlaps(child.Tags))
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
            if (_blockingRequests.Overlaps(additionCandidate.Tags))
                return;
            
            if (additionCandidate is CyclopsRoutine addition)
            {
                var skipPredicate = addition.SkipPredicate;
                
                if (skipPredicate?.Invoke() ?? false)
                    return;

                _routines.Enqueue(addition);
            }

            Register(additionCandidate);
        }

        private void ProcessResumeRequests()
        {
            foreach (string tag in _resumeRequests)
                if (_registry.TryGetValue(tag, out var resumptionCandidates))
                    foreach (ICyclopsTaggable candidate in resumptionCandidates)
                        if (candidate is ICyclopsPausable pausable)
                            pausable.IsPaused = false;

            _resumeRequests.Clear();
        }

        // Q. Why aren't paused items removed from the update list for efficiency?
        // A. It would cause non-deterministic reinsertion.
        // Retaining initial order reduces complexity by removing the need to query state.
        // Fragmentation makes a placeholder solution unlikely.
        private void ProcessPauseRequests()
        {
            foreach (string tag in _pauseRequests)
                if (_registry.TryGetValue(tag, out var pausationCandidates))
                    foreach (ICyclopsTaggable candidate in pausationCandidates)
                        if (candidate is ICyclopsPausable pausable)
                            pausable.IsPaused = true;

            _pauseRequests.Clear();
        }
        
        /// <summary>
        /// <para>This is the main update method that drives CyclopsEngine and it is expected to be called once per frame.
        /// It uses delta time and it's important to note that MaxDeltaTime is limited to 1/4 of a second.</para>
        /// <para>This method can be driven using either real time or fixed time depending on what is needed.</para>
        /// <para><b>Please take a look under the hood.<br/>The ORDER OF EXECUTION is VERY IMPORTANT.</b></para>
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            Debug.Assert(!_isNextAdditionImmediate, "CyclopsEngine should not currently be in immediate additions mode.  Add() should follow the use of Immediately.");
            _isNextAdditionImmediate = false;

            Debug.Assert(ValidateTimingValue(deltaTime, out string reason), reason);
            
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
            _blockingRequests.Clear();
        }
    }
}
