using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    private static Dictionary<uint, Singleton> _instances = new Dictionary<uint, Singleton>();

    [SerializeField]
    private uint m_key;

    public uint Id
    {
        get
        {
            return m_key;
        }
    }

    public Singleton()
    {
    }

    private void Awake()
    {
        if (!_instances.ContainsKey(m_key))
        {
            _instances.Add(m_key, this);
            GameObject.DontDestroyOnLoad(base.gameObject);
        }
        else
        {
            GameObject.Destroy(base.gameObject);
        }
    }
}
