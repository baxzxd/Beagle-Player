using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Music_Player_WPF
{
    public class PlayerButton
    {
        private Image base_image;
        private BitmapImage Icon_Hovered;
        private BitmapImage Icon_Idle;
        public PlayerButton(Image i, BitmapImage ii, BitmapImage ih)
        {
            Icon_Hovered = ih;
            Icon_Idle = ii;

            base_image = i;
            base_image.MouseEnter += new System.Windows.Input.MouseEventHandler(Mouse_Entered);
            base_image.MouseLeave += new System.Windows.Input.MouseEventHandler(Mouse_Exit);

            base_image.Source = ii;
        }
        private void Mouse_Entered(object sender, System.Windows.Input.MouseEventArgs e)
        {
            base_image.Source = Icon_Hovered;
        }
        private void Mouse_Exit(object sender, System.Windows.Input.MouseEventArgs e)
        {
            base_image.Source = Icon_Idle;
        }
    }
}
