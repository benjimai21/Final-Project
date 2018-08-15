using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public class PhalangeData
    {
        public Finger Finger;
        public int Index;
        public device_type_t DeviceTypeT;

        public PhalangeData(Finger finger, int Index, device_type_t DeviceTypeT)
        {
            this.Finger = finger;
            this.Index = Index;
            this.DeviceTypeT = DeviceTypeT;
        }

        public override string ToString()
        {
            return (int) Finger + Index + "";
        }
    }

    public class Phalange : MonoBehaviour
    {
        private Collider[] _colliders;
        public CollisionDetector Detector { get; private set; }

        public PhalangeData PhalangeData { get; set; }
        public Rigidbody Rigidbody { get; private set; }

        public Action<PhalangeData, Collision, CollisionType> CollisionEntered;

        // Use this for initialization
        void Awake () {
            Rigidbody = gameObject.GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();

            Rigidbody.mass = 3f;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            Rigidbody.useGravity = false;
            Detector =  gameObject.AddComponent<CollisionDetector>();

            PhysicsManager.Instance.Register(GetComponents<Collider>(), GetComponent<Rigidbody>(), PhysicsLayer.Phalange);
        }

        void Start()
        {
            _colliders = GetComponents<Collider>();

            Detector.CollisionEnter += CollisionEnter;
            Detector.CollisionStay += CollisionStay;
            Detector.CollisionExit += CollisionExit;
        }

        private void CollisionEnter(Collision collision)
        {
            CheckCollision(collision, CollisionType.Enter);
        }

        private void CollisionStay(Collision collision)
        {
            CheckCollision(collision, CollisionType.Stay);
        }

        private void CollisionExit(Collision collision)
        {
            CheckCollision(collision, CollisionType.Exit);
        }

        /// <summary>
        /// Happens when this phalange is colliding with an object
        /// </summary>
        /// <param name="collision"></param>
        private void CheckCollision(Collision collision, CollisionType type)
        {
            foreach (var collider in _colliders)
            {
                if (PhysicsManager.Instance.ProcessCollision(collider, collision) && CollisionEntered != null)
                    CollisionEntered(PhalangeData, collision, type);
            } 
        }
    }
}