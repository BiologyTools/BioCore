﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BioCore
{
    public class Win32
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        /// It returns true if the key is pressed, and false if it isn't
        /// 
        /// @param vKey The key you want to check.
        /// 
        /// @return The return value is a short integer with the high order bit set if the key is down
        /// and the low order bit set if the key was pressed after the previous call to
        /// GetAsyncKeyState.
        public static bool GetKeyState(System.Windows.Forms.Keys vKey)
        {
            int si = (int)GetAsyncKeyState(vKey);
            if (si == 0)
                return false;
            else
                return true;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        /* A struct that is used to get the position of the window. */
        public struct Rect
        {
            public int X;        // x position of upper-left corner
            public int Y;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;//The left button is down.
        public const int MOUSEEVENTF_LEFTUP = 0x0004;//The left button is up.
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;//The middle button is down.
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;//The middle button is up.
        public const int MOUSEEVENTF_MOVE = 0x0001;//Movement occurred.
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;//The right button is down.
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;//The right button is up.
        public const int MOUSEEVENTF_WHEEL = 0x0800;//The wheel has been moved, if the mouse has a wheel.The amount of movement is specified in dwData
        public const int MOUSEEVENTF_XDOWN = 0x0080;//An X button was pressed.
        public const int MOUSEEVENTF_XUP = 0x0100;//An X button was released.
        public const int MOUSEEVENTF_HWHEEL = 0x01000; //The wheel button is tilted.

        /// GetWindowRect() returns the coordinates of the window's upper-left and lower-right corners
        /// in screen coordinates.
        /// 
        /// @param IntPtr A pointer to a window handle.
        /// @param Rect A struct that contains the coordinates of the upper-left and lower-right corners
        /// of the rectangle.
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        /// "MouseWheelUp()" is a function that simulates a mouse wheel up event.
        public static void MouseWheelUp()
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
        }
        /// MouseWheelDown() simulates a mouse wheel down event.
        public static void MouseWheelDown()
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
        }
        /// It takes a string as a parameter, and then sets the focus to the process that matches the
        /// string
        /// 
        /// @param process The name of the process you want to focus on.
        public static void SetFocus(string process)
        {
            Process pr = Process.GetProcessesByName(process)[0];
            SetForegroundWindow(pr.MainWindowHandle);
        }
        /// If the process "winlogon" is running, then we are on Windows. If it is not running, then we
        /// are on Wine
        /// 
        /// @return The return value is a boolean value.
        public static bool RunningOnWine()
        {
            //We see if winlogon is running indicating if we are on windows
            Process[] pr = Process.GetProcessesByName("winlogon");
            if (pr.Length != 0)
                return true;
            else
                return false;
        }
        /// It gets the title of the active window
        /// 
        /// @return The title of the active window.
        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

    }
    public class MouseOperations
    {
        [Flags]
        /* A enum that is used to define the mouse events. */
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }

        /// "SetCursorPos" is a function that is defined in the user32.dll library. 
        /// It takes two parameters, an x and a y coordinate, and returns a boolean value
        /// 
        /// @param x The x coordinate of the cursor.
        /// @param y The y-coordinate of the cursor's position.
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        /// GetCursorPos(out MousePoint lpMousePoint)
        /// 
        /// The function takes a pointer to a MousePoint structure and returns a boolean value
        /// 
        /// @param MousePoint A structure that contains the x and y coordinates of the mouse pointer.
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out MousePoint lpMousePoint);

        /// The mouse_event function synthesizes mouse motion and button clicks.
        /// 
        /// @param dwFlags The mouse event you want to simulate.
        /// @param dx The x-coordinate of the mouse pointer, relative to the upper-left corner of the
        /// screen.
        /// @param dy The y-coordinate of the mouse pointer, relative to the upper-left corner of the
        /// screen.
        /// @param dwData The amount of wheel movement. A positive value indicates that the wheel was
        /// rotated forward, away from the user; a negative value indicates that the wheel was rotated
        /// backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120.
        /// @param dwExtraInfo A value that specifies an extra value associated with the mouse event. An
        /// application calls GetMessageExtraInfo to obtain this extra information.
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        /// It sends a message to the operating system that the left mouse button has been pressed and
        /// released
        public static void LeftClick()
        {
            MouseEvent(MouseEventFlags.LeftDown | MouseEventFlags.LeftUp);
        }
       /// RightClick() is a function that calls the MouseEvent() function with the
       /// MouseEventFlags.RightDown and MouseEventFlags.RightUp flags
        public static void RightClick()
        {
            MouseEvent(MouseEventFlags.RightDown | MouseEventFlags.RightUp);
        }
        /// The function takes the mouse event flag and calls the mouse event function
        public static void LeftDown()
        {
            MouseEvent(MouseEventFlags.LeftDown);
        }
       /// It sends a message to the operating system that the right mouse button has been pressed
        public static void RightDown()
        {
            MouseEvent(MouseEventFlags.RightDown);
        }
        /// LeftUp() is a function that calls the MouseEvent() function with the MouseEventFlags.LeftUp
        /// parameter.
        public static void LeftUp()
        {
            MouseEvent(MouseEventFlags.LeftUp);
        }
        /// RightUp() is a function that calls the MouseEvent() function with the
        /// MouseEventFlags.RightUp flag
        public static void RightUp()
        {
            MouseEvent(MouseEventFlags.RightUp);
        }
        /// SetCursorPos(x, y);
        /// 
        /// @param x The x coordinate of the cursor.
        /// @param y The y coordinate of the cursor.
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

/// It takes a MousePoint object, which contains an X and Y coordinate, and sets the cursor position to
/// that point
/// 
/// @param MousePoint A struct that contains the X and Y coordinates of the mouse.
        public static void SetCursorPosition(MousePoint point)
        {
            SetCursorPos(point.X, point.Y);
        }

/// Get the current mouse position, if it fails, return a point of 0,0
/// 
/// @return A MousePoint object.
        public static MousePoint GetCursorPosition()
        {
            MousePoint currentMousePoint;
            var gotPoint = GetCursorPos(out currentMousePoint);
            if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
            return currentMousePoint;
        }

        /// It takes a MouseEventFlags enum value and calls the mouse_event function with the current
        /// cursor position
        /// 
        /// @param MouseEventFlags This is the type of mouse event you want to simulate.
        public static void MouseEvent(MouseEventFlags value)
        {
            MousePoint position = GetCursorPosition();

            mouse_event
                ((int)value,
                 position.X,
                 position.Y,
                 0,
                 0)
                ;
        }

        [StructLayout(LayoutKind.Sequential)]
        /* A struct that contains the x and y coordinates of the mouse. */
        public struct MousePoint
        {
            public int X;
            public int Y;

            public MousePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
