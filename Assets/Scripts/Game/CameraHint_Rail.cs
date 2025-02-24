using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public class CameraHint_Rail : CameraHint
    {
        [SerializeField]
        Transform       m_start;

        [SerializeField]
        Transform       m_end;

        [SerializeField, Range(1.0f, 25.0f)]
        public float    m_fDistanceOffset = 10.0f;

        #region Properties

        protected Vector3 PlayerPosition
        {
            get
            {
                if (m_start == null || 
                    m_end == null ||
                    Lara.Instance == null)
                {
                    return Vector3.zero;
                }

                return MathUtil.ClosestPointOnSegment(Lara.Instance.transform.position, m_start.position, m_end.position);
            }
        }

        protected Vector3 LookTarget
        {
            get
            {
                if (Lara.Instance == null)
                {
                    return Vector3.zero;
                }

                return Lara.Instance.transform.position + Vector3.up * 0.6f + Vector3.Normalize(m_end.position - m_start.position) * m_fDistanceOffset * 0.5f;
            }
        }

        protected Vector3 EyeTarget
        {
            get
            {
                if (m_start == null ||
                    m_end == null ||
                    Lara.Instance == null)
                {
                    return Vector3.zero;
                }

                float fDistance = Vector3.Distance(m_start.position, PlayerPosition) - m_fDistanceOffset;
                return m_start.position + Vector3.Normalize(m_end.position - m_start.position) * fDistance;
            }
        }

        #endregion

        public override void OnUpdate()
        {
            base.OnUpdate();

            // move to eye target
            Transform transform = CameraController.Instance.transform;
            transform.position += (EyeTarget - transform.position) * Time.deltaTime * 1.0f;

            // look at target
            Quaternion qTarget = Quaternion.LookRotation(LookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, qTarget, Time.deltaTime * 1.0f);
        }

        private void OnDrawGizmos()
        {
            if (m_start == null || m_end == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(m_start.position, Vector3.one * 0.2f);
            Gizmos.DrawLine(m_start.position, m_end.position);
            Gizmos.DrawCube(m_end.position, Vector3.one * 0.2f);

            if (Application.isPlaying && Lara.Instance != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(PlayerPosition, 0.25f);
                Gizmos.DrawLine(PlayerPosition, Lara.Instance.transform.position);
            }
        }
    }
}