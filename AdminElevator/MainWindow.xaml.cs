namespace AdminElevator {
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Win32;
    using SysIO = System.IO;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowPrototype
    {
		/// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
		public MainWindow() : base() {
            this.InitializeComponent();

            this.listView.AllowDrop = true;
            this.listView.DragEnter += this.MainWindow_DragEnter;
            this.listView.Drop += this.MainWindow_Drop;

			this.GetListFromRegistry(true);
		}

        /// <summary>
        /// Adds files into list.
        /// </summary>
        /// <returns></returns>
        protected void AddFiles(string[] files)
        {
            RegistryKey _registryKey = this.GetRegistryKey();
            try
            {
                foreach (string _fileName in files)
                {
                    _registryKey.SetValue(_fileName, "RUNASADMIN", RegistryValueKind.String);
                }
                _registryKey.Flush();
            }
            finally
            {
                _registryKey.Close();
            }
        }

        /// <summary>
        /// Handles drop event for the window.
        /// </summary>
        /// <returns></returns>
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.AddFiles(files);

            this.GetListFromRegistry(false); 
        }

        /// <summary>
        /// Handles dragenter event for the window.
        /// </summary>
        /// <returns></returns>
        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Gets the registry key.
        /// </summary>
        /// <returns></returns>
		private RegistryKey GetRegistryKey() {
            RegistryKey _appCompatFlags = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags", RegistryKeyPermissionCheck.ReadWriteSubTree);
            RegistryKey _layers = _appCompatFlags.OpenSubKey("Layers", RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            if(_layers == null) {
                _layers = _appCompatFlags.CreateSubKey("Layers", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            return _layers;
		}

        /// <summary>
        /// Gets the list from registry.
        /// </summary>
        /// <param name="isEmptyAlready">if set to <c>true</c> [is empty already].</param>
		private void GetListFromRegistry(bool isEmptyAlready) {
			if(!isEmptyAlready) {
				this.listView.Items.Clear();
			}

			RegistryKey _registryKey = this.GetRegistryKey();
			try {
				foreach(string _keyName in _registryKey.GetValueNames()) {
					object _data = _registryKey.GetValue(_keyName);
					if(_data == null) {
						continue;
					}

					if(((string)_data).Contains("RUNASADMIN")) {
						this.listView.Items.Add(new ListViewItem() { Content = SysIO.Path.GetFileName(_keyName), Tag = _keyName });
					}
				}

				if(this.listView.Items.Count > 0) {
					this.listView.SelectedItem = this.listView.Items[0];
					// (this.listView.Items[0] as ListViewItem).IsSelected = true;
				}
			}
			finally {
				_registryKey.Close();
			}
		}

        /// <summary>
        /// Handles the Click event of the btnAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnAdd_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog _openFileDialog = new OpenFileDialog() {
				AddExtension = true,
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = ".exe",
				DereferenceLinks = true,
				Filter = "Executable Files|*.exe|All Files|*.*",
				Multiselect = true,
				ShowReadOnly = false,
				ValidateNames = true
			};

			bool? _result = _openFileDialog.ShowDialog();

			if(!_result.HasValue || !_result.Value) {
				return;
			}

            this.AddFiles(_openFileDialog.FileNames);

			this.GetListFromRegistry(false);
		}

        /// <summary>
        /// Handles the Click event of the btnRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnRemove_Click(object sender, RoutedEventArgs e) {
			if(this.listView.SelectedItems.Count <= 0) {
				MessageBox.Show("Select the items first.", this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			if(MessageBox.Show("Are you sure to delete all selected entries?", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes) {
				return;
			}

			RegistryKey _registryKey = this.GetRegistryKey();
			try {
				foreach(ListViewItem _listViewItem in this.listView.SelectedItems) {
					_registryKey.DeleteValue((string)_listViewItem.Tag);
				}
			}
			finally {
				_registryKey.Close();
			}

			this.GetListFromRegistry(false);
		}

        /// <summary>
        /// Handles the SelectionChanged event of the listView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
		private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(this.listView.SelectedItems.Count <= 0) {
				this.fullPathLabel.Content = "";
				return;
			}

			this.fullPathLabel.Content = (string)((this.listView.SelectedItems[0] as ListViewItem).Tag);
		}

        /// <summary>
        /// Handles the Click event of the btnAbout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
		private void btnAbout_Click(object sender, RoutedEventArgs e) {
			if(MessageBox.Show(
                    "This program is designed to be a part of Knopfler Desktop Tools.\nDo you want to visit http://eser.ozvataf.com/ for checking latest updates?\n\nHomepage: http://eser.ozvataf.com/\nGithub: https://github.com/larukedi/AdminElevator\nE-mail: eser@sent.com",
                    this.Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information
                ) != MessageBoxResult.Yes) {
				return;
			}

			Process.Start("http://eser.ozvataf.com/");
		}
	}
}
