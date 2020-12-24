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
		public static BitmapImage GetAlbumArt(TagLib.File tfile)
		{
			try
			{
				MemoryStream ms = new MemoryStream(tfile.Tag.Pictures[0].Data.Data);
				var bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.StreamSource = ms;
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				bitmap.Freeze();
				return bitmap;
			}
			catch (System.Exception)
			{
				return null;
			}

			return null;
		}

		public static Dictionary<string, List<string>> GetAlbum(string path, Dictionary<string, List<string>> albums = null, bool debug = false)
		{
			if (albums == null)
			{
				albums = new Dictionary<string, List<string>>();
			}

			string[] newFolders = Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
			for (int j = 0; j < newFolders.Length; j++)
			{
				string[] song_files = Directory.GetFiles(newFolders[j]);
				for (int i = 0; i < song_files.Length; i++)
				{
					if (HasExtension(song_files[i], "mp3"))
					{
						var tfile = TagLib.File.Create(song_files[i]);
						string albumName = tfile.Tag.Album;
						if (albumName != null)
						{
							if (!albums.ContainsKey(albumName))
							{
								albums.Add(albumName, new List<string>());
							}
							albums[albumName].Add(song_files[i]);
						}
					}
				}
			}

			if (debug)
			{
				foreach (KeyValuePair<string, List<string>> t in albums)
				{
					Console.WriteLine(t.Key);
					for (int i = 0; i < albums[t.Key].Count; i++)
					{
						Console.WriteLine(t.Value[i]);
					}
				}
			}

			return albums;
		}

		private static bool HasExtension(string name, string ext)
		{
			string tempExtension = name.Substring(name.Length - ext.Length);
			return ext == tempExtension;
		}
	}
}
