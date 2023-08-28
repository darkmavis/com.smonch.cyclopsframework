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

namespace Smonch.CyclopsFramework
{
    public abstract class CyclopsCommon
    {
        public const string TagAll = "*";
        public const string TagLog = "Cyclops.Log";
        public const string TagPrefixNoncascading = "!";
        public const float MaxDeltaTime = .25f;
        
        public CyclopsRoutine Context { get; protected set; }
        
        public static Action<string> Logger { get; set; }

        /// <summary>
        /// Validates that a Cyclops Framework tag is actually useful.
        /// A tag can't be null and must contain at least one non-whitespace character.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected static bool ValidateTag(string tag, out string reason)
        {
            reason = string.Empty;

            if (!string.IsNullOrWhiteSpace(tag))
                return true;
            
            reason = tag is null ? "Tag is null." : "A tag must contain a least one non-whitespace character.";
            
            return false;
        }

        /// <summary>
        /// Validates that a timing value which could be used for a timer or routine is actually useful.
        /// </summary>
        /// <param name="timingValue"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected static bool ValidateTimingValue(double timingValue, out string reason)
        {
            if (!ValidateTimingValueWhereZeroIsOk(timingValue, out reason))
                return false;

            if (timingValue != 0d)
                return true;
            
            reason = "Timing value must be greater than zero.";
            
            return false;
        }

        /// <summary>
        /// Validates that a timing value which could be used for a timer or routine is actually useful.
        /// Zero is Ok.
        /// </summary>
        /// <param name="timingValue"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected static bool ValidateTimingValueWhereZeroIsOk(double timingValue, out string reason)
        {
            bool result;
            
            switch (timingValue)
            {
                case double.NegativeInfinity or double.PositiveInfinity:
                    reason = "Timing value must be finite.";
                    result = false;
                    break;
                case double.NaN:
                    reason = "Timing value must be a number.";
                    result = false;
                    break;
                case < 0d:
                    reason = "Timing value must be positive.";
                    result = false;
                    break;
                default:
                    reason = null;
                    result = true;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Validates various aspects of ICyclopsTaggable and instantiates the appropriate reason as needed.<br/>
        /// If validation is successful, reason will be null.<br/>
        /// </summary>
        /// <param name="taggable"></param>
        /// <param name="reason"></param>
        /// <returns>Returns false if validation fails, otherwise returns true.</returns>
        protected static bool ValidateTaggable(ICyclopsTaggable taggable, out string reason)
        {
            reason = null;

            if (taggable == null)
            {
                reason = "ICyclopsTaggable.Tags must not be null.";
                return false;
            }

            foreach (string tag in taggable.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                    continue;
                
                reason = $"Tags for {taggable.GetType()} can't be null and must contain non-whitespace characters.";
                return false;
            }

            return true;
        }
    }
}