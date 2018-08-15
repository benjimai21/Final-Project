using ManusVR.ManusInterface;
using UnityEditor;
using UnityEngine;

namespace Assets.ManusVR.VRToolkit
{
    [CustomEditor(typeof(PhysicsButton)), CanEditMultipleObjects]
    public class PhysicsButtonEditor : Editor
    {
        private SerializedProperty heightLimits;
        private SerializedProperty triggerHeight;

        void OnEnable()
        {
            heightLimits = serializedObject.FindProperty("HeightLimits");
            triggerHeight = serializedObject.FindProperty("TriggerDistance");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            //EditorGUILayout.PropertyField(heightLimits);
            heightLimits.vector2Value = EditorGUILayout.Vector2Field("MinMaxHeight", heightLimits.vector2Value);
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Tools/MyTool/Do It in C#")]
        static void DoIt()
        {
            EditorUtility.DisplayDialog("MyTool", "Do It in C# !", "OK", "");
        }
    }
}