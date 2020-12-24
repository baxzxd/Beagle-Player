using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using TagLib;

namespace Music_Player_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Threading.Timer _timer;
        private DispatcherTimer dispatcherTimer;
        private List<SongData> playlist;
        private Window1 win2;
        private string music_path;
        public float SliderToVolume
        {
            get
            {
                return (float)control_Volume_Slider.Value / 100f;
            }
        }
        public MainWindow()
        {
            //XAML Stuffs
            /*
             * <Image x:Name="AlbumImage" Width="100" Height="50" Source="{Binding AlbumImage_Source}" />
             * 
                    <GridViewColumn x:Name="Header_Album" Header="Album"  Width="200">
                        <GridViewColumn.CellTemplate>
                            <DataTemplate>
                                <StackPanel Orientation="Horizontal">
                                    <Label Content="{Binding AlbumName}" Height="30"  />
                                </StackPanel>
                            </DataTemplate>
                        </GridViewColumn.CellTemplate>
                    </GridViewColumn>
             */

            InitializeComponent();
            InitializeTimer();
            InitializeControlEvents();

            MainGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MainGrid.Arrange(new Rect(0, 0, MainGrid.DesiredSize.Width, MainGrid.DesiredSize.Height));

            playlist = new List<SongData>();
            
            headerTitle.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            control_Timestamp.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));

            MainGrid.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));

            //ColorGrid();
            string primus_path = @"D:\My Music\Alice in Chains";
            string image_path = "C:\\Users\\Adam Mason\\Pictures\\Album Covers\\Alice in Chains\\Dirt.jpg";


            Image testImage = new Image();
            testImage.Source = new BitmapImage(new Uri(image_path, UriKind.Relative));

            control_Play_Image.Source = MyConstants.Play_Idle_Icon;
            control_Next_Image.Source = MyConstants.ffw_Idle_Icon;
            control_Previous_Image.Source = MyConstants.reverse_Idle_Icon;

            headerImage.Source = MyConstants.HeaderImage;

            Console.WriteLine("Begin file search");
            string current_directory = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(current_directory);
            for( int i = 0; i < files.Length; i++ )
            {
                string file_name = files[i].Substring(current_directory.Length + 1);
                Console.WriteLine(file_name);
                if( file_name == "config.txt" )
                {
                    Console.WriteLine("Found config file");
                    string config_text = @System.IO.File.ReadAllText(files[i]);
                    GetSongsFromPath(config_text);
                    Console.WriteLine(config_text);
                    string[] s = Directory.GetFiles(config_text);
                    for( int j = 0; j < s.Length; j++ )
                    {
                        Console.WriteLine(s[j]);
                    }
                    break;
                }
            }
        }

        private void GetSongsFromPath(string p)
        {
            //Remove old songs need to create cache
            if(this.testListView.Items.Count > 0 )
            {
                for( int i = 0; i < this.testListView.Items.Count; i++ )
                {
                    this.testListView.Items.RemoveAt(0);
                }
            }


            // Populate list
            Dictionary<string, List<SongData>> albums = UsefulFunctions.GetSongDataFromPath(p);
            foreach (KeyValuePair<string, List<SongData>> t in albums)
            {
                for (int i = 0; i < t.Value.Count; i++)
                {
                    SongData temp_song = t.Value[i];
                    this.testListView.Items.Add(temp_song);
                }
            }
        }

        private void InitializeControlEvents()
        {
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(Window_SizeChanged);
            this.control_Volume_Slider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(Volume_Changed); 
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
            this.testListView.MouseDoubleClick += new MouseButtonEventHandler(PanelTest_MouseEnter);

            this.control_Play_Image.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Play_Pressed);
            this.control_Play_Image.MouseEnter += new System.Windows.Input.MouseEventHandler(Play_Entered);
            this.control_Play_Image.MouseLeave += new System.Windows.Input.MouseEventHandler(Play_Left);
        }

        private void Play_Entered(object sender, EventArgs e)
        {
            control_Play_Image.Source = MyConstants.Play_Pressed_Icon;
        }

        private void Play_Left(object sender, EventArgs e)
        {
            control_Play_Image.Source = MyConstants.Play_Idle_Icon;
        }
        private void SetPath_Clicked(object sender, EventArgs e)
        {
            win2 = new Window1();
            win2.Show();
            win2.pathButton.Click += new RoutedEventHandler(SetPath_Button_Clicked);
        }

        private void SetPath_Button_Clicked(object sender, RoutedEventArgs e)
        {
            music_path = win2.pathTextBox.Text;
            GetSongsFromPath(music_path);
        }

        private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioPlayer.SetVolume(SliderToVolume);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            PlaybackBarUpdate();
        }

        public void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PositionControls()
        {
            double control_panel_width = MainGrid.ColumnDefinitions[0].ActualWidth;
            double control_panel_height = MainGrid.RowDefinitions[2].ActualHeight;

            double play_image_x = (control_panel_width / 2);// - (controlPanelButtonGroup.Width / 2);
            double play_image_y = (control_panel_height / 2);// - (controlPanelButtonGroup.Height / 2);

            //Margin="left,top,right,bottom"
            Thickness margin = new Thickness();
            margin.Left = 0;
            margin.Right = 0;
            margin.Top = 0;
            margin.Bottom = 0;
            controlPanelButtonGroup.Margin = margin;
            Console.WriteLine("Left Margin:" + play_image_x);

            playbackBar.Width = control_panel_width - 20;
            playbackBar.Height = 5;
            Thickness progressBar_Margin = new Thickness();
            progressBar_Margin.Left = 10;
            progressBar_Margin.Right = 10;
            progressBar_Margin.Top = 5;
            progressBar_Margin.Bottom = 5;
            playbackBar.Margin = progressBar_Margin;
        }

        private void PanelTest_MouseEnter(object sender, MouseEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                SongData i = (SongData)item;
                AudioPlayer.Open(i.path);
                AudioPlayer.Play();

                long total_seconds = (long)(AudioPlayer.soundOut.WaveSource.Length / AudioPlayer.soundOut.WaveSource.WaveFormat.BytesPerSecond);
                int minutes = (int)(total_seconds / 60);
                int seconds = (int)(total_seconds - (minutes * 60));
                string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
                AudioPlayer.currentLength = time_string;
                playlistListView.Items.Add(i);

                var tfile = TagLib.File.Create(i.path);
                nowPlaying_Art.Source = UsefulFunctions.GetAlbumArt(tfile);
                nowPlaying_Title.Text = tfile.Tag.Title;
                nowPlaying_Album.Text = tfile.Tag.Album;
                nowPlaying_Artist.Text = tfile.Tag.Artists[0];
            }
        }

        private void Play_Pressed(object sender, MouseButtonEventArgs e)
        {
            if (AudioPlayer.soundOut == null)
                return;

            if (AudioPlayer.soundOut.PlaybackState == CSCore.SoundOut.PlaybackState.Playing)
            {
                AudioPlayer.Stop();
            }
            else
            {
                AudioPlayer.Play();
            }
        }

        private void Window_SizeChanged(object sender, System.EventArgs e)
        {
            Console.WriteLine("Size Changed");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AudioPlayer.CleanupPlayback();
        }

        private void PlaybackBarUpdate()
        {
            if (AudioPlayer.soundOut != null)
            {
                if (AudioPlayer.soundOut.PlaybackState == CSCore.SoundOut.PlaybackState.Playing)
                {
                    int resolution = 10000;
                    float p = (float)AudioPlayer.soundOut.WaveSource.Position / (float)AudioPlayer.soundOut.WaveSource.Length;
                    int progress = (int)(p * resolution);
                    playbackBar.Maximum = resolution;
                    playbackBar.Value = UsefulFunctions.Clamp(progress, 0, resolution);

                    //playbackHandle. = new Point((int)(playbackBar.Width * p) - playbackBar.Width / 2, 0);

                    long total_seconds = (long)(AudioPlayer.soundOut.WaveSource.Position / AudioPlayer.soundOut.WaveSource.WaveFormat.BytesPerSecond);
                    int minutes = (int)(total_seconds / 60);
                    int seconds = (int)(total_seconds - (minutes * 60));
                    string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
                    control_Timestamp.Text = time_string + "/" + AudioPlayer.currentLength;
                    //Console.WriteLine(timestamp.Text);
                }
            }
        }

        private void InitializeTimer()
        {
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            dispatcherTimer.Tick += new EventHandler(Timer_Tick);

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(200);
            dispatcherTimer.Start();
        }
    }

}
