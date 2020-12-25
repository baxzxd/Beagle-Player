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
        private DispatcherTimer dispatcherTimer;
        private List<SongData> playlist;
        private Window1 win2;
        private string music_path;

        private PlayerButton button_Play;
        private PlayerButton button_Next;
        private PlayerButton button_Previous;

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
            StyleControls();

            MainGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MainGrid.Arrange(new Rect(0, 0, MainGrid.DesiredSize.Width, MainGrid.DesiredSize.Height));

            playlist = new List<SongData>();
            
            //Make options thingy
            string current_directory = Directory.GetCurrentDirectory();
            string[] files = Directory.GetFiles(current_directory);
            for( int i = 0; i < files.Length; i++ )
            {
                string file_name = files[i].Substring(current_directory.Length + 1);
                if( file_name == "config.txt" )
                {
                    string config_text = @System.IO.File.ReadAllText(files[i]);
                    GetSongsFromPath(config_text);
                    break;
                }
            }
        }

        private void StyleControls()
        {
            //Create button objects to add hover effect (Prolly not the best way but it works)
            button_Play = new PlayerButton(control_Play_Image, MyConstants.Play_Idle_Icon, MyConstants.Play_Pressed_Icon);
            button_Next = new PlayerButton(control_Next_Image, MyConstants.Next_Idle_Icon, MyConstants.Next_Pressed_Icon);
            button_Previous = new PlayerButton(control_Previous_Image, MyConstants.Previous_Idle_Icon, MyConstants.Previous_Pressed_Icon);

            //Set images
            headerImage.Source = MyConstants.HeaderImage;

            //Set Colors
            //Standardize colors and later on allow customization
            SolidColorBrush text_color = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            headerTitle.Foreground = text_color;
            control_Timestamp.Foreground = text_color;
            nowPlaying_Title.Foreground = text_color;
            nowPlaying_Album.Foreground = text_color;
            nowPlaying_Artist.Foreground = text_color;
            MainGrid.Background = new SolidColorBrush(Color.FromRgb(45, 45, 45));

            //Font setups
            nowPlaying_Title.FontSize = 12;
        }

        private void GetSongsFromPath(string p)
        {
            //Remove old songs need to create cache
            if(this.mainListView.Items.Count > 0 )
            {
                for( int i = 0; i < this.mainListView.Items.Count; i++ )
                {
                    this.mainListView.Items.RemoveAt(0);
                }
            }

            //Populate mainListView
            Dictionary<string, AlbumData> albums = MediaTools.GetSongDataFromPath(p, new Dictionary<string, AlbumData>());
            if( albums != null )
            {
                foreach (KeyValuePair<string, AlbumData> t in albums)
                {
                    MediaTools.SaveAlbum(t.Value, @"C:\Users\Adam Mason\source\repos\Beagle-Player\bin\Debug\");
                    for (int i = 0; i < t.Value.tracks.Count; i++)
                    {
                        SongData temp_song = t.Value.tracks[i];
                        this.mainListView.Items.Add(temp_song);
                    }
                }
            }
        }
        

        private void InitializeControlEvents()
        {
            //Check if the window size has been changed (Does nothing yet as it doesnt need to)
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(Window_SizeChanged);

            //Volume changed
            this.control_Volume_Slider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(Volume_Changed); 

            //Song double clicked in main window
            this.mainListView.MouseDoubleClick += new MouseButtonEventHandler(PanelTest_MouseEnter);

            //Play button actions (Can prolly make this more standard)
            this.control_Play_Image.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Play_Pressed);

            //Allow user to click on playbackBar to scrub
            playbackBar.MouseDown += new System.Windows.Input.MouseButtonEventHandler(PlaybackBar_Clicked);

            //Cleanup before closing
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);
        }

        private void PlaybackBar_Clicked(object sender, MouseEventArgs e)
        {
            int new_value = (int)((e.GetPosition(playbackBar).X / playbackBar.ActualWidth) * playbackBar.Maximum);
            playbackBar.Value = new_value;
        }

        private void SetPath_Clicked(object sender, EventArgs e)
        {
            win2 = new Window1();
            win2.Show();
            win2.pathButton.Click += new RoutedEventHandler(SetPath_Button_Clicked);
            win2.configSave.Click += new RoutedEventHandler(SaveConfig_Button_Clicked);
        }

        private void SetPath_Button_Clicked(object sender, RoutedEventArgs e)
        {
            //win2.configText.Text += "\n" + win2.pathTextBox.Text;
        }
        private void SaveConfig_Button_Clicked(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine(win2.configText.Text);
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

        private void PanelTest_MouseEnter(object sender, MouseEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                SongData i = (SongData)item;
                AudioPlayer.Open(i.path);
                AudioPlayer.Play();
                AudioPlayer.currentLength = AudioPlayer.GetLengthString();

                playlistListView.Items.Add(i);

                //Setup caching of art by artist perhaps?
                //nowPlaying_Art.Source = UsefulFunctions.GetAlbumArt(tfile);

                nowPlaying_Title.Text = i.Title;
                nowPlaying_Album.Text = i.AlbumName; 
                nowPlaying_Artist.Text = i.Artist;
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

                    long total_seconds = (long)(AudioPlayer.soundOut.WaveSource.Position / AudioPlayer.soundOut.WaveSource.WaveFormat.BytesPerSecond);
                    int minutes = (int)(total_seconds / 60);
                    int seconds = (int)(total_seconds - (minutes * 60));
                    string time_string = minutes.ToString("00") + ":" + seconds.ToString("00");
                    control_Timestamp.Text = time_string + "/" + AudioPlayer.currentLength;
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
