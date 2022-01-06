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

namespace Smonch.CyclopsFramework
{
	public class CyclopsInterceptMessage : CyclopsRoutine, ICyclopsMessageInterceptor
	{
        public static readonly string Tag = TagPrefix_Cyclops + "CyclopsInterceptMessage";
		
		public delegate void Fd(CyclopsMessage msg);
		
		Fd _fd;
		
		public CyclopsInterceptMessage(double period, double cycles, Fd f)
            : base(period, cycles, null, Tag)
        {
			_fd = f;
        }
		
		public void InterceptMessage (CyclopsMessage msg)
		{
			_fd(msg);
		}

	}
}
