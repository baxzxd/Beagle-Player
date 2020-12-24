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
		}

		public static Dictionary<string, List<string>> GetAlbum(string path, Dictionary<string, List<string>> albums = null, bool debug = false)
		{
			if (albums == null)
			{
				albums = new Dictionary<string, List<string>>();
			}

			string[] files_in_base_path = Directory.GetFiles(path);
			for( int i = 0; i < files_in_base_path.Length; i++ )
            {
				if (HasExtension(files_in_base_path[i], "mp3"))
				{
					var tfile = TagLib.File.Create(files_in_base_path[i]);
					string albumName = tfile.Tag.Album;
					if (albumName != null)
					{
						if (!albums.ContainsKey(albumName))
						{
							albums.Add(albumName, new List<string>());
						}
						albums[albumName].Add(files_in_base_path[i]);
					}
				}
			}

			string[] new_folders = Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
			if( new_folders.Length == 0 )
			
			for (int j = 0; j < new_folders.Length; j++)
			{
				string[] song_files = Directory.GetFiles(new_folders[j]);
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

			return albums;
		}

		//New version of GetAlbum
		public static Dictionary<string, List<SongData>> GetSongDataFromPath(string path, Dictionary<string, List<SongData>> albums = null, bool debug = false)
		{
			if (albums == null)
				albums = new Dictionary<string, List<SongData>>();

			//Get all subfolders from path 
			List<string> new_folders = Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories).ToList();
			new_folders.Add(path);

			for (int j = 0; j < new_folders.Count; j++)
			{
				string[] song_files = Directory.GetFiles(new_folders[j]);
				for (int i = 0; i < song_files.Length; i++)
				{
					//Change later
					if (HasExtension(song_files[i], "mp3"))
					{
						//Create tag file to get album name and create dictionary key
						var tfile = TagLib.File.Create(song_files[i]);
						string albumName = tfile.Tag.Album;
						if (albumName != null)
						{
							if (!albums.ContainsKey(albumName))
							{
								albums.Add(albumName, new List<SongData>());
							}
							albums[albumName].Add(new SongData(song_files[i]));
						}
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
	public class SongData
	{
		public string AlbumName { get; set; }
		public string GridViewColumnName_LabelContent { get; set; }
		public string AlbumImage_Source { get; set; }
		public string Title { get; set; }
		public string Length { get; set; }
		public string path { get; set; }

		public SongData(string song_path)
        {
			path = song_path;

			TagLib.File tag_file = TagLib.File.Create(path);
			AlbumName = tag_file.Tag.Album;
			Title = tag_file.Tag.Title;
			Length = tag_file.Properties.Duration.ToString().Substring(0, 8);
		}
	}

}
