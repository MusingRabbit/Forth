using Assets.Scripts.Input;
using Assets.Scripts.UI.Models;
using System;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static InputManager _instance;

        public static InputManager Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Player input 
        /// </summary>
        [SerializeField]
        private PlayerInput m_controller;

        /// <summary>
        /// Settings
        /// </summary>
        private SettingsModel m_settings;

        /// <summary>
        /// Gets the player input controller
        /// </summary>
        public PlayerInput Controller
        {
            get
            {
                return m_controller;
            }
        }

        /// <summary>
        /// Gets the current settings
        /// </summary>
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

        /// <summary>
        /// Constructor
        /// </summary>
        public InputManager()
        {
        }

        /// <summary>
        /// Called on load
        /// -> Handles singleton instantiation
        /// </summary>
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

        /// <summary>
        /// Called every frame
        /// -> Resets input controller
        /// -> Processes inputs
        /// -> Updates controller look axis
        /// </summary>
        void Update()
        {
            if (m_controller != null)
            {
                m_controller.Reset();
                ProcessInputs();
                UpdateControllerLookAxis();
            }
        }

        /// <summary>
        /// Resets input controller
        /// </summary>
        public void Reset()
        {
            m_controller?.Reset();
        }

        /// <summary>
        /// Registers player input controller with this input manager
        /// </summary>
        /// <param name="playerInput">Player input controller</param>
        /// <exception cref="ArgumentNullException">player input cannot be null</exception>
        public void RegisterPlayerController(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                throw new ArgumentNullException(nameof(playerInput));
            }

            m_controller = playerInput;
        }

        /// <summary>
        /// Updates the look axis of the controller from mouse x, and y axes. Applies any sensetivity modifiers
        /// </summary>
        private void UpdateControllerLookAxis()
        {
            var mouseX = UnityEngine.Input.GetAxis("Mouse X");
            var mouseY = UnityEngine.Input.GetAxis("Mouse Y");
            var multiplier = GetSensitivityMultiplier();

            m_controller.LookAxis = new Vector2(mouseX * multiplier, mouseY * multiplier);
        }

        /// <summary>
        /// Gets the sensetivity multiplier from game settings.
        /// </summary>
        /// <returns></returns>
        private float GetSensitivityMultiplier()
        {
            return m_settings.Game.MouseSensetivity * 10.0f;
        }

        /// <summary>
        /// Processes key inputs
        /// </summary>
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