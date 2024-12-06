using Silk.NET.Input;
using Silk.NET.SDL;
using Silk.NET.Windowing;
using System.Numerics;

namespace MalignEngine
{
    public class InputSystem : BaseSystem, IPostUpdate
    {
        [Dependency]
        protected WindowSystem Window = default!;

        internal IInputContext input;

        private IMouse usedMouse;
        private IKeyboard usedKeyboard;

        private float lastMouseScroll = 0;

        private bool[] heldMouseButtons = new bool[11];
        private bool[] mouseButtonsState = new bool[11];
        private Dictionary<Key, bool> keysHeld = new Dictionary<Key, bool>();
        private Dictionary<Key, bool> keysState = new Dictionary<Key, bool>();

        public override void OnInitialize()
        {
            input = Window.window.CreateInput();

            if (input.Keyboards.Count > 0)
            {
                usedKeyboard = input.Keyboards[0];
                Logger.LogVerbose($"Using first found keyboard \"{usedKeyboard.Name}\".");
            }

            if (input.Mice.Count > 0)
            {
                usedMouse = input.Mice[0];
                Logger.LogVerbose($"Using first found mouse \"{usedMouse.Name}\".");
                usedMouse.Scroll += UsedMouseScroll;
            }
        }

        private void UsedMouseScroll(IMouse arg1, ScrollWheel arg2)
        {
            //var io = ImGuiNET.ImGui.GetIO();
            //io.AddMouseWheelEvent(arg2.X, arg2.Y);
            lastMouseScroll = arg2.Y;
        }

        public Vector2 MousePosition
        {
            get { return usedMouse.Position; }
        }

        public Vector2 LastMousePosition { get; private set; }

        public Vector2 MouseDelta { get; private set; }

        public bool IsMouseInsideRectangle(Vector2 position, Vector2 size, bool center = true)
        {
            Vector2 mousePosition = MousePosition;
            if (center)
            {
                size = size / 2f;
                return mousePosition.X >= position.X - size.X && mousePosition.X < position.X + size.X &&
                    mousePosition.Y >= position.Y - size.Y && mousePosition.Y < position.Y + size.Y;
            }
            else
            {
                return mousePosition.X >= position.X && mousePosition.X < position.X + size.X &&
                    mousePosition.Y >= position.Y && mousePosition.Y < position.Y + size.Y;
            }
        }

        public float MouseScroll
        {
            get
            {
                return lastMouseScroll;
            }
        }

        public bool IsMouseButtonPressed(int button)
        {
            return usedMouse.IsButtonPressed((MouseButton)button);
        }

        public bool IsMouseButtonHeld(int button)
        {
            return heldMouseButtons[button];
        }

        public bool IsKeyDown(Key key)
        {
            return usedKeyboard.IsKeyPressed((Silk.NET.Input.Key)key);
        }

        public bool IsKeyHeld(Key key)
        {
            if (!keysHeld.ContainsKey(key))
            {
                keysState[key] = false;
                keysHeld[key] = false;
            }

            return keysHeld[key];
        }

        public void OnPostUpdate(float delta)
        {
            MouseDelta = MousePosition - LastMousePosition;

            LastMousePosition = MousePosition;

            lastMouseScroll = 0f;
            for (int button = 0; button < mouseButtonsState.Length; button++)
            {
                if (usedMouse.IsButtonPressed((MouseButton)button) && !mouseButtonsState[button])
                {
                    mouseButtonsState[button] = true;
                    heldMouseButtons[button] = true;
                }
                else if (usedMouse.IsButtonPressed((MouseButton)button) && mouseButtonsState[button])
                {
                    mouseButtonsState[button] = true;
                    heldMouseButtons[button] = false;
                }
                else if (!usedMouse.IsButtonPressed((MouseButton)button))
                {
                    mouseButtonsState[button] = false;
                    heldMouseButtons[button] = false;
                }
            }

            foreach (Key key in keysState.Keys.AsEnumerable())
            {
                if (usedKeyboard.IsKeyPressed((Silk.NET.Input.Key)key) && !keysState[key])
                {
                    keysState[key] = true;
                    keysHeld[key] = true;
                }
                else if (usedKeyboard.IsKeyPressed((Silk.NET.Input.Key)key) && keysState[key])
                {
                    keysState[key] = true;
                    keysHeld[key] = false;
                }
                else if (!usedKeyboard.IsKeyPressed((Silk.NET.Input.Key)key))
                {
                    keysState[key] = false;
                    keysHeld[key] = false;
                }
            }
        }
    }
}
