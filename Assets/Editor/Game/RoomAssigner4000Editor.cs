using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Game
{
    [CustomEditor(typeof(RoomAssigner4000))]
    public class RoomAssigner4000Editor : Editor
    {
        private string  m_name = "Your First Name Here";
        private int     m_iMaxRooms = 8;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(20);
            m_name = EditorGUILayout.TextField("First Name", m_name);
            m_iMaxRooms = EditorGUILayout.IntSlider("Max Rooms", m_iMaxRooms, 2, 8);

            // calculate seed
            string ucaseName = m_name.ToUpper();
            int iSeed = 0;
            foreach (char c in ucaseName)
            {
                int iLetter = c - 'A';
                iSeed += iLetter;
            }

            // add today's day
            iSeed += System.DateTime.Now.DayOfYear;

            // get random room
            Random.InitState(iSeed);
            int iRoom = Random.Range(1, m_iMaxRooms + 1);
            EditorGUILayout.LabelField("Your Room", iRoom.ToString());
        }
    }
}