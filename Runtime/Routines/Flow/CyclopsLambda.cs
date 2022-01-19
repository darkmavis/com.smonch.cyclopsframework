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
    public class CyclopsLambda : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "CyclopsLambda";
		
		private object _data;

		Action _f;
		Action<object> _fd;
		
        public CyclopsLambda(Action f)
			: base(0, 1, null, Tag)
        {
			_f = f;
        }

        public CyclopsLambda(object data, Action<object> f)
			: base(0, 1, null, Tag)
        {
			_data = data;
			_fd = f;
        }
		
		public CyclopsLambda(double period, double cycles, Action f)
			: base(period, cycles, null, Tag)
        {
			_f = f;
        }
		
		public CyclopsLambda(double period, double cycles, object data, Action<object> f)
			: base(period, cycles, null, Tag)
        {
			_data = data;
			_fd = f;
        }

		protected override void OnFirstFrame()
		{
			if (_data != null)
				_fd(_data);
			else
				_f();
		}
    }
}
