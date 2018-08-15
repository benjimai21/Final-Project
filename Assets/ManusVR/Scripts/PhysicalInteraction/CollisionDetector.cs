using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public enum CollisionType
    {
        Enter,
        Stay,
        Exit
    }

    public class CollisionDetector : MonoBehaviour
    {
        public Rigidbody Rigidbody { private set; get; }

        private HashSet<Collider> _triggerColliders = new HashSet<Collider>();
        private HashSet<Collider> _validCollisions = new HashSet<Collider>();
        private HashSet<Collider> _collisions = new HashSet<Collider>();

        public bool IsColliding { get; set; }
        public PhysicsLayer DetectionLayer = PhysicsLayer.Grab;

        // Actions
        public Action<Collision> CollisionEnter;
        public Action<Collision> CollisionStay;
        public Action<Collision> CollisionExit;

        /// <summary>
        /// Only called when the object is allowed to have collision with the given object
        /// </summary>
        public Action<Collision> ValidCollisionEnter;
        public Action<Collision> ValidCollisionStay;
        public Action<Collision> ValidCollisionExit;

        public int ObjectsInTrigger { get { return _triggerColliders.Count; } }
        public int ValidCollisions { get { return _validCollisions.Count; } }
        public int Collisions { get { return _collisions.Count; } }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        // Use this for initialization
        void Start()
        {
            IsColliding = false;

        }

        void OnCollisionEnter(Collision collision)
        {
            //todo check if still necessary
            if (ValidCollision(collision.gameObject))
            {
                _validCollisions.Add(collision.collider);
                if (ValidCollisionEnter != null)
                    ValidCollisionEnter(collision);

                IsColliding = true;
                StopAllCoroutines();
                StartCoroutine(Entered());
            }
            _collisions.Add(collision.collider);

            if (CollisionEnter != null)
                CollisionEnter(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            //todo check if still necessary
            if (ValidCollision(collision.gameObject))
            {
                if (ValidCollisionStay != null)
                    ValidCollisionStay(collision);

                IsColliding = true;
                StopAllCoroutines();
                StartCoroutine(Entered());
            }

            if (CollisionStay != null)
                CollisionStay(collision);
        }

        void OnCollisionExit(Collision collision)
        {
            if (ValidCollision(collision.gameObject))
            {
                _validCollisions.Remove(collision.collider);
                if (ValidCollisionExit != null)
                    ValidCollisionExit(collision);
            }
            _collisions.Remove(collision.collider);
            

            if (CollisionExit != null)
                CollisionExit(collision);


        }

        void OnTriggerEnter(Collider collider)
        {
            if (ValidCollision(collider.gameObject))
            {
                _triggerColliders.Add(collider);
            }

        }

        void OnTriggerExit(Collider collider)
        {
            if (ValidCollision(collider.gameObject))
            {
                _triggerColliders.Remove(collider);
            }

        }

        /// <summary>
        /// Check if this is a collision that should be registered
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        bool ValidCollision(GameObject gameObject)
        {
            PhysicsObject physicsObject;
            return PhysicsManager.Instance.GetPhysicsObject(gameObject, out physicsObject) &&
                   physicsObject.PhysicsLayer == DetectionLayer;
        }

        IEnumerator Entered()
        {
            yield return new WaitForSeconds(0.05f);
            IsColliding = false;
        }
    }
}