using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class AxisTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		// designed to work in a pair with another axis touch button
		// (typically with one having -1 and one having 1 axisValues)
		public string axisName = "Horizontal"; // The name of the axis
		public float axisValue = 1; // The axis that the value has
		public float responseSpeed = 3; // The speed at which the axis touch button responds
		public float returnToCentreSpeed = 3; // The speed at which the button will return to its centre

		AxisTouchButton _mPairedWith; // Which button this one is paired with
		CrossPlatformInputManager.VirtualAxis _mAxis; // A reference to the virtual axis as it is in the cross platform input

		void OnEnable()
		{
			if (!CrossPlatformInputManager.AxisExists(axisName))
			{
				// if the axis doesnt exist create a new one in cross platform input
				_mAxis = new CrossPlatformInputManager.VirtualAxis(axisName);
				CrossPlatformInputManager.RegisterVirtualAxis(_mAxis);
			}
			else
			{
				_mAxis = CrossPlatformInputManager.VirtualAxisReference(axisName);
			}
			FindPairedButton();
		}

		void FindPairedButton()
		{
			// find the other button witch which this button should be paired
			// (it should have the same axisName)
			var otherAxisButtons = FindObjectsOfType(typeof(AxisTouchButton)) as AxisTouchButton[];

			if (otherAxisButtons != null)
			{
				for (int i = 0; i < otherAxisButtons.Length; i++)
				{
					if (otherAxisButtons[i].axisName == axisName && otherAxisButtons[i] != this)
					{
						_mPairedWith = otherAxisButtons[i];
					}
				}
			}
		}

		void OnDisable()
		{
			// The object is disabled so remove it from the cross platform input system
			_mAxis.Remove();
		}


		public void OnPointerDown(PointerEventData data)
		{
			if (_mPairedWith == null)
			{
				FindPairedButton();
			}
			// update the axis and record that the button has been pressed this frame
			_mAxis.Update(Mathf.MoveTowards(_mAxis.GetValue, axisValue, responseSpeed * Time.deltaTime));
		}


		public void OnPointerUp(PointerEventData data)
		{
			_mAxis.Update(Mathf.MoveTowards(_mAxis.GetValue, 0, responseSpeed * Time.deltaTime));
		}
	}
}