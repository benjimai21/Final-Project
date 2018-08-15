using System.Collections;
using System.Collections.Generic;
using ManusVR.PhysicalInteraction;
using UnityEngine;

public class P_Bucket : MonoBehaviour
{
    public GameObject EffectAnchor;
    public GameObject BalloonPop;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Interactable>() != null)
        {
            Destroy(Instantiate(BalloonPop, EffectAnchor.transform.position, EffectAnchor.transform.rotation), 2f);
        }
    }
}
