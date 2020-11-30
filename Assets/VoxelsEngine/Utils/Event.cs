using UnityEngine;

namespace VoxelsEngine.Utils
{
    public static class Event
    {
        public const int LeftBtn = 0;
        public const int RightBtn = 1;

        private const EventType MouseDown = EventType.MouseDown;
        private const EventType MouseUp = EventType.MouseUp;
        private const EventType Used = EventType.Used;
        private const EventType Repaint = EventType.Repaint;
        private const EventType Layout = EventType.Layout;

        public static UnityEngine.Event Current => UnityEngine.Event.current;
        private static EventType EventType => Current.type;
        public static int ControlID => GUIUtility.GetControlID(FocusType.Passive);
        public static EventType GetControlType(int controlID) => Current.GetTypeForControl(controlID);
        public static bool IsLeftBtn => Current.button == 0;
        public static bool IsRightBtn => Current.button == 1;

        public static bool IsXKey => Current.isKey && Current.keyCode == KeyCode.X;
        public static bool Shift => Current.shift;
        public static bool Alt => Current.alt;
        public static bool LeftMouseDown => EventType == MouseDown && IsLeftBtn;
        public static bool LeftMouseUp => EventType == MouseUp && IsLeftBtn;
        public static bool RightMouseDown => EventType == MouseDown && IsRightBtn;
        public static bool RightMouseUp => EventType == MouseUp && IsRightBtn;
        public static bool IsRepaint => EventType == Repaint;
        public static bool IsLayout => EventType == Layout;
        public static bool IsUsed => EventType == Used;
        public static bool MouseMove => EventType == EventType.MouseMove;
        public static Vector2 MousePosition => Current.mousePosition;
        
        public static void SetHotControl(int controlID) => GUIUtility.hotControl = controlID;
        public static void ClearHotControl() => GUIUtility.hotControl = 0;
        public static void Use() => Current.Use();
    }
}