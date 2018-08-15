using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuToggle : MonoBehaviour {

    public GameObject m_MenuObj;

	// Use this for initialization
	void Start () {
        m_MenuObj.SetActive(false);
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Finger")
            m_MenuObj.SetActive(!m_MenuObj.activeSelf);
    }
}
