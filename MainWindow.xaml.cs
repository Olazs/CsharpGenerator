using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NameGenerator
{
    public partial class MainWindow : Window
    {
        private OpenFileDialog OFG;
        private SaveFileDialog SFG;

        private ObservableCollection<string> xSurname;
        private ObservableCollection<string> xForename;
        private ObservableCollection<string> miscName;

        private uint _maxNameCount;
        public uint MaxNameCount { get => _maxNameCount;}

        public MainWindow()
        {
            InitializeComponent();
            xSurname = new ObservableCollection<string>();
            xForename = new ObservableCollection<string>();
            miscName = new ObservableCollection<string>();
            OFG = InitializeOpenFileDialog();
            SFG = InitializeSaveFileDialog();
            ReloadLists();
        }
        private string GetDownloadFolderPath()
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();
        }
        private SaveFileDialog InitializeSaveFileDialog() 
        { 
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = "txt";
            saveFileDialog.Filter = "Szöveges fájl (*.txt) | *.txt|CSV fájl (*.csv) |*.csv|Összes fájl (*.*) |*.*";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Title = "Adja meg hová szeretné menteni a fájlt.";
            return saveFileDialog;
        }
        private OpenFileDialog InitializeOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = "txt";
            openFileDialog.Filter = "Szöveges fájl (*.txt) | *.txt|CSV fájl (*.csv) |*.csv|Összes fájl (*.*) |*.*";
            openFileDialog.InitialDirectory = GetDownloadFolderPath();
            openFileDialog.Title = "Adja meg a fájlt.";
            return openFileDialog;
        }
        private ObservableCollection<string> LoadNamesToList(ObservableCollection<string> parent, List<string> names)
        {
            parent = parent ?? new ObservableCollection<string>();
            foreach (string name in names)
            {
                parent.Add(name);
            }
            return parent;
        }
        private void ReloadLists()
        {
            SurList.ItemsSource = xSurname;
            FornList.ItemsSource = xForename;
            CombinedList.ItemsSource = miscName;
            SetMaxNameCount();
            JumpToTheEndOfNameList();
            StatusBarSort.Content = "";
        }
        private void SetMaxNameCount() 
        {
            int a = xSurname.Count;
            int b = xForename.Count;
            _maxNameCount = Convert.ToUInt32(a < b ? a : b);
            sliderValue.Maximum = MaxNameCount;
        }
        private void JumpToTheEndOfNameList()
        {
            SurList.Items.MoveCurrentToLast();
            FornList.Items.MoveCurrentToLast();
            CombinedList.Items.MoveCurrentToLast();
            SurList.ScrollIntoView(SurList.Items.CurrentItem);
            FornList.ScrollIntoView(FornList.Items.CurrentItem);
            CombinedList.ScrollIntoView(CombinedList.Items.CurrentItem);
        }
        private void NamesLoader(object sender, RoutedEventArgs e)
        {
            if (OFG.ShowDialog() == true)
            {
                List<string> temp = new List<string>();
                var source = e.Source as Button;

                foreach (string filename in OFG.FileNames)
                {
                    foreach (string line in File.ReadAllLines(filename))
                    {
                        temp.Add(line);
                    }
                }
                switch (source.Name)
                {
                    case "SurnameLoadButton":
                        xSurname = LoadNamesToList(xSurname, temp);
                        break;
                    case "ForenameLoadButton":
                        xForename = LoadNamesToList(xForename, temp);
                        break;
                    default:
                        MessageBox.Show("Unhandled source.", "Error in NamesLoader", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }
                ReloadLists();
            }
        }
        private void NameDeleter(object sender, MouseButtonEventArgs e)
        {
            var source = e.Source as ListBox;
            try
            {
                switch (source.Name)
                {
                    case "SurList":
                        xSurname.RemoveAt(SurList.SelectedIndex);
                        break;
                    case "FornList":
                        xForename.RemoveAt(FornList.SelectedIndex);
                        break;
                    case "CombinedList":
                        miscName.RemoveAt(CombinedList.SelectedIndex);
                        break;
                    default:
                        throw new Exception("Nincs lekezelve ez a forrás.");
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("Üres a lista nem tudsz benne törölni.", "Error in NameDeleter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in NameDeleter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ReloadLists();
            }
        }
        private void DeleteNamesButton_Click(object sender, RoutedEventArgs e)
        {
            miscName.Clear();
            ReloadLists();
        }
        private void GenerateNamesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = Convert.ToInt32(sliderValue.Value);

                if (count <= 0)
                {
                    throw new Exception("0 vagy annál kevesebb nevet nem tudsz generálni.");
                }

                Random r = new Random();
                string surname = String.Empty;
                string forename = String.Empty;
                string middlename = null;
                
                for (int i = 0; i < count; i++)
                {
                    surname = xSurname[r.Next(0, xSurname.Count)];
                    forename = xForename[r.Next(0, xForename.Count)];
                    xSurname.Remove(surname);
                    xForename.Remove(forename);
                    if (!(bool)rbSelectionOne.IsChecked)
                    {
                        middlename = xForename[r.Next(0, xForename.Count)];
                        xForename.Remove(middlename);
                    }
                    miscName.Add($"{surname} {forename}{(middlename == null ? "": " " + middlename)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in GenerateNames", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ReloadLists();
            }
        }
        private void SortNamesButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> temp = new ObservableCollection<string>(miscName.OrderBy(p => p));
            miscName.Clear();
            miscName = temp;
            ReloadLists();
            StatusBarSort.Content = "Rendezett névsor!";
        }
        private void SaveNamesButton_Click(object sender, RoutedEventArgs e)
        {
            if (SFG.ShowDialog() == true)
            {
                try
                {
                    if (Path.GetExtension(SFG.FileName) == ".csv")
                    {
                        List<string> temp = new List<string>();
                        temp = miscName.ToList();
                        for (int i = 0; i < temp.Count(); i++)
                        {
                            temp[i] = temp[i].Replace(" ", ";");
                        }
                        File.WriteAllLines(SFG.FileName, temp);
                    }
                    else
                    {
                        File.WriteAllLines(SFG.FileName, miscName);
                    }
                    MessageBox.Show("Sikeres mentés!", "Mentés", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Sikertelen mentés!");
                }
            }
        }
    }
}
