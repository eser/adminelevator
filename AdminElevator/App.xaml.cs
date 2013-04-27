namespace AdminElevator {
    using System;
    using System.Diagnostics;
    using System.Windows;
    
    /// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
		public App() {
			// this.Startup += this.Application_Startup;

			// If the app is running outside of the debugger then report the exception using
			// a ChildWindow control.
			if(!Debugger.IsAttached) {
    			AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
            }

			this.InitializeComponent();
		}

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			// NOTE: This will allow the application to continue running after an exception has been thrown
			// but not handled. 
			// For production applications this error handling should be replaced with something that will 
			// report the error to the website and stop the application.
			Exception _ex = e.ExceptionObject as Exception;
			MessageBox.Show(_ex.Message, _ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Stop);
		}
	}
}
