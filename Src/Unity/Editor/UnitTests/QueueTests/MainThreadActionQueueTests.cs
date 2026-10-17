/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using NUnit.Framework;
using Runevision.LayerProcGen;

namespace Runevision.Testing {
    public class MainThreadActionQueueTests {
        float previousBudget;

        [SetUp]
        public void SetUp() {
            Assert.That(MainThreadActionQueue.idle, Is.True,
                "Run these Edit Mode tests with no active generation.");
            previousBudget = MainThreadActionQueue.budgetPerFrame;
            MainThreadActionQueue.watch.Reset();
            // Remove timing sensitivity: even a slow machine must execute the item.
            MainThreadActionQueue.budgetPerFrame = float.PositiveInfinity;
        }

        [TearDown]
        public void TearDown() {
            try {
                MainThreadActionQueue.watch.Reset();
                MainThreadActionQueue.budgetPerFrame = float.PositiveInfinity;
                // A C# iterator that threw finishes on its next MoveNext call.
                MainThreadActionQueue.ProcessQueue();
            }
            finally {
                MainThreadActionQueue.watch.Reset();
                MainThreadActionQueue.budgetPerFrame = previousBudget;
            }
        }

        [TestCase("Regular")]
        [TestCase("FastTrack")]
        [TestCase("NextFrame")]
        [TestCase("Routine")]
        public void ProcessQueue_ResetsWatch_WhenProcessingThrows(string queue) {
            var expected = new InvalidOperationException("Expected test failure");
            Action fail = () => { throw expected; };

            switch (queue) {
                case "Regular":
                    MainThreadActionQueue.Enqueue(fail);
                    break;
                case "FastTrack":
                    MainThreadActionQueue.EnqueueFastTrack(fail);
                    break;
                case "NextFrame":
                    MainThreadActionQueue.EnqueueNextFrame(fail);
                    break;
                case "Routine":
                    MainThreadActionQueue.Enqueue(new ThrowingRoutine(expected));
                    break;
                default:
                    throw new ArgumentException("Unknown queue", nameof(queue));
            }

            var actual = Assert.Throws<InvalidOperationException>(
                () => MainThreadActionQueue.ProcessQueue());

            Assert.That(actual, Is.SameAs(expected),
                "Queue cleanup must preserve the original exception.");
            Assert.That(MainThreadActionQueue.watch.IsRunning, Is.False,
                "The stopwatch must stop even when queued work throws.");
            Assert.That(MainThreadActionQueue.watch.ElapsedTicks, Is.Zero,
                "The following frame must not inherit elapsed time.");
        }

        [Test]
        public void ProcessQueue_ResetsWatch_WhenProcessingSucceeds() {
            MainThreadActionQueue.EnqueueNextFrame(() => { });
            MainThreadActionQueue.ProcessQueue();

            Assert.That(MainThreadActionQueue.watch.IsRunning, Is.False);
            Assert.That(MainThreadActionQueue.watch.ElapsedTicks, Is.Zero);
        }

        sealed class ThrowingRoutine : IQueuedRoutine {
            readonly InvalidOperationException exception;

            public ThrowingRoutine(InvalidOperationException exception) {
                this.exception = exception;
            }

            public IEnumerator Process() {
                yield return null;
                throw exception;
            }
        }
    }
}
