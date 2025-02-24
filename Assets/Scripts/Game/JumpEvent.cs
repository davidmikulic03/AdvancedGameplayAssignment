using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class JumpEvent : Lara.LaraEvent
    {
        private Vector3 m_vVelocity;
        private float   m_fTime;

        public JumpEvent(Lara lara) : base(lara)
        {
        }

        public override void OnBegin(bool bFirstTime)
        {
            base.OnBegin(bFirstTime);

            if (bFirstTime)
            {
                m_vVelocity = (Transform.forward + Vector3.up * 1.2f) * 5.5f;
                Animator.SetBool("Jump", true);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // wait for initial jump a bit
            if (m_fTime > 0.4f)
            {
                // add gravity
                m_vVelocity += Vector3.down * 9.82f * Time.deltaTime;
                Controller.Move(m_vVelocity * Time.deltaTime);
            }

            // add time
            m_fTime += Time.deltaTime;
        }

        public override bool IsDone()
        {
            if (m_fTime < 0.6f)
            {
                return false;
            }

            return Controller.isGrounded;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Animator.SetBool("Jump", false);
        }
    }
}