using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace Music_Player_WPF
{
    public class Preferences
    {
        public List<string> music_directories { get; set; }

        public int[] TextColor_rgb;
            
        public Preferences()
        {
        }
    }
    public static class MyConstants
    {
        public static string ConfigName = "config.bpc";
        public static string CacheDirectory = "cache";
        public static string CurrentDirectory = Directory.GetCurrentDirectory();

        public static Preferences user_preferences;

        public static BitmapImage HeaderImage = new BitmapImage(new Uri(@"/images/logo.png", UriKind.Relative));
        public static BitmapImage PlayImage = new BitmapImage(new Uri(@"/images/play_white.png", UriKind.Relative));

        public static BitmapImage Play_Idle_Icon = new BitmapImage(new Uri(@"/images/play_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage Play_Pressed_Icon = new BitmapImage(new Uri(@"/images/play_Pressed_Icon.png", UriKind.Relative));

        public static BitmapImage Next_Idle_Icon = new BitmapImage(new Uri(@"/images/ffw_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage Next_Pressed_Icon = new BitmapImage(new Uri(@"/images/ffw_Pressed_Icon.png", UriKind.Relative));

        public static BitmapImage Previous_Idle_Icon = new BitmapImage(new Uri(@"/images/reverse_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage Previous_Pressed_Icon = new BitmapImage(new Uri(@"/images/reverse_Pressed_Icon.png", UriKind.Relative));

        public static SolidColorBrush TextColor = new SolidColorBrush(Color.FromRgb(200, 200, 200));

        public static void init()
        {
            bool config_exists = LoadPreferences();
            if( !config_exists )
            {
                user_preferences = new Preferences();
                SavePreferences();
            }

            MediaTools.init();
        }

        public static bool LoadPreferences()
        {
            bool config_exists = false;
            string[] files = Directory.GetFiles(MyConstants.CurrentDirectory);
            for (int i = 0; i < files.Length; i++)
            {
                string file_name = files[i].Substring(MyConstants.CurrentDirectory.Length + 1);
                if (file_name == MyConstants.ConfigName)
                {
                    Preferences p = new Preferences();
                    XmlSerializer formatter = new XmlSerializer(p.GetType());
                    FileStream songDataFile = new FileStream(files[i], FileMode.Open);
                    byte[] buffer = new byte[songDataFile.Length];
                    songDataFile.Read(buffer, 0, (int)songDataFile.Length);
                    MemoryStream stream = new MemoryStream(buffer);
                    user_preferences = (Preferences)formatter.Deserialize(stream);

                    Console.WriteLine("Config file exists!");
                    config_exists = true;
                }
            }
            return config_exists;
        }

        public static void SavePreferences()
        {
            string full_path = CurrentDirectory + @"\" + ConfigName;
            FileStream outFile = File.Create(full_path);
            XmlSerializer formatter = new XmlSerializer(user_preferences.GetType());
            formatter.Serialize(outFile, user_preferences);
        }
    }
}
