// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using Mk.Debugs;
using Mk.Routines;
using NUnit.Framework;
using UnityEditor;

#pragma warning disable 4014
#pragma warning disable 1998

namespace AsyncTests.Async
{
    [TestFixture]
    public class RoutineTests_T
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
            Assert.AreEqual(42, aw.GetResult());

            async Routine<int> Empty()
            {
                return 42;
            }
        }

        [Test]
        public void FirstContinuation_Invoked_Immediately()
        {
            var visited = false;
            Sample();
            Assert.IsTrue(visited);

            async Routine<int> Sample()
            {
                visited = true;
                await _Never.Instance;
                return 42;
            }
        }


        [Test]
        public void MultipleDispose()
        {
            var visited = false;
            Sample();
            Assert.IsTrue(visited);

            var dispose1 = React.Scope(out var scope1);
            var r = Outer(scope1);
            dispose1.Dispose();
            Asr.IsTrue(r.GetAwaiter().IsCompleted);

            async Routine Outer(IScope scope)
            {
                var res = await Sample().DisposeOn(scope);
                Asr.IsFalse(res.HasValue);
            }

            async Routine<int> Sample()
            {
                visited = true;
                await _Never.Instance;
                return 42;
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
                Assert.AreEqual(42, aw.GetResult());
            }

            async Routine<int> Single(ISubscribe sub)
            {
                await sub;
                return 42;
            }
        }

        [Test]
        public void Dispose_DoesNotInvokeContinuation_SubAwaiter()
        {
            var r = Sample().ToOptional();
            r.Dispose();

            async Routine<int> Sample()
            {
                await _Never.Instance;
                Assert.Fail("Should not happen");
                return 42;
            }
        }

        [Test]
        public void Dispose_StopsUsualFlow_SubAwaiter()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var r = Sample().ToOptional();
                r.GetAwaiter();
                r.Dispose();
                pub.Publish();

                async Routine<int> Sample()
                {
                    await sub;
                    Assert.Fail("Should not happen");
                    return 42;
                }
            }
        }


        [Test]
        public void OuterRoutine_Dispose_Breaks_Inner()
        {
            Routine<int> innerClosure = null;
            var outer = Outer().ToOptional();
            var innerAwaiter = innerClosure.GetAwaiter();
            outer.Dispose();
            Assert.IsTrue(innerAwaiter.IsCompleted);

            async Routine<int> Outer()
            {
                innerClosure = Inner();
                var res = await innerClosure;
                return res;
            }

            async Routine<int> Inner()
            {
                await _Never.Instance;
                return 42;
            }
        }

        [Test]
        public void GetScope_DoesntBreakRoutine()
        {
            using (React.Scope(out var scope))
            {
                Routine<int> closure = null;
                var r = Outer().ToOptional();
                var aw = closure.ToOptional().GetAwaiter();
                r.Dispose();
                Assert.IsFalse(aw.IsCompleted);

                async Routine<int> Outer()
                {
                    closure = Inner();
                    await closure.ToOptional().GetScope(scope);
                    return 42;
                }

                async Routine<int> Inner()
                {
                    await _Never.Instance;
                    return 42;
                }
            }
        }

        [Test]
        public void Dispose_AfterCompleted_DoesNothing()
        {
            using (React.Scope(out var scope))
            {
                var (pub, sub) = scope.PubSub();
                var r = Sample().ToOptional();
                pub.Publish();
                var aw = r.GetAwaiter();
                int i = 0;
                aw.OnCompleted(() => ++i);
                Assert.AreEqual(1, i);
                Assert.IsTrue(aw.IsCompleted);
                Assert.AreEqual(42, aw.GetResult().GetOrFail());


                r.Dispose();
                Assert.AreEqual(1, i);


                async Routine<int> Sample()
                {
                    await sub;
                    return 42;
                }
            }
        }

        [Test]
        public void DisposedRoutine_Awaiter_IsCompleted()
        {
            var r = Sample().ToOptional();
            r.Dispose();
            var aw = r.GetAwaiter();
            Assert.IsTrue(aw.IsCompleted);

            async Routine<int> Sample()
            {
                await _Never.Instance;
                return 42;
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
                Assert.AreEqual(42, aw.GetResult());

                async Routine<int> Sample()
                {
                    await sub;
                    return 42;
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

                async Routine<int> Routine1()
                {
                    r1Scope = await Routine.SelfScope();
                    r1Scope.Subscribe(() => completed = true);

                    await sub;
                    return 42;
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