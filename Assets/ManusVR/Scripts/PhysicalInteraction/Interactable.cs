using System;
using System.Collections.Generic;
using System.Linq;
using cakeslice;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ManusVR.PhysicalInteraction
{
    public abstract class Interactable : MonoBehaviour
    {
        internal readonly HashSet<CollisionDetector> _detectors = new HashSet<CollisionDetector>();
        private Collider[] _colliders;
        internal Joint _connection;

        public Rigidbody Rigidbody;
        //[HideInInspector]
        public ObjectGrabber Hand;
        [Tooltip("Grab: The user is able to touch and grab this object. \n" +
                 "Push: The user is able to touch this object but not grab it. \n" + 
                 "Phalange: Not supported \n" +
                 "UI: Not supported \n")]
        public PhysicsLayer PhysicsLayer = PhysicsLayer.Grab;

        [Tooltip("Highlight object when touched by a finger")]
        public bool HighlightOnImpact = true;
        [Tooltip("Highlight when object is grabbed")]
        public bool HighlightWhenGrabbed = true;
        [Header("Settings for grab PhysicsLayer")]
        [Tooltip("Help the user by automaticly releasing the grabbed object when placed on floor")]
        public bool PlacementAssistance = false;
        [Tooltip("Release the object when the hand is to far away from the object")]
        public bool ReleaseOnDistance = false;
        [Range(0, 2f)]
        public float ReleaseDistance = 0.1f;
        [Tooltip("Release the object when all of the phalanges (bones) are out of the object")]
        public bool ReleaseWithPhalanges = false;
        [Tooltip("Pull the object towards the hand instead of attaching it. If disabled the user will release the object when the hand is open")]
        public bool Magnetic = true;

        public bool GravityWhenGrabbed = true;
        public bool GravityWhenReleased = true;
        public bool KinematicWhenReleased = false;
        public bool IsGrabbed { get { return _connection != null; } }

        public Action OnGrabbed;
        public Action OnReleased;

        [SerializeField, FormerlySerializedAs("OnGrabbed")]
        private UnityEvent _onGrabbed;

        [SerializeField, FormerlySerializedAs("OnReleased")]
        private UnityEvent _onReleased;


        /// <summary>
        /// The amount of objects that are inside of the trigger of this interactable
        /// </summary>
        /// <returns></returns>
        public int TotalObjectsInTriggers { get { return _detectors.Sum(detector => detector.ObjectsInTrigger); } }

        /// <summary>
        /// The total amount of objects that currently have collision with this interactable
        /// </summary>
        /// <returns></returns>
        public int TotalCollidingObjects { get { return _detectors.Sum(detector => detector.Collisions); } }

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>();
            Initialize(_colliders);
            if (Rigidbody == null)
                Rigidbody = GetComponent<Rigidbody>();

            OnGrabbed += () =>
            {
                if (_onGrabbed != null) _onGrabbed.Invoke();
            };

            OnReleased += () =>
            {
                if (OnReleased != null) _onReleased.Invoke();
            };
        }

        private void Start()
        {           
            if (Rigidbody == null)
                Debug.LogWarning("Interactable needs to have a reference to the beloning rigidbody");
            PhysicsManager.Instance.Register(_colliders, Rigidbody, PhysicsLayer);
        }

        protected virtual void CollisionEnter(Collision collision)
        {

        }

        protected virtual void CollisionExit(Collision collision)
        {

        }

        /// <summary>
        /// Attach this interactable to the given rigidbody
        /// </summary>
        /// <param name="connectedBody"></param>
        /// <param name="attachedJoint"></param>
        public virtual void Attach(Rigidbody connectedBody, ObjectGrabber hand)
        {
            if (Hand != null)
                Hand.ReleaseItem(this);
            Hand = hand;

            if (OnGrabbed != null)
                OnGrabbed.Invoke();
        }

        /// <summary>
        /// Dettach the given joint from this interactable
        /// </summary>
        /// <param name="connection"></param>
        public virtual void Dettach(ObjectGrabber hand)
        {
            Hand = null;
            if (OnReleased != null)
                OnReleased.Invoke();
        }

        protected virtual void Initialize(Collider[] colliders)
        {
            foreach (var child in colliders)
            {
                var detector = child.GetComponent<CollisionDetector>();
                if (detector == null)
                    detector = child.gameObject.AddComponent<CollisionDetector>();

                detector.DetectionLayer = PhysicsLayer.Phalange;
                _detectors.Add(detector);

                if (PhysicsLayer == PhysicsLayer.Push || PhysicsLayer == PhysicsLayer.Grab)
                {
                    detector.CollisionEnter += CollisionEnter;
                    detector.CollisionExit += CollisionExit;
                }

                // Copies of triggers are only neccesary when the object is grabbable
                if (child.isTrigger || PhysicsLayer != PhysicsLayer.Grab)
                    continue;
                CopyTriggerCollider(child);
            }
        }

        /// <summary>
        /// Make a copy of the given collider
        /// </summary>
        /// <param name="child"></param>
        private Collider CopyTriggerCollider(Collider child)
        {
            const float colliderSizeMultiplier = 1.05f;
            // Add a extra trigger collider
            var colliderCopy = CopyComponent(child, child.gameObject) as Collider;

            if (colliderCopy is MeshCollider)
            {
                var meshCopy = colliderCopy as MeshCollider;
                // Make it convex otherwise it will not work with triggers
                meshCopy.convex = true;
                meshCopy.inflateMesh = true;
                meshCopy.skinWidth = 0.002f;
            }

            else if (colliderCopy is BoxCollider)
            {
                var boxCopy = colliderCopy as BoxCollider;
                var boxChild = child as BoxCollider;
                boxCopy.size = boxChild.size * colliderSizeMultiplier;
                boxCopy.center = boxChild.center;
            }

            else if (colliderCopy is SphereCollider)
            {
                var sphereCopy = colliderCopy as SphereCollider;
                var sphereChild = child as SphereCollider;
                sphereCopy.radius = sphereChild.radius * colliderSizeMultiplier;
                sphereCopy.center = sphereChild.center;
            }

            else if (colliderCopy is CapsuleCollider)
            {
                var capsuleCopy = colliderCopy as CapsuleCollider;
                var capsuleChild = child as CapsuleCollider;
                capsuleCopy.radius = capsuleChild.radius * colliderSizeMultiplier;
                capsuleCopy.height = capsuleChild.height;
                capsuleCopy.center = capsuleChild.center;
                capsuleCopy.direction = capsuleChild.direction;
            }
         
            colliderCopy.isTrigger = true;
            return colliderCopy;
        }

        private void OnDestroy()
        {
            PhysicsManager.Instance.Remove(GetComponentsInChildren<Collider>(), Rigidbody);
        }

        /// <summary>
        /// Make a copy of the given component
        /// </summary>
        /// <param name="original"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private Component CopyComponent(Component original, GameObject destination)
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            var fields = type.GetFields();
            foreach (var field in fields)
                field.SetValue(copy, field.GetValue(original));
            return copy;
        }
    }
}