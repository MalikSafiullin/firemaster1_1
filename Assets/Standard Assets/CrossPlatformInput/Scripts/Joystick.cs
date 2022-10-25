using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			// Options for which axes to use
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}

		[FormerlySerializedAs("MovementRange")] public int movementRange = 100;
		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

		Vector3 _mStartPos;
		bool _mUseX; // Toggle for using the x axis
		bool _mUseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis _mHorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis _mVerticalVirtualAxis; // Reference to the joystick in the cross platform input

		void OnEnable()
		{
			CreateVirtualAxes();
		}

        void Start()
        {
            _mStartPos = transform.position;
        }

		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = _mStartPos - value;
			delta.y = -delta.y;
			delta /= movementRange;
			if (_mUseX)
			{
				_mHorizontalVirtualAxis.Update(-delta.x);
			}

			if (_mUseY)
			{
				_mVerticalVirtualAxis.Update(delta.y);
			}
		}

		void CreateVirtualAxes()
		{
			// set axes to use
			_mUseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
			_mUseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

			// create new axes based on axes to use
			if (_mUseX)
			{
				_mHorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(_mHorizontalVirtualAxis);
			}
			if (_mUseY)
			{
				_mVerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
				CrossPlatformInputManager.RegisterVirtualAxis(_mVerticalVirtualAxis);
			}
		}


		public void OnDrag(PointerEventData data)
		{
			Vector3 newPos = Vector3.zero;

			if (_mUseX)
			{
				int delta = (int)(data.position.x - _mStartPos.x);
				delta = Mathf.Clamp(delta, - movementRange, movementRange);
				newPos.x = delta;
			}

			if (_mUseY)
			{
				int delta = (int)(data.position.y - _mStartPos.y);
				delta = Mathf.Clamp(delta, -movementRange, movementRange);
				newPos.y = delta;
			}
			transform.position = new Vector3(_mStartPos.x + newPos.x, _mStartPos.y + newPos.y, _mStartPos.z + newPos.z);
			UpdateVirtualAxes(transform.position);
		}


		public void OnPointerUp(PointerEventData data)
		{
			transform.position = _mStartPos;
			UpdateVirtualAxes(_mStartPos);
		}


		public void OnPointerDown(PointerEventData data) { }

		void OnDisable()
		{
			// remove the joysticks from the cross platform input
			if (_mUseX)
			{
				_mHorizontalVirtualAxis.Remove();
			}
			if (_mUseY)
			{
				_mVerticalVirtualAxis.Remove();
			}
		}
	}
}