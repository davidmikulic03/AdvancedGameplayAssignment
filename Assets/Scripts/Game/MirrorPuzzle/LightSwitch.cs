using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MirrorPuzzle
{
    public class LightSwitch : MonoBehaviour
    {
        [SerializeField]
        public Transform    m_door;

        private Material    m_material;
        private bool        m_bRecievedLight;
        private Vector3     m_vDoorOriginalPosition;

        private void Start()
        {
            m_vDoorOriginalPosition = m_door.localPosition;
            m_material = GetComponent<MeshRenderer>().material;
        }

        public void OnReceiveLight()
        {
            m_bRecievedLight = true;
        }

        private void Update()
        {
            m_material.EnableKeyword("_EMISSION");
            m_material.SetColor("_EmissionColor", m_bRecievedLight ? new Color(2.0f, 1.0f, 0.0f) : Color.black);
            Vector3 vDoorTarget = m_vDoorOriginalPosition + (m_bRecievedLight ? Vector3.down * 5.0f : Vector3.zero);
            m_door.localPosition = Vector3.MoveTowards(m_door.localPosition, vDoorTarget, Time.deltaTime * 1.0f);
            m_bRecievedLight = false;
        }
    }
}