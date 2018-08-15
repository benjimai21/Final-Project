using System;
using System.Collections.Generic;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public class TriggerBinder : MonoBehaviour {

        BoxQuery query;
        BoxMessage message;
        ConnectivityBox connectivity;

        
        public Collider Collider { get; set; }
        //public List<Rigidbody> CollidingObjects = new List<Rigidbody>();
        public HashSet<Interactable> CollidingInteractables = new HashSet<Interactable>();

        void Start()
        {
            Collider = GetComponent<Collider>();

            query = FindObjectOfType<BoxQuery>();
            message = FindObjectOfType<BoxMessage>();
            connectivity = FindObjectOfType<ConnectivityBox>();
        }

        /// <summary>
        /// Happens when the manus hand is entering a trigger
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerEnter(Collider collider)
        {

            if (collider.gameObject.tag == "Clap") {
                print("clap");
                query.TriggerPulled();
                message.TriggerPulled();
                connectivity.TriggerPulled();
    
            }


            PhysicsObject physicsObject = null;
            if (!PhysicsManager.Instance.GetPhysicsObject(collider.gameObject, out physicsObject))
                return;
            var interactable = collider.GetComponent<Interactable>();
            if (interactable == null)
                interactable = physicsObject.GameObject.GetComponent<Interactable>();
            if (interactable != null && interactable.PhysicsLayer == PhysicsLayer.Grab)
            {
                CollidingInteractables.Add(interactable);
            }


        }

        /// <summary>
        /// Happens when the manus hand exits a trigger
        /// </summary>
        /// <param name="collider"></param>
        void OnTriggerExit(Collider collider)
        {
            PhysicsObject physicsObject = null;
            if (!PhysicsManager.Instance.GetPhysicsObject(collider.gameObject, out physicsObject) || physicsObject.PhysicsLayer == PhysicsLayer.Phalange)
                return;

            var interactable = collider.GetComponent<Interactable>();
            if (interactable == null)
                interactable = physicsObject.GameObject.GetComponent<Interactable>();
            CollidingInteractables.Remove(interactable);
        }

    }
}
