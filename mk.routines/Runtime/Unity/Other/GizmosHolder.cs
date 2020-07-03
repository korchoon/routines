// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using UnityEngine;

namespace Mk.Routines
{
    class GizmosHolder : MonoBehaviour
    {
#if UNITY_EDITOR && FALSE
        [UnityEditor.CustomEditor(typeof(GizmosHolder))]
        class Ed : UnityEditor.Editor
        {
            void OnSceneGUI()
            {
                var runner = (GizmosHolder) target;
                UnityEditor.Handles.BeginGUI();
                runner._handles?.Next();
                UnityEditor.Handles.EndGUI();
            }
        }
#endif
        IPublish _handles;
        IPublish _gizmo;

        public void Init(IPublish publishGizmo, IPublish publishHandles)
        {
            _gizmo = publishGizmo;
            _handles = publishHandles;
        }

        void OnDrawGizmos()
        {
            _gizmo?.Publish();
        }
    }
}