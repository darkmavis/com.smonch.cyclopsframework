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

using System.Threading;

namespace Smonch.CyclopsFramework
{
	public class CyclopsInterceptMessage : CyclopsRoutine, ICyclopsMessageInterceptor
	{
		public delegate void Fd(CyclopsMessage msg);
		
		private Fd _fd;
		
		public static CyclopsInterceptMessage Instantiate(double period, double cycles, Fd f)
        {
            var result = InstantiateFromPool<CyclopsInterceptMessage>(period, cycles);
			
			result._fd = f;

			return result;
		}

		protected override void OnRecycle()
		{
			_fd = null;
		}

		public void InterceptMessage(CyclopsMessage msg)
		{
			_fd(msg);
		}
    }
}
