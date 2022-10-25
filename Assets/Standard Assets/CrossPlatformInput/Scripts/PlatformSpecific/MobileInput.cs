using System;
using UnityEngine;

namespace UnityStandardAssets.CrossPlatformInput.PlatformSpecific
{
    public class MobileInput : VirtualInput
    {
        private void AddButton(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            CrossPlatformInputManager.RegisterVirtualButton(new CrossPlatformInputManager.VirtualButton(name));
        }


        private void AddAxes(string name)
        {
            // we have not registered this button yet so add it, happens in the constructor
            CrossPlatformInputManager.RegisterVirtualAxis(new CrossPlatformInputManager.VirtualAxis(name));
        }


        public override float GetAxis(string name, bool raw)
        {
            if (!MVirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            return MVirtualAxes[name].GetValue;
        }


        public override void SetButtonDown(string name)
        {
            if (!MVirtualButtons.ContainsKey(name))
            {
                AddButton(name);
            }
            MVirtualButtons[name].Pressed();
        }


        public override void SetButtonUp(string name)
        {
            if (!MVirtualButtons.ContainsKey(name))
            {
                AddButton(name);
            }
            MVirtualButtons[name].Released();
        }


        public override void SetAxisPositive(string name)
        {
            if (!MVirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            MVirtualAxes[name].Update(1f);
        }


        public override void SetAxisNegative(string name)
        {
            if (!MVirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            MVirtualAxes[name].Update(-1f);
        }


        public override void SetAxisZero(string name)
        {
            if (!MVirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            MVirtualAxes[name].Update(0f);
        }


        public override void SetAxis(string name, float value)
        {
            if (!MVirtualAxes.ContainsKey(name))
            {
                AddAxes(name);
            }
            MVirtualAxes[name].Update(value);
        }


        public override bool GetButtonDown(string name)
        {
            if (MVirtualButtons.ContainsKey(name))
            {
                return MVirtualButtons[name].GetButtonDown;
            }

            AddButton(name);
            return MVirtualButtons[name].GetButtonDown;
        }


        public override bool GetButtonUp(string name)
        {
            if (MVirtualButtons.ContainsKey(name))
            {
                return MVirtualButtons[name].GetButtonUp;
            }

            AddButton(name);
            return MVirtualButtons[name].GetButtonUp;
        }


        public override bool GetButton(string name)
        {
            if (MVirtualButtons.ContainsKey(name))
            {
                return MVirtualButtons[name].GetButton;
            }

            AddButton(name);
            return MVirtualButtons[name].GetButton;
        }


        public override Vector3 MousePosition()
        {
            return VirtualMousePosition;
        }
    }
}
