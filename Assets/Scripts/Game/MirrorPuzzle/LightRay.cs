using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MirrorPuzzle
{
    public class LightRay : MonoBehaviour
    {
        GameObject          m_lineTemplate;
        List<LineRenderer>  m_lines = new List<LineRenderer>();
        


        void Start()
        {
            m_lineTemplate = transform.Find("RayTemplate").gameObject;
        }

        void Update () 
        {
            // gather light points
            List<Vector3> points = new List<Vector3>();
            points.Add(transform.position);
            LightBounce(transform.position, transform.forward, points);

            // needs more lines?
            int iNumLines = points.Count - 1;
            while (m_lines.Count < iNumLines)
            {
                GameObject go = Instantiate(m_lineTemplate, m_lineTemplate.transform.parent);
                go.name = "Ray #" + transform.childCount;
                go.SetActive(true);
                m_lines.Add(go.GetComponent<LineRenderer>());
            }

            // update lines
            for (int i = 0; i < m_lines.Count; i++)
            {
                LineRenderer lr = m_lines[i];
                lr.enabled = i < iNumLines;

                if (lr.enabled)
                {
                    lr.positionCount = 2;
                    lr.SetPositions(new Vector3[] { points[i], points[i + 1] });
                }
            }
        }

        private void LightBounce(Vector3 vSource, Vector3 vDirection, List<Vector3> points)
        {
            Ray ray = new Ray(vSource, vDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 200.0f))
            {
                // did we hit mirror?
                Mirror mirror = hit.collider.GetComponent<Mirror>();
                if (mirror)
                {
                    points.Add(hit.point);

                    if (Vector3.Dot(vDirection, mirror.transform.forward) < -0.05f)
                    {
                        // reflect light!
                        Vector3 vReflect = Vector3.Reflect(vDirection, mirror.transform.forward);
                        LightBounce(hit.point, vReflect.normalized, points);
                    }
                }
                else
                {
                    points.Add(hit.point + vDirection * 0.2f);
                }

                // did we hit light switch?
                LightSwitch ls = hit.collider.GetComponent<LightSwitch>();
                if (ls != null)
                {
                    ls.OnReceiveLight();
                }
            }
        }
    }
}