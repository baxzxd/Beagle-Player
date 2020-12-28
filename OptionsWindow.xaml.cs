using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Music_Player_WPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            for (int i = 0; i < MyConstants.user_preferences.music_directories.Count; i++)
            {
                this.listPaths.Items.Add(MyConstants.user_preferences.music_directories[i]);
            }

            this.pathButton.Click += new RoutedEventHandler(SetPath_Button_Clicked);
            this.configSave.Click += new RoutedEventHandler(SaveConfig_Button_Clicked);
            this.removePath.Click += new RoutedEventHandler(RemoveSelectedPath_Clicked);
            this.clearCache.Click += new RoutedEventHandler(ClearCache_Clicked);

            //TODO - add to main class
            //this.scanSongs.Click += new RoutedEventHandler(ScanButton_Clicked);
        
        InitializeComponent();
        }

        private void SetPath_Button_Clicked(object sender, RoutedEventArgs e)
        {
            string text = this.pathTextBox.Text;
            if (UsefulFunctions.IsPathValid(text))
            {
                Console.WriteLine("Path Added");
                ListBoxItem lvi = new ListBoxItem();
                lvi.Content = this.pathTextBox.Text;
                this.listPaths.Items.Add(lvi);
            }
            else
            {
                Console.WriteLine("Invalid path");
            }
            this.pathTextBox.Text = "";
        }

        private void SaveConfig_Button_Clicked(object sender, RoutedEventArgs e)
        {
            MyConstants.user_preferences.music_directories = new List<string>();
            for (int i = 0; i < this.listPaths.Items.Count; i++)
            {
                ListBoxItem lbi = (ListBoxItem)this.listPaths.Items[i];
                MyConstants.user_preferences.music_directories.Add(lbi.Content.ToString());
                Console.WriteLine("Saved directory : " + lbi.Content.ToString());
            }
        }
        private void RemoveSelectedPath_Clicked(object sender, RoutedEventArgs e)
        {
            this.listPaths.Items.Remove(this.listPaths.SelectedItem);
        }
        private void ClearCache_Clicked(object sender, RoutedEventArgs e)
        {
            MediaTools.ClearCache();
            //ClearSongs();
        }


    }
}
