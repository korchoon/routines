// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using Mk.Routines;
using NUnit.Framework;
using UnityEditor;

#pragma warning disable 4014
#pragma warning disable 1998

namespace AsyncTests.Async
{
    [TestFixture]
    public class RoutineTests
    {
        IDisposable _dispose;

        [SetUp]
        public void Setup()
        {
            _dispose = React.Scope(out var scope);
            Sch.Scope = scope;

            var (pubUpd, onUpd) = scope.PubSub();
            Sch.Update = onUpd;
            EditorApplication.update += pubUpd.Publish;
            scope.Subscribe(() => EditorApplication.update -= pubUpd.Publish);
        }

        [TearDown]
        public void TearDown()
        {
            _dispose.Dispose();
        }

        [Test]
        public void EmptyRoutine_Completed_Immediately()
        {
            var routine = Empty();
            var aw = routine.GetAwaiter();
            Assert.IsTrue(aw.IsCompleted);

            async Routine Empty()
            {
            }
        }

        [Test]
        public void FirstContinuation_Invoked_Immediately()
        {
            var visited = false;
            Sample();
            Assert.IsTrue(visited);

            async Routine Sample()
            {
                visited = true;
                await _Never.Instance;
            }
        }

        [Test]
        public void SubAwaiter_Flow_Completed_AfterNext()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var aw = Single(sub).GetAwaiter();
                Assert.IsFalse(aw.IsCompleted);
                pub.Publish();
                Assert.IsTrue(aw.IsCompleted);
            }

            async Routine Single(ISubscribe sub)
            {
                await sub;
            }
        }

        [Test]
        public void Dispose_DoesNotInvokeContinuation_SubAwaiter()
        {
            var r = Name();
            r.Dispose();

            async Routine Name()
            {
                await _Never.Instance;
                Assert.Fail("Should not happen");
            }
        }

        [Test]
        public void Dispose_StopsUsualFlow_SubAwaiter()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var r = Name();
                r.Dispose();
                pub.Publish();

                async Routine Name()
                {
                    await sub;
                    Assert.Fail("Should not happen");
                }
            }
        }


        [Test]
        public void OuterRoutine_Dispose_Breaks_Inner()
        {
            Routine innerClosure = null;
            var outer = Outer();
            var innerAwaiter = innerClosure.GetAwaiter();
            outer.Dispose();
            Assert.IsTrue(innerAwaiter.IsCompleted);

            async Routine Outer()
            {
                innerClosure = Inner();
                await innerClosure;
            }

            async Routine Inner()
            {
                await _Never.Instance;
            }
        }

        [Test]
        public void GetScope_DoesntBreakRoutine()
        {
            using (React.Scope(out var scope))
            {
                Routine closure = null;
                var r = Outer();
                var aw = closure.GetAwaiter();
                r.Dispose();
                Assert.IsFalse(aw.IsCompleted);

                async Routine Outer()
                {
                    closure = Inner();
                    await closure.GetScope(scope);
                }

                async Routine Inner()
                {
                    await _Never.Instance;
                }
            }
        }

        [Test]
        public void SelfScope_Await()
        {
            var flag = false;
            var s = Sample();
            Assert.IsTrue(flag);

            var aw = s.GetAwaiter();
            Assert.IsFalse(aw.IsCompleted);

            s.Dispose();
            Assert.IsTrue(aw.IsCompleted);

            async Routine Sample()
            {
                var scope = await Routine.SelfScope();
                flag = true;

                await scope;
            }
        }

        [Test]
        public void GetScope_Disposed()
        {
            using (React.Scope(out var scope))
            {
                Routine closure = null;
                var r = Outer();
                var aw = closure.GetAwaiter();
                r.Dispose();
                Assert.IsFalse(aw.IsCompleted);

                async Routine Outer()
                {
                    closure = Inner();
                    await closure.GetScope(scope);
                }

                async Routine Inner()
                {
                    await _Never.Instance;
                }
            }
        }

        [Test]
        public void Dispose_AfterCompleted_DoesNothing()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var r = Sample();
                pub.Publish();
                var aw = r.GetAwaiter();
                int i = 0;
                aw.OnCompleted(() => ++i);
                Assert.AreEqual(1, i);
                Assert.IsTrue(aw.IsCompleted);


                r.Dispose();
                Assert.AreEqual(1, i);


                async Routine Sample()
                {
                    await sub;
                }
            }
        }

        [Test]
        public void DisposedRoutine_Awaiter_IsCompleted()
        {
            var r = Sample();
            r.Dispose();
            var aw = r.GetAwaiter();
            Assert.IsTrue(aw.IsCompleted);

            async Routine Sample()
            {
                await _Never.Instance;
            }
        }

        [Test]
        public void CompletedRoutine_Awaiter_IsCompleted()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var r = Sample();
                pub.Publish();
                var aw = r.GetAwaiter();
                Assert.IsTrue(aw.IsCompleted);

                async Routine Sample()
                {
                    await sub;
                }
            }
        }

        [Test]
        public void Scope_Ends_Before_GetAwaiter_Continues()
        {
            using (React.Scope(out var outer))
            {
                var (pub, sub) = outer.PubSub();

                IScope r1Scope = default;

                bool completed = false;
                var aw = Routine1().GetAwaiter();
                aw.OnCompleted(Asserts);
                pub.Publish();

                void Asserts()
                {
                    Assert.IsNotNull(r1Scope);
                    Assert.IsTrue(r1Scope.Disposing);
                    Assert.IsFalse(r1Scope.Disposed);
                    Assert.IsTrue(completed);
                }

                async Routine Routine1()
                {
                    r1Scope = await Routine.SelfScope();
                    r1Scope.Subscribe(() => completed = true);

                    await sub;
                }
            }
        }

        class _Never : ISubscribe
        {
            public static _Never Instance { get; } = new _Never();

            _Never()
            {
            }

            public void Subscribe(Action callback, IScope scope)
            {
            }
        }
    }
}