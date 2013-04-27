namespace AdminElevator
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Shapes;

    /// <summary>
    /// WindowPrototype class.
    /// </summary>
    public class WindowPrototype : Window
    {
        private const Int32 WM_SYSCOMMAND = 0x112;

        private static readonly TimeSpan s_doubleClick;

        private HwndSource m_hwndSource;
        private DateTime m_headerLastClicked;

        /// <summary>
        /// Initializes the <see cref="WindowPrototype"/> class.
        /// </summary>
        static WindowPrototype()
        {
            WindowPrototype.s_doubleClick = TimeSpan.FromMilliseconds(500);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowPrototype"/> class.
        /// </summary>
        public WindowPrototype()
            : base()
        {
            this.SourceInitialized += this.HandleSourceInitialized;

            this.GotKeyboardFocus += this.HandleGotKeyboardFocus;
            this.LostKeyboardFocus += this.HandleLostKeyboardFocus;

            this.Closing += this.WindowPrototype_Closing;
        }

        /// <summary>
        /// Handles the Closing event of the WindowPrototype control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        public void WindowPrototype_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("You sure to quit?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the source initialized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleSourceInitialized(Object sender, EventArgs e)
        {
            m_hwndSource = (HwndSource)PresentationSource.FromVisual(this);

            // Returns the HwndSource object for the window
            // which presents WPF content in a Win32 window.
            HwndSource.FromHwnd(m_hwndSource.Handle).AddHook(
                new HwndSourceHook(NativeMethods.WindowProc));

            // http://msdn.microsoft.com/en-us/library/aa969524(VS.85).aspx
            Int32 DWMWA_NCRENDERING_POLICY = 2;
            NativeMethods.DwmSetWindowAttribute(
                m_hwndSource.Handle,
                DWMWA_NCRENDERING_POLICY,
                ref DWMWA_NCRENDERING_POLICY,
                4);

            // http://msdn.microsoft.com/en-us/library/aa969512(VS.85).aspx
            NativeMethods.ShowShadowUnderWindow(m_hwndSource.Handle);
        }

        /// <summary>
        /// Handles the preview mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs" />
        /// instance containing the event data.</param>
        [DebuggerStepThrough]
        public void HandlePreviewMouseMove(Object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Handles the header preview mouse down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs" />
        /// instance containing the event data.</param>
        public void HandleHeaderPreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {

            if (DateTime.Now.Subtract(m_headerLastClicked) <= s_doubleClick)
            {
                // Execute the code inside the event handler for the 
                // restore button click passing null for the sender
                // and null for the event args.
                HandleRestoreClick(null, null);
            }

            m_headerLastClicked = DateTime.Now;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Handles the minimize click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleMinimizeClick(Object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handles the restore click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleRestoreClick(Object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal)
                ? WindowState.Maximized : WindowState.Normal;

            (this.FindName("m_frameGrid") as Grid).IsHitTestVisible
                = WindowState == WindowState.Maximized
                ? false : true;

            (this.FindName("m_resize") as Path).Visibility = (WindowState == WindowState.Maximized)
                ? Visibility.Hidden : Visibility.Visible;

            (this.FindName("m_roundBorder") as Border).Visibility = (WindowState == WindowState.Maximized)
                ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Handles the got keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/>
        /// instance containing the event data.</param>
        public void HandleGotKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e)
        {
            (this.FindName("m_roundBorder") as Border).Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the lost keyboard focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/>
        /// instance containing the event data.</param>
        public void HandleLostKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e)
        {
            (this.FindName("m_roundBorder") as Border).Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the close click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleCloseClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the rectangle mouse move.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleRectangleMouseMove(Object sender, MouseEventArgs e)
        {
            Rectangle clickedRectangle = (Rectangle)sender;

            switch (clickedRectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the rectangle preview mouse down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> 
        /// instance containing the event data.</param>
        public void HandleRectanglePreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            Rectangle clickedRectangle = (Rectangle)sender;

            switch (clickedRectangle.Name)
            {
                case "top":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "bottom":
                    Cursor = Cursors.SizeNS;
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "left":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "right":
                    Cursor = Cursors.SizeWE;
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "topLeft":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "topRight":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "bottomLeft":
                    Cursor = Cursors.SizeNESW;
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "bottomRight":
                    Cursor = Cursors.SizeNWSE;
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Resizes the window.
        /// </summary>
        /// <param name="direction">The direction.</param>
        public void ResizeWindow(ResizeDirection direction)
        {
            NativeMethods.SendMessage(m_hwndSource.Handle, WM_SYSCOMMAND,
                (IntPtr)(61440 + direction), IntPtr.Zero);
        }
    }
}
