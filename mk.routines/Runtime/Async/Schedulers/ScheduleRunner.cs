// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Mk.Debugs;
using UnityEngine;

namespace Mk.Routines
{
    [Serializable]
    public class ScheduleSettings
    {
        public float Logic = 0.1f;

        static float Physics
        {
            get => Time.fixedDeltaTime;
            set => Time.fixedDeltaTime = value;
        }
    }


    class ScheduleRunner : MonoBehaviour
    {
        static ScheduleRunner _instance;
        IPublish _update;
        IPublish _fixedUpdate;
        bool _completed;
        IDisposable _dispose;
        IPublish<float> _fixedUpdateTime;
        IPublish<float> _updateTime;
        IPublish _lateUpdate;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            _TryInit();
        }

        static bool _TryInit(Option<ScheduleSettings> settings = default)
        {
            if (_instance) return false;

            Application.quitting += _Dispose;

            var go = new GameObject
            {
                name = "Schedulers"
            };
            _instance = go.AddComponent<ScheduleRunner>();
            _instance.transform.SetParent(null, false);
            DontDestroyOnLoad(go);

            _instance._Init(settings.ValueOr(new ScheduleSettings()));
            return true;
        }

        void _Init(ScheduleSettings scheduleSettings)
        {
            Asr.IsTrue(_instance == this);

            _dispose = React.Scope(out var scope);
            Sch.Scope = scope;
            _completed = false;

            Sch.Logic = StartSch(scheduleSettings.Logic, scope);
            (_update, Sch.Update) = scope.PubSub();
            (_lateUpdate, Sch.LateUpdate) = scope.PubSub();
            (_fixedUpdate, Sch.Physics) = scope.PubSub();
            (_updateTime, Sch.UpdateTime) = scope.PubSub<float>();
            (_fixedUpdateTime, Sch.PhysicsTime) = scope.PubSub<float>();
            (SchPub.PublishError, Sch.OnError) = Sch.Scope.PubSub<Exception>();
        }


        void Update()
        {
            _updateTime.Publish(Time.time);
            _update.Publish();
        }

        void LateUpdate()
        {
            _lateUpdate.Publish();
        }

        void FixedUpdate()
        {
            _fixedUpdateTime.Publish(Time.fixedTime);
            _fixedUpdate.Publish();
        }

        ISubscribe StartSch(float time, IScope scope)
        {
            var (pub, sub) = scope.PubSub();
            var yield = new WaitForSeconds(time);
            StartCoroutine(Ticker());
            return sub;

            IEnumerator Ticker()
            {
                while (true)
                {
                    yield return yield;

                    pub.Publish();
                }
            }
        }

        static void _Dispose()
        {
            if (WasTrue(ref _instance._completed)) return;

            _instance.StopAllCoroutines();
            _instance._dispose.Dispose();

            Destroy(_instance.gameObject);
            _instance = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool WasTrue(ref bool flag)
        {
            var copy = flag;
            flag = true;
            return copy;
        }
    }
}