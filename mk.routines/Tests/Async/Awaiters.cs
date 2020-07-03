// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Mk.Debugs;
using Mk.Routines;
using NUnit.Framework;

namespace AsyncTests.Async
{
    [TestFixture]
    public class Awaiters
    {
        static IDisposable AwaiterScope(out (Awaiter Awaiter, Action NextAwait) res)
        {
            var d = React.Scope(out var scope);
            var (next, onNext) = scope.PubSub();

            onNext.Subscribe(d.Dispose, scope);
            var awaiter = new Awaiter(scope, d.Dispose);
            res = (awaiter, next.Publish);
            return d;
        }

        [Test]
        public void Next_MakesCompleted()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;
                nextAwait.Invoke();
                Assert.IsTrue(awaiter.IsCompleted);
            }
        }

        [Test]
        public void Next_ExecutesContinuation()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;
                var done = false;
                awaiter.OnCompleted(() => done = true);
                Assert.IsFalse(done);

                nextAwait.Invoke();
                Assert.IsTrue(done);
            }
        }

        [Test]
        public void Next_ContinuationsDirectOrder()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;

                var queue = new Queue<int>();
                awaiter.OnCompleted(() => queue.Enqueue(1));
                awaiter.OnCompleted(() => queue.Enqueue(2));
                awaiter.OnCompleted(() => queue.Enqueue(3));

                Assert.AreEqual(0, queue.Count);
                nextAwait.Invoke();

                Assert.AreEqual(1, queue.Dequeue());
                Assert.AreEqual(2, queue.Dequeue());
                Assert.AreEqual(3, queue.Dequeue());
            }
        }

        // ticks all awaits 
        [Test]
        public void Break_InvokesAllContinuationsInOrder()
        {
            var queue = new Queue<int>();
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;

                awaiter.OnCompleted(() => queue.Enqueue(1));
                awaiter.OnCompleted(() => queue.Enqueue(2));
                awaiter.OnCompleted(() => queue.Enqueue(3));
                Assert.AreEqual(0, queue.Count);
            }
            Assert.AreEqual(1, queue.Dequeue());
            Assert.AreEqual(2, queue.Dequeue());
            Assert.AreEqual(3, queue.Dequeue());
        }

        [Test]
        public void ContinuationInvokedImmediatelyIfCompleted()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;

                nextAwait.Invoke();

                var continued = false;
                awaiter.OnCompleted(() => continued = true);
                Assert.IsTrue(continued);
            }
        }

        [Test]
        public void Break_ContinuationInvokedImmediately()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;

                awaiter.BreakInner();

                var continued = false;
                awaiter.OnCompleted(() => continued = true);
                Assert.IsTrue(continued);
            }
        }

        [Test]
        public void Break_ContinuationsInvokedOnce()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;
                var i = 0;
                awaiter.OnCompleted(() => ++i);

                awaiter.BreakInner();
                Assert.AreEqual(1, i);
                awaiter.BreakInner();
                Assert.AreEqual(1, i);
            }
        }

        [Test]
        public void MultipleNext_Assert()
        {
            Assert.Catch<Asr.AssertException>(Inner);
            
            void Inner()
            { 
                using (AwaiterScope(out var tuple))
                {
                    var (awaiter, nextAwait) = tuple;
                    var i = 0;
                    awaiter.OnCompleted(() => ++i);

                    nextAwait.Invoke();
                    Assert.AreEqual(1, i);
                    nextAwait.Invoke(); // throws
                    Assert.AreEqual(1, i);
                }
            }
        }

        [Test]
        public void BreakAfterNext_DoesntInvokeContinuations()
        {
            using (AwaiterScope(out var tuple))
            {
                var (awaiter, nextAwait) = tuple;
                var i = 0;
                awaiter.OnCompleted(() => ++i);

                nextAwait.Invoke();
                Assert.AreEqual(1, i);
                awaiter.BreakInner(); // throws
//                Assert.AreEqual(1, i);
            }
        }

        [Test]
        public void NextAfterBreak_Assert()
        {
            Assert.Catch<Asr.AssertException>(Inner);

            void Inner()
            {
                using (AwaiterScope(out var tuple))
                {
                    var (awaiter, nextAwait) = tuple;
                    var i = 0;
                    awaiter.OnCompleted(() => ++i);

                    awaiter.BreakInner();
                    Assert.AreEqual(1, i);
                    nextAwait.Invoke(); // throws
//                    Assert.AreEqual(1, i);
                }
            }
        }
    }
}