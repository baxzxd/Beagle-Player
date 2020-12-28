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


		public static List<string> GetAllFilePaths(string path)
		{
			List<string> directories = Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories).ToList();
			directories.Add(path);

			List<string> file_paths = new List<string>();
			for (int i = 0; i < directories.Count; i++)
			{
				file_paths.AddRange(Directory.GetFiles(directories[i]).ToList());
			}

			//Get Size for debugging
			Console.WriteLine("Amount of files " + file_paths.Count);
			return file_paths;
		}


		public static BitmapImage LoadBitmapImage(string path)
		{
			BitmapImage bImage = new BitmapImage(new Uri(@path));
			return bImage;
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

		public static BitmapImage SaveBitmapImageFromStream(MemoryStream ms, int width, int height)
		{
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.StreamSource = ms;
			bitmap.DecodePixelHeight = width;
			bitmap.DecodePixelWidth = height;
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
		}
	}
}
