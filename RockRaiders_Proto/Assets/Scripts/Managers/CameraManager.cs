using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera manager
/// </summary>
public class CameraManager : MonoBehaviour
{
    /// <summary>
    ///  Singleton instance
    /// </summary>
    private static CameraManager _instance;

    /// <summary>
    /// Stores the list of cameras that have been added to this camera manager
    /// </summary>
    [SerializeField]
    private List<Camera> m_cameras; 

    /// <summary>
    /// Gets the currently active camera
    /// </summary>
    private Camera m_currCamera;

    /// <summary>
    /// Stores the current selected index (active camera)
    /// </summary>
    [SerializeField]
    private int m_selectedIdx;

    /// <summary>
    /// Constructor
    /// </summary>
    public CameraManager()
    {
        m_currCamera = null;
        m_cameras = new List<Camera>();
    }

    /// <summary>
    /// Called on load - handles singleton instantiation
    /// </summary>
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            GameObject.DontDestroyOnLoad(base.gameObject);
        }
        else
        {
            GameObject.Destroy(base.gameObject);
        }
    }

    /// <summary>
    ///  Start is called before the first frame update
    ///  -> Disables cameras
    ///  -> Enables the camera an index 0
    /// </summary>
    private void Start()
    {
        this.DisableCameras();

        if (m_cameras.Count > 0)
        {
            m_cameras[0].enabled = true;
        }
    }

    /// <summary>
    /// Called every frame
    /// Clears any null cameras from the cameras list
    /// </summary>
    private void Update()
    {
        this.ClearNullCameras();
    }

    /// <summary>
    /// Clears any null cameras that may be within the camera list 
    /// (Where gameobject have been destroyed / disposed of)
    /// </summary>
    private void ClearNullCameras()
    {
        var indices = new List<int>();

        for (int i = 0; i < m_cameras.Count; i++)
        {
            if (m_cameras[i] == null)
            {
                indices.Add(i);
            }
        }

        for (int i = 0; i < indices.Count; i++)
        {
            try
            {
                m_cameras.RemoveAt(indices[i]);
            }
            catch(Exception ex)
            {
                NotificationService.Instance.Warning(ex.Message);
            }
        }
    }

    /// <summary>
    /// Registers a camera with this camera manager
    /// </summary>
    /// <param name="camera">Camera to be added</param>
    /// <param name="makeActive">Whether this camera is to be made the active camera or not</param>
    /// <exception cref="ArgumentNullException">camera cannot be null</exception>
    public void AddCamera(Camera camera, bool makeActive = false)
    {
        if (camera == null)
        {
            throw new ArgumentNullException(nameof(camera));
        }

        if (!m_cameras.Contains(camera))
        {
            m_cameras.Add(camera);

            if (makeActive)
            {
                this.SelectCamera(camera);
            }
            else
            {
                camera.enabled = false;
            }
        }
    }

    /// <summary>
    /// Disables all cameras controlled by this camera manager
    /// </summary>
    private void DisableCameras()
    {
        foreach (var camera in m_cameras)
        {
            if (camera != null)
            {
                camera.enabled = false;
            }
        }
    }

    /// <summary>
    /// Selects camera controlled by this camera manager by index.
    /// </summary>
    /// <param name="idx">Index of the camera to be selected</param>
    public void SelectCamera(int idx)
    {
        this.DisableCameras();

        if (idx > -1 && m_cameras.Count >= idx)
        {
            m_currCamera = m_cameras[idx];
            m_selectedIdx = idx;
        }

        m_currCamera.enabled = true;
    }

    /// <summary>
    /// Selects camera controlled by this camera manager by instance.
    /// </summary>
    /// <param name="camera">Instance of the camera to be selected</param>
    public void SelectCamera(Camera camera)
    {
        int idx = -1;

        for(int i = 0; i < m_cameras.Count; i++)
        {
            var currCam = m_cameras[i];

            if (currCam?.GetInstanceID() == camera.GetInstanceID())
            {
                idx = i;
                break;
            }
        }

        if (idx != -1)
        {
            this.SelectCamera(idx);
        }
    }

    /// <summary>
    /// Gets the currently selected camera
    /// </summary>
    /// <returns>Camera <see cref="Camera"/></returns>
    public Camera GetSelectedCamera()
    {
        return m_cameras[m_selectedIdx];
    }

    /// <summary>
    /// Selects the next camera in the camera list.
    /// </summary>
    public void NextCamera()
    {
        var idx = 0;

        if (m_selectedIdx < m_cameras.Count - 1)
        {
            idx = m_selectedIdx + 1;
        }

        this.SelectCamera(idx);
    }

    /// <summary>
    /// Selects the previous camera in the camera list.
    /// </summary>
    public void PrevCamera()
    {
        var idx = 0;

        if (m_selectedIdx > 0)
        {
            idx = m_selectedIdx - 1;
        }

        this.SelectCamera(idx);
    }
}
