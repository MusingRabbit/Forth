using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyDecelerate: MonoBehaviour
{
    private Rigidbody m_rigidBody;

    [SerializeField]
    private float m_decelRate;

    public RigidbodyDecelerate()
    {
        m_decelRate = 0.05f;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(m_rigidBody.velocity.x) > m_decelRate && Mathf.Abs(m_rigidBody.velocity.y) > m_decelRate)
        {
            m_rigidBody.velocity -= m_rigidBody.velocity * ((1.0f - m_decelRate) * Time.deltaTime);
        }
    }
}
