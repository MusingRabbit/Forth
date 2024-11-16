using Assets.Scripts.Input;
using Assets.Scripts.UI.Models;
using System;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;

        public static InputManager Instance
        {
            get
            {
                return _instance;
            }
        }


        [SerializeField]
        private PlayerInput m_controller;
        private SettingsModel m_settings;

        public PlayerInput Controller
        {
            get
            {
                return m_controller;
            }
        }

        public SettingsModel Settings
        {
            get
            {
                return m_settings;
            }
            set
            {
                m_settings = value;
            }
        }

        public InputManager()
        {
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(base.gameObject);
            }
            else
            {
                Destroy(base.gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (m_controller != null)
            {
                m_controller.Reset();
                ProcessInputs();
                UpdateControllerLookAxis();
            }
        }

        public void Reset()
        {
            m_controller?.Reset();
        }

        public void RegisterPlayerController(PlayerInput controllerComponent)
        {
            if (controllerComponent == null)
            {
                throw new ArgumentNullException(nameof(controllerComponent));
            }

            m_controller = controllerComponent;
        }

        private void UpdateControllerLookAxis()
        {
            //var currEvent = Event.current;
            //var mousePos = UnityEngine.Input.mousePosition;
            var mouseX = UnityEngine.Input.GetAxis("Mouse X");
            var mouseY = UnityEngine.Input.GetAxis("Mouse Y");
            var multiplier = GetSensitivityMultiplier();

            m_controller.LookAxis = new Vector2(mouseX * multiplier, mouseY * multiplier);

            //Debug.Log("Look Axis : " + m_controller.LookAxis);
        }

        private float GetSensitivityMultiplier()
        {
            return m_settings.Game.MouseSensetivity * 10.0f;
        }

        private void ProcessInputs()
        {
            Vector2 moveAxis = Vector2.zero;

            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                moveAxis.y = 1;
            }

            if (UnityEngine.Input.GetKey(KeyCode.S))
            {
                moveAxis.y = -1;
            }

            if (UnityEngine.Input.GetKey(KeyCode.A))
            {
                moveAxis.x = -1;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D))
            {
                moveAxis.x = 1;
            }

            if (moveAxis != Vector2.zero)
            {
                m_controller.MoveAxis = moveAxis;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_controller.SetActionState(ControllerActions.EquipSide, ActionState.Active);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_controller.SetActionState(ControllerActions.EquipMain, ActionState.Active);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.G))
            {
                m_controller.SetActionState(ControllerActions.Drop, ActionState.Active);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                m_controller.SetActionState(ControllerActions.Jump, ActionState.Active);
            }

            if (UnityEngine.Input.GetKey(KeyCode.Space))
            {
                m_controller.SetActionState(ControllerActions.ThrustUp, ActionState.Active);
            }

            if (UnityEngine.Input.GetKey(KeyCode.Mouse0))
            {
                m_controller.SetActionState(ControllerActions.Trigger, ActionState.Active);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
            {
                m_controller.SetActionState(ControllerActions.GravBoots, ActionState.Active);
            }

            if (UnityEngine.Input.GetKey(KeyCode.Q))
            {
                m_controller.SetActionState(ControllerActions.RollLeft, ActionState.Active);
            }

            if (UnityEngine.Input.GetKey(KeyCode.E))
            {
                m_controller.SetActionState(ControllerActions.RollRight, ActionState.Active);
            }

            if (UnityEngine.Input.GetKey(KeyCode.LeftControl))
            {
                m_controller.SetActionState(ControllerActions.Crouch);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                m_controller.SetActionState(ControllerActions.Pause);
            }
        }
    }
}