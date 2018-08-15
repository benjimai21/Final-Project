//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Editor for VRInput
//
//=============================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using Valve.VR;

namespace VRInteraction
{
	[CustomEditor(typeof(VRInput))]
	public class VRInputEditor : Editor {

		// target component
		public VRInput input = null;
		SerializedObject serializedInput;

		static bool editActionsFoldout;
		string newActionName = "";

		public virtual void OnEnable()
		{
			input = (VRInput)target;
			serializedInput = new SerializedObject(input);
		}

		public override void OnInspectorGUI()
		{
			serializedInput.Update();

			editActionsFoldout = EditorGUILayout.Foldout(editActionsFoldout, "Edit Actions");
			if (editActionsFoldout)
			{
				if (input.VRActions != null)
				{
					for(int i=0; i<input.VRActions.Length; i++)
					{
						EditorGUILayout.BeginHorizontal();
						input.VRActions[i] = EditorGUILayout.TextField(input.VRActions[i]);
						if (GUILayout.Button("X"))
						{
							string[] newActions = new string[input.VRActions.Length-1];
							int offset = 0;
							for(int j=0; j<newActions.Length; j++)
							{
								if (i == j) offset = 1;
								newActions[j] = input.VRActions[j+offset];
							}
							input.VRActions = newActions;

							if (input.triggerKey > i)
								input.triggerKey -= 1;
							else if (input.triggerKey == i)
								input.triggerKey = 0;
							if (input.padTop > i)
								input.padTop -= 1;
							else if (input.padTop == i)
								input.padTop = 0;
							if (input.padLeft > i)
								input.padLeft -= 1;
							else if (input.padLeft == i)
								input.padLeft = 0;
							if (input.padRight > i)
								input.padRight -= 1;
							else if (input.padRight == i)
								input.padRight = 0;
							if (input.padBottom > i)
								input.padBottom -= 1;
							else if (input.padBottom == i)
								input.padBottom = 0;
							if (input.padCentre > i)
								input.padCentre -= 1;
							else if (input.padCentre == i)
								input.padCentre = 0;
							if (input.gripKey > i)
								input.gripKey -= 1;
							else if (input.gripKey == i)
								input.gripKey = 0;
							if (input.menuKey > i)
								input.menuKey -= 1;
							else if (input.menuKey == i)
								input.menuKey = 0;
							if (input.aButtonKey > i)
								input.aButtonKey -= 1;
							else if (input.aButtonKey == i)
								input.aButtonKey = 0;

							if (input.triggerKeyOculus > i)
								input.triggerKeyOculus -= 1;
							else if (input.triggerKeyOculus == i)
								input.triggerKeyOculus = 0;
							if (input.padTopOculus > i)
								input.padTopOculus -= 1;
							else if (input.padTopOculus == i)
								input.padTopOculus = 0;
							if (input.padLeftOculus > i)
								input.padLeftOculus -= 1;
							else if (input.padLeftOculus == i)
								input.padLeftOculus = 0;
							if (input.padRightOculus > i)
								input.padRightOculus -= 1;
							else if (input.padRightOculus == i)
								input.padRightOculus = 0;
							if (input.padBottomOculus > i)
								input.padBottomOculus -= 1;
							else if (input.padBottomOculus == i)
								input.padBottomOculus = 0;
							if (input.padCentreOculus > i)
								input.padCentreOculus -= 1;
							else if (input.padCentreOculus == i)
								input.padCentreOculus = 0;
							if (input.gripKeyOculus > i)
								input.gripKeyOculus -= 1;
							else if (input.gripKeyOculus == i)
								input.gripKeyOculus = 0;
							if (input.menuKeyOculus > i)
								input.menuKeyOculus -= 1;
							else if (input.menuKeyOculus == i)
								input.menuKeyOculus = 0;
							if (input.aButtonKeyOculus > i)
								input.aButtonKeyOculus -= 1;
							else if (input.aButtonKeyOculus == i)
								input.aButtonKeyOculus = 0;

							EditorUtility.SetDirty(input);
							break;
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.BeginHorizontal();
				newActionName = EditorGUILayout.TextField(newActionName);
				GUI.enabled = (newActionName != "");
				if (GUILayout.Button("Add Action"))
				{
					string[] newActions = new string[1];
					if (input.VRActions != null) newActions = new string[input.VRActions.Length+1];
					else input.VRActions = new string[0];
					for(int i=0; i<newActions.Length; i++)
					{
						if (i == input.VRActions.Length)
						{
							newActions[i] = newActionName;
							break;
						}
						newActions[i] = input.VRActions[i];
					}
					input.VRActions = newActions;
					newActionName = "";
					EditorUtility.SetDirty(input);
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
			}

			if (input.VRActions == null)
			{
				serializedInput.ApplyModifiedProperties();
				return;
			}

			SerializedProperty triggerKey = serializedInput.FindProperty("triggerKey");
			SerializedProperty padTop = serializedInput.FindProperty("padTop");
			SerializedProperty padLeft = serializedInput.FindProperty("padLeft");
			SerializedProperty padRight = serializedInput.FindProperty("padRight");
			SerializedProperty padBottom = serializedInput.FindProperty("padBottom");
			SerializedProperty padCentre = serializedInput.FindProperty("padCentre");
			SerializedProperty gripKey = serializedInput.FindProperty("gripKey");
			SerializedProperty menuKey = serializedInput.FindProperty("menuKey");
			SerializedProperty aButtonKey = serializedInput.FindProperty("aButtonKey");

			GUIContent viveDisplayModeText = new GUIContent("Display Vive Buttons", "Or Oculus Buttons When Set To False");
			SerializedProperty displayViveButtons = serializedInput.FindProperty("displayViveButtons");
			displayViveButtons.boolValue = EditorGUILayout.Toggle(viveDisplayModeText, displayViveButtons.boolValue);

			GUIContent mirrorControlsText = new GUIContent("Mirror Controls", "If Set To False Will Seperate Oculus And Vive Controls");
			SerializedProperty mirrorControls = serializedInput.FindProperty("mirrorControls");
			mirrorControls.boolValue = EditorGUILayout.Toggle(mirrorControlsText, mirrorControls.boolValue);

			if (!mirrorControls.boolValue)
			{
				SerializedProperty triggerKeyOculus = serializedInput.FindProperty("triggerKeyOculus");
				SerializedProperty padTopOculus = serializedInput.FindProperty("padTopOculus");
				SerializedProperty padLeftOculus = serializedInput.FindProperty("padLeftOculus");
				SerializedProperty padRightOculus = serializedInput.FindProperty("padRightOculus");
				SerializedProperty padBottomOculus = serializedInput.FindProperty("padBottomOculus");
				SerializedProperty padCentreOculus = serializedInput.FindProperty("padCentreOculus");
				SerializedProperty gripKeyOculus = serializedInput.FindProperty("gripKeyOculus");
				SerializedProperty menuKeyOculus = serializedInput.FindProperty("menuKeyOculus");
				SerializedProperty aButtonKeyOculus = serializedInput.FindProperty("aButtonKeyOculus");

				int newTriggerKey = EditorGUILayout.Popup("Trigger Key", displayViveButtons.boolValue ? triggerKey.intValue : triggerKeyOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) triggerKey.intValue = newTriggerKey;
				else triggerKeyOculus.intValue = newTriggerKey;
				int newPadTop = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Up Key" : "Thumbstick Up"), displayViveButtons.boolValue ? padTop.intValue : padTopOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) padTop.intValue = newPadTop;
				else padTopOculus.intValue = newPadTop;
				int newPadLeft = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Left Key" : "Thumbstick Left"), displayViveButtons.boolValue ? padLeft.intValue : padLeftOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) padLeft.intValue = newPadLeft;
				else padLeftOculus.intValue = newPadLeft;
				int newPadRight = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Right Key" : "Thumbstick Right"), displayViveButtons.boolValue ? padRight.intValue : padRightOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) padRight.intValue = newPadRight;
				else padRightOculus.intValue = newPadRight;
				int newPadBottom = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Down Key" : "Thumbstick Down"), displayViveButtons.boolValue ? padBottom.intValue : padBottomOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) padBottom.intValue = newPadBottom;
				else padBottomOculus.intValue = newPadBottom;
				int newPadCentre = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Centre Key" : "Thumbstick Button"), displayViveButtons.boolValue ? padCentre.intValue : padCentreOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) padCentre.intValue = newPadCentre;
				else padCentreOculus.intValue = newPadCentre;
				int newGripKey = EditorGUILayout.Popup("Grip Key", displayViveButtons.boolValue ? gripKey.intValue : gripKeyOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) gripKey.intValue = newGripKey;
				else gripKeyOculus.intValue = newGripKey;
				int newMenuKey = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Menu Key" : "B/Y"), displayViveButtons.boolValue ? menuKey.intValue : menuKeyOculus.intValue, input.VRActions);
				if (displayViveButtons.boolValue) menuKey.intValue = newMenuKey;
				else menuKeyOculus.intValue = newMenuKey;
				if (!displayViveButtons.boolValue) aButtonKeyOculus.intValue = EditorGUILayout.Popup("A/X", aButtonKeyOculus.intValue, input.VRActions);

			} else
			{
				triggerKey.intValue = EditorGUILayout.Popup("Trigger Key", triggerKey.intValue, input.VRActions);
				padTop.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Up Key" : "Thumbstick Up"), padTop.intValue, input.VRActions);
				padLeft.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Left Key" : "Thumbstick Left"), padLeft.intValue, input.VRActions);
				padRight.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Right Key" : "Thumbstick Right"), padRight.intValue, input.VRActions);
				padBottom.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Down Key" : "Thumbstick Down"), padBottom.intValue, input.VRActions);
				padCentre.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Pad Centre Key" : "Thumbstick Button"), padCentre.intValue, input.VRActions);
				gripKey.intValue = EditorGUILayout.Popup("Grip Key", gripKey.intValue, input.VRActions);
				menuKey.intValue = EditorGUILayout.Popup((displayViveButtons.boolValue ? "Menu Key" : "B/Y"), menuKey.intValue, input.VRActions);
				if (!displayViveButtons.boolValue) aButtonKey.intValue = EditorGUILayout.Popup("A/X", aButtonKey.intValue, input.VRActions);
			}

			SerializedProperty debugMode = serializedInput.FindProperty("debugMode");
			debugMode.boolValue = EditorGUILayout.Toggle("Debug Mode", debugMode.boolValue);

			serializedInput.ApplyModifiedProperties();
		}
	}

}