using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Music_Player_WPF
{
    public class ResizablePanel : StackPanel
    {
        private Canvas canvas;
        private bool mouse_in_panel;
        private bool is_dragging;
        private bool is_resizing;

        int width = 100;
        int height = 100;

        public ResizablePanel(Canvas c)
        {
            canvas = c;
            init();
        }
        public ResizablePanel()
        {
            init();
        }

        private void init()
        {
            this.Width = width;
            this.Height = height;
            this.Background = Brushes.Green;
            this.HorizontalAlignment = HorizontalAlignment.Left;

            this.MouseDown += new System.Windows.Input.MouseButtonEventHandler(BasePanel_MouseDown);
            this.MouseMove += new System.Windows.Input.MouseEventHandler(BasePanel_MouseMove);
            this.MouseUp += new System.Windows.Input.MouseButtonEventHandler(BasePanel_MouseUp);
        }

        private void BasePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Console.WriteLine("resizable panel clicked");
                is_resizing = true;
            }
        }

        private void BasePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (is_resizing)
            {
                Console.WriteLine(e.GetPosition(canvas).X);
                this.Width = e.GetPosition(canvas).X;
                this.Height = height;
            }
        }

        private void BasePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                is_resizing = false;
            }
        }
    }
}
