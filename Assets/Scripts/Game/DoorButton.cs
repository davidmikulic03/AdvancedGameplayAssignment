using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DoorButton : MonoBehaviour, InteractionManager.IInteraction
    {
        private class PushDaButton : Lara.LaraEvent
        {
            DoorButton      m_button;
            bool            m_bDone;

            public PushDaButton(Lara lara, DoorButton button) : base(lara)
            {
                m_button = button;
            }

            public override void OnBegin(bool bFirstTime)
            {
                base.OnBegin(bFirstTime);
                m_button.StartCoroutine(PushLogic());
            }

            public override bool IsDone()
            {
                return m_bDone;
            }

            IEnumerator PushLogic()
            {
                Controller.enabled = false;

                // move lara into position
                Vector3 vInteractPosition = m_button.transform.position - m_button.transform.forward * 0.5f;
                vInteractPosition.y = Transform.position.y;
                while (Math2D.GetDistance2D(Lara.transform.position, vInteractPosition) > 0.001f)
                {
                    Transform.position = Vector3.MoveTowards(Transform.position, vInteractPosition, Time.deltaTime);
                    Transform.rotation = Quaternion.Slerp(Transform.rotation, m_button.transform.rotation, Time.deltaTime * 6.0f);
                    yield return null;
                }

                // TODO: trigger button pulling animation

                // move button
                for (float f = 0.0f; f < 1.0f; f += Time.deltaTime)
                {
                    m_button.transform.position += Vector3.down * Time.deltaTime;
                    yield return null;
                }

                // open the door
                for (float f = 0.0f; f < 1.0f; f += Time.deltaTime)
                {
                    m_button.m_door.transform.position += Vector3.down * Time.deltaTime * 3.0f;
                    yield return null;
                }

                // cleanup
                Controller.enabled = true;
                m_bDone = true;
            }
        }

        [SerializeField]
        public Transform m_door;

        #region Properties

        public InteractionManager.ActionType ActionType => InteractionManager.ActionType.Active;

        #endregion

        void OnEnable()
        {
            InteractionManager.Instance.AddInteraction(this);
        }

        public bool CanInteract(Lara lara)
        {
            // too far away
            if (GetInteractionDistance(lara) > 1.0f)
            {
                return false;
            }

            // are we facing the button?
            Vector3 vToButton = Math2D.GetDirection2D(transform.position - lara.transform.position);
            if (Vector3.Dot(vToButton, lara.transform.forward) < 0.5f)
            {
                return false;
            }

            return true;
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
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
            lara.PushEvent(new PushDaButton(lara, this));
        }
    }
}
