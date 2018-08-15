//========= Copyright 2017, Sam Tague, All rights reserved. ===================
//
// Processes controller input for both Vive and Oculus and uses
// SendMessage to broadcast activity to attached scripts
//
//=============================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

namespace VRInteraction
{
	[RequireComponent (typeof (SteamVR_TrackedController))]
	public class VRInput : MonoBehaviour 
	{
		public enum HMDType
		{
			VIVE,
			OCULUS
		}

		public HMDType hmdType = HMDType.VIVE;
		public string[] VRActions;

		public bool mirrorControls = true;
		// Will display oculus buttons when false
		public bool displayViveButtons = true;
		public bool debugMode;

		public int triggerKey;
		public int padTop;
		public int padLeft;
		public int padRight;
		public int padBottom;
		public int padCentre;
		public int gripKey;
		public int menuKey;
		public int aButtonKey;

		//Oculus alternative buttons
		public int triggerKeyOculus;
		public int padTopOculus;
		public int padLeftOculus;
		public int padRightOculus;
		public int padBottomOculus;
		public int padCentreOculus;
		public int gripKeyOculus;
		public int menuKeyOculus;
		public int aButtonKeyOculus;

		//Oculus A Button
		public bool aButtonPressed;
		public event ClickedEventHandler AButtonClicked;
		public event ClickedEventHandler AButtonUnclicked;

		protected SteamVR_TrackedController _controller;
		public SteamVR_TrackedController controller
		{
			get { return _controller; }
		}

		public bool ActionPressed(string action)
		{
			for(int i=0; i<VRActions.Length; i++)
			{
				if (action == VRActions[i])
				{
					return ActionPressed(i);
				}
			}
			if (debugMode)
			{
				string debugString = "No action with the name " + action + ". Current options are ";
				foreach(string availableOption in VRActions)
				{
					debugString += availableOption + " ";
				}
				Debug.LogWarning(debugString);
			}
			return false;
		}

		public bool ActionPressed(int action)
		{
			if (mirrorControls || hmdType == HMDType.VIVE)
			{
				if (triggerKey == action && controller.triggerPressed)
					return true;
				if (padTop == action && PadUpPressed)
					return true;
				if (padLeft == action && PadLeftPressed)
					return true;
				if (padRight == action && PadRightPressed)
					return true;
				if (padBottom == action && PadDownPressed)
					return true;
				if (padCentre == action && PadCentrePressed)
					return true;
				if (menuKey == action && controller.menuPressed)
					return true;
				if (gripKey == action && controller.gripped)
					return true;
				if (aButtonKey == action && aButtonPressed)
					return true;
			} else
			{
				if (triggerKeyOculus == action && controller.triggerPressed)
					return true;
				if (padTopOculus == action && PadUpPressed)
					return true;
				if (padLeftOculus == action && PadLeftPressed)
					return true;
				if (padRightOculus == action && PadRightPressed)
					return true;
				if (padBottomOculus == action && PadDownPressed)
					return true;
				if (padCentreOculus == action && PadCentrePressed)
					return true;
				if (menuKeyOculus == action && controller.menuPressed)
					return true;
				if (gripKeyOculus == action && controller.gripped)
					return true;
				if (aButtonKeyOculus == action && aButtonPressed)
					return true;
			}
			return false;
		}

		public float TriggerPressure
		{
			get
			{
				var device = SteamVR_Controller.Input((int)controller.controllerIndex);
				return device.GetAxis(EVRButtonId.k_EButton_SteamVR_Trigger).x;
			}
		}
		public bool PadUpPressed
		{
			get
			{
				if (controller.padPressed)
				{
					var device = SteamVR_Controller.Input((int)controller.controllerIndex);
					Vector2 axis = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
					if (axis.y > 0.4f &&
						axis.x < axis.y &&
						axis.x > -axis.y)
						return true;
				}
				return false;
			}
		}
		public bool PadLeftPressed
		{
			get
			{
				if (controller.padPressed)
				{
					var device = SteamVR_Controller.Input((int)controller.controllerIndex);
					Vector2 axis = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
					if (axis.x < -0.4f &&
						axis.y > axis.x &&
						axis.y < -axis.x)
						return true;
				}
				return false;
			}
		}
		public bool PadRightPressed
		{
			get
			{
				if (controller.padPressed)
				{
					var device = SteamVR_Controller.Input((int)controller.controllerIndex);
					Vector2 axis = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
					if (axis.x > 0.4f &&
						axis.y < axis.x &&
						axis.y > -axis.x)
						return true;
				}
				return false;
			}
		}
		public bool PadDownPressed
		{
			get
			{
				if (controller.padPressed)
				{
					var device = SteamVR_Controller.Input((int)controller.controllerIndex);
					Vector2 axis = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
					if ((axis.y < -0.4f &&
						axis.x > axis.y &&
						axis.x < -axis.y) ||
						axis == Vector2.zero)
						return true;
				}
				return false;
			}
		}
		public bool PadCentrePressed
		{
			get
			{
				if (controller.padPressed)
				{
					var device = SteamVR_Controller.Input((int)controller.controllerIndex);
					Vector2 axis = device.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);

					if (axis.y >= -0.4f && axis.y <= 0.4f && axis.x >= -0.4f && axis.x <= 0.4f)
						return true;
				}
				return false;
			}
		}

		protected virtual void Start()
		{
		}

		protected virtual void OnEnable()
		{
			if (SteamVR.instance.hmd_TrackingSystemName == "oculus")
				hmdType = HMDType.OCULUS;

			_controller = GetComponent<SteamVR_TrackedController>();
			if (controller == null)
			{
				Debug.LogError("No controller SteamVR_TrackedController found");
				return;
			}
			controller.TriggerClicked += TriggerClicked;
			controller.TriggerUnclicked += TriggerReleased;
			controller.PadClicked += TrackpadDown;
			controller.PadUnclicked += TrackpadUp;
			controller.Gripped += Gripped;
			controller.Ungripped += UnGripped;
			controller.MenuButtonClicked += MenuClicked;
			controller.MenuButtonUnclicked += MenuReleased;
			if (hmdType == HMDType.OCULUS)
			{
				AButtonClicked += AButtonPressed;
				AButtonUnclicked += AButtonReleased;
			}
		}

		protected virtual void OnDisable()
		{
			if (controller == null) return;
			controller.TriggerClicked -= TriggerClicked;
			controller.TriggerUnclicked -= TriggerReleased;
			controller.PadClicked -= TrackpadDown;
			controller.PadUnclicked -= TrackpadUp;
			controller.Gripped -= Gripped;
			controller.Ungripped -= UnGripped;
			controller.MenuButtonClicked -= MenuClicked;
			controller.MenuButtonUnclicked -= MenuReleased;
			if (hmdType == HMDType.OCULUS)
			{
				AButtonClicked -= AButtonPressed;
				AButtonUnclicked -= AButtonReleased;
			}
		}

		protected virtual void Update()
		{
			if (hmdType != HMDType.OCULUS) return;

			//Check if A button is pressed
			var system = OpenVR.System;
			if (system != null && system.GetControllerState(controller.controllerIndex, ref controller.controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t))))
			{
				ulong AButton = controller.controllerState.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_A));
				if (AButton > 0L && !aButtonPressed)
				{
					aButtonPressed = true;
					ClickedEventArgs e;
					e.controllerIndex = controller.controllerIndex;
					e.flags = (uint)controller.controllerState.ulButtonPressed;
					e.padX = controller.controllerState.rAxis0.x;
					e.padY = controller.controllerState.rAxis0.y;
					OnAButtonClicked(e);
				}
				else if (AButton == 0L && aButtonPressed)
				{
					aButtonPressed = false;
					ClickedEventArgs e;
					e.controllerIndex = controller.controllerIndex;
					e.flags = (uint)controller.controllerState.ulButtonPressed;
					e.padX = controller.controllerState.rAxis0.x;
					e.padY = controller.controllerState.rAxis0.y;
					OnAButtonUnclicked(e);
				}
			}
		}

		public virtual void OnAButtonClicked(ClickedEventArgs e)
		{
			if (AButtonClicked != null)
				AButtonClicked(this, e);
		}

		public virtual void OnAButtonUnclicked(ClickedEventArgs e)
		{
			if (AButtonUnclicked != null)
				AButtonUnclicked(this, e);
		}

		void AButtonPressed(object sender, ClickedEventArgs e)
		{
			int aButtonKey = this.aButtonKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) aButtonKey = this.aButtonKeyOculus;
			if (aButtonKey >= VRActions.Length)
			{
				Debug.LogWarning("A Button key index (" + aButtonKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[aButtonKey], SendMessageOptions.DontRequireReceiver);
		}

		void AButtonReleased(object sender, ClickedEventArgs e)
		{
			int aButtonKey = this.aButtonKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) aButtonKey = this.aButtonKeyOculus;
			if (aButtonKey >= VRActions.Length)
			{
				Debug.LogWarning("A Button key index (" + aButtonKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[aButtonKey]+"Released", SendMessageOptions.DontRequireReceiver);
		}

		void TriggerClicked(object sender, ClickedEventArgs e)
		{
			int triggerKey = this.triggerKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) triggerKey = this.triggerKeyOculus;
			if (triggerKey >= VRActions.Length)
			{
				Debug.LogWarning("Trigger key index (" + triggerKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[triggerKey], SendMessageOptions.DontRequireReceiver);
		}

		void TriggerReleased(object sender, ClickedEventArgs e)
		{
			int triggerKey = this.triggerKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) triggerKey = this.triggerKeyOculus;
			if (triggerKey >= VRActions.Length)
			{
				Debug.LogWarning("Trigger key index (" + triggerKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[triggerKey]+"Released", SendMessageOptions.DontRequireReceiver);
		}

		void TrackpadDown(object sender, ClickedEventArgs e)
		{
			int action = 0;
			if (PadUpPressed) action = mirrorControls || hmdType == HMDType.VIVE ? padTop : padTopOculus;
			if (PadLeftPressed) action = mirrorControls || hmdType == HMDType.VIVE ? padLeft : padLeftOculus;
			if (PadRightPressed) action = mirrorControls || hmdType == HMDType.VIVE ? padRight : padRightOculus;
			if (PadDownPressed) action = mirrorControls || hmdType == HMDType.VIVE ? padBottom : padBottomOculus;
			if (PadCentrePressed) action = mirrorControls || hmdType == HMDType.VIVE ? padCentre : padCentreOculus;
			if (action >= VRActions.Length)
			{
				Debug.LogWarning("Pad key index (" + action + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[action], SendMessageOptions.DontRequireReceiver);
		}

		void TrackpadUp(object sender, ClickedEventArgs e)
		{
			for(int i=0; i<VRActions.Length; i++)
			{
				if ((mirrorControls || hmdType == HMDType.VIVE ? padLeft : padLeftOculus) == i || (mirrorControls || hmdType == HMDType.VIVE ? padTop : padTopOculus) == i 
					|| (mirrorControls || hmdType == HMDType.VIVE ? padRight : padRightOculus) == i || (mirrorControls || hmdType == HMDType.VIVE ? padBottom : padBottomOculus) == i
					|| (mirrorControls || hmdType == HMDType.VIVE ? padCentre : padCentreOculus) == i)
					SendMessage(VRActions[i]+"Released", SendMessageOptions.DontRequireReceiver);
			}
		}

		void Gripped(object sender, ClickedEventArgs e)
		{
			int gripKey = this.gripKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) gripKey = this.gripKeyOculus;
			if (gripKey >= VRActions.Length)
			{
				Debug.LogWarning("Gripped key index (" + gripKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[gripKey], SendMessageOptions.DontRequireReceiver);
		}

		void UnGripped(object sender, ClickedEventArgs e)
		{
			int gripKey = this.gripKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) gripKey = this.gripKeyOculus;
			if (gripKey >= VRActions.Length)
			{
				Debug.LogWarning("Gripped key index (" + gripKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[gripKey]+"Released", SendMessageOptions.DontRequireReceiver);
		}

		void MenuClicked(object sender, ClickedEventArgs e)
		{
			int menuKey = this.menuKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) menuKey = this.menuKeyOculus;
			if (menuKey >= VRActions.Length)
			{
				Debug.LogWarning("Menu key index (" + menuKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[menuKey], SendMessageOptions.DontRequireReceiver);
		}

		void MenuReleased(object sender, ClickedEventArgs e)
		{
			int menuKey = this.menuKey;
			if (!mirrorControls && hmdType == HMDType.OCULUS) menuKey = this.menuKeyOculus;
			if (menuKey >= VRActions.Length)
			{
				Debug.LogWarning("Menu key index (" + menuKey + ") out of range (" + VRActions.Length+")");
				return;
			}
			SendMessage(VRActions[menuKey]+"Released", SendMessageOptions.DontRequireReceiver);
		}
	}

}