using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class RunEvent : ExplorationEvent {
        private float m_fSpeed = 20f;
        private float m_fOriginalSpeed;
        bool m_bIsActive = true;

        public RunEvent(Lara lara) : base(lara) {
            m_fOriginalSpeed = lara.MOVE_SPEED;
        }
        public override bool IsDone() {
            return !m_bIsActive;
        }
        public override void OnBegin(bool bFirstTime) {
            base.OnBegin(bFirstTime);
            Lara.MOVE_SPEED = m_fSpeed;
        }
        public override void OnUpdate() {
            base.OnUpdate();
            if (!IsRunning) {
                m_bIsActive = false;
                Lara.MOVE_SPEED = m_fOriginalSpeed;
            }
        }
    }
}
