using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class ExplorationCameraEvent : CameraController.CameraEvent
    {
        #region Properties

        public virtual Vector3 LookTarget
        {
            get
            {
                if (Lara.Instance == null)
                {
                    return Vector3.zero;
                }

                return Lara.Instance.transform.position + Vector3.up * 1.2f;
            }
        }

        public virtual Vector3 EyeTarget
        {
            get
            {
                if (Lara.Instance == null)
                {
                    return Vector3.zero;
                }

                Vector3 vDir = -Lara.Instance.transform.forward;
                vDir = Quaternion.AngleAxis(Controller.m_fAngle * CameraController.MAX_ANGLE, Lara.Instance.transform.right) * vDir;
                return LookTarget + vDir * Controller.m_fDistance;
            }
        }


        #endregion

        public ExplorationCameraEvent(CameraController controller) : base(controller)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // move to eye target
            Transform transform = Controller.transform;
            transform.position += (EyeTarget - transform.position) * Time.deltaTime * 6.0f;     // TODO: test different speeds here

            // look at target
            transform.rotation = Quaternion.LookRotation(LookTarget - transform.position);

            // do collision
            //CameraCollision_RayCast();
            //CameraCollision_SphereCast();
            CameraCollision_MultiSphereCast();
        }


        private void CameraCollision_RayCast()
        {
            Transform transform = Controller.transform;
            Vector3 vToCamera = transform.position - LookTarget;
            Ray ray = new Ray(LookTarget, vToCamera.normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, vToCamera.magnitude, Controller.LevelMask))
            {
                transform.position = hit.point;
                Debug.DrawLine(LookTarget, hit.point, Color.red);
            }
        }

        private void CameraCollision_SphereCast()
        {
            const float RADIUS = 0.2f;

            Transform transform = Controller.transform;
            Vector3 vToCamera = transform.position - LookTarget;
            Ray ray = new Ray(LookTarget, vToCamera.normalized);
            RaycastHit hit;

            if (Physics.SphereCast(ray, RADIUS, out hit, vToCamera.magnitude, Controller.LevelMask))
            {
                transform.position = ray.origin + ray.direction * hit.distance;
                Debug.DrawLine(LookTarget, hit.point, Color.red);
            }
        }

        private void CameraCollision_MultiSphereCast()
        {
            const int NUM_RAYS = 64;
            const float RADIUS = 0.1f;

            // sphere cast in a circle to find distances to level geometry
            Transform transform = Controller.transform;
            float[] rayDistances = new float[NUM_RAYS];
            Vector3[] directions = new Vector3[NUM_RAYS];
            Vector3 vLookTarget = LookTarget;
            RaycastHit hit;
            for (int i = 0; i < NUM_RAYS; i++)
            {
                float fAngle = (i / (float)NUM_RAYS) * 360.0f;
                Vector3 vDir = Quaternion.Euler(Controller.m_fAngle * -CameraController.MAX_ANGLE, fAngle, 0.0f) * Vector3.forward;
                directions[i] = vDir;

                Ray ray = new Ray(vLookTarget, vDir);
                if (Physics.SphereCast(ray, RADIUS, out hit, Controller.m_fDistance, Controller.LevelMask))
                {
                    rayDistances[i] = hit.distance;
                }
                else
                {
                    rayDistances[i] = Controller.m_fDistance;
                }
            }

            // smooth the distances
            for (int iSmooth = 0; iSmooth < Controller.m_iSmoothingIterations; ++iSmooth)
            {
                float[] newDistances = new float[NUM_RAYS];

                for (int i = 0; i < NUM_RAYS; i++)
                {
                    int iPrev = (i + NUM_RAYS - 1) % NUM_RAYS;
                    int iNext = (i + 1) % NUM_RAYS;

                    float fPrev = rayDistances[iPrev];
                    float fCurr = rayDistances[i];
                    float fNext = rayDistances[iNext];

                    newDistances[i] = (Mathf.Min(fPrev, fCurr) + fCurr + Mathf.Min(fCurr, fNext)) / 3.0f;
                }

                rayDistances = newDistances;
            }

            // draw the circle
            for (int i = 0; i < NUM_RAYS; i++)
            {
                Vector3 vA = vLookTarget + directions[i] * rayDistances[i];
                Vector3 vB = vLookTarget + directions[(i + 1) % NUM_RAYS] * rayDistances[(i + 1) % NUM_RAYS];
                Debug.DrawLine(vA, vB, Color.green);
            }

            // limit the camera to the Green Blob (TM)
            Vector3 vToCamera = transform.position - LookTarget;
            float fCameraAngle = Mathf.Atan2(vToCamera.x, vToCamera.z) * Mathf.Rad2Deg;
            if (fCameraAngle < 0.0f) fCameraAngle += 360.0f;

            float fSegment = (fCameraAngle / 360.0f) * NUM_RAYS;
            int iSegment = (int)fSegment;
            float fBlend = fSegment - iSegment;
            float fBlendedDistance = Mathf.Lerp(rayDistances[iSegment], rayDistances[(iSegment + 1) % NUM_RAYS], fBlend);

            Debug.DrawLine(LookTarget, LookTarget + vToCamera.normalized * fBlendedDistance, new Color(1.0f, 0.5f, 0.0f));
            transform.position = LookTarget + vToCamera.normalized * fBlendedDistance;
        }
    }
}