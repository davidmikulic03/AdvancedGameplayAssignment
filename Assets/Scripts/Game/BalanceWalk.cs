using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BalanceWalk : MonoBehaviour, InteractionManager.IInteraction
    {
        private class BalanceEvent : Lara.LaraEvent
        {
            BalanceWalk m_walk;
            float       m_fSpeed;
            float       m_fDistance = 0.0f;
            bool        m_bInverted;
            Quaternion  m_torsoLean;
            float       m_fTorsoTime;
            Transform   m_torso;

            public BalanceEvent(Lara lara, BalanceWalk walk) : base(lara)
            {
                m_walk = walk;
            }

            public override void OnBegin(bool bFirstTime)
            {
                base.OnBegin(bFirstTime);
                Animator.SetBool("BalanceWalk", true);
                Controller.enabled = false;
                m_fDistance = Mathf.Clamp(m_walk.GetDistance(Transform.position), 0.02f, 0.98f);
                m_bInverted = m_fDistance > 0.5f;
                m_torsoLean = Quaternion.Euler(0.0f, 0.0f, Random.Range(-20.0f, 20.0f));
                m_fTorsoTime = Random.Range(0.5f, 2.5f);
                m_torso = Transform.Find("Pelvis/Torso");
            }

            public override bool IsDone()
            {
                return m_fDistance < 0.0f || m_fDistance > 1.0f;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                // forward / back
                bool bForward = Movement.y > 0.4f;
                bool bBackward = Movement.y < -0.4f;
                m_fSpeed = Mathf.MoveTowards(m_fSpeed, (bForward ? 1.0f : 0.0f) + (bBackward ? -1.0f : 0.0f), Time.deltaTime * 1.0f);
                if (Mathf.Abs(m_fSpeed) > 0.001f)
                {
                    float fMove = (1.5f / m_walk.Length) * m_fSpeed * Time.deltaTime;
                    m_fDistance += fMove * (m_bInverted ? -1.0f : 1.0f);
                }

                // slide into place
                Pose target = m_walk.GetPoseAtDistance(m_fDistance);
                Transform.position += (target.position - Transform.position) * Time.deltaTime;
                Quaternion qTarget = target.rotation * (m_bInverted ? Quaternion.Euler(0.0f, 180.0f, 0.0f) : Quaternion.identity);
                Transform.rotation = Quaternion.Slerp(Transform.rotation, qTarget, Time.deltaTime * 2.0f);

                // rotate torso
                m_torso.localRotation = Quaternion.Slerp(m_torso.localRotation, m_torsoLean, Time.deltaTime * 0.5f);
                m_fTorsoTime -= Time.deltaTime;
                if (m_fTorsoTime < 0.0f)
                {
                    m_torsoLean = Quaternion.Euler(0.0f, 0.0f, Random.Range(-20.0f, 20.0f));
                    m_fTorsoTime = Random.Range(0.5f, 2.5f);
                }
            }

            public override void OnEnd()
            {
                base.OnEnd();
                Animator.SetBool("BalanceWalk", false);
                Controller.enabled = true;
            }
        }

        [SerializeField]
        public Vector3[] m_segment = new Vector3[] { Vector3.forward, -Vector3.forward };

        #region Properties

        public InteractionManager.ActionType ActionType => InteractionManager.ActionType.Movement;

        protected Vector3[] WorldSegment => System.Array.ConvertAll(m_segment, s => transform.TransformPoint(s));

        public Vector3 Forward => Vector3.Normalize(WorldSegment[1] - WorldSegment[0]);

        public float Length => Vector3.Distance(WorldSegment[0], WorldSegment[1]);

        #endregion

        protected float GetDistance(Vector3 v)
        {
            Vector3[] segment = WorldSegment;
            Vector3 vCP = MathUtil.ClosestPointOnSegment(v, segment[0], segment[1]);
            return Vector3.Dot(vCP - segment[0], segment[1] - segment[0]);
        }

        protected Pose GetPoseAtDistance(float fDistance)
        {
            return new Pose
            {
                position = Vector3.Lerp(WorldSegment[0], WorldSegment[1], fDistance),
                rotation = Quaternion.LookRotation(Forward)
            };
        }

        void OnEnable()
        {
            InteractionManager.Instance.AddInteraction(this);
        }

        public bool CanInteract(Lara lara)
        {
            // close enough?
            if (GetInteractionDistance(lara) > 0.4f)
            {
                return false;
            }

            // face direction
            Vector3 vCenter = (WorldSegment[0] + WorldSegment[1]) * 0.5f;
            Vector3 vToCenter = Vector3.Normalize(vCenter - lara.transform.position);
            if(Vector3.Dot(lara.transform.forward, vToCenter) < 0.5f)
            {
                return false;
            }

            return true;
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3[] segment = WorldSegment;
            Gizmos.DrawLine(segment[0], segment[1]);
        }

        public float GetInteractionDistance(Lara lara)
        {
            Vector3[] segment = WorldSegment;
            Vector3 vCP = MathUtil.ClosestPointOnSegment(lara.transform.position, segment[0], segment[1]);
            return Vector3.Distance(lara.transform.position, vCP);
        }

        public bool InsideArea(Rect area)
        {
            Vector3[] segment = WorldSegment;
            return area.Contains(segment[0].ToXZ()) || area.Contains(segment[1].ToXZ());
        }

        public void PerformInteraction(Lara lara)
        {
            lara.PushEvent(new BalanceEvent(lara, this));
        }

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }
    }
}
