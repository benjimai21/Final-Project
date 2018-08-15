using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingZone : MonoBehaviour
{

    bool zone;
    // Use this for initialization
    private void Start() {
        Collider collider = GetComponent<BoxCollider>();
        zone = false;
    }
    public bool GetZone() {

        return zone;
    }

    void OnTriggerEnter(Collider collider) {

        if (collider.gameObject.tag == "Hand") {
            print("ENTER");
            zone = true;
            Destroy(collider);

        }
            
        
    }
/*
    void OnTriggerExit(Collider collider) {
        if (collider.gameObject.tag == "Hand") {
            print("OUT");
            zone = false;
        }
    }
*/
}
