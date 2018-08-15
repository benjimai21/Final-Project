using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flash : MonoBehaviour {

    private bool m_active;
    private Material m_mat;
    private bool m_increase;

    private float m_maxA;

    public float m_flashRate;

	// Use this for initialization
	void Start () {
        m_mat = GetComponent<Renderer>().material;
        m_maxA = m_mat.color.a;
	}

    public void SetFlashActive(bool active)
    {
        m_active = active;

        if(!active)
        {
            Color c = m_mat.color;
            m_mat.color = new Color(c.r, c.g, c.b, m_maxA);
            m_increase = false;
        }

    }
	
	// Update is called once per frame
	void Update () {
		if(m_active)
        {
            if(m_increase)
            {
                m_mat.color += new Color(0, 0, 0, m_flashRate);
                if (m_mat.color.a >= m_maxA)
                    m_increase = false;
            }
            else
            {
                m_mat.color -= new Color(0, 0, 0, m_flashRate);
                if (m_mat.color.a <= 0)
                    m_increase = true;
            }
        }
	}
}
