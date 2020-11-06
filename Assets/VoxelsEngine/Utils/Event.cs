using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class Event
    {
        private static UnityEngine.Event Evt => UnityEngine.Event.current;

        public static class Mouse
        {
            public static bool Move => Evt.type == EventType.MouseMove;
            public static Vector2 Position => Evt.mousePosition;
            
            public static class Click
            {
                public static bool Left => Evt.type == EventType.MouseDown && Evt.button == 0;
                public static bool Right => Evt.type == EventType.MouseMove && Evt.button == 1;
            }
        }
    }
}