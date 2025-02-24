using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Ledge : InteractionManager.IInteraction
    {
        private class LedgeClimb : Lara.LaraEvent
        {
            private Vector3     m_vLedgePos;
            private Vector3     m_vSourcePos;
            private float       m_fTime;

            const float         DURATION = 0.8f;

            #region Properties

            #endregion

            public LedgeClimb(Lara lara, Vector3 vLedgePos) : base(lara)
            {
                m_vLedgePos = vLedgePos;
            }

            public override void OnBegin(bool bFirstTime)
            {
                base.OnBegin(bFirstTime);
                Controller.enabled = false;
                Animator.SetTrigger("LedgeClimb");
                m_vSourcePos = Transform.position;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                // move Lara
                m_fTime += Time.deltaTime;
                float fBlend = MathUtil.SmoothStep(Mathf.Clamp01(m_fTime / DURATION));
                Transform.position = Vector3.Lerp(m_vSourcePos, m_vLedgePos, fBlend);
            }

            public override bool IsDone()
            {
                return m_fTime > DURATION;
            }

            public override void OnEnd()
            {
                base.OnEnd();
                Controller.enabled = true;
            }
        }

        private Vector3 m_vA;
        private Vector3 m_vB;
        private Vector3 m_vNormal;

        static Color    sm_color = new Color(1.0f, 0.5f, 0.0f);

        #region Properties

        public InteractionManager.ActionType ActionType => InteractionManager.ActionType.Movement;

        #endregion

        public Ledge(Vector3 vA, Vector3 vB, Vector3 vNormal)
        {
            m_vA = vA;
            m_vB = vB;
            m_vNormal = vNormal;
        }

        public void DrawGizmos()
        {
            Gizmos.color = sm_color;
            Gizmos.DrawLine(m_vA, m_vB);

            // draw normal
            Gizmos.color = Color.green;
            Vector3 vCenter = (m_vA + m_vB) * 0.5f;
            Gizmos.DrawLine(vCenter, vCenter + m_vNormal * 0.2f);
        }

        public bool InsideArea(Rect area)
        {
            return area.Contains(m_vA.ToXZ()) ||
                   area.Contains(m_vB.ToXZ());
        }

        public bool CanInteract(Lara lara)
        {
            Vector3 vLara = lara.transform.position;
            Vector3 vCP = MathUtil.ClosestPointOnSegment(vLara, m_vA, m_vB);

            // are we climbing up?
            if (vCP.y < vLara.y + 0.1f)
            {
                return false;
            }

            // too far away?
            float fLaraRadius = lara.Controller.radius + lara.Controller.skinWidth + 0.02f;
            if (Math2D.GetDistance2D(vLara, vCP) > fLaraRadius)
            {
                return false;
            }

            // pointing in the right direction?
            if (Vector3.Dot(Math2D.GetDirection2D(lara.transform.forward), Math2D.GetDirection2D(m_vNormal)) > -0.9f)
            {
                return false;
            }

            return true;
        }

        public float GetInteractionDistance(Lara lara)
        {
            Vector3 vCP = MathUtil.ClosestPointOnSegment(lara.transform.position, m_vA, m_vB);
            return Vector3.Distance(vCP, lara.transform.position);
        }

        public void PerformInteraction(Lara lara)
        {
            Vector3 vCP = MathUtil.ClosestPointOnSegment(lara.transform.position, m_vA, m_vB);
            lara.PushEvent(new LedgeClimb(lara, vCP));
        }
    }
}