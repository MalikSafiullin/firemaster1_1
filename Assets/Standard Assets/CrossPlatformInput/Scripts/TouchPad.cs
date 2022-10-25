using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityStandardAssets.CrossPlatformInput
{
	[RequireComponent(typeof(Image))]
	public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		// Options for which axes to use
		public enum AxisOption
		{
			Both, // Use both
			OnlyHorizontal, // Only horizontal
			OnlyVertical // Only vertical
		}


		public enum ControlStyle
		{
			Absolute, // operates from teh center of the image
			Relative, // operates from the center of the initial touch
			Swipe, // swipe to touch touch no maintained center
		}


		public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
		public ControlStyle controlStyle = ControlStyle.Absolute; // control style to use
		public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
		public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input
		[FormerlySerializedAs("Xsensitivity")] public float xsensitivity = 1f;
		[FormerlySerializedAs("Ysensitivity")] public float ysensitivity = 1f;

		Vector3 _mStartPos;
		Vector2 _mPreviousDelta;
		Vector3 _mJoytickOutput;
		bool _mUseX; // Toggle for using the x axis
		bool _mUseY; // Toggle for using the Y axis
		CrossPlatformInputManager.VirtualAxis _mHorizontalVirtualAxis; // Reference to the joystick in the cross platform input
		CrossPlatformInputManager.VirtualAxis _mVerticalVirtualAxis; // Reference to the joystick in the cross platform input
		bool _mDragging;
		int _mId = -1;
		Vector2 _mPreviousTouchPos; // swipe style control touch


#if !UNITY_EDITOR
    private Vector3 m_Center;
    private Image m_Image;
#else
		Vector3 _mPreviousMouse;
#endif

		void OnEnable()
		{
			CreateVirtualAxes();
		}

        void Start()
        {
#if !UNITY_EDITOR
            m_Image = GetComponent<Image>();
            m_Center = m_Image.transform.position;
#endif
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

		void UpdateVirtualAxes(Vector3 value)
		{
			value = value.normalized;
			if (_mUseX)
			{
				_mHorizontalVirtualAxis.Update(value.x);
			}

			if (_mUseY)
			{
				_mVerticalVirtualAxis.Update(value.y);
			}
		}


		public void OnPointerDown(PointerEventData data)
		{
			_mDragging = true;
			_mId = data.pointerId;
#if !UNITY_EDITOR
        if (controlStyle != ControlStyle.Absolute )
            m_Center = data.position;
#endif
		}

		void Update()
		{
			if (!_mDragging)
			{
				return;
			}
			if (Input.touchCount >= _mId + 1 && _mId != -1)
			{
#if !UNITY_EDITOR

            if (controlStyle == ControlStyle.Swipe)
            {
                m_Center = m_PreviousTouchPos;
                m_PreviousTouchPos = Input.touches[m_Id].position;
            }
            Vector2 pointerDelta = new Vector2(Input.touches[m_Id].position.x - m_Center.x , Input.touches[m_Id].position.y - m_Center.y).normalized;
            pointerDelta.x *= Xsensitivity;
            pointerDelta.y *= Ysensitivity;
#else
				Vector2 pointerDelta;
				pointerDelta.x = Input.mousePosition.x - _mPreviousMouse.x;
				pointerDelta.y = Input.mousePosition.y - _mPreviousMouse.y;
				_mPreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
#endif
				UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));
			}
		}


		public void OnPointerUp(PointerEventData data)
		{
			_mDragging = false;
			_mId = -1;
			UpdateVirtualAxes(Vector3.zero);
		}

		void OnDisable()
		{
			if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
				CrossPlatformInputManager.UnRegisterVirtualAxis(horizontalAxisName);

			if (CrossPlatformInputManager.AxisExists(verticalAxisName))
				CrossPlatformInputManager.UnRegisterVirtualAxis(verticalAxisName);
		}
	}
}