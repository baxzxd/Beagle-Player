using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Music_Player_WPF
{
    public static class MyConstants
    {
        /*
        public static bool initCheck = false;
        public static System.Drawing.Color formBackColor = System.Drawing.Color.FromArgb(45, 45, 45);
        public static System.Drawing.Color headerBackColor = System.Drawing.Color.FromArgb(60, 60, 60);
        public static System.Drawing.Color headerTextColor = System.Drawing.Color.FromArgb(200, 200, 200);
        
        public static System.Drawing.Color resizablePanelBackColor = System.Drawing.Color.FromArgb(45, 45, 45);
        public static System.Drawing.Color resizablePanelFrontColor = System.Drawing.Color.FromArgb(80, 80, 80);
        public static System.Drawing.Color resizablePanelFrontColorSelected = System.Drawing.Color.FromArgb(100, 100, 100);

        public static Font headerFont = new Font("Microsoft Sans Serif", 12);
        public static Font bodyFont = new Font("Microsoft Sans Serif", 8);
        */

        public static BitmapImage HeaderImage = new BitmapImage(new Uri(@"/images/logo.png", UriKind.Relative));
        public static BitmapImage PlayImage = new BitmapImage(new Uri(@"/images/play_white.png", UriKind.Relative));

        public static BitmapImage Play_Idle_Icon = new BitmapImage(new Uri(@"/images/play_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage Play_Pressed_Icon = new BitmapImage(new Uri(@"/images/play_Pressed_Icon.png", UriKind.Relative));

        public static BitmapImage ffw_Idle_Icon = new BitmapImage(new Uri(@"/images/ffw_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage ffw_Pressed_Icon = new BitmapImage(new Uri(@"/images/ffw_Pressed_Icon.png", UriKind.Relative));

        public static BitmapImage reverse_Idle_Icon = new BitmapImage(new Uri(@"/images/reverse_Idle_Icon.png", UriKind.Relative));
        public static BitmapImage reverse_Pressed_Icon = new BitmapImage(new Uri(@"/images/reverse_Pressed_Icon.png", UriKind.Relative));
    }
}
