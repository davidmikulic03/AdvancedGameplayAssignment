using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public class CameraHint_GotoHint : CameraHint
    {
        protected class ExplorationHintEvent : ExplorationCameraEvent
        {
            Vector3             m_vHintPosition;
            float               m_fHintTime;
            float               m_fTime;

            #region Properties

            /*
            public override Vector3 LookTarget => m_hint.m_goHere.position;

            public override Vector3 EyeTarget
            {
                get
                {
                    Vector3 vHead = Lara.Instance.transform.position + Vector3.up * 1.6f;
                    Vector3 vToPlayer = (Lara.Instance.transform.position - LookTarget).normalized;
                    return vHead + vToPlayer * CameraController.Instance.m_fDistance;
                }
            }*/

            #endregion

            public ExplorationHintEvent(CameraController controller, Vector3 vHintPosition, float fHintTime) : base(controller)
            {
                m_vHintPosition = vHintPosition;
                m_fHintTime = fHintTime;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                m_fTime += Time.deltaTime;
                float fBlend = Mathf.Min(Mathf.Clamp01(m_fTime), Mathf.Clamp01(1.0f - (m_fTime - (m_fHintTime - 1.0f))));
                Transform transform = CameraController.Instance.transform;
                Quaternion qHintRotation = Quaternion.LookRotation(m_vHintPosition - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, qHintRotation, fBlend);
            }

            public override bool IsDone()
            {
                return m_fTime > m_fHintTime;
            }
        }

        [SerializeField]
        Transform       m_goHere;

        [SerializeField]
        public bool     m_bOnlyOnce = true;

        [SerializeField, Range(1.0f, 10.0f)]
        float           m_fHintTime = 3.0f;

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Lara>() != null && m_goHere != null)
            {
                CameraController.Instance.PushEvent(new ExplorationHintEvent(CameraController.Instance, m_goHere.position, m_fHintTime));

                if (m_bOnlyOnce)
                {
                    Destroy(this);
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
        }
    }
}