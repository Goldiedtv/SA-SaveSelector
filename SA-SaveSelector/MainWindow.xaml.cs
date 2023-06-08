using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace SA_SaveSelector {

	public enum Section {
		LosSantos,
		BadLands,
		SanFierro,
		Desert,
		LasVenturas,
		ReturnLS
	}

	public struct MissionSaveFile {
		public string Name { get; set; }
		public string Path { get; set; }
		public MissionSaveFile(string _name, string _path) {
			Name = _name;
			Path = _path;
		}

		public override string ToString() {
			return Name;
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private MemoryStream zipStream = new MemoryStream();

		private bool hasInitialized = false;

		//private string saveFolderPath = "";
		private string SASaveFolderPath = "";

		private string saveSlotName = "GTASAsf1.b";

		public MainWindow() {
			InitializeComponent();

			hasInitialized = true;

			SASaveFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\GTA San Andreas User Files";

			//saveFolderPath = ConfigurationManager.AppSettings["savesFolder"];

			InitializeSaves();
		}

		private void InitializeSaves() {

			if (!hasInitialized)
				return;

			SaveLocationTextBox.Text = SASaveFolderPath;

			if (!System.IO.Directory.Exists(SASaveFolderPath)) {
				WriteOutput("SA Save folder not found.");
				MainBorder.IsEnabled = false;
				return;
			}

			MissionComboBox.Items.Clear();

			if (LSRadio.IsChecked == true) {
				PopulateCombobox(Section.LosSantos);
			}
			else if (BadlandsRadio.IsChecked == true) {
				PopulateCombobox(Section.BadLands);
			}
			else if (SFRadio.IsChecked == true) {
				PopulateCombobox(Section.SanFierro);
			}
			else if (DesertRadio.IsChecked == true) {
				PopulateCombobox(Section.Desert);
			}
			else if (LVRadio.IsChecked == true) {
				PopulateCombobox(Section.LasVenturas);
			}
			else if (ReturnRadio.IsChecked == true) {
				PopulateCombobox(Section.ReturnLS);
			}

			MainBorder.IsEnabled = true;
			WriteOutput("All good.");
		}

		private MemoryStream ReturnZipStream(Section section) {
			MemoryStream _zipStream = new MemoryStream();

			switch (section) {
				case Section.LosSantos:
					_zipStream = new MemoryStream(Properties.Resources.LosSantos);
					break;
				case Section.BadLands:
					_zipStream = new MemoryStream(Properties.Resources.Badlands);
					break;
				case Section.SanFierro:
					_zipStream = new MemoryStream(Properties.Resources.SanFierro);
					break;
				case Section.Desert:
					_zipStream = new MemoryStream(Properties.Resources.Desert);
					break;
				case Section.LasVenturas:
					_zipStream = new MemoryStream(Properties.Resources.LasVenturas);
					break;
				case Section.ReturnLS:
					_zipStream = new MemoryStream(Properties.Resources.ReturnLS);
					break;
			}
			return _zipStream;
		}

		private void PopulateCombobox(Section section) {
			zipStream = ReturnZipStream(section);

			using (ZipArchive archive = new ZipArchive(zipStream)) {
				foreach (ZipArchiveEntry entry in archive.Entries) {
					if (entry.Name != "") {
						var split = entry.FullName.Split('/');
						int countOfSameName = 1;
						foreach (MissionSaveFile item in MissionComboBox.Items) {
							if (item.Name == split[0])
								countOfSameName++;
						}
						if (countOfSameName > 1)
							MissionComboBox.Items.Add(new MissionSaveFile(split[0]+countOfSameName.ToString(), entry.FullName));
						else
							MissionComboBox.Items.Add(new MissionSaveFile(split[0], entry.FullName));
					}
				}
			}
			MissionComboBox.SelectedIndex = 0;
		}

		private void WriteOutput(string output) {
			outputField.Text = output;
		}

		private void SaveToSlotButton_Click(object sender, RoutedEventArgs e) {
			string selectedFilePath = ((MissionSaveFile)MissionComboBox.SelectedItem).Path;

			if (LSRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.LosSantos);
			}
			else if (BadlandsRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.BadLands);
			}
			else if (SFRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.SanFierro);
			}
			else if (DesertRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.Desert);
			}
			else if (LVRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.LasVenturas);
			}
			else if (ReturnRadio.IsChecked == true) {
				zipStream = ReturnZipStream(Section.ReturnLS);
			}

			if (System.IO.File.Exists(Path.Combine(SASaveFolderPath, saveSlotName))) {
				var res = MessageBox.Show($"There is a savefile in {saveSlotName}. Overwrite?", "Save File Exists", MessageBoxButton.YesNo);

				if (res == MessageBoxResult.No) {
					WriteOutput("Save File copy canceled!");
					return;
				}
			}

			using (ZipArchive archive = new ZipArchive(zipStream)) {
				foreach (ZipArchiveEntry entry in archive.Entries) {
					if (entry.FullName == selectedFilePath) {
						string destinationPath = Path.GetFullPath(Path.Combine(SASaveFolderPath, saveSlotName));

						// Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
						// are case-insensitive.
						if (destinationPath.StartsWith(SASaveFolderPath, StringComparison.Ordinal))
							entry.ExtractToFile(destinationPath, true);
					}
				}
			}
			WriteOutput(selectedFilePath + "\nwas copied to:\n" + Path.Combine(SASaveFolderPath, saveSlotName));
		}

		private void browseButton_Click(object sender, RoutedEventArgs e) {

			VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
			dlg.ShowNewFolderButton = true;

			if (dlg.ShowDialog() == true) {
				SASaveFolderPath = dlg.SelectedPath;
				InitializeSaves();
			}
		}

		private void MissionRadio_Checked(object sender, RoutedEventArgs e) {
			InitializeSaves();
		}

		private void SaveRadioButton_Checked(object sender, RoutedEventArgs e) {
			if (SaveSlotRadio1.IsChecked == true)
				saveSlotName = "GTASAsf1.b";
			else if (SaveSlotRadio2.IsChecked == true)
				saveSlotName = "GTASAsf2.b";
			else if (SaveSlotRadio3.IsChecked == true)
				saveSlotName = "GTASAsf3.b";
			else if (SaveSlotRadio4.IsChecked == true)
				saveSlotName = "GTASAsf4.b";
			else if (SaveSlotRadio5.IsChecked == true)
				saveSlotName = "GTASAsf5.b";
			else if (SaveSlotRadio6.IsChecked == true)
				saveSlotName = "GTASAsf6.b";
			else if (SaveSlotRadio7.IsChecked == true)
				saveSlotName = "GTASAsf7.b";
			else if (SaveSlotRadio8.IsChecked == true)
				saveSlotName = "GTASAsf8.b";
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			//Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			//config.AppSettings.Settings["savesFolder"].Value = SaveLocationTextBox.Text;
			//config.Save(ConfigurationSaveMode.Modified);
		}
	}
}
