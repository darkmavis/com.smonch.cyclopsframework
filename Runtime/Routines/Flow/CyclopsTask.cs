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
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.Assertions;

namespace Smonch.CyclopsFramework
{
    public sealed class CyclopsTask : CyclopsRoutine
    {
        private Task _task;
        private CancellationTokenSource _tokenSource;
        private Action<CyclopsTask> _f;

        /// <summary>
        /// Reference this property from the Action that is passed into the Task in order to see if it should be canceled.
        /// If cancellation is requested, it is the responsibility of the Action to ensure that the request is respected in a timely manner.
        /// OnExit blocks and waits for cancelation.
        /// </summary>
        public bool IsCancellationRequested { get; private set; }

        public static CyclopsTask Instantiate(Task task, CancellationTokenSource tokenSource)
        {
            var result = InstantiateFromPool<CyclopsTask>();

            result._task = task;
            result._tokenSource = tokenSource;

            return result;
        }

        public static CyclopsTask Instantiate(Action<CyclopsTask> f)
        {
            var result = InstantiateFromPool<CyclopsTask>();
            
            result._f = f;

            return result;
        }

        public static CyclopsTask Instantiate(double period, double cycles, Action<CyclopsTask> f)
        {
            var result = InstantiateFromPool<CyclopsTask>(period, cycles);

            result._f = f;
            
            return result;
        }

        protected override void OnRecycle()
        {
            _task = null;
            _tokenSource = null;
            _f = null;

            IsCancellationRequested = false;
        }

        protected override void OnEnter()
        {
            if (_task == null)
            {
                _task = Task.Run(() => _f(this));
            }
            else
            {
                _task.Start();
            }
        }

        protected override void OnUpdate(float t)
        {
            if (_task.IsCanceled || _task.IsCompleted || _task.IsFaulted)
            {
                Stop();
            }
        }

        protected override void OnExit()
        {
            if (_tokenSource != null)
            {
                if (!_tokenSource.IsCancellationRequested)
                    _tokenSource.Cancel();
            }

            IsCancellationRequested = true;

            while (!(_task.IsCanceled || _task.IsCompleted || _task.IsFaulted))
                Thread.Sleep(0);
        }
    }
}
