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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Smonch.CyclopsFramework
{
    [TestFixture]
    [Author("Mark Davis")]
    [Description("Unit Tests")]
    public class CyclopsEngineTests
    {
        private const float MaxDeltaTime = CyclopsCommon.MaxDeltaTime;
        private const float Timeout_TwiceMaxDeltaTime = MaxDeltaTime * 2;
        private const string ValidTag = "test";
        private const string ValidMessageName = "TestMessage";
        private const string ValidMessageSender = "sender";

        private static string[] InvalidTags => new[]
        {
            "", " ", "\t", "\r", "\v", "\f", "\n", "\t\t", "\t\n\t", "\t  \n  \t"
        };

        //private static string[] ValidTags => new[]
        //{
        //    "\t\n!\n\t!\n",
        //    "*", "*A", "**Abc",
        //    "!", "!A", "!!Abc",
        //    " ._#$@!foo.barAbc!@$#_. ",
        //    string.Join("", Enumerable.Repeat("0123456789ABCDEF", 17))
        //};

        //private static string[] ValidMessageNames => new[]
        //{
        //    "A", "z",
        //    "A0", "z9",
        //    "1", "1234567890",
        //    "The Quick Brown Fox Jumped Over The Lazy Dogs",
        //    "0___Abc123___9",
        //    string.Join("", Enumerable.Repeat("0123456789ABCDEF", 17))
        //};

        //private static object[] ValidMessageSenders => new[]
        //{
        //    "", " ", "__a", "Foo", "123",
        //    0, new object(), new[] { 1, 2, 3 },
        //};

        private static float[] ValidDeltaTimes => new[] {
            float.MaxValue,
            CyclopsCommon.MaxDeltaTime,
            float.Epsilon
        };

        private static float[] InvalidDeltaTimes => new[] {
            0f,
            -float.Epsilon,
            -CyclopsCommon.MaxDeltaTime / 3f,
            -CyclopsCommon.MaxDeltaTime,
            -CyclopsCommon.MaxDeltaTime * 3f,
            -1f,
            -2f,
            -8/3f,
            float.MinValue,
            float.NegativeInfinity,
            float.PositiveInfinity,
            float.NaN
        };

        private CyclopsEngine Host { get; set; }

        [SetUp]
        public void OnSetup()
        {
            LogAssert.ignoreFailingMessages = false;
            Host = new CyclopsEngine();
        }

        [TearDown]
        public void OnTeardown()
        {
            Host.Dispose();
            Host = null;
        }

        [Test]
        [Category("Smoke")]
        [Category("Tags")]
        public void Nop_WithDefaultTag_ExistsOnNextFrame(
            [Values(MaxDeltaTime)] float deltaTime)
        {
            Host.Nop();
            Host.Update(deltaTime);

            Assert.NotZero(Host.Count(CyclopsCommon.Tag_Nop));
        }

        [Test]
        [Category("Smoke")]
        [Category("Tags")]
        public void Nop_WithDefaultTag_CountIsOneOnNextFrame(
            [Values(CyclopsCommon.Tag_Nop)] string tag,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            Host.Nop();
            Host.Update(deltaTime);

            Assert.NotZero(Host.Count(tag));
        }

        [Test]
        [Category("Smoke")]
        [Category("Tags")]
        public void Nop_WithValidTag_CountIsOneOnNextFrame(
            [Values(ValidTag)] string tag,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            Host.Nop(tag: tag);
            Host.Update(deltaTime);

            Assert.NotZero(Host.Count(tag));
        }

        [Test]
        [Category("Smoke")]
        [Category("Updates")]
        public void UpdateLambda_WithValidDeltaTime_DecrementsToZero(
            [ValueSource(nameof(ValidDeltaTimes))] float deltaTime)
        {
            int x = 2;

            Host.Add(() => --x)
                .Add(() => --x);

            Host.Update(deltaTime); // Added
            Host.Update(deltaTime); // --x == 1
            Host.Update(deltaTime); // --x == 0

            Assert.Zero(x);
        }

        [Test]
        [Category("Smoke")]
        [Category("Updates")]
        public void UpdateLambda_WithInvalidDeltaTime_ThrowsAssertionException(
            [ValueSource(nameof(InvalidDeltaTimes))] float deltaTime)
        {
            Assert.Throws(typeof(UnityEngine.Assertions.AssertionException), () => Host.Update(deltaTime));
        }

        [Test]
        [Category("Smoke")]
        [Category("Nested Routines")]
        public void ImmediatelyAdd_OneNopWithDefaultMaxNestingDepthOfOne_DoesNotThrowAssertionException(
            [Values(MaxDeltaTime)] float deltaTime)
        {
            Host.Immediately.Nop();

            Assert.DoesNotThrow(() => Host.Update(CyclopsCommon.MaxDeltaTime));
        }

        [Test]
        [Category("Smoke")]
        [Category("Nested Routines")]
        public void ImmediatelyAdd_TwoNestedRoutinesWithDefaultMaxNestingDepthOfOne_ThrowsAssertionException(
            [Values(MaxDeltaTime)] float deltaTime)
        {
            bool isOkA = false;
            bool isOkB = false;
            bool wasCaught = false;

            Host.Immediately.Add(() => { isOkA = true; Host.Immediately.Add(() => isOkB = true); });
            Host.RoutineExceptionCaught += (r, s, e) => { wasCaught = e is UnityEngine.Assertions.AssertionException; };

            LogAssert.ignoreFailingMessages = true;
            Host.Update(CyclopsCommon.MaxDeltaTime);
            LogAssert.ignoreFailingMessages = false;

            Assert.IsTrue(wasCaught && isOkA && !isOkB);
        }

        [Test]
        [Category("Smoke")]
        [Category("Nested Routines")]
        public void ImmediatelyAdd_FiveNestedRoutinesWithDefaultMaxNestingDepthOfFive_DoesNotThrowAndDecrementsToZero(
            [Values(MaxDeltaTime)] float deltaTime)
        {
            int x = 5;

            Host.MaxNestingDepth = 5;

            // Please note: Real world code is not going to look anything like this... unless you really want it to.
            // The actual problem would most likely present itself in an overridden CyclopsRoutine virtual method
            // and look all nice, spiffy, and totally legit.
            Host.Immediately.Add(() => {
                --x; Host.Immediately.Add(() => {
                    --x; Host.Immediately.Add(() => {
                        --x; Host.Immediately.Add(() => {
                            --x; Host.Immediately.Add(() => {
                                --x;
                            });
                        });
                    });
                });
            });

            Assert.DoesNotThrow(() => Host.Update(CyclopsCommon.MaxDeltaTime));
            Assert.Zero(x);
        }

        [Test]
        [Category("Smoke")]
        [Category("Nested Routines")]
        public void ImmediatelyAdd_ThreeNestedRoutinesWithDefaultMaxNestingDepthOfTwo_ThrowsAssertionException(
            [Values(MaxDeltaTime)] float deltaTime)
        {
            bool isOkA = false;
            bool isOkB = false;
            bool isOkC = false;
            bool wasCaught = false;

            Host.MaxNestingDepth = 2;

            // Please note: Real world code is not going to look anything like this... unless you really want it to.
            // The actual problem would most likely present itself in an overridden CyclopsRoutine virtual method
            // and look all nice, spiffy, and totally legit.
            Host.Immediately.Add(() => {
                isOkA = true; Host.Immediately.Add(() => {
                    isOkB = true; Host.Immediately.Add(() => {
                        isOkC = true;
            }); }); });

            Host.RoutineExceptionCaught += (r, s, e) => { wasCaught = e is UnityEngine.Assertions.AssertionException; };

            LogAssert.ignoreFailingMessages = true;
            Host.Update(CyclopsCommon.MaxDeltaTime);
            LogAssert.ignoreFailingMessages = false;

            Assert.IsTrue(wasCaught && isOkA && isOkB & !isOkC);
        }

        [Test]
        [Category("Smoke")]
        [Category("Messages")]
        public void SendMessage_WithInvalidReceiverTag_ThrowsAssertionException(

            [Values(ValidMessageName)] string name,
            [ValueSource(nameof(InvalidTags))] string receiverTag,
            [Values(ValidMessageSender)] object sender,
            [Values(CyclopsMessage.DeliveryStage.AfterRoutines)] CyclopsMessage.DeliveryStage stage,
            [Values(Timeout_TwiceMaxDeltaTime)] float listenerTimeOut,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            Assert.Throws(typeof(UnityEngine.Assertions.AssertionException), () => Host.Send(receiverTag, name, sender, data: null, stage));
        }

        [Test]
        [Category("Smoke")]
        [Category("Messages")]
        public void SendMessage_ToImmediateReceiverAndDeliveredAfterRoutines_IsReceived(

            [Values(ValidMessageName)] string name,
            [Values(ValidTag)] string receiverTag,
            [Values(ValidMessageSender)] object sender,
            [Values(CyclopsMessage.DeliveryStage.AfterRoutines)] CyclopsMessage.DeliveryStage stage,
            [Values(Timeout_TwiceMaxDeltaTime)] float listenerTimeOut,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            bool wasReceived = false;

            Host.Immediately
                .Listen(receiverTag, name, listenerTimeOut)
                .OnSuccess(inboundMessage => wasReceived = true);

            Host.Send(receiverTag, name, sender, data: null, stage);

            Host.Update(deltaTime);

            Assert.IsTrue(wasReceived);
        }

        [Test]
        [Category("Smoke")]
        [Category("Messages")]
        public void SendMessage_ToImmediateReceiversAndDeliveredAfterRoutines_IsReceived(
            [Values(61)] int receiverCountPerTag,
            [Values(31)] int uniqueTagCount,
            [Values(ValidMessageName)] string name,
            [Values(ValidTag)] string receiverTagPrefix,
            [Values(ValidMessageSender)] object sender,
            [Values(CyclopsMessage.DeliveryStage.AfterRoutines)] CyclopsMessage.DeliveryStage stage,
            [Values(Timeout_TwiceMaxDeltaTime)] float listenerTimeOut,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            int interceptionCount = 0;

            for (int i = 0; i < uniqueTagCount; ++i)
            {
                string tag = receiverTagPrefix + i;

                for (int j = 0; j < receiverCountPerTag; ++j)
                {
                    Host.Immediately
                        .Listen(tag, name, listenerTimeOut)
                        .OnSuccess(inboundMessage => ++interceptionCount);
                }

                Host.Send(tag, name, sender, data: null, stage);
            }

            Host.Update(deltaTime);

            int expectedInterceptionCount = receiverCountPerTag * uniqueTagCount;

            Assert.AreEqual(interceptionCount, expectedInterceptionCount);
        }

        [Test]
        [Category("Smoke")]
        [Category("Messages")]
        public void SendMessage_ToNonImmediateReceiverAndDeliveredAfterRoutines_IsNotReceived(
            [Values(ValidMessageName)] string name,
            [Values(ValidTag)] string receiverTag,
            [Values(ValidMessageSender)] object sender,
            [Values(CyclopsMessage.DeliveryStage.AfterRoutines)] CyclopsMessage.DeliveryStage stage,
            [Values(Timeout_TwiceMaxDeltaTime)] float listenerTimeOut,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            bool wasReceived = false;

            Host.Listen(receiverTag, name, listenerTimeOut)
                .OnSuccess(inboundMessage => wasReceived = true);

            Host.Send(receiverTag, name, sender, data: null, stage);

            Host.Update(deltaTime);
            Host.Update(deltaTime);

            Assert.IsFalse(wasReceived);
        }

        [Test]
        [Category("Smoke")]
        [Category("Messages")]
        public void SendMessageNextFrame_ToNonImmediateReceiverAndDeliveredAfterRoutines_IsReceived(
            [Values(ValidMessageName)] string name,
            [Values(ValidTag)] string receiverTag,
            [Values(ValidMessageSender)] object sender,
            [Values(CyclopsMessage.DeliveryStage.AfterRoutines)] CyclopsMessage.DeliveryStage stage,
            [Values(Timeout_TwiceMaxDeltaTime)] float listenerTimeOut,
            [Values(MaxDeltaTime)] float deltaTime)
        {
            bool wasReceived = false;

            Host.Listen(receiverTag, name, listenerTimeOut)
                .OnSuccess(inboundMessage => wasReceived = true);

            Host.Add(() => Host.Send(receiverTag, name, sender, data: null, stage));

            Host.Update(deltaTime);
            Host.Update(deltaTime);

            Assert.IsTrue(wasReceived);
        }
    }
}
