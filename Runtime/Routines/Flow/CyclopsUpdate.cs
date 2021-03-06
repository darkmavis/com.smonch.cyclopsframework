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

namespace Smonch.CyclopsFramework
{
    public class CyclopsUpdate : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + nameof(CyclopsUpdate);

        private Action _f = null;
		private Action<float> _ft = null;

        private CyclopsUpdate(
            double period,
            Action f)
            : base(period, 1.0, null, Tag)
        {
            _f = f;
        }

        private CyclopsUpdate(
            double period,
            double cycles,
            Func<float, float> bias,
            Action<float> ft)
            : base(period, cycles, bias, Tag)
        {
            _ft = ft;
		}

        public static CyclopsUpdate Instantiate(
            double period,
            Action f)
        {
            return new CyclopsUpdate(period, f);

            //if (TryInstantiateFromPool(() => new CyclopsUpdate(period, f), out var result))
            //{
            //    result.Period = period;

            //    result._f = f;
            //}

            //return result;
        }

        public static CyclopsUpdate Instantiate(
            double period,
            double cycles,
            Func<float, float> bias,
            Action<float> ft)
        {
            return new CyclopsUpdate(period, cycles, bias, ft);

            //if (TryInstantiateFromPool(() => new CyclopsUpdate(period, cycles, bias, ft), out var result))
            //{
            //    result.Period = period;
            //    result.MaxCycles = cycles;
            //    result.Bias = bias;

            //    result._ft = ft;
            //}

            //return result;
        }

        protected override void OnRecycle()
        {
            _f = null;
            _ft = null;
        }

        protected override void OnUpdate(float t)
        {
            if (_ft == null)
            {
                _f();
            }
            else
            {
                _ft(t);
            }
        }
    }
}
