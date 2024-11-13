using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWell : MonoBehaviour
{
    private Dictionary<int, Rigidbody> m_rigidBodies;

    [SerializeField]
    private SphereCollider m_sphereCollider;

    [SerializeField]
    private float m_gravStrength;

    public GravityWell()
    {
        m_gravStrength = 9.8f;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_sphereCollider = this.GetComponent<SphereCollider>();
        m_rigidBodies = this.GetAllReigidBodiesInInflucence();
    }

    // Update is called once per frame
    void Update()
    {
        var removeList = new List<int>();

        foreach(var kvp in m_rigidBodies)
        {
            var rb = kvp.Value;

            if (rb != null)
            {
                var objPos = rb.transform.position;
                var centre = m_sphereCollider.center;
                var pullDir = (centre - objPos).normalized;
                var totalForce = pullDir * (m_gravStrength * rb.mass);

                Debug.Log($"Grav:{m_gravStrength}|Mass:{rb.mass}|Force:{totalForce}|Obj:{rb.gameObject.name}");
                rb.AddForce(totalForce * Time.deltaTime, ForceMode.Force);
            }
            else
            {
                removeList.Add(kvp.Key);
            }
        }

        foreach (var id in removeList)
        {
            m_rigidBodies.Remove(id);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var obj = other.gameObject;
        var rb = obj.GetComponent<Rigidbody>();
        var objId = obj.GetInstanceID();

        if (rb != null && !m_rigidBodies.ContainsKey(objId))
        {
            m_rigidBodies.Add(objId, rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var obj = other.gameObject;
        var rb = obj.GetComponent<Rigidbody>();
        var objId = obj.GetInstanceID();

        if (m_rigidBodies.ContainsKey(objId))
        {
            m_rigidBodies.Remove(objId);
        }
    }

    private Dictionary<int, Rigidbody> GetAllReigidBodiesInInflucence()
    {
        var result = new Dictionary<int, Rigidbody>();

        var colliders = Physics.OverlapSphere(m_sphereCollider.center, m_sphereCollider.radius);

        foreach (var col in colliders)
        {
            var objId = col.gameObject.GetInstanceID();
            var rb = col.gameObject.GetComponent<Rigidbody>();

            if (!result.ContainsKey(objId) && rb != null)
            {
                result.Add(objId, rb);
            }
        }

        return result;
    }
}
