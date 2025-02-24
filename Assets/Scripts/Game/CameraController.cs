using Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : EventHandler
    {
        public abstract class CameraEvent : GameEvent
        {
            private CameraController m_controller;

            #region Properties

            public CameraController Controller => m_controller;

            #endregion

            public CameraEvent(CameraController controller)
            {
                m_controller = controller;
            }

            public override bool IsDone()
            {
                return false;
            }
        }

        [SerializeField, Range(0.0f, 1.0f)]
        public float                        m_fAngle = 0.0f;

        [SerializeField, Range(1.0f, 10.0f)]
        public float                        m_fDistance = 4.0f;

        [SerializeField, Range(1, 10)]
        public int                          m_iSmoothingIterations = 1;

        public const float                  MAX_ANGLE = 70.0f;

        private Camera                      m_camera;
        private int                         m_iLevelMask;
        private static CameraController     sm_instance;

        #region Properties

        public Camera Camera => m_camera;

        public int LevelMask => m_iLevelMask;

        public static CameraController Instance => sm_instance;

        #endregion

        private void OnEnable()
        {
            m_camera = GetComponent<Camera>();
            m_iLevelMask = LayerMask.GetMask(new string[] { "Level" });
            sm_instance = this;

            PushEvent(new ExplorationCameraEvent(this));
        }

        private void OnDisable()
        {
            sm_instance = (sm_instance == this ? null : sm_instance);
        }
    }
}