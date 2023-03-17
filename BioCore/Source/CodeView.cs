namespace Bio
{
    public partial class CodeView : UserControl
    {
        private ScrollTextBox textBox = new ScrollTextBox();
        private ScrollTextBox lineBox = new ScrollTextBox();
        private int tabSize = 15;
        /* Initializing the textbox and the linebox. */
        public CodeView()
        {
            InitializeComponent();
            textBox.Dock = DockStyle.Fill;
            textBox.MouseWheel += new MouseEventHandler(Code_MouseWheel);
            textBox.TextChanged += new EventHandler(textBox_TextChanged);
            textBox.VScroll += new EventHandler(textBox_Scroll);
            textBox.FontChanged += new EventHandler(textBox_FontChanged);
            textBox.WordWrap = false;
            textBox.AcceptsTab = true;
            panel.Controls.Add(textBox);
            lineBox.Dock = DockStyle.Fill;
            lineBox.ScrollBars = RichTextBoxScrollBars.None;
            panel2.Controls.Add(lineBox);
            //MouseWheel += new MouseEventHandler(Code_MouseWheel);
            textBox.SelectionTabs = new int[] { tabSize, tabSize * 2, tabSize * 3, tabSize * 4, tabSize * 5, tabSize * 6 };
            UpdateScroll();
        }
        /* A property of a RichTextBox. */
        public RichTextBox TextBox
        {
            get
            {
                return textBox;
            }
        }

        /* A property of the textbox. */
        public bool WordWrap
        {
            get
            {
                return textBox.WordWrap;
            }
            set
            {
                textBox.WordWrap = value;
            }
        }
        /// The lineBox's vertical scroll position is set to the textBox's vertical scroll position
        public void UpdateScroll()
        {
            lineBox.VerticalScrollPosition = textBox.VerticalScrollPosition;
        }

        /// <summary>
        /// TextBox with support for getting and setting the vertical scroll bar
        /// position, as well as listening to vertical scroll events.
        /// </summary>
        public class ScrollTextBox : RichTextBox
        {

            /* Creating a new instance of the ScrollTextBox class. */
            public ScrollTextBox()
            {
                _components = new System.ComponentModel.Container();
                // Calculate width of "W" to set as the small horizontal increment
                OnFontChanged(null);
            }

            [System.ComponentModel.DefaultValue(0)
            , System.ComponentModel.Category("Appearance")
            , System.ComponentModel.Description
               ("Gets or sets the vertical scroll bar's position"
               )
            ]
            /// The function takes a value and sets the scroll position of the window
            /// 
            /// @param value The position to scroll to
            /// @param  Win32.WM_VSCROLL - The message to send to the control to scroll it.
            /// @param  Win32.WM_VSCROLL - The message to send to the control to scroll it.
            public int VerticalScrollPosition
            {
                set { SetScroll(value, Win32.WM_VSCROLL, Win32.SB_VERT); }
                get { return GetScroll(Win32.SB_VERT); }
            }

            [System.ComponentModel.DefaultValue(0)
            , System.ComponentModel.Category("Appearance")
            , System.ComponentModel.Description
               ("Gets or sets the horizontal scroll bar's position"
               )
            ]
            /// SetScroll(value, Win32.WM_HSCROLL, Win32.SB_HORZ);
            /// 
            /// The first parameter is the value to set the scrollbar to. The second parameter is the
            /// message to send to the control. The third parameter is the scrollbar to set
            /// 
            /// @param value The value to set the scrollbar to.
            /// @param  Win32.WM_HSCROLL - The message to send to the control
            /// @param  Win32.WM_HSCROLL - The message to send to the control
            public int HorizontalScrollPosition
            {
                set { SetScroll(value, Win32.WM_HSCROLL, Win32.SB_HORZ); }
                get { return GetScroll(Win32.SB_HORZ); }
            }

            [System.ComponentModel.Description
               ("Fired when the scroll bar's vertical position changes"
               )
            , System.ComponentModel.Category("Property Changed")
            ]
            /* Creating an event handler for the ScrollChanged event. */
            public event System.Windows.Forms.ScrollEventHandler ScrollChanged;

            // Fire scroll event if the scroll-bars are moved
            protected override void WndProc
            (ref Message message
            )
            {
                base.WndProc(ref message);
                if (message.Msg == Win32.WM_VSCROLL
                || message.Msg == Win32.WM_HSCROLL
                || message.Msg == Win32.WM_MOUSEWHEEL
                ) TryFireScrollEvent();
            }

            // Key-down includes navigation keys, but also typing and pasting can
            // cause a scroll event
            protected override void OnKeyDown
            (System.Windows.Forms.KeyEventArgs e
            )
            {
                base.OnKeyDown(e);
                TryFireScrollEvent();
            }

            protected override void OnKeyUp
            (System.Windows.Forms.KeyEventArgs e
            )
            {
                base.OnKeyUp(e);
                TryFireScrollEvent();
            }

            // Resizing can alter the word-wrap or fit the content causing the text
            // to scroll
            protected override void OnResize
            (System.EventArgs e
            )
            {
                base.OnResize(e);
                TryFireScrollEvent();
            }

            // Clicking an empty space can move the carot to the beginning of the
            // line causing a scroll event
            protected override void OnMouseDown
            (System.Windows.Forms.MouseEventArgs e
            )
            {
                base.OnMouseDown(e);
                TryFireScrollEvent();
            }

            protected override void OnMouseUp
            (System.Windows.Forms.MouseEventArgs e
            )
            {
                base.OnMouseUp(e);
                TryFireScrollEvent();
            }

            // If the mouse button is down, a text selection is probably being done
            // where dragging the selection can caues scroll events
            protected override void OnMouseMove
            (System.Windows.Forms.MouseEventArgs e
            )
            {
                base.OnMouseMove(e);
                if (e.Button != System.Windows.Forms.MouseButtons.None)
                    TryFireScrollEvent();
            }

            // Changing the size of the font can cause a scroll event. Also, when
            // scrolling horizontally, the event will notify whether the scroll
            // was a large or small change. For vertical, small increments are 1
            // line, but for horizontal, it is several pixels. To guess what a
            // small increment is, get the width of the W character and anything
            // smaller than that will be represented as a small increment
            protected override void OnFontChanged
            (System.EventArgs e
            )
            {
                base.OnFontChanged(e);
                using (System.Drawing.Graphics graphics = this.CreateGraphics())
                    _fontWidth = (int)graphics.MeasureString("W", this.Font).Width;
                TryFireScrollEvent();
            }

            /// If the object is being disposed, and the components are not null, then dispose of the
            /// components
            /// 
            /// @param disposing true if managed resources should be disposed; otherwise, false.
            protected override void Dispose
            (bool disposing
            )
            {
                if (disposing && (_components != null))
                    _components.Dispose();
                base.Dispose(disposing);
            }

            /// It sets the scrollbar position and then sends a message to the window to update the
            /// scrollbar
            /// 
            /// @param value The value to set the scrollbar to.
            /// @param windowsMessage The message to send to the window.
            /// @param scrollBarMessage 
            private void SetScroll
            (int value
            , uint windowsMessage
            , int scrollBarMessage
            )
            {
                Win32.SetScrollPos
                ((System.IntPtr)this.Handle
                , scrollBarMessage
                , value
                , true
                );
                Win32.PostMessage
                ((System.IntPtr)this.Handle
                , windowsMessage
                , 4 + 0x10000 * value
                , 0
                );
            }

           /// GetScrollPos returns the current position of the scroll bar
           /// 
           /// @param scrollBarMessage 
           /// 
           /// @return The current position of the scroll bar.
            private int GetScroll
            (int scrollBarMessage
            )
            {
                return Win32.GetScrollPos
                ((System.IntPtr)this.Handle
                , scrollBarMessage
                );
            }

            // Fire both horizontal and vertical scroll events seperately, one
            // after the other. These first test if a scroll actually occurred
            // and won't fire if there was no actual movement
            private void TryFireScrollEvent()
            {  // Don't do anything if there is no event handler
                if (ScrollChanged == null)
                    return;
                TryFireHorizontalScrollEvent();
                TryFireVerticalScrollEvent();
            }

            /// If the scroll position has changed, fire a scroll event.
            /// 
            /// The function is called from the `OnMouseWheel` function.
            /// 
            /// @return The ScrollEventArgs object is being returned.
            private void TryFireHorizontalScrollEvent()
            {

                // Don't do anything if there is no event handler
                if (ScrollChanged == null)
                    return;

                int lastScrollPosition = _lastHorizontalScrollPosition;
                int scrollPosition = HorizontalScrollPosition;

                // Don't do anything if there was no change in position
                if (scrollPosition == lastScrollPosition)
                    return;

                _lastHorizontalScrollPosition = scrollPosition;

                ScrollChanged
                (this
                , new System.Windows.Forms.ScrollEventArgs
                   (scrollPosition < lastScrollPosition - _fontWidth
                      ? System.Windows.Forms.ScrollEventType.LargeDecrement
                      : scrollPosition > lastScrollPosition + _fontWidth
                      ? System.Windows.Forms.ScrollEventType.LargeIncrement
                      : scrollPosition < lastScrollPosition
                      ? System.Windows.Forms.ScrollEventType.SmallDecrement
                      : System.Windows.Forms.ScrollEventType.SmallIncrement
                   , lastScrollPosition
                   , scrollPosition
                   , System.Windows.Forms.ScrollOrientation.HorizontalScroll
                   )
                );

            }

            /// If the scroll position has changed, fire a scroll event.
            /// 
            /// @return The ScrollEventArgs object is being returned.
            private void TryFireVerticalScrollEvent()
            {
                // Don't do anything if there is no event handler
                if (ScrollChanged == null)
                    return;
                int lastScrollPosition = _lastVerticalScrollPosition;
                int scrollPosition = VerticalScrollPosition;

                // Don't do anything if there was no change in position
                if (scrollPosition == lastScrollPosition)
                    return;

                _lastVerticalScrollPosition = scrollPosition;

                ScrollChanged
                (this
                , new System.Windows.Forms.ScrollEventArgs
                   (scrollPosition < lastScrollPosition - 1
                      ? System.Windows.Forms.ScrollEventType.LargeDecrement
                      : scrollPosition > lastScrollPosition + 1
                      ? System.Windows.Forms.ScrollEventType.LargeIncrement
                      : scrollPosition < lastScrollPosition
                      ? System.Windows.Forms.ScrollEventType.SmallDecrement
                      : System.Windows.Forms.ScrollEventType.SmallIncrement
                   , lastScrollPosition
                   , scrollPosition
                   , System.Windows.Forms.ScrollOrientation.VerticalScroll
                   )
                );

            }

            private int _lastVerticalScrollPosition;
            private int _lastHorizontalScrollPosition;
            private int _fontWidth;
            private System.ComponentModel.IContainer _components = null;

            /* It's a class that allows you to set the scroll position of a control. */
            private static class Win32
            {

                public const uint WM_HSCROLL = 0x114;
                public const uint WM_VSCROLL = 0x115;
                public const uint WM_MOUSEWHEEL = 0x20A;
                public const int SB_VERT = 0x1;
                public const int SB_HORZ = 0x0;

                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int SetScrollPos
                (System.IntPtr hWnd
                , int nBar
                , int nPos
                , bool bRedraw
                );
                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int GetScrollPos
                (System.IntPtr hWnd
                , int nBar
                );

                [System.Runtime.InteropServices.DllImport
                   ("User32.Dll"
                   , EntryPoint = "PostMessageA")
                ]
                public static extern bool PostMessage
                (System.IntPtr hWnd
                , uint msg
                , int wParam
                , int lParam
                );

            }

        }
        /// It updates the scrollbar's position when the mouse wheel is scrolled
        /// 
        /// @param sender The object that raised the event.
        /// @param e The mouse event arguments.
        private void Code_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            UpdateScroll();
        }
        /// It updates the scrollbar.
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void textBox_Scroll(object sender, EventArgs e)
        {
            UpdateScroll();
        }

       /// It takes the number of lines in the textbox and adds them to the linebox
       /// 
       /// @param sender The object that raised the event.
       /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            lineBox.Text = "";
            for (int i = 0; i < textBox.Lines.Length; i++)
            {
                lineBox.Text += (i + 1).ToString() + Environment.NewLine;
            }
            UpdateScroll();
        }

        /// When the font of the textbox changes, the font of the linebox changes to match it
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void textBox_FontChanged(object sender, EventArgs e)
        {
            lineBox.Font = textBox.Font;
        }

        /// > When the code view is resized, update the scroll bar
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void CodeView_Resize(object sender, EventArgs e)
        {
            UpdateScroll();
        }
    }
}
