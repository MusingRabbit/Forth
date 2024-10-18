using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    [SerializeField]
    private PlayerInput m_controller;

    public InputManager()
    {
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
            this.ProcessInputs();
            this.UpdateControllerLookAxis();
        }
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
        //var mousePos = Input.mousePosition;
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

        m_controller.LookAxis = new Vector2(mouseX, mouseY);

       //Debug.Log("Look Axis : " + m_controller.LookAxis);
    }

    private void ProcessInputs()
    {
        Vector2 moveAxis = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveAxis.y = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveAxis.y = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveAxis.x = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveAxis.x = 1;
        }

        if (moveAxis != Vector2.zero)
        {
            m_controller.MoveAxis = moveAxis;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_controller.SetActionState(ControllerActions.EquipSide, ActionState.Active);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_controller.SetActionState(ControllerActions.EquipMain, ActionState.Active);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            m_controller.SetActionState(ControllerActions.Drop, ActionState.Active);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_controller.SetActionState(ControllerActions.Jump, ActionState.Active);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            m_controller.SetActionState(ControllerActions.ThrustUp, ActionState.Active);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            m_controller.SetActionState(ControllerActions.Trigger, ActionState.Active);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            m_controller.SetActionState(ControllerActions.GravBoots, ActionState.Active);
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            m_controller.SetActionState(ControllerActions.Crouch);
        }
    }
}
