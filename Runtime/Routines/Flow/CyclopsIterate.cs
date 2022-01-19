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

namespace Smonch.CyclopsFramework
{
	public class CyclopsIterate<T> : CyclopsRoutine
    {
        public const string Tag = TagPrefix_Cyclops + "CyclopsIterate";
		
		IEnumerable<T> _collection;
		int _maxSuccessesPerFrame;
		int _maxIterationsPerFrame;
		IEnumerator<T> _enumerator;
		Func<T, bool> _f;
		
		public CyclopsIterate(IEnumerable<T> collection, int maxSuccessesPerFrame, int maxIterationsPerFrame,
			Func<T, bool> f) : base(0, double.MaxValue, null, Tag)
		{
			_collection = collection;
			_maxSuccessesPerFrame = maxSuccessesPerFrame;
			_maxIterationsPerFrame = maxIterationsPerFrame;
			_enumerator = _collection.GetEnumerator();
			_f = f;
		}
		
		protected override void OnFirstFrame()
		{
			int numIterationsRemaining = _maxIterationsPerFrame;
			
			for (int i = 0; ((i < _maxSuccessesPerFrame) && (numIterationsRemaining > 0)); ++i)
			{
				while (--numIterationsRemaining >= 0)
				{
					if (!_enumerator.MoveNext())
					{
						Stop();
						return;
					}
					
					if (_f(_enumerator.Current))
                        break;
				}
			}
		}
	}
}

