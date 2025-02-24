using Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class InteractionManager : MonoBehaviour
    {
        public enum ActionType
        {
            Active,
            Movement,
        };

        public interface IInteraction
        {
            ActionType ActionType { get; }

            void DrawGizmos();

            bool InsideArea(Rect area);

            bool CanInteract(Lara lara);

            float GetInteractionDistance(Lara lara);

            void PerformInteraction(Lara lara);
        }

        private class Node
        {
            public Rect                 m_area;
            public Node[]               m_children;
            public List<IInteraction>   m_interactions;
        }

        private HashSet<IInteraction>       m_interactions = new HashSet<IInteraction>();
        private Node                        m_root;

        private static InteractionManager   sm_instance;
        private static bool                 sm_bIsQuitting;

        #region Properties

        public static InteractionManager Instance
        {
            get
            {
                if (sm_instance == null &&
                    Application.isPlaying)
                {
                    GameObject go = new GameObject("InteractionManager");
                    sm_instance = go.AddComponent<InteractionManager>();
                }

                return sm_instance;
            }
        }

        #endregion

        private void OnApplicationQuit()
        {
            
        }

        #region Creation

        private void Update()
        {
            // should we create the quad tree?
            if (m_root == null)
            {
                // find bounds
                Bounds b = new Bounds(Vector3.zero, Vector3.zero);
                foreach (Tomb tomb in FindObjectsByType<Tomb>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    b.Encapsulate(tomb.Bounds);
                }

                // create root
                Rect levelArea = new Rect(b.min.x, b.min.z, b.size.x, b.size.z);
                m_root = CreateNode(levelArea, new List<IInteraction>(m_interactions));
            }
        }

        public void AddInteraction(IInteraction interaction)
        {
            if (!m_interactions.Contains(interaction))
            {
                m_interactions.Add(interaction);
                m_root = null;
            }
        }

        public void RemoveInteraction(IInteraction interaction)
        {
            if (m_interactions.Contains(interaction))
            {
                m_interactions.Remove(interaction);
                m_root = null;
            }
        }

        private Node CreateNode(Rect area, List<IInteraction> remaingingInteractions)
        {
            // do we have remainging interactions?
            if (remaingingInteractions == null ||
                remaingingInteractions.Count == 0)
            {
                return null;
            }

            // create node
            Node node = new Node { m_area = area };

            if (area.width < 2.0f || area.height < 2.0f)
            {
                // leaf node?
                node.m_interactions = remaingingInteractions;
            }
            else
            {
                // create children
                Vector2 vSizeHalf = area.size * 0.5f;
                Rect[] subAreas = new Rect[]
                {
                    new Rect(area.x, area.y, vSizeHalf.x, vSizeHalf.y),
                    new Rect(area.x + vSizeHalf.x, area.y, vSizeHalf.x, vSizeHalf.y),
                    new Rect(area.x + vSizeHalf.x, area.y + vSizeHalf.y, vSizeHalf.x, vSizeHalf.y),
                    new Rect(area.x, area.y + vSizeHalf.y, vSizeHalf.x, vSizeHalf.y)
                };

                // create children
                node.m_children = System.Array.ConvertAll(subAreas, sa => CreateNode(sa, remaingingInteractions.FindAll(ii => ii.InsideArea(sa))));
            }

            return node;
        }

        #endregion

        #region Drawing

        private void OnDrawGizmosSelected()
        {
            const float QUERY_SIZE = 10.0f;

            if (Lara.Instance == null)
            {
                return;
            }

            Vector2 vLara = Lara.Instance.transform.position.ToXZ();
            Rect query = new Rect(vLara.x - QUERY_SIZE * 0.5f, vLara.y - QUERY_SIZE * 0.5f, QUERY_SIZE, QUERY_SIZE);
            DrawNode(m_root, query);   
        }

        private void DrawNode(Node node, Rect query)
        {
            if (node == null)
            {
                return;
            }

            // do we overlap the query?
            bool bOverlapsArea = node.m_area.Overlaps(query);

            // draw the area
            Gizmos.color = bOverlapsArea ? Color.yellow : Color.gray;
            Gizmos.DrawWireCube(node.m_area.center.ToXYZ(), node.m_area.size.ToXYZ());

            if (!bOverlapsArea)
            {
                return;
            }

            // draw node interactions
            if (node.m_interactions != null)
            {
                foreach (IInteraction interaction in node.m_interactions)
                {
                    interaction.DrawGizmos();
                }
            }

            // draw node children
            if (node.m_children != null)
            {
                foreach (Node child in node.m_children)
                {
                    DrawNode(child, query);
                }
            }
        }

        #endregion

        #region Queries

        public HashSet<IInteraction> GetInteractions(Vector3 vPosition, float fRadius, ActionType actionType)
        {
            Rect query = new Rect(vPosition.x - fRadius, vPosition.z - fRadius, fRadius * 2.0f, fRadius * 2.0f);
            HashSet<IInteraction> result = new HashSet<IInteraction>();
            QueryInteractions(m_root, query, actionType, result);
            return result;
        }

        private void QueryInteractions(Node node, Rect query, ActionType actionType, HashSet<IInteraction> result)
        {
            if (node == null ||
                !node.m_area.Overlaps(query))
            {
                return;
            }

            // add interactions!
            if (node.m_interactions != null)
            {
                result.UnionWith(node.m_interactions.FindAll(ii => ii.ActionType == actionType));
            }

            // query children
            if (node.m_children != null)
            {
                foreach (Node child in node.m_children)
                {
                    QueryInteractions(child, query, actionType, result);
                }
            }
        }


        #endregion
    }
}