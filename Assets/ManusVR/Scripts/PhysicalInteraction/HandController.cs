using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public class HandController : MonoBehaviour
    {
        public HashSet<Phalange> Phalanges { get { return _phalanges; } }
        private readonly HashSet<Phalange> _phalanges = new HashSet<Phalange>();
        private readonly HingeJoint[][] _joints = new HingeJoint[5][];

        private readonly Transform[][] _positionHolders = new Transform[5][];
        private readonly List<CollisionDetector> _thumbDetectors = new List<CollisionDetector>();

        public Rigidbody HandRigidbody { get { return _handRigidbody; } }
        private Rigidbody _handRigidbody;
        private ObjectGrabber _grabber;


        private JointMotor _motor;
        private Rigidbody _thumbRigidbody;
        public device_type_t device_type;

        public Transform[][] GameTransforms { get; private set; }

        [Header("Hand physic setting")]
        [Tooltip("How much force will be applied when the hand is colliding with a object (More force makes it unstable")]
        [Range(0, 5000f)] public float HandForce = 100;

        public bool KinematicWhenNotColliding = true;
        private Transform[][] _modelTransforms;

        [Range(0, 50000)] [Tooltip("The amount of force on the fingers")] public int MotorForce = 1000;

        [Range(0, 1000)] [Tooltip("How much time will the target velocity be multiplied")]
        [Header("Finger physic settings")] public int MotorVelocity = 30;

        public Transform RootTranform;

        public Transform Target;
        public Transform TargetUnderArm;

        public Transform Thumb;

        private CharacterJoint _thumbJoint;
        public float ThumbMovementSpeed;
        public Rigidbody UnderArm;

        public Action<PhalangeData, Collision, CollisionType> CollisionEntered;

        /// <summary>
        /// How many of the phalanges are currently colliding with a object
        /// </summary>
        public int AmountOfCollidingPhalanges
        {
            get { return _phalanges.Count(phalange => phalange.Detector.IsColliding); }
        }

        /// <summary>
        /// Is the thumb of this hand currently colliding with a object
        /// </summary>
        public bool IsThumbColliding
        {
            get { return _thumbDetectors.Count(detector => detector.IsColliding) > 0; }
        }

        // Use this for initialization
        private void Start()
        {
            Application.runInBackground = true;

            string[] fingers =
            {
                "thumb_0",
                "index_0",
                "middle_0",
                "ring_0",
                "pinky_0"
            };

            // Associate the game transforms with the skeletal model.
            GameTransforms = new Transform[5][];
            _modelTransforms = new Transform[5][];
            for (var i = 0; i < 5; i++)
            {
                GameTransforms[i] = new Transform[4];
                _modelTransforms[i] = new Transform[4];
                for (var j = 1; j < 4; j++)
                {
                    var postfix = device_type == device_type_t.GLOVE_LEFT ? "_l" : "_r";
                    var finger = fingers[i] + j + postfix;
                    GameTransforms[i][j] = FindDeepChild(RootTranform, finger);
                }
            }

            // Initializing the hand
            _handRigidbody = RootTranform.GetComponent<Rigidbody>();
            Phalange handPhalange = _handRigidbody.gameObject.AddComponent<Phalange>();
            handPhalange.PhalangeData = new PhalangeData(0, 0, device_type);
            _phalanges.Add(_handRigidbody.GetComponent<Phalange>());

            GameObject phalangeParent = new GameObject("Phalanges");
            ConvertFingersToPhysics(phalangeParent.transform);
            ReleaseHand();
        
            foreach (var phalange in _phalanges)
                phalange.CollisionEntered += PhalangeCollisionEntered;

            //Prototype
            var grabbers = GetComponents<ObjectGrabber>();
            foreach (var grabber in grabbers)
                if (grabber.DeviceType == device_type)
                    _grabber = grabber;

        }

        /// <summary>
        /// Happens when a phalange has collision with an cetrain object
        /// </summary>
        /// <param name="finger">The finger that is colliding</param>
        /// <param name="index">The index of this finger</param>
        /// <param name="collision">All of the collision data</param>
        private void PhalangeCollisionEntered(PhalangeData phalangeData, Collision collision, CollisionType type)
        {
            if (CollisionEntered != null)
                CollisionEntered(phalangeData, collision, type);
        }

        /// <summary>
        ///     Convert the static fingers to physical fingers
        /// </summary>
        private void ConvertFingersToPhysics(Transform phalangeParent)
        {
            for (var index = 0; index < 5; index++)
            {
                _positionHolders[index] = new Transform[4];
                _joints[index] = new HingeJoint[4];
                for (var phalange = 1; phalange < 4; phalange++)
                {
                    // Create a copy of the transform
                    _positionHolders[index][phalange] = CopyTransform(GameTransforms[index][phalange]);
                    _positionHolders[index][phalange].name = "Positionholder" + index + phalange;
                    // Custom localrotation for the index, middel, pink en ring finger
                    if (index < 4 && phalange != 1)
                        _positionHolders[index][phalange].localRotation = Quaternion.Euler(new Vector3(0, 0
                            , 0));
                    else
                        _positionHolders[index][phalange].localRotation = Quaternion.Euler(new Vector3(0, 0
                            , _positionHolders[index][phalange].localRotation.eulerAngles.z));

                    // Unparent the phalangeIndex 
                    GameTransforms[index][phalange].parent = phalangeParent;

                    // When it is a thumb
                    if (index == 0 && phalange == 1)
                    {
                        // todo code cleaning
                        _thumbRigidbody = GameTransforms[index][phalange].gameObject.AddComponent<Rigidbody>();
                        _thumbRigidbody.useGravity = false;
                        _thumbJoint = GameTransforms[index][phalange].gameObject.AddComponent<CharacterJoint>();
                        _thumbJoint.connectedBody = RootTranform.GetComponent<Rigidbody>();

                        var limit = new SoftJointLimit();
                        limit.limit = 170;

                        _thumbJoint.highTwistLimit = limit;
                        _thumbJoint.swing1Limit = limit;
                        _thumbJoint.swing2Limit = limit;

                        limit.limit = -170;
                        _thumbJoint.lowTwistLimit = limit;

                        _thumbRigidbody.isKinematic = true;

                        continue;
                    }

                    // Get the rigidbody where it should connect to
                    Rigidbody connectedBody;
                    if (phalange == 1) connectedBody = RootTranform.GetComponent<Rigidbody>();
                    else connectedBody = GameTransforms[index][phalange - 1].GetComponent<Rigidbody>();

                    // Add a hingejoint to the gameobject
                    if (index == 0)
                        _joints[index][phalange] = AddHingeJoint(GameTransforms[index][phalange].gameObject,
                            -RootTranform.right, connectedBody);
                    else
                        _joints[index][phalange] = AddHingeJoint(GameTransforms[index][phalange].gameObject,
                            RootTranform.forward, connectedBody);


                    ConfigurePhalange(GameTransforms[index][phalange].gameObject, phalange, (Finger)index);

                    if (index == 0)
                        _thumbDetectors.Add(GameTransforms[index][phalange].GetComponent<CollisionDetector>());

                    ChangeJointLimit(_joints[index][phalange], -150, 150);
                }
            }
        }

        private void ConfigurePhalange(GameObject gameObject, int phalangeIndex, Finger finger)
        {
            Phalange phalange =  gameObject.AddComponent<Phalange>();
            _phalanges.Add(phalange);
            phalange.PhalangeData = new PhalangeData(finger, phalangeIndex, device_type);
        }

        /// <summary>
        ///     Change the jointlimit on a give Hingjoint
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        private void ChangeJointLimit(HingeJoint joint, float minLimit, float maxLimit)
        {
            var limit = new JointLimits();
            limit.min = minLimit;
            limit.max = maxLimit;
            joint.limits = limit;
        }

        /// <summary>
        ///     Add a hingejoint to the gameobject and provide it with the needed settings
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="axis"></param>
        /// <param name="connectedBody"></param>
        /// <returns></returns>
        private HingeJoint AddHingeJoint(GameObject gameObject, Vector3 axis, Rigidbody connectedBody)
        {
            var joint = gameObject.GetComponent<HingeJoint>();
            if (joint != null)
            {
                Debug.LogWarning(gameObject.name + " already has a hingejoint attached to it");
                return joint;
            }
            joint = gameObject.AddComponent<HingeJoint>();

            joint.useLimits = true;
            joint.axis = axis;

            joint.connectedBody = connectedBody;
            joint.useMotor = true;

            return joint;
        }

        /// <summary>
        ///     Create a copy of the given transform
        /// </summary>
        /// <returns></returns>
        private Transform CopyTransform(Transform transf)
        {
            var newTransform = new GameObject().transform;
            newTransform.parent = transf.parent;
            newTransform.localPosition = transf.localPosition;
            newTransform.localRotation = transf.localRotation;

            return newTransform;
        }

        private void ReleaseHand()
        {
            RootTranform.parent = null;
            RootTranform.position = Target.position;
        }

        /// <summary>
        ///     Finds a deep child in a transform
        /// </summary>
        /// <param name="aParent">Transform to be searched</param>
        /// <param name="aName">Name of the (grand)child to be found</param>
        /// <returns></returns>
        private static Transform FindDeepChild(Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = FindDeepChild(child, aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void FixedUpdate()
        {
            const float wristRotationSpeed = 10;
            if (Target != null)
                MoveBody(Target, _handRigidbody);

            // Rotate the underarm
            UnderArm.MoveRotation(Quaternion.RotateTowards(UnderArm.rotation, TargetUnderArm.rotation, wristRotationSpeed));
            UnderArm.MovePosition(TargetUnderArm.position);

            // Rotate the hand
            _handRigidbody.MoveRotation(Quaternion.RotateTowards(_handRigidbody.rotation, Target.rotation,
                wristRotationSpeed));
            UpdateFingers();

            // Clamp the maximum velocity of the hand an phalanges
            //ClampVelocity(HandForce, HandForce, HandForce, HandForce);
        }

        private void MoveBody(Transform target, Rigidbody body)
        {
            if (_grabber.GrabbedItem == null && AmountOfCollidingPhalanges <= 0)
                body.isKinematic = true;
            else if (_grabber != null && AmountOfCollidingPhalanges <= 0 &&
                     _grabber.GrabbedItem.TotalCollidingObjects <= 0)
                body.isKinematic = true;
            else
            {
                body.isKinematic = false;
            }

            // Move the hand when it is kinematic
            if (body.isKinematic)               
                body.MovePosition(Vector3.MoveTowards(body.position, target.position, 0.035f));

            // If the hand is not kinematic
            else
            {
                // Predict the target velocity
                var targetVelocity = (target.position - body.position) * 5;

                targetVelocity = Vector3.ClampMagnitude(targetVelocity * Mathf.Sqrt(targetVelocity.magnitude), 1);
                body.velocity = targetVelocity * HandForce;
            }
        }

       

        private void SetDetectCollision(bool detect)
        {
            foreach (var phalange in _phalanges)
                phalange.Detector.Rigidbody.detectCollisions = detect;
            _handRigidbody.detectCollisions = detect;
        }

        private IEnumerator SwitchKinematic(Transform target, Rigidbody body, float minimumDistance)
        {
            SetDetectCollision(false);
            body.isKinematic = true;
            // Turn the physics back on when the hand has reached the target location
            yield return new WaitUntil(() => Vector3.Distance(body.position, target.position) < minimumDistance);
            SetDetectCollision(true);
            body.isKinematic = false;
        }

        /// <summary>
        ///     Cap the velocity and angular velocity on this hand when it is colliding
        /// </summary>
        private void ClampVelocity(float maxFingerLinear, float maxFingerAngular, float maxHandLinear, float maxHandAngular)
        {
            foreach (var phalange in _phalanges)
            {
                VelocityClamper.ClampVelocity(phalange.Detector.Rigidbody, maxFingerLinear);
                VelocityClamper.ClampAngularVelocity(phalange.Detector.Rigidbody, maxFingerAngular);
            }
            VelocityClamper.ClampVelocity(_handRigidbody, maxHandLinear);
            VelocityClamper.ClampAngularVelocity(_handRigidbody, maxHandAngular);
            VelocityClamper.ClampVelocity(UnderArm, maxHandLinear);
            VelocityClamper.ClampAngularVelocity(UnderArm, maxHandAngular);
        }

        private void RotateThumb()
        {
            if (Thumb != null)
            {
                var targetDelta = Thumb.position + Thumb.right - _thumbJoint.transform.position;

                //get the angle between transform.forward and target delta
                var angleDiff = Vector3.Angle(_thumbJoint.transform.right, targetDelta);

                // get its cross product, which is the axis of rotation to
                var cross = Vector3.Cross(_thumbJoint.transform.right, targetDelta);

                // set the angular velocity
                //TODO find cleaner solution for reseting the state of the thumb
                _thumbRigidbody.isKinematic = false;
                _thumbRigidbody.angularVelocity = cross * (float) Math.Sqrt(angleDiff) * ThumbMovementSpeed;

                int dir;
                float angle;
                // Rotate the thumb
                dir = OffsetDirection(_thumbJoint.transform.right, _thumbJoint.transform.forward, Thumb.forward, out angle);
                _thumbRigidbody.AddRelativeTorque(new Vector3(dir * angle * ThumbMovementSpeed, 0, 0));
            }
        }

        /// <summary>
        ///     Check if something is moving towards or away from a calculated offset angle
        /// </summary>
        /// <param name="n">Normal</param>
        /// <param name="from">Form</param>
        /// <param name="to">to</param>
        /// <param name="angle">The angle between from and to</param>
        /// <returns></returns>
        private int OffsetDirection(Vector3 n, Vector3 from, Vector3 to, out float angle)
        {
            var offsetAngle = Vector3.Angle(Quaternion.AngleAxis(1, n) * from, to);
            angle = Vector3.Angle(from, to);

            return angle > offsetAngle ? 1 : -1;
        }

        /// <summary>
        ///     Get the angle between two given vectors with a positive and negative offset
        /// </summary>
        /// <param name="n">Normal</param>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="offset"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private float OffsetAngleBetween(Vector3 n, Vector3 from, Vector3 to, float offset, out int dir)
        {
            var offsetAngle1 = Vector3.Angle(Quaternion.AngleAxis(offset + 0.01f, n) * from, to);
            var offsetAngle2 = Vector3.Angle(Quaternion.AngleAxis(offset - 0.01f, n) * from, to);
            dir = offsetAngle1 > offsetAngle2 ? 1 : -1;

            return Vector3.Angle(Quaternion.AngleAxis(offset, n) * from, to);
        }

        private void UpdateFingers()
        {
            // Update the motor of each finger
            RotateThumb();
            UpdateMotorVelocity(Finger.thumb);
            UpdateMotorVelocity(Finger.index);
            UpdateMotorVelocity(Finger.middle);
            UpdateMotorVelocity(Finger.ring);
            UpdateMotorVelocity(Finger.pink);
        }

        /// <summary>
        ///     Change the motor on the hinge joint
        /// </summary>
        /// <param name="finger"></param>
        private void UpdateMotorVelocity(Finger finger)
        {
            int index = (int) finger;
            for (var phalange = 1; phalange < 4; phalange++)
            {
                if (index == 0 && phalange == 1)
                    continue;
                var fingerEulerRot = HandData.Instance.GetFingerRotation(finger, device_type, phalange).eulerAngles;
                int dir;
                float angle;
                if (index == 0)
                {
                    angle = OffsetAngleBetween(_positionHolders[index][phalange].forward, _positionHolders[index][phalange].up
                        , GameTransforms[index][phalange].up, fingerEulerRot.z, out dir);

                    // rotate the direction if this is the left glove              
                    if (device_type == device_type_t.GLOVE_LEFT) dir = dir * -1;
                }
                else
                {
                    // Check what way the phalangeIndex should move towards
                    var q1 = Quaternion.Euler(0, fingerEulerRot.y + 0.01f, 0);
                    var q2 = Quaternion.Euler(0, fingerEulerRot.y - 0.01f, 0);

                    var angle1 = Quaternion.Angle(_positionHolders[index][phalange].rotation * q1,
                        GameTransforms[index][phalange].rotation);
                    var angle2 = Quaternion.Angle(_positionHolders[index][phalange].rotation * q2,
                        GameTransforms[index][phalange].rotation);

                    dir = angle1 > angle2 ? 1 : -1;

                    var sensorRot = Quaternion.Euler(0, fingerEulerRot.y, 0);
                    angle = Quaternion.Angle(_positionHolders[index][phalange].rotation * sensorRot,
                        GameTransforms[index][phalange].rotation);
                }

                // set the velocity and force to the motor of the hinge joint
                var joint = _joints[index][phalange];
                _motor.targetVelocity = Mathf.Sqrt(angle) * MotorVelocity * dir;
                _motor.force = MotorForce;
                joint.motor = _motor;
            }
        }
    }
}