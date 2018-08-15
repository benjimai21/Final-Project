using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_collider : MonoBehaviour {


    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Index") {
            print("button");
        }
    }
    
}
