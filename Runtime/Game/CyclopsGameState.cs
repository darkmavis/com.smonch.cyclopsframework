﻿// Cyclops Framework
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

using UnityEngine;
using UnityEngine.Pool;

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsGameState : CyclopsBaseState
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected CyclopsEngine Engine { get; } = GenericPool<CyclopsEngine>.Get();
        
        protected virtual float DeltaTime => Time.deltaTime;
        
        internal sealed override void Update()
        {
            base.Update();
            Engine.Update(DeltaTime);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            
            Engine.Reset();
            GenericPool<CyclopsEngine>.Release(Engine);
        }
    }
}