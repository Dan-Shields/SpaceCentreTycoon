using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Keybinds")]
        public Key roadKeybind = Key.R;
        public Key demolitionKeybind = Key.Delete;
        public Key buildingKeybind = Key.B;

        public delegate void InteractionModeChange(InteractionMode newMode, InteractionMode oldMode);
        public event InteractionModeChange OnInteractionModeChange;

        private InteractionMode _currentInteractionMode = InteractionMode.Free;

        // Update is called once per frame
        void Update()
        {
            InteractionMode oldInteractionMode = _currentInteractionMode;

            if (Keyboard.current[roadKeybind].wasPressedThisFrame && _currentInteractionMode != InteractionMode.Disabled)
            {
                if (_currentInteractionMode == InteractionMode.RoadCreator)
                    _currentInteractionMode = InteractionMode.Free;
                else
                    _currentInteractionMode = InteractionMode.RoadCreator;
                
                OnInteractionModeChange(_currentInteractionMode, oldInteractionMode);
            } else if (Keyboard.current[demolitionKeybind].wasPressedThisFrame && _currentInteractionMode != InteractionMode.Disabled)
            {
                if (_currentInteractionMode == InteractionMode.Demolition)
                    _currentInteractionMode = InteractionMode.Free;
                else
                    _currentInteractionMode = InteractionMode.Demolition;

                OnInteractionModeChange(_currentInteractionMode, oldInteractionMode);
            } else if (Keyboard.current[buildingKeybind].wasPressedThisFrame && _currentInteractionMode != InteractionMode.Disabled)
            {
                if (_currentInteractionMode == InteractionMode.BuildingCreator)
                    _currentInteractionMode = InteractionMode.Free;
                else
                    _currentInteractionMode = InteractionMode.BuildingCreator;

                OnInteractionModeChange(_currentInteractionMode, oldInteractionMode);
            } else if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                _currentInteractionMode = InteractionMode.Free;
                OnInteractionModeChange(_currentInteractionMode, oldInteractionMode);
            }
        }

        public void Enable()
        {
            OnInteractionModeChange(InteractionMode.Free, _currentInteractionMode);
            _currentInteractionMode = InteractionMode.Free;
        }

        public void Disable()
        {
            OnInteractionModeChange(InteractionMode.Disabled, _currentInteractionMode);
            _currentInteractionMode = InteractionMode.Disabled;
        }

        public void OnGUI()
        {
            GUI.Box(new Rect(10, 10, 100, 25), _currentInteractionMode.ToString());
        }
    }
}

