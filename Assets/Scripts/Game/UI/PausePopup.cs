using UnityEngine;
using Events;

namespace Game.UI {
    [RequireComponent(typeof(CanvasGroup))]
    public class PausePopup : EventHandler.GameEventBehaviour {
        private bool m_bIsDone = false;
        private CanvasGroup m_canvasGroup = null;

        private void OnEnable() {
            m_canvasGroup = GetComponent<CanvasGroup>();
            m_canvasGroup.alpha = 0.0f;
        }

        public override void OnUpdate() {
            base.OnUpdate();

            // update alpha
            m_canvasGroup.alpha = Mathf.MoveTowards(m_canvasGroup.alpha, m_bIsDone ? 0.0f : 1.0f, Time.deltaTime);
        }

        public virtual void OnResume() {
            m_bIsDone = true;
            m_canvasGroup.interactable = false;
        }

        public virtual void OnCancel() {
            m_bIsDone = true;
            m_canvasGroup.interactable = false;
        }

        public override bool IsDone() {
            return m_bIsDone && m_canvasGroup.alpha < 0.001f;
        }

        public override void OnEnd() {
            base.OnEnd();
            Destroy(gameObject);
        }

        public static void Create<T>(Lara lara) where T : EventHandler.GameEventBehaviour {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + typeof(T).Name);
            GameObject go = Instantiate(prefab);
            T om = go.GetComponent<T>();
            lara.PushEvent(om);
        }
    }
}