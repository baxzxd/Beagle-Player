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
using System.Drawing;
using System.IO;
using System.ComponentModel;

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
        private OptionsWindow optionsWindow;
        private string music_path;

        private PlayerButton button_Play;
        private PlayerButton button_Next;
        private PlayerButton button_Previous;

        private System.ComponentModel.BackgroundWorker backgroundWorker1;

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

            MyConstants.init();

            //Temp
            backgroundWorker1 = new BackgroundWorker();
            BackgroundWorkerSetup();
            backgroundWorker1.RunWorkerAsync();

            MainGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            MainGrid.Arrange(new Rect(0, 0, MainGrid.DesiredSize.Width, MainGrid.DesiredSize.Height));

            playlist = new List<SongData>();


        }

        private void BackgroundWorkerSetup()
        {
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);

            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);

            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            backgroundWorker1.WorkerReportsProgress = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            MediaTools.ScanCacheForSongsWorker(worker, e);
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AddSongs();
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }


        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort(header, direction);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = mainListView.Items;

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void ClearSongs()
        {
            this.mainListView.Items.Clear();
        }

        private void AddSongs()
        {
            foreach (KeyValuePair<string, AlbumData> t in MediaTools.albums)
            {
                for (int i = 0; i < t.Value.tracks.Count; i++)
                {
                    this.mainListView.Items.Add(t.Value.tracks[i]);
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


        private void InitializeControlEvents()
        {
            //Check if the window size has been changed (Does nothing yet as it doesnt need to)
            this.SizeChanged += new System.Windows.SizeChangedEventHandler(Window_SizeChanged);

            //Volume changed
            this.control_Volume_Slider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(Volume_Changed);

            //Song double clicked in main window
            this.mainListView.MouseDoubleClick += new MouseButtonEventHandler(Song_DoubleClicked);
            this.playlistListView.MouseDoubleClick += new MouseButtonEventHandler(Song_DoubleClicked);

            //Play button actions (Can prolly make this more standard)
            this.control_Play_Image.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Play_Pressed);

            //Allow user to click on playbackBar to scrub
            playbackBar.MouseDown += new System.Windows.Input.MouseButtonEventHandler(PlaybackBar_Clicked);

            //Cleanup before closing
            this.Closing += new System.ComponentModel.CancelEventHandler(Window_Closing);

            this.testButton.Click += new RoutedEventHandler(testClicked);

        }

        private void testClicked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("test clicked");
            List<AlbumData> found_albums = MediaTools.GetAlbumByArtist("Alice in Chains");

            ClearSongs();

            for (int i = 0; i < found_albums.Count; i++)
            {
                for (int j = 0; j < found_albums[i].tracks.Count; j++)
                {
                    this.mainListView.Items.Add(found_albums[i].tracks[j]);
                }
            }
        }

        private void PlaybackBar_Clicked(object sender, MouseEventArgs e)
        {
            double percent = e.GetPosition(playbackBar).X / playbackBar.ActualWidth;
            int new_value = (int)(percent * playbackBar.Maximum);
            playbackBar.Value = new_value;
            AudioPlayer.soundOut.WaveSource.Position = (long)(AudioPlayer.soundOut.WaveSource.Length * percent);
        }

        public void Options_Clicked(object sender, EventArgs e)
        {
            optionsWindow = new OptionsWindow();
            optionsWindow.Show();
        }

        private void ScanButton_Clicked(object sender, RoutedEventArgs e)
        {
            ClearSongs();
            AddSongs();
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

        private void Song_DoubleClicked(object sender, MouseEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                //Setup caching of art by artist perhaps?
                string cache_directory = MyConstants.CurrentDirectory + @"\" + MyConstants.CacheDirectory + @"\";

                SongData i = (SongData)item;
                playlistListView.Items.Add(i);

                //Set now playing details
                nowPlaying_Title.Text = i.Title;
                nowPlaying_Album.Text = i.AlbumName;
                nowPlaying_Artist.Text = i.Artist;
                try
                {
                    BitmapImage im = UsefulFunctions.LoadBitmapImage(cache_directory + i.AlbumGUID);
                    nowPlaying_Art.Source = im;
                }
                catch (System.Exception)
                {
                    nowPlaying_Art.Source = MediaTools.DefaultSongCover;
                }


                AudioPlayer.Open(i.path);
                AudioPlayer.Play();
                AudioPlayer.SetVolume(SliderToVolume);
                AudioPlayer.currentLength = AudioPlayer.GetLengthString();
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

                    control_Timestamp.Text = AudioPlayer.CurrentTimestamp + "/" + AudioPlayer.currentLength;
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

        //Window Events
        private void Window_SizeChanged(object sender, System.EventArgs e)
        {
            Console.WriteLine("Size Changed");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AudioPlayer.CleanupPlayback();
        }

    }

}
