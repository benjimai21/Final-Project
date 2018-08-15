using System.Collections;
using System.Collections.Generic;
using ManusVR.PhysicalInteraction;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ManusVR.ManusInterface
{
    public class PhysicsSlider : Slider
    {
        [SerializeField, HideInInspector]
        public Vector2 MinMaxMovement;

        private Vector3 _minPosition, _maxPosition;
        private Rigidbody _rb;

        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR
            if (Application.isPlaying && GetComponent<RectTransform>() != null)
            {
                Debug.LogError("Object with PhysicsSlider component can't be part of canvas. Please use the InterfaceSlider component instead!");
                EditorApplication.isPlaying = false;
            }
#endif
            _rb = GetComponent<Rigidbody>();
            _minPosition = transform.localPosition;
            _minPosition.z += MinMaxMovement.x;
            _maxPosition = transform.localPosition;
            _maxPosition.z += MinMaxMovement.y;
        }

        void FixedUpdate()
        {
            if (transform.localPosition.z <= _minPosition.z)
            {
                _rb.isKinematic = true;
                transform.localPosition = _minPosition;
                StartCoroutine(IgnoreCollisionWhileColliding());
            }
            if (transform.localPosition.z >= _maxPosition.z)
            {
                _rb.isKinematic = true;
                transform.localPosition = _maxPosition;
                StartCoroutine(IgnoreCollisionWhileColliding());
            }
        }

        IEnumerator IgnoreCollisionWhileColliding()
        {
            var interactable = GetComponent<Interactable>();
            while (interactable.TotalCollidingObjects != 0)
            {
                yield return new WaitForFixedUpdate();
            }
            _rb.isKinematic = false;
        }

        protected override float GetCurrentInverseLerpValue()
        {
            return Mathf.InverseLerp(_minPosition.z, _maxPosition.z, transform.localPosition.z);
        }

        protected override void SetSliderPosition(float value)
        {
            var localPos = transform.localPosition;
            localPos.z = Mathf.Lerp(_minPosition.z, _maxPosition.z,
                Mathf.InverseLerp(MinMaxValue.x, MinMaxValue.y, value));

            transform.localPosition = localPos;
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying || MinMaxMovement == null)
                return;

            var bottomLimit = transform.localPosition;
            bottomLimit.y = 0;
            bottomLimit.z += MinMaxMovement.x;

            var upperLimit = transform.localPosition;
            upperLimit.y = 0;
            upperLimit.z += MinMaxMovement.y;


            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.TransformPoint(bottomLimit), transform.TransformPoint(upperLimit));
        }
    }
}
