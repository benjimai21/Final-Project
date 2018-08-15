//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Editor for ArcTeleporter
//
//=============================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace VRInteraction
{
	[CustomEditor(typeof(ArcTeleporter))]
	public class ArcTeleporterEditor : Editor
	{
		// target component
		public ArcTeleporter teleporter = null;
		SerializedObject serializedTeleporter;
		static bool raycastLayerFoldout = false;
		int raycastLayersSize = 0;
		static bool tagsFoldout = false;
		int tagsSize = 0;

		public virtual void OnEnable()
		{
			teleporter = (ArcTeleporter)target;
			serializedTeleporter = new SerializedObject(teleporter);
			if (teleporter.raycastLayer != null)
				raycastLayersSize = teleporter.raycastLayer.Count;
			else raycastLayersSize = 0;
			if (teleporter.tags != null)
				tagsSize = teleporter.tags.Count;
			else tagsSize = 0;

			VRInput input = teleporter.GetComponent<VRInput>();
			if (input.VRActions == null) SetDefaultActions(input);
		}

		private void SetDefaultActions(VRInput input)
		{
			string[] premadeActions = {"NONE", "SHOW", "TELEPORT"};
			input.triggerKey = 2;
			input.triggerKeyOculus = 2;
			input.padTop = 1;
			input.padTopOculus = 0;
			input.padLeft = 1;
			input.padLeftOculus = 0;
			input.padRight = 1;
			input.padRightOculus = 0;
			input.padBottom = 1;
			input.padBottomOculus = 0;
			input.padCentre = 1;
			input.padCentreOculus = 0;
			input.gripKey = 0;
			input.gripKeyOculus = 0;
			input.menuKey = 0;
			input.menuKeyOculus = 0;
			input.aButtonKey = 0;
			input.aButtonKeyOculus = 1;
			input.VRActions = premadeActions;
			EditorUtility.SetDirty(input);
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("SHOW: Shows and hides the arc when on two button mode.\nTELEPORT: Activates teleport when showing arc in two button mode or teleports on press and release mode", MessageType.Info);

			VRInput input = teleporter.GetComponent<VRInput>();
			if (GUILayout.Button("Reset Actions To Teleporter Default"))
			{
				SetDefaultActions(input);
			}

			serializedTeleporter.Update();

			SerializedProperty controlScheme = serializedTeleporter.FindProperty("controlScheme");
			controlScheme.intValue = (int)(ArcTeleporter.ControlScheme)EditorGUILayout.EnumPopup("Control Scheme", (ArcTeleporter.ControlScheme)controlScheme.intValue);
			ArcTeleporter.ControlScheme controlSchemeEnum = (ArcTeleporter.ControlScheme)controlScheme.intValue;

			if (controlSchemeEnum == ArcTeleporter.ControlScheme.PRESS_AND_RELEASE)
			{
				int showIndex = -1;
				for(int i=0; i<input.VRActions.Length; i++)
				{
					if (input.VRActions[i] == "SHOW")
					{
						showIndex = i;
						break;
					}
				}
				if (showIndex != -1)
				{
					if (input.triggerKey == showIndex || input.padCentre == showIndex || input.padTop ==showIndex ||
						input.padLeft == showIndex || input.padRight == showIndex || input.padBottom == showIndex ||
						input.gripKey == showIndex || input.menuKey == showIndex || input.aButtonKey == showIndex)
					{
						EditorGUILayout.HelpBox("The Show button has no effect when in press and release control scheme", MessageType.Warning);
					}
				}
			}

			SerializedProperty transition = serializedTeleporter.FindProperty("transition");
			transition.intValue = (int)(ArcTeleporter.Transition)EditorGUILayout.EnumPopup("Transition", (ArcTeleporter.Transition)transition.intValue);
			ArcTeleporter.Transition transitionEnum = (ArcTeleporter.Transition)transition.intValue;
			EditorGUI.indentLevel++;
			switch(transitionEnum)
			{
			case ArcTeleporter.Transition.FADE:
				SerializedProperty fadeMat = serializedTeleporter.FindProperty("fadeMat");
				fadeMat.objectReferenceValue = EditorGUILayout.ObjectField("Fade Material", fadeMat.objectReferenceValue, typeof(Material), false);
				EditorGUILayout.HelpBox("Material should be using a transparent shader with a colour field. Use the ExampleFade material in the materials folder or make your own", MessageType.Info);
				SerializedProperty fadeDuration = serializedTeleporter.FindProperty("fadeDuration");
				fadeDuration.floatValue = EditorGUILayout.FloatField("Fade Duration", fadeDuration.floatValue);
				break;
			case ArcTeleporter.Transition.DASH:
				SerializedProperty dashSpeed = serializedTeleporter.FindProperty("dashSpeed");
				dashSpeed.floatValue = EditorGUILayout.FloatField("Dash Speed", dashSpeed.floatValue);

				SerializedProperty useBlur = serializedTeleporter.FindProperty("useBlur");
				useBlur.boolValue = EditorGUILayout.Toggle("Use Blur", useBlur.boolValue);
				break;
			}
			EditorGUI.indentLevel--;

			SerializedProperty firingMode = serializedTeleporter.FindProperty("firingMode");
			firingMode.intValue = (int)(ArcTeleporter.FiringMode)EditorGUILayout.EnumPopup("Firing Mode", (ArcTeleporter.FiringMode)firingMode.intValue);
			ArcTeleporter.FiringMode firingModeEnum = (ArcTeleporter.FiringMode)firingMode.intValue;

			EditorGUI.indentLevel++;

			switch(firingModeEnum)
			{
			case ArcTeleporter.FiringMode.ARC:
				SerializedProperty arcImplementation = serializedTeleporter.FindProperty("arcImplementation");
				arcImplementation.intValue = (int)(ArcTeleporter.ArcImplementation)EditorGUILayout.EnumPopup("Arc Implementation", (ArcTeleporter.ArcImplementation)arcImplementation.intValue);
				ArcTeleporter.ArcImplementation arcImplementationEnum = (ArcTeleporter.ArcImplementation)arcImplementation.intValue;
				switch(arcImplementationEnum)
				{
				case ArcTeleporter.ArcImplementation.FIXED_ARC:
					EditorGUI.indentLevel++;
					SerializedProperty maxDistance = serializedTeleporter.FindProperty("maxDistance");
					maxDistance.floatValue = EditorGUILayout.FloatField("Max Distance", maxDistance.floatValue);
					EditorGUI.indentLevel--;
					break;
				case ArcTeleporter.ArcImplementation.PHYSICS_ARC:
					EditorGUI.indentLevel++;
					SerializedProperty gravity = serializedTeleporter.FindProperty("gravity");
					gravity.floatValue = EditorGUILayout.FloatField("Gravity", gravity.floatValue);

					SerializedProperty initialVelMagnitude = serializedTeleporter.FindProperty("initialVelMagnitude");
					initialVelMagnitude.floatValue = EditorGUILayout.FloatField("Initial Velocity Magnitude", initialVelMagnitude.floatValue);

					SerializedProperty timeStep = serializedTeleporter.FindProperty("timeStep");
					timeStep.floatValue = EditorGUILayout.FloatField("Time Step", timeStep.floatValue);
					EditorGUI.indentLevel--;
					break;
				}

				SerializedProperty arcLineWidth = serializedTeleporter.FindProperty("arcLineWidth");
				arcLineWidth.floatValue = EditorGUILayout.FloatField("Arc Width", arcLineWidth.floatValue);

				SerializedProperty arcMat = serializedTeleporter.FindProperty("arcMat");
				arcMat.intValue = (int)(ArcTeleporter.ArcMaterial)EditorGUILayout.EnumPopup("Use Material", (ArcTeleporter.ArcMaterial)arcMat.intValue);
				ArcTeleporter.ArcMaterial arcMatEnum = (ArcTeleporter.ArcMaterial)arcMat.intValue;

				if (arcMatEnum == ArcTeleporter.ArcMaterial.MATERIAL)
				{
					SerializedProperty goodTeleMat = serializedTeleporter.FindProperty("goodTeleMat");
					goodTeleMat.objectReferenceValue = EditorGUILayout.ObjectField("Good Material", goodTeleMat.objectReferenceValue, typeof(Material), false);

					SerializedProperty badTeleMat = serializedTeleporter.FindProperty("badTeleMat");
					badTeleMat.objectReferenceValue = EditorGUILayout.ObjectField("Bad Material", badTeleMat.objectReferenceValue, typeof(Material), false);

					SerializedProperty matScale = serializedTeleporter.FindProperty("matScale");
					matScale.floatValue = EditorGUILayout.FloatField("Material scale", matScale.floatValue);

					SerializedProperty texMovementSpeed = serializedTeleporter.FindProperty("texMovementSpeed");
					texMovementSpeed.vector2Value = EditorGUILayout.Vector2Field("Material Movement Speed", texMovementSpeed.vector2Value);
				} else
				{
					SerializedProperty goodSpotCol = serializedTeleporter.FindProperty("goodSpotCol");
					goodSpotCol.colorValue = EditorGUILayout.ColorField("Good Colour", goodSpotCol.colorValue);

					SerializedProperty badSpotCol = serializedTeleporter.FindProperty("badSpotCol");
					badSpotCol.colorValue = EditorGUILayout.ColorField("Bad Colour", badSpotCol.colorValue);
				}
				break;
			case ArcTeleporter.FiringMode.PROJECTILE:

				SerializedProperty teleportProjectile = serializedTeleporter.FindProperty("teleportProjectilePrefab");
				teleportProjectile.objectReferenceValue = EditorGUILayout.ObjectField("Teleport Projectile Prefab", teleportProjectile.objectReferenceValue, typeof(GameObject), false);
				EditorGUILayout.HelpBox("Projectile prefab should have a rigidbody attached", MessageType.Info);

				SerializedProperty initVelocity = serializedTeleporter.FindProperty("maxDistance");
				initVelocity.floatValue = EditorGUILayout.FloatField("Inital Velocity", initVelocity.floatValue);
				break;
			}

			EditorGUI.indentLevel--;

			SerializedProperty teleportCooldown = serializedTeleporter.FindProperty("teleportCooldown");
			teleportCooldown.floatValue = EditorGUILayout.FloatField("Teleport Cooldown", teleportCooldown.floatValue);

			SerializedProperty disableRoomRotationWithTrackpad = serializedTeleporter.FindProperty("disableRoomRotationWithTrackpad");
			disableRoomRotationWithTrackpad.boolValue = EditorGUILayout.Toggle("Disable Room Rotation", disableRoomRotationWithTrackpad.boolValue);

			SerializedProperty teleportHighlight = serializedTeleporter.FindProperty("teleportHighlight");
			teleportHighlight.objectReferenceValue = EditorGUILayout.ObjectField("Teleport Highlight", teleportHighlight.objectReferenceValue, typeof(GameObject), false);

			SerializedProperty roomShape = serializedTeleporter.FindProperty("roomShape");
			roomShape.objectReferenceValue = EditorGUILayout.ObjectField("Room Highlight", roomShape.objectReferenceValue, typeof(GameObject), false);

			SerializedProperty onlyLandOnFlat = serializedTeleporter.FindProperty("onlyLandOnFlat");
			onlyLandOnFlat.boolValue = EditorGUILayout.Toggle("Only land on flat", onlyLandOnFlat.boolValue);
			if (onlyLandOnFlat.boolValue)
			{
				SerializedProperty slopeLimit = serializedTeleporter.FindProperty("slopeLimit");
				slopeLimit.floatValue = EditorGUILayout.FloatField("Slope limit", slopeLimit.floatValue);
			}

			SerializedProperty onlyLandOnTag = serializedTeleporter.FindProperty("onlyLandOnTag");
			onlyLandOnTag.boolValue = EditorGUILayout.Toggle("Only land on tagged", onlyLandOnTag.boolValue);

			if (onlyLandOnTag.boolValue)
			{
				tagsFoldout = EditorGUILayout.Foldout(tagsFoldout, "Tags");
				if (tagsFoldout)
				{
					EditorGUI.indentLevel++;
					tagsSize = EditorGUILayout.IntField("Size", tagsSize);

					SerializedProperty tags = serializedTeleporter.FindProperty("tags");
					if (tagsSize != tags.arraySize) tags.arraySize = tagsSize;

					for (int i=0 ; i<tagsSize ; i++)
					{
						SerializedProperty tagName = tags.GetArrayElementAtIndex(i);
						tagName.stringValue = EditorGUILayout.TextField("Element "+i, tagName.stringValue);
					}
					EditorGUI.indentLevel--;
				}
			}

			raycastLayerFoldout = EditorGUILayout.Foldout(raycastLayerFoldout, "Raycast Layers");
			if (raycastLayerFoldout)
			{
				EditorGUI.indentLevel++;
				raycastLayersSize = EditorGUILayout.IntField("Size", raycastLayersSize);

				SerializedProperty raycastLayers = serializedTeleporter.FindProperty("raycastLayer");
				if (raycastLayersSize != raycastLayers.arraySize) raycastLayers.arraySize = raycastLayersSize;
				for(int i=0 ; i<raycastLayersSize ; i++)
				{
					SerializedProperty raycastLayerName = raycastLayers.GetArrayElementAtIndex(i);
					raycastLayerName.stringValue = EditorGUILayout.TextField("Element "+i, raycastLayerName.stringValue);
				}
				EditorGUILayout.HelpBox("Leave raycast layers empty to collide with everything", MessageType.Info);
				if (raycastLayers.arraySize > 0)
				{
					SerializedProperty ignoreRaycastLayers = serializedTeleporter.FindProperty("ignoreRaycastLayers");
					ignoreRaycastLayers.boolValue = EditorGUILayout.Toggle("Ignore raycast layers", ignoreRaycastLayers.boolValue);
					EditorGUILayout.HelpBox("Ignore raycast layers True: Ignore anything on the layers specified. False: Ignore anything on layers not specified", MessageType.Info);
				}
				EditorGUI.indentLevel--;
			}

			SerializedProperty offsetTrans = serializedTeleporter.FindProperty("offsetTrans");
			if (offsetTrans.objectReferenceValue == null)
			{
				if (GUILayout.Button("Create Offset Transform"))
				{
					GameObject newOffset = new GameObject("Offset");
					newOffset.transform.parent = teleporter.transform;
					newOffset.transform.localPosition = Vector3.zero;
					newOffset.transform.localRotation = Quaternion.identity;
					newOffset.transform.localScale = Vector3.one;
					offsetTrans.objectReferenceValue = newOffset.transform;
				}
			}
			offsetTrans.objectReferenceValue = EditorGUILayout.ObjectField("Offset", offsetTrans.objectReferenceValue, typeof(Transform), true);

			serializedTeleporter.ApplyModifiedProperties();
		}
	}

}