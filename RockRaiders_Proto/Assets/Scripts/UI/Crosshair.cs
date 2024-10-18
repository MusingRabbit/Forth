using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeAs("Actor")]
    [SerializeField]
    private GameObject m_actor;

    [SerializeAs("UIPlaneObject")]
    [SerializeField]
    private GameObject m_uiPlaneObject;

    [SerializeAs("Camera")]
    [SerializeField]
    private Camera m_camera;

    [SerializeAs("IsEnabled")]
    [SerializeField]
    private bool isEnabled;

    private PlayerInput m_playerController;
    private MeshCollider m_uiMeshCollider;

    public GameObject Actor
    {
        get
        {
            return m_actor;
        }
        set
        {
            m_actor = value;
        }
    }

    public Camera Camera
    {
        get
        {
            return m_camera;
        }
        set
        {
            m_camera = value;
        }
    }

    public PlayerInput PlayerController
    {
        get
        {
            return m_playerController;
        }
        set
        {
            m_playerController = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_playerController = m_actor.GetComponent<PlayerInput>();
        m_uiMeshCollider = m_uiPlaneObject.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        if (isEnabled)
        {
            var mouseVector = new Vector3(m_playerController.LookAxis.x, m_playerController.LookAxis.y, m_camera.nearClipPlane);
            var ray = m_camera.ScreenPointToRay(mouseVector);

            if (m_uiMeshCollider.Raycast(m_camera.ScreenPointToRay(mouseVector), out var hitInfo, 100))
            {
                var mouseWorldPos = hitInfo.point;
                this.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
                //Debug.DrawRay(ray.origin, ray.direction, Color.green);
            }
        }
    }

    private void OnGUI()
    {
    }
}
