using System.Collections;
using System.Collections.Generic;
using cakeslice;
using ManusVR.PhysicalInteraction;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public class InteractableItem : Interactable
    {
        /*
        public GameObject cortex;
        private Rigidbody rigid;
        */
        private bool touchObject = false;

        public ModelManipulator[] objects;

        private readonly HashSet<Outline> _outlines = new HashSet<Outline>();
        private float _grabDistance;
        private Vector3 _grabOffset;
        private bool _assistanceEnabled = false;

        private const int VelocityMultiplier = 500;
        private const int RequiredYForce = -1000;
        private const int MaxHorizontalForce = 500;
        

        /*
        private void Start() {
            
            rigid = cortex.GetComponent<Rigidbody>();
            
        }
    */

        protected override void Initialize(Collider[] colliders)
        {
            base.Initialize(colliders);
            
            // Add outlines to all of the colliders
            foreach (Collider child in colliders)
            {
                var outline = child.GetComponent<Outline>();
                if (outline == null && child.GetComponent<MeshRenderer>())
                    outline = child.gameObject.AddComponent<Outline>();
                if (outline != null)
                    _outlines.Add(outline);
            }
        }

        public override void Attach(Rigidbody connectedBody, ObjectGrabber hand)
        {
            base.Attach(connectedBody, hand);
            if (!Magnetic)
            {
                // Add a fixed joint to the object
                FixedJoint joint = GetComponent<FixedJoint>();
                Destroy(joint);
                joint = gameObject.AddComponent<FixedJoint>();

                // Attach the created joint to the hand
                joint.connectedBody = connectedBody;
                joint.anchor = Rigidbody.position - connectedBody.position;

                _connection = joint;
            }

            _grabDistance = Vector3.Distance(transform.position, Hand.HandRigidbody.position);
            _grabOffset = connectedBody.position - transform.position;
            Rigidbody.useGravity = GravityWhenGrabbed;

            StartCoroutine(AssistanceCheck(transform.position));

            if (HighlightWhenGrabbed)
                ActivateOutline(true);
        }

        /// <summary>
        /// Only enable the placement assistance when the object traveled a certain distance.
        /// Otherwise it would happen that the grabbed item gets dropped instantly.
        /// </summary>
        /// <param name="grabLocation"></param>
        /// <returns></returns>
        private IEnumerator AssistanceCheck(Vector3 grabLocation)
        {
            _assistanceEnabled = false;
            yield return new WaitUntil((() => Vector3.Distance(grabLocation, transform.position) > 0.1f));
            _assistanceEnabled = true;
        }

        void Update()
        {
            if (Hand == null) return;
            if (Magnetic)
                PushTowards(Hand.HandRigidbody);

            // release this object when it is to far away from the hand
            if (ReleaseOnDistance && (Vector3.Distance(transform.position, Hand.HandRigidbody.position) - _grabDistance) > ReleaseDistance)
                Hand.ReleaseItem(this);

            if (_connection == null) return;
            var withinValue = Mathf.Abs(_connection.currentForce.x) + Mathf.Abs(_connection.currentForce.z) < MaxHorizontalForce;
            if (PlacementAssistance && _assistanceEnabled && _connection.currentForce.y < RequiredYForce && withinValue)
                Hand.ReleaseItem(this);

        }

        /// <summary>
        /// Push the object towards the given target
        /// </summary>
        /// <param name="target"></param>
        /// 
        /// 
       private void PushTowards(Rigidbody target) 
        {
            var distance = transform.position + _grabOffset;
            var velocity = target.position - distance;
         

            Rigidbody.AddForce(velocity.normalized * Mathf.Sqrt(velocity.magnitude) * 10 / Time.deltaTime);
        }
        
        public override void Dettach(ObjectGrabber hand)
        {
            base.Dettach(hand);
            Rigidbody.useGravity = GravityWhenReleased;
            Rigidbody.isKinematic = KinematicWhenReleased;
            Destroy(_connection);
            if (HighlightWhenGrabbed)
                ActivateOutline(false);
        }

        internal void ActivateOutline(bool active)
        {
            foreach (var outline in _outlines)
                outline.enabled = active;
        }

        public bool GetTouch() {
            return touchObject;
        }

        public void UpdateTouch() {
            touchObject = false;

        }

        /// <summary>
        /// Active the outline when there is collision between the object and fingers
        /// </summary>
        /// <param name="collision"></param>
        protected override void CollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Hand") {
                print("touch");
                touchObject = true;
            }

            for (int j = 0; j < objects.Length; j++) {
                if (objects[j].GetDistance())
                    objects[j].ChangeZoneFistActivation(false);
            }


            //fix cortex model when an object enters in contact with it
            /*
            if (collision.gameObject.tag == "Cortex") {
                rigid.isKinematic = true;
                rigid.velocity = Vector3.zero;

            }
            */

            for (int i = 0; i < objects.Length; i++)
                objects[i].ChangeStatutStandard();

            base.CollisionEnter(collision);
            if (!HighlightOnImpact) return;
            foreach (var detector in _detectors)
            {
                if (detector.ValidCollisions <= 0) continue;
                ActivateOutline(true);
                return;
            }
        }

        /// <summary>
        /// Deactive the outline
        /// </summary>
        /// <param name="collision"></param>
        protected override void CollisionExit(Collision collision)
        {
            base.CollisionExit(collision);

            if (Hand != null || !HighlightOnImpact) return;
            foreach (var detector in _detectors)
                if (detector.ValidCollisions > 0)
                    return;

            ActivateOutline(false);
        }
    }
}



