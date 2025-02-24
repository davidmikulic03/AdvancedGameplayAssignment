using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class MinimapRenderer : MonoBehaviour
    {
        private Camera          m_minimapCamera;
        private RenderTexture   m_minimapTexture;
        private RectTransform   m_minimapImage;

        private void Start()
        {
            m_minimapTexture = new RenderTexture(256, 256, 24);
            m_minimapCamera = GetComponentInChildren<Camera>();
            m_minimapCamera.targetTexture = m_minimapTexture;
            m_minimapCamera.enabled = false;
            m_minimapCamera.Render();

            // assign minimap texture to canvas image
            RawImage minimap = transform.Find("Canvas/MinimapContainer/MinimapMask/Minimap").GetComponent<RawImage>();
            minimap.texture = m_minimapTexture;
            m_minimapImage = minimap.GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (Lara.Instance == null)
            {
                return;
            }

            // place camera above Lara
            m_minimapCamera.transform.position = Lara.Instance.transform.position + Vector3.up * 20.0f;
            m_minimapCamera.Render();

            // rotate minimap use Lara's forward
            Vector3 vRotation = new Vector3(0.0f, 0.0f, Lara.Instance.transform.rotation.eulerAngles.y);
            m_minimapImage.rotation = Quaternion.Euler(vRotation);
        }
    }
}