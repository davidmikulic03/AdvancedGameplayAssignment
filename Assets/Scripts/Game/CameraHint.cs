using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;

namespace Game
{
    [RequireComponent(typeof(Collider))]
    public abstract class CameraHint : EventHandler.GameEventBehaviour
    {
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Lara>() != null)
            {
                CameraController.Instance.PushEvent(this);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<Lara>() != null)
            {
                CameraController.Instance.RemoveEvent(this);
            }
        }

        public override bool IsDone()
        {
            return false;
        }
    }
}