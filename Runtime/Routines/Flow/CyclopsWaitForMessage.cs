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

namespace Smonch.CyclopsFramework
{
    public class CyclopsWaitForMessage : CyclopsRoutine, ICyclopsMessageInterceptor
    {
        private string _messageName = null;
        private bool _timedOut = true;

        private Action<CyclopsMessage> SuccessHandler { get; set; }

        public static CyclopsWaitForMessage Instantiate(string receiverTag, string messageName)
        {
            var result = InstantiateFromPool<CyclopsWaitForMessage>(double.MaxValue);

            result._messageName = messageName;
            result.AddTag(receiverTag);

            return result;
        }

        public static CyclopsWaitForMessage Instantiate(string receiverTag, string messageName, double timeout, double cycles)
        {
            var result = InstantiateFromPool<CyclopsWaitForMessage>(timeout, cycles);
            
            result._messageName = messageName;
            result.AddTag(receiverTag);

            return result;
        }

        protected override void OnRecycle()
        {
            _messageName = null;
            _timedOut = true;

            SuccessHandler = null;
        }

        public CyclopsRoutine OnSuccess(Action<CyclopsMessage> successHandler)
        {
            SuccessHandler = successHandler;
            return this;
        }

        void ICyclopsMessageInterceptor.InterceptMessage(CyclopsMessage msg)
        {
            if ((_messageName == null) || (_messageName == msg.Name))
            {
                _timedOut = false;
                SuccessHandler?.Invoke(msg);
                StepForward();
            }
        }

        protected override void OnLastFrame()
        {
            if (_timedOut)
                Fail();
        }
    }
}