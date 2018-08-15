using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ManusVR.Scripts;
using ManusVR;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace ManusVR.PhysicalInteraction
{
    public enum PhysicsLayer
    {
        Phalange,       // Parts of the hand
        Grab,           // Objects that can be pushed and grabbed
        Push,           // Objects that can be pushed
        UI              

    }

    public class PhysicsObject
    {
        public Collider[] Colliders;
        public Rigidbody Rigidbody;
        public PhysicsLayer PhysicsLayer;
        public GameObject GameObject;

        public PhysicsObject(Rigidbody rigidbody, Collider[] colliders, PhysicsLayer layer)
        {
            Rigidbody = rigidbody;
            Colliders = colliders;
            PhysicsLayer = layer;
            GameObject = rigidbody.gameObject;
        }
    }

    public class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager Instance;
        public Action<PhalangeData, Collision, CollisionType> OnHandCollision;
        public Action<PhysicsObject, device_type_t> OnGrabbedItem;

        public PhysicsObject[] GetPhysicsObjects { get { return _physicsObjects.Values.ToArray(); } }
        private readonly Dictionary<Transform, PhysicsObject> _physicsObjects = new Dictionary<Transform, PhysicsObject>();

        private Dictionary<Transform, Transform> _childs = new Dictionary<Transform, Transform>();

        
        [Tooltip(
            "How much times will the velocity of the hand be multiplied, higher force means more realistic but unstable behaviour")]
        [Range(0, 1000)] public int HandVelocityMultiplier = 100;

        [Header("The rigidbodys of the physical underarms")]
        public Rigidbody LeftRootUnderarm;
        public Rigidbody RightRootUnderarm;

        [Header("Phalange settings")] [Tooltip("The velocity multiplier of a phalange (100 is recommended)")]
        [Range(0, 300)] public int PhalangeVelocity = 100;
        [Tooltip("The force of the phalanges (20 is recommended)")] [Range(0, 100)] public int PhalangeForce = 20;

        [Tooltip("When the grabbed object is this far away from the hand then release it")] [Range(0, 2)]
        public float ReleaseDistance = 0.1f;

        [Tooltip("Should the physical hands be visible")] public bool ShowPhysicalArms = false;

        [Tooltip("How fast should the physical thumb move towards the target thumb")] [Range(0, 200)]
        public int ThumbMovementSpeed = 100;

        [Header("Visual settings")]
        public SkinnedMeshRenderer VisualArms;
        public SkinnedMeshRenderer PhysicalArms;

        public Transform VisualBody;

        /// <summary>
        ///     Register a physics object to the physics manager.
        /// </summary>
        /// <param name="colliders">All of the colliders on the given object</param>
        /// <param name="rigidbody"></param>
        public void Register(Collider[] colliders, Rigidbody rigidbody, PhysicsLayer type)
        {
            if (rigidbody == null)
                Debug.LogError("Rigidbody can not be null");

            if (!_physicsObjects.ContainsKey(rigidbody.transform))
                _physicsObjects.Add(rigidbody.transform,
                    new PhysicsObject(rigidbody, colliders, type));

            foreach (var collider in colliders)
                RegisterChild(collider.gameObject, rigidbody.gameObject);
        }

        public void Remove(Collider[] colliders, Rigidbody rigidbody)
        {
            _physicsObjects.Remove(rigidbody.transform);
            foreach (var collider in colliders)
                _childs.Remove(collider.transform);
        }

        /// <summary>
        /// Register a child gameobject as physics object
        /// </summary>
        /// <param name="child"></param>
        /// <param name="customRoot"></param>
        private void RegisterChild(GameObject child, GameObject customRoot)
        {
            if (!_childs.ContainsKey(child.transform))
                _childs.Add(child.transform, customRoot.transform);
        }

        /// <summary>
        ///     Check if the given GameObject is registered with the physics manager
        /// </summary>
        /// <param name="root">Th</param>
        /// <returns></returns>
        public bool ContainsPhysicsObject(GameObject gameObject)
        {
            return _physicsObjects.ContainsKey(gameObject.transform.root);
        }

        /// <summary>
        ///     Process the collision between objects and check if it should be ignored
        /// </summary>
        /// <param name="objectCollider"></param>
        /// <param name="collision"></param>
        public bool ProcessCollision(Collider objectCollider, Collision collision)
        {
            PhysicsObject physicsObject = null;
            if (!GetPhysicsObject(collision.gameObject, out physicsObject)
                // Also ignore collision if both of them are phalanges
                || physicsObject.PhysicsLayer == PhysicsLayer.Phalange
                && _physicsObjects[objectCollider.transform].PhysicsLayer == PhysicsLayer.Phalange)
            {
                Physics.IgnoreCollision(objectCollider, collision.collider);
                return false;
            }
            return true;

        }

        /// <summary>
        ///     Get the beloning physics object
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="physicsObject"></param>
        /// <returns></returns>
        public bool GetPhysicsObject(GameObject gameObject, out PhysicsObject physicsObject)
        {
            if (_physicsObjects.ContainsKey(gameObject.transform))
            {
                physicsObject = _physicsObjects[gameObject.transform];
                return true;
            }

            if (_childs.ContainsKey(gameObject.transform))
            {
                physicsObject = _physicsObjects[_childs[gameObject.transform]];
                return true;
            }

            physicsObject = null;
            return false;
        }

        public PhysicsObject[] GetPhysicsObjectsWithLayer(PhysicsLayer physicsLayer)
        {
            return _physicsObjects.Values.Where(physicsObject => physicsObject.PhysicsLayer == physicsLayer).ToArray();
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (PhysicsPreferences.ShouldPromptFixedTimestep && Time.fixedDeltaTime > PhysicsPreferences.SuggestedTimestep)
                {
                    if (EditorUtility.DisplayDialog("Incorrect Timestep Settings",
                        "We've found that setting the Fixed Timestep to at least " + PhysicsPreferences.SuggestedTimestep +
                        " results in the most accurate physics interactions. \n\n" +
                        "Consider adjusting the Fixed Timestep settings in: Edit/Project Settings/Time.",
                        "Don't Remind Me Again!",
                        "I Will,\n Copy Value To My Clipboard!"))
                    {
                        PhysicsPreferences.ShouldPromptFixedTimestep = false;
                    }
                    else
                    {
                        EditorGUIUtility.systemCopyBuffer = PhysicsPreferences.SuggestedTimestep.ToString();
                    }
                    EditorApplication.isPlaying = false;
                }
                else if (PhysicsPreferences.ShouldPromptGravitySettings && !FloatComparer.AreEqual(Physics.gravity.y, PhysicsPreferences.SuggestedGravityForce, 0.0001f))
                {
                    if (EditorUtility.DisplayDialog("Gravity Settings",
                        "We've found that setting the gravity to " + PhysicsPreferences.SuggestedGravityForce +
                        " results in the most accurate physics interactions. \n\n" +
                        "Consider adjusting your gravity settings in: Edit/Project Settings/Physics.",
                        "Don't Remind Me Again!",
                        "I Will,\n Copy Value To My Clipboard!"))
                    {
                        PhysicsPreferences.ShouldPromptGravitySettings = false;
                    }
                    else
                    {
                        EditorGUIUtility.systemCopyBuffer = PhysicsPreferences.SuggestedGravityForce.ToString();
                    }
                    EditorApplication.isPlaying = false;
                }
                else if (PhysicsPreferences.ShouldPromptDefaultSolverIterations && Physics.defaultSolverIterations != PhysicsPreferences.SuggestedDefaultSolverIterations)
                {
                    if (EditorUtility.DisplayDialog("Default Solver Iterations",
                        "We've found that setting the amount of Default Solver Iterations to " + PhysicsPreferences.SuggestedDefaultSolverIterations +
                        " results in the most accurate physics interactions. \n" +
                        "Consider adjusting your Default Solver Iterations Settings in: Edit/Project Settings/Physics.",
                        "Don't Remind Me Again!",
                        "I Will,\n Copy Value To My Clipboard!"))
                    {
                        PhysicsPreferences.ShouldPromptDefaultSolverIterations = false;
                    }
                    else
                    {
                        EditorGUIUtility.systemCopyBuffer = PhysicsPreferences.SuggestedDefaultSolverIterations.ToString();
                    }
                    EditorApplication.isPlaying = false;
                }
                else if (PhysicsPreferences.ShouldPromptDefaultSolverVelocityIterations && Physics.defaultSolverVelocityIterations != PhysicsPreferences.SuggestedDefaultSolverVelocityIterations)
                {
                    if (EditorUtility.DisplayDialog("Default Solver Velocity Iterations",
                        "We've found that setting the amount of Default Solver Velocity Iterations to " + PhysicsPreferences.SuggestedDefaultSolverVelocityIterations +
                        " results in the most accurate physics interactions. \n" +
                        "Consider adjusting your Default Solver Velocity Iterations Settings in: Edit/Project Settings/Physics.",
                        "Don't Remind Me Again!",
                        "I Will,\n Copy Value To My Clipboard!"))
                    {
                        PhysicsPreferences.ShouldPromptDefaultSolverVelocityIterations = false;
                    }
                    else
                    {
                        EditorGUIUtility.systemCopyBuffer = PhysicsPreferences.SuggestedDefaultSolverVelocityIterations.ToString();
                    }
                    EditorApplication.isPlaying = false;
                }
            }
#endif
            if (Instance == null)
                Instance = this;

            Initialize(device_type_t.GLOVE_LEFT, FindDeepChild(transform, "hand_l"), FindDeepChild(VisualBody, "hand_l"), FindDeepChild(VisualBody, "thumb_01_l"),
                FindDeepChild(VisualBody, "lowerarm_l"), LeftRootUnderarm);
            Initialize(device_type_t.GLOVE_RIGHT, FindDeepChild(transform, "hand_r"), FindDeepChild(VisualBody, "hand_r"), FindDeepChild(VisualBody, "thumb_01_r"),
                FindDeepChild(VisualBody, "lowerarm_r"), RightRootUnderarm);

            PhysicalArms.enabled = ShowPhysicalArms;
        }

        private void Initialize(device_type_t deviceType, Transform root, Transform target, Transform thumbTransform,
            Transform targetUnderArm, Rigidbody underArmRb)
        {
            // Initialize visual body
            Rigidbody rb = target.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            SphereCollider collider = target.gameObject.AddComponent<SphereCollider>();
            collider.center = new Vector3(0, -0.12f, -0.03f);
            collider.radius = 0.06f;
            collider.isTrigger = true;
            target.gameObject.AddComponent<TriggerBinder>();

            var controller = gameObject.AddComponent<HandController>();
            controller.device_type = deviceType;
            controller.MotorVelocity = PhalangeVelocity;
            controller.MotorForce = PhalangeForce;
            controller.HandForce = HandVelocityMultiplier;

            controller.RootTranform = root;
            controller.Target = target;
            controller.Thumb = thumbTransform;
            controller.ThumbMovementSpeed = ThumbMovementSpeed;
            controller.UnderArm = underArmRb;
            controller.TargetUnderArm = targetUnderArm;

            controller.CollisionEntered += HandCollisionEntered;

            var grabber = gameObject.AddComponent<ObjectGrabber>();
            grabber.DeviceType = deviceType;
            grabber.TriggerBinder = target.GetComponent<TriggerBinder>();

            grabber.OnItemGrabbed += GrabbedItem;
        }

        private void HandCollisionEntered(PhalangeData data, Collision collision, CollisionType type)
        {
            if (OnHandCollision != null)
                OnHandCollision(data, collision, type);
        }

        private void GrabbedItem(GameObject gameObject, device_type_t type)
        {
            if (OnGrabbedItem == null) return;
            PhysicsObject physicsObject = null;
            GetPhysicsObject(gameObject, out physicsObject);
            OnGrabbedItem(physicsObject, type);
        }

        /// <summary>
        /// Finds a deep child in a transform
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
    }
}