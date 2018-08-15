using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour {

    public GameObject m_QueryMode;
    public GameObject m_MessageMode;
    public GameObject m_ControlModel;
    public GameObject m_ControlBox;
    public GameObject m_QueryLoading;
    public GameObject m_ConnectivityMode;


    public void SetQueryMode()
    {
        m_QueryMode.SetActive(true);
        m_MessageMode.SetActive(false);
        m_ConnectivityMode.SetActive(false);
    }

    public void SetMessageMode()
    {
        m_MessageMode.SetActive(true);
        m_QueryMode.SetActive(false);
        m_ConnectivityMode.SetActive(false);
    }

    public void SetModelControl()
    {
        m_ControlBox.SetActive(false);
        m_ControlModel.SetActive(true);
    }

    public void SetBoxControl()
    {
        m_ControlBox.SetActive(true);
        m_ControlModel.SetActive(false);
    }

    public void ShowQueryLoading(bool show)
    {
        m_QueryLoading.SetActive(show);
    }

    internal void SetConnectivityMode()
    {
        m_ConnectivityMode.SetActive(true);
        m_QueryMode.SetActive(false);
        m_MessageMode.SetActive(false);
    }
}
