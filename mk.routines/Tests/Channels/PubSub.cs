// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using Mk.Debugs;
using Mk.Routines;
using NUnit.Framework;

namespace AsyncTests.Channels
{
    // todo PubSub<T>
    [TestFixture]
    public class PubSub
    {
        [Test]
        public void Pub_Next_ExecuteCallbackImmediately()
        {
            using (React.Scope(out var scope))
            {
                var received = false;
                var (pub, sub) = scope.PubSub();
                sub.Subscribe(() => received = true, scope);
                Assert.IsFalse(received);
                pub.Publish();
                Assert.IsTrue(received);
            }
        }

        [Test]
        public void Pub_Next_ExecutesMultipleTimes()
        {
            const int times = 5;
            using (React.Scope(out var scope))
            {
                var i = 0;
                var (pub, sub) = scope.PubSub();
                sub.Subscribe(() => ++i, scope);

                for (var ii = 0; ii < times; ii++)
                    pub.Publish();

                Assert.AreEqual(times, i);
            }
        }

        [Test]
        public void Pub_Next_DeliveredToAllSubscribers()
        {
            const int times = 5;
            using (React.Scope(out var scope))
            {
                var timesReceived = 0;
                var (pub, sub) = scope.PubSub();
                sub.Subscribe(() => ++timesReceived, scope);

                for (var ii = 0; ii < times; ii++)
                    pub.Publish();

                Assert.AreEqual(times, timesReceived);
            }
        }


        [Test]
        public void CreateFromDisposedScope_Assert()
        {
            Assert.Catch<Asr.AssertException>(Inner);

            void Inner()
            {
                React.Scope(out var disposedScope).Dispose();
                var _ = disposedScope.PubSub(); // throws
            }
        }

        [Test]
        public void SubscribeWithDisposedScope_Assert()
        {
            Assert.Catch<Asr.AssertException>(Inner);

            void Inner()
            {
                using (React.Scope(out var scope))
                {
                    var (_, sub) = scope.PubSub();

                    React.Scope(out var disposedScope).Dispose();

                    sub.Subscribe(() => { }, disposedScope); // throws 
                }
            }
        }

        [Test]
        public void NextToDisposedPub_Assert()
        {
            Assert.Catch<Asr.AssertException>(Inner);

            void Inner()
            {
                var d = React.Scope(out var disposedAfterPubSubCreated);
                var (pub, _) = disposedAfterPubSubCreated.PubSub();

                d.Dispose();
                pub.Publish(); // throws
            }
        }

        [Test]
        public void NextSkipsCallbacksWithDisposedScopes()
        {
            using (React.Scope(out var scope))
            {
                var d = React.Scope(out var disposedAfterOnNext);
                var (pub, sub) = scope.PubSub();
                sub.Subscribe(() => Assert.Fail("Not executed"), disposedAfterOnNext);

                d.Dispose();
                pub.Publish();
            }
        }
    }
}