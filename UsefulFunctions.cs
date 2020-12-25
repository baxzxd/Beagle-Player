using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using TagLib;

namespace Music_Player_WPF
{
    public static class UsefulFunctions
	{
		public static int Clamp(int i, int min, int max)
        {
			if (i > max)
				return max;
			if (i < min)
				return min;
			return i;
		}

		public static void SaveBitmapImage(BitmapImage image, string filePath)
		{
			BitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(image));

			using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
			{
				encoder.Save(fileStream);
			}
		}

		public static bool IsPathValid(string path)
		{
			bool exists = false;
			try
			{
				exists = Directory.Exists(path);
				return exists;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
        
	}
}
