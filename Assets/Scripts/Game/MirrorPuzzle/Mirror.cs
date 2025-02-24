using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.MirrorPuzzle
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Mirror : MonoBehaviour
    {
        private Camera          m_mirrorCamera;
        private RenderTexture   m_mirrorTexture;
        private Material        m_mirrorMaterial;
        private MeshRenderer    m_mirrorRenderer;

        private void Start()
        {
            // create mirror texture and assign to camera
            m_mirrorTexture = new RenderTexture(64, 64, 24);
            m_mirrorCamera = GetComponentInChildren<Camera>();
            m_mirrorCamera.targetTexture = m_mirrorTexture;
            m_mirrorCamera.enabled = false;
            m_mirrorCamera.Render();

            // PS1 GFX :)
            m_mirrorTexture.filterMode = FilterMode.Point;

            // assign texture to mirror material
            m_mirrorRenderer = GetComponent<MeshRenderer>();
            m_mirrorMaterial = m_mirrorRenderer.materials[1];
            m_mirrorMaterial.SetTexture("_MainTex", m_mirrorTexture);
        }

        private void Update()
        {
            if (!m_mirrorRenderer.isVisible ||
                CameraController.Instance == null)
            {
                return;
            }

            // get vector to mirror 
            Vector3 vSource = CameraController.Instance.transform.position;
            Vector3 vToMirror = transform.position - vSource;
            Vector3 vReflect = Vector3.Reflect(vToMirror, transform.forward);

            // move mirror camera into position
            m_mirrorCamera.transform.position = transform.position - vReflect;
            m_mirrorCamera.transform.rotation = Quaternion.LookRotation(vReflect);

            // update mirror texture
            m_mirrorCamera.Render();
        }
    }
}