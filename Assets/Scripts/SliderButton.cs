using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderButton : MonoBehaviour {

    public GameObject m_BasicBacking;
    public GameObject m_HighlightBacking;
    public MenuManager m_MenuManager;

    public SLIDERFUNCTION m_function;
    public enum SLIDERFUNCTION { MESSAGELENGTH, MESSAGESPEED, CONNECTIONRANGE }

    private bool m_attatched;
    private GameObject m_attatchedObj;

	// Use this for initialization
	void Start () {
        m_attatched = false;
    }
	
	// Update is called once per frame
	void Update () {
		if(m_attatched)
        {
            // Slider can get stuck on finger if hand changes back to 'default mode' while attached
            // Check for this here
            if(!m_attatchedObj.activeInHierarchy)
            {
                m_attatched = false;
                m_attatchedObj = null;
                m_BasicBacking.SetActive(true);
                m_HighlightBacking.SetActive(false);
            }
            else
            {
                transform.position = m_attatchedObj.transform.position;

                float z = Mathf.Clamp(transform.localPosition.z, -5, 5);
                transform.localPosition = new Vector3(0, 0.075f, z);
                Function(z);
            }
            
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finger")
        {
            m_attatched = true;
            m_attatchedObj = other.gameObject;
            m_BasicBacking.SetActive(false);
            m_HighlightBacking.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Finger")
        {
            m_attatched = false;
            m_attatchedObj = null;
            m_BasicBacking.SetActive(true);
            m_HighlightBacking.SetActive(false);
        }
    }

    private void Function(float val)
    {
        m_MenuManager.SliderFunction(m_function, val);
    }
        
    public void SetPosition(float val)
    {
        float z = Mathf.Clamp(val, -5, 5);
        transform.localPosition = new Vector3(0, 0.075f, z);
    }
}
