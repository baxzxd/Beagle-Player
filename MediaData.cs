using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Music_Player_WPF
{
	public static class MediaTools
	{
		/*Todo
		 * Add check in album in case on or more files dont have art but one does
		 * Album GUID to easily get art?
		 *		Allow user to define what size the art is saved in
		 */

		//New New version of GetAlbum
		public static Dictionary<string, AlbumData> GetSongDataFromPath(string path, Dictionary<string, AlbumData> albums = null)
		{
			if (albums == null)
				albums = new Dictionary<string, AlbumData>();

			//Get all subfolders from path  and add path argument
			List<string> new_folders = Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories).ToList();
			new_folders.Add(path);

			for (int j = 0; j < new_folders.Count; j++)
			{
				string[] song_paths = Directory.GetFiles(new_folders[j]);
				for (int i = 0; i < song_paths.Length; i++)
				{
					if (ValidMediaFormat(song_paths[i]))
					{
						//Create tag file to get album name
						var tfile = TagLib.File.Create(song_paths[i]);
						string albumName = tfile.Tag.Album;

						if (albumName != null)
						{
							//If album has not been found create new object
							if (!albums.ContainsKey(albumName))
								albums.Add(albumName, new AlbumData(albumName));

							//Add song to new Album's track listing
							albums[albumName].tracks.Add(new SongData(song_paths[i]));

							//If there hasnt been art found check new song for art
							if( !albums[albumName].HasArt )
                            {
								if( tfile.Tag.Pictures.Length > 0 )
                                {
									albums[albumName].HasArt = true;
									//Get picture, save it with albums GUID <----
                                }
                            }
						}
					}
				}
			}
			return albums;
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

		public static void SaveAlbum(AlbumData a, string path)
        {
			string full_path = path + @"\" + a.Name;
			FileStream outFile = File.Create(full_path);
			XmlSerializer formatter = new XmlSerializer(a.GetType());
			formatter.Serialize(outFile, a);
			Console.WriteLine(full_path);
		}

		public static SongData LoadSong(string p)
		{
			SongData sd = new SongData();
			XmlSerializer formatter = new XmlSerializer(sd.GetType());
			FileStream songDataFile = new FileStream(p, FileMode.Open);
			byte[] buffer = new byte[songDataFile.Length];
			songDataFile.Read(buffer, 0, (int)songDataFile.Length);
			MemoryStream stream = new MemoryStream(buffer);
			return (SongData)formatter.Deserialize(stream);
		}

		public static bool ValidMediaFormat(string path)
        {
			string file_extension = Path.GetExtension(path);
			switch(file_extension)
            {
				case ".mp3":
				case ".wav":
				case ".ogg":
					return true;
            }
			return false;
		}
	}

	public class AlbumData
	{
		public string Name { get; set; }
		public string Artist { get; set; }
		public List<SongData> tracks { get; set; }
		public string GUID { get; set; }
		public bool HasArt { get; set; }

		//Blank constructor for loading
		public AlbumData(){}

		public AlbumData(string n, List<SongData> t = null)
		{
			Name = n;
			if( t == null )
            {
				tracks = new List<SongData>();
            }
			else
            {
				tracks = t;
            }
		}

		public void Save(string path, string file_name)
		{
			string full_path = path + @"\" + file_name;
			FileStream outFile = File.Create(full_path);
			XmlSerializer formatter = new XmlSerializer(this.GetType());
			formatter.Serialize(outFile, this);
			Console.WriteLine(full_path);
		}
	}

	public class SongData
	{
		public string Title { get; set; }
		public string AlbumName { get; set; }
		public string Artist { get; set; }
		public string Length { get; set; }
		public string path { get; set; }
		public string GUID { get; set; }
		public bool contains_art { get; set; }
		public SongData()
        {

        }
		public SongData(string song_path = "")
		{
			if( song_path != "" )
            {
				path = song_path;

				TagLib.File tag_file = TagLib.File.Create(path);

				//Set default details if stuff dont exist - (Taken from Redd)
				string temp_title = tag_file.Tag.Title;
				if( temp_title == null )
                {
					Title = MediaTools.DefaultSongTitle;
                }
				else
                {
					Title = temp_title;
				}

				string temp_artist = tag_file.Tag.Performers[0];
				if( temp_artist == null)
                {
					Artist = MediaTools.DefaultSongArtist;
                }
				else
                {
					Artist = temp_artist;
                }

				if( tag_file.Tag.Pictures.Length == 0 )
                {

                }

				AlbumName = tag_file.Tag.Album;
				Artist = tag_file.Tag.Performers[0];
				Length = tag_file.Properties.Duration.ToString().Substring(0, 8);
			}
		}
		public void Save(string path, string file_name)
		{
			string full_path = path + @"\" + file_name;
			FileStream outFile = File.Create(full_path);
			XmlSerializer formatter = new XmlSerializer(this.GetType());
			formatter.Serialize(outFile, this);
			Console.WriteLine(full_path);
		}
	}
}
