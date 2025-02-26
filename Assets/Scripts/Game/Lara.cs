using Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Game {
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class Lara : EventHandler {
        public abstract class LaraEvent : GameEvent {
            private Lara m_lara;

            #region Properties

            public Lara Lara => m_lara;

            public Animator Animator => m_lara.Animator;

            public CharacterController Controller => m_lara.Controller;

            public Transform Transform => m_lara.transform;

            public Vector2 Movement => m_lara.m_movement.ReadValue<Vector2>();

            public bool Interact => m_lara.m_interact.triggered;

            public bool InteractHeld => m_lara.m_interact.ReadValue<float>() > 0.5f;

            public bool Jump => m_lara.m_jump.triggered;

            public bool Pause => m_lara.m_pause.triggered;

            public bool Run => m_lara.m_run.triggered;
            public bool IsRunning => m_lara.m_run.ReadValue<float>() > 0.5f;

            #endregion

            public LaraEvent(Lara lara) {
                m_lara = lara;
            }
        }

        private Animator m_animator;
        private CharacterController m_controller;

        private InputAction m_movement;
        private InputAction m_interact;
        private InputAction m_jump;
        private InputAction m_pause;
        private InputAction m_run;

        private static Lara sm_instance;

        public float MOVE_SPEED = 4.0f;

        #region Properties

        public Animator Animator => m_animator;

        public CharacterController Controller => m_controller;

        public static Lara Instance => sm_instance;

        #endregion

        private void OnEnable() {
            m_animator = GetComponent<Animator>();
            m_controller = GetComponent<CharacterController>();
            sm_instance = this;

            PlayerInput pi = GetComponent<PlayerInput>();
            m_movement = pi.actions["Movement"];
            m_interact = pi.actions["Interact"];
            m_jump = pi.actions["Jump"];
            m_pause = pi.actions["Pause"];
            m_run = pi.actions["Run"];

            PushEvent(new ExplorationEvent(this));
        }

        private void OnDisable() {
            sm_instance = (sm_instance == this ? null : sm_instance);
        }
    }
}