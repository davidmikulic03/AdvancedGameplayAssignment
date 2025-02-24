using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public class CameraHint_FlyOnTheWall : CameraHint
    {
        Transform m_cameraPos;

        private void OnEnable()
        {
            m_cameraPos = transform.Find("CameraPos");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (CameraController.Instance == null ||
                Lara.Instance == null)
            {
                return;
            }

            // move to eye target
            Transform transform = CameraController.Instance.transform;
            transform.position = m_cameraPos.position;

            // look at target
            Vector3 vHead = Lara.Instance.transform.position + Vector3.up * 1.6f;
            transform.rotation = Quaternion.LookRotation(vHead - transform.position);
        }
    }
}