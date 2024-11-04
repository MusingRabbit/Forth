using Assets.Scripts.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;


    [SerializeField]
    private List<Camera> m_cameras; 

    private Camera m_currCamera;

    [SerializeField]
    private int m_selectedIdx;

    public CameraManager()
    {
        m_currCamera = null;
        m_cameras = new List<Camera>();
    }

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

    // Start is called before the first frame update
    private void Start()
    {
        this.DisableCameras();

        if (m_cameras.Count > 0)
        {
            m_cameras[0].enabled = true;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        this.ClearNullCameras();
    }

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

    public void AddCamera(Camera camera, bool makeActive = false)
    {
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

    public Camera GetSelectedCamera()
    {
        return m_cameras[m_selectedIdx];
    }

    public void NextCamera()
    {
        var idx = 0;

        if (m_selectedIdx < m_cameras.Count - 1)
        {
            idx = m_selectedIdx + 1;
        }

        this.SelectCamera(idx);
    }

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
