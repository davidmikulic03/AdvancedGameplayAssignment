using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.MirrorPuzzle
{
    public class MirrorCaster : MonoBehaviour, InteractionManager.IInteraction
    {
        private class ControlMirror : Lara.LaraEvent
        {
            MirrorCaster m_caster;

            public ControlMirror(Lara lara, MirrorCaster caster) : base(lara)
            {
                m_caster = caster;
            }

            public override void OnBegin(bool bFirstTime)
            {
                base.OnBegin(bFirstTime);
                Controller.enabled = false;
                Animator.SetFloat("Speed", 0.0f);
                Animator.SetBool("ControlMirror", true);
                CameraController.Instance.PushEvent(new MirrorCameraEvent(CameraController.Instance, m_caster));
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                // rotate mirror arm
                m_caster.m_mirrorArm.rotation *= Quaternion.Euler(0.0f, Movement.x * 20.0f * Time.deltaTime, 0.0f);

                // rotate mirror up/down
                m_caster.m_fMirrorAngle += Movement.y * 20.0f * Time.deltaTime;
                m_caster.m_fMirrorAngle = Mathf.Clamp(m_caster.m_fMirrorAngle, -35.0f, 35.0f);
                m_caster.m_mirror.localEulerAngles = new Vector3(m_caster.m_fMirrorAngle, 0.0f, 0.0f);

                // move lara into position
                Vector3 vTarget = m_caster.transform.position - m_caster.m_mirrorArm.forward * 0.4f;
                Transform.position += (vTarget - Transform.position) * Time.deltaTime * 4.0f;
                Transform.rotation = Quaternion.Slerp(Transform.rotation, m_caster.m_mirrorArm.rotation, Time.deltaTime * 4.0f);
            }

            public override bool IsDone()
            {
                return !InteractHeld;
            }

            public override void OnEnd()
            {
                base.OnEnd();
                Controller.enabled = true;
                Animator.SetBool("ControlMirror", false);
            }
        }

        private class MirrorCameraEvent : CameraController.CameraEvent
        {
            private MirrorCaster    m_caster;

            public MirrorCameraEvent(CameraController controller, MirrorCaster caster) : base(controller)
            {
                m_caster = caster;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                // move to eye target
                Transform transform = Controller.transform;
                Transform arm = m_caster.m_mirrorArm;
                Vector3 vCameraTarget = arm.position - arm.forward * 1.5f + arm.right * 0.4f + Vector3.up * 0.5f;
                transform.position += (vCameraTarget - transform.position) * Time.deltaTime * 3.0f;

                // look at target
                Quaternion qTarget = Quaternion.Slerp(arm.rotation, m_caster.m_mirror.rotation, 0.5f);
                transform.rotation = Quaternion.Slerp(transform.rotation, qTarget, Time.deltaTime * 2.0f);
            }

            public override bool IsDone()
            {
                return Lara.Instance.CurrentEvent is not ControlMirror;
            }
        }

        private Transform   m_mirrorArm;
        private Transform   m_mirror;
        private float       m_fMirrorAngle;

        #region Properties

        public InteractionManager.ActionType ActionType => InteractionManager.ActionType.Active;

        #endregion

        private void Start()
        {
            m_mirrorArm = transform.Find("MirrorArm");
            m_mirror = m_mirrorArm.Find("Mirror");
            InteractionManager.Instance.AddInteraction(this);
        }

        public bool CanInteract(Lara lara)
        {
            // too far?
            if (GetInteractionDistance(lara) > 1.0f)
            {
                return false;
            }

            // interacting from behind the mirror?
            Vector3 vToMirror = transform.position - lara.transform.position;
            if (Vector3.Dot(vToMirror, m_mirrorArm.forward) < 0.1f)
            {
                return false;
            }

            return true;
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }

        public float GetInteractionDistance(Lara lara)
        {
            return Math2D.GetDistance2D(lara.transform.position, transform.position);
        }

        public bool InsideArea(Rect area)
        {
            return area.Contains(transform.position.ToXZ());
        }

        public void PerformInteraction(Lara lara)
        {
            lara.PushEvent(new ControlMirror(lara, this));
        }
    }
}