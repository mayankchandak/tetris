using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Shape[] m_allShapes;
    public Transform[] m_queueXforms = new Transform[3];

    Shape[] m_queuedShapes = new Shape[3];
    float m_queueScale = 0.5f;

    Shape GetRandomShape()
    {
        int i = Random.Range(0, m_allShapes.Length);
        if (m_allShapes[i])
        {
            return m_allShapes[i];
        }
        else
        {
            Debug.Log("WARNING! Invalid shape in spawner.");
            return null;
        }
    }

    public Shape SpawnShape()
    {
        Shape shape = null;

        shape = GetQueuedShape();
        shape.transform.position = transform.position;
        shape.transform.localScale = new Vector3(1, 1, 1);
        if (shape)
        {
            return shape;
        }
        else
        {
            Debug.LogWarning("WARNING! Invalid shape in spawner.");
            return null;
        }
    }

    void InitQueue()
    {
        for(int i = 0; i < m_queuedShapes.Length; i++)
        {
            m_queuedShapes[i] = null;
        }
        FillQueue();
    }

    void FillQueue()
    {
        for(int i = 0; i < m_queuedShapes.Length; i++)
        {
            if (!m_queuedShapes[i])
            {
                m_queuedShapes[i] = Instantiate(GetRandomShape(), transform.position, Quaternion.identity) as Shape;
                m_queuedShapes[i].transform.position = m_queueXforms[i].transform.position + m_queuedShapes[i].m_queueOffset;
                m_queuedShapes[i].transform.localScale = new Vector3(m_queueScale, m_queueScale, m_queueScale);
            }
        }
    }

    Shape GetQueuedShape()
    {
        Shape firstShape = null;

        if (m_queuedShapes[0])
        {
            firstShape = m_queuedShapes[0];
        }

        for (int i = 1; i < m_queuedShapes.Length; i++)
        {
            m_queuedShapes[i - 1] = m_queuedShapes[i];
            m_queuedShapes[i - 1].transform.position = m_queueXforms[i - 1].position + m_queuedShapes[i].m_queueOffset;

        }

        m_queuedShapes[m_queuedShapes.Length - 1] = null;

        FillQueue();

        return firstShape;
    }

    void Awake()
    {
        InitQueue();
    }

}
