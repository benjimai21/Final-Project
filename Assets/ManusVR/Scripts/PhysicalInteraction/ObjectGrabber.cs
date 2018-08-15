using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManusVR;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    /// <summary>
    ///     Use this script to grab objects with the manus gloves
    /// </summary>
    public class ObjectGrabber : MonoBehaviour
    {
        public device_type_t DeviceType;                            // The deviceType that belongs to the grabber
        public TriggerBinder TriggerBinder;                         // The triggerbinder on the hand
        public Action<GameObject, device_type_t> OnItemGrabbed;
        public Interactable GrabbedItem { get { return _grabbedItem; } }
        public Rigidbody HandRigidbody { get { return _handController.HandRigidbody; } }

        private ThrowHandler _throwHandler;

        // Variables for releasing objects
        private const int _ReleaseTrigger = -45;
        private double _oldOpeningSpeed;

        private Interactable _grabbedItem;
        private HandController _handController;

        private void Start()
        {
            var controllers = GetComponents<HandController>();
            foreach (var controller in controllers)
                if (controller.device_type == DeviceType)
                    _handController = controller;

            _throwHandler = gameObject.GetComponentsInChildren<ThrowHandler>()
                .FirstOrDefault(handler => handler.DeviceType == DeviceType);

            if (_throwHandler == null)
            {
                _throwHandler = gameObject.AddComponent<ThrowHandler>();
                _throwHandler.DeviceType = DeviceType;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateGrabObjects();
            UpdateReleaseObjects();
        }

        /// <summary>
        /// Try to grab items
        /// </summary>
        private void UpdateGrabObjects()
        {
            if (_grabbedItem != null) return;

            // Always try to grab a item when the user is making a fist
            if (HandData.Instance.HandClosed(DeviceType))
                foreach (var rb in TriggerBinder.CollidingInteractables)
                    GrabItem(rb);

            // Grab when enough phalanges are colliding
            if (_handController.IsThumbColliding && _handController.AmountOfCollidingPhalanges > 1
                     && HandData.Instance.GetCloseValue(DeviceType) != CloseValue.Open)
            {
                foreach (var rb in TriggerBinder.CollidingInteractables)
                    GrabItem(rb);
            }

        }

        /// <summary>
        /// Try to release items
        /// </summary>
        private void UpdateReleaseObjects()
        {

            var openingspeed = UpdateOpeningspeed();
            if (_grabbedItem == null)
                return;
            if (_grabbedItem != null && _grabbedItem.Hand != this)
                return;

            // Release item when the hand is completely open
            if (HandData.Instance.HandOpened(DeviceType))
            {
                ReleaseItem(_grabbedItem);
                //Debug.Log("Released because the hand was fully open");
            }

            // Release the object when the openingspeed is high enough
            if (openingspeed < _ReleaseTrigger)
            {
                ReleaseItem(_grabbedItem);
                //Debug.Log("Release because the openingspeed was high enough");
            }

            // Release object if 0 phalanges are inside of the object
            if (_grabbedItem != null && _grabbedItem.ReleaseWithPhalanges && _grabbedItem.TotalObjectsInTriggers <= 1)
            {
                ReleaseItem(_grabbedItem);
                //Debug.Log("Release because all of the phalanges are out of the object");
            }
        }

        /// <summary>
        /// Gets the current openingspeed of the hand,
        /// A negative value means that the hand is opening
        /// </summary>
        /// <returns></returns>
        private float UpdateOpeningspeed()
        {
            float difference = (float)HandData.Instance.Average(DeviceType) - (float)_oldOpeningSpeed;
            float openingSpeed = difference * 100000 * Time.deltaTime;
            _oldOpeningSpeed = (float)HandData.Instance.Average(DeviceType);
            return openingSpeed;
        }

        /// <summary>
        /// Release the grabbed item and turn collision with the object back on
        /// </summary>
        /// <returns></returns>
        private IEnumerator Release(Interactable interactable)
        {
            if (interactable == null) yield break;

            if (interactable == _grabbedItem)
                _grabbedItem = null;

            interactable.Dettach(this);
            _throwHandler.OnObjectRelease(interactable.Rigidbody);

            yield return new WaitUntil(() => interactable.TotalObjectsInTriggers <= 0);
            IgnoreCollision(interactable.Rigidbody, false);
        }

        public void ReleaseItem(Interactable interactable)
        {
            StartCoroutine(Release(interactable));
        }

        /// <summary>
        /// Ignore collision between the grabbed item and the hand
        /// </summary>
        /// <param name="rb"></param>
        /// <param name="ignore"></param>
        public void IgnoreCollision(Rigidbody rb, bool ignore)
        {
            foreach (var phalange in _handController.Phalanges)
            {
                var fingerCollider = phalange.GetComponent<Collider>();
                if (fingerCollider != null)
                {
                    PhysicsObject physicsObject = null;
                    if (PhysicsManager.Instance.GetPhysicsObject(rb.gameObject, out physicsObject))
                        foreach (var c in physicsObject.Colliders)
                            Physics.IgnoreCollision(fingerCollider, c, ignore);
                }
            }
        }

        /// <summary>
        /// Try to grab the given rigidbody
        /// </summary>
        /// <param name="interactable"></param>
        private void GrabItem(Interactable interactable)
        {
            if (_grabbedItem != null) return;
            PhysicsObject physicsObject = null;
            if (!PhysicsManager.Instance.GetPhysicsObject(interactable.gameObject, out physicsObject) || interactable.PhysicsLayer != PhysicsLayer.Grab)
                return;

            _grabbedItem = interactable;
            _grabbedItem.Attach(_handController.HandRigidbody, this);

            // Ignore collision between the grabbed object and the grabbing hand
            IgnoreCollision(physicsObject.Rigidbody, true);

            if (OnItemGrabbed != null)
                OnItemGrabbed(_grabbedItem.gameObject, DeviceType);
            VibrateHand();
        }

        /// <summary>
        /// Vibrate the glove
        /// </summary>
        private void VibrateHand()
        {
            Manus.ManusSetVibration(HandData.Instance.Session, DeviceType, 0.7f, 150);
        }
    }
}

