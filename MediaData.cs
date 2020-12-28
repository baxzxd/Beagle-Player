using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.ComponentModel;


namespace Music_Player_WPF
{

	public static class MediaTools
	{
		/*Todo
		 * ADDITIONS
		 * --------------
		 * Sort by artist or other song data using album data
		 * ---------------
		 * 
		 * Figure out why "You Borrowed" and "FBLA II" arent playing?
		 * 
		 * Remove AlbumData prolly and instead of lists use HashSets
		 *			
		 * EXAMPLE
		 * ---------------------------------------
		 * HashSet<int> data = new HashSet<int>();
			for (int i = 0; i < 1000000; i++)
			{
				data.Add(rand.Next(50000000));
			}
			bool contains = data.Contains(1234567); // etc
		 * ---------------------------------------
		 */

		public static Dictionary<string, AlbumData> albums;
		public static string DefaultSongTitle = "(No Title)";
		public static string DefaultSongAlbum = "Unknown Album";
		public static string DefaultSongArtist = "Unknown Artist";
		public static BitmapImage DefaultSongCover;

		public static void init()
		{
			Uri uri = new Uri(MyConstants.CurrentDirectory + @"\" + "defaultCoverArt");
			DefaultSongCover = new BitmapImage(uri);
		}


		private static int highestPercentageReached = 0;
		public static void ScanCacheForSongsWorker(BackgroundWorker worker, DoWorkEventArgs e)
		{
			if (albums == null)
				albums = new Dictionary<string, AlbumData>();

			string cache_directory = MyConstants.CurrentDirectory + @"\" + MyConstants.CacheDirectory + @"\";

			//Scan and find all album database files
			string[] files = Directory.GetFiles(cache_directory);
			for (int i = 0; i < files.Length; i++)
			{
				if (System.IO.Path.GetExtension(files[i]) == ".bdb")
				{
					AlbumData temp_album = MediaTools.LoadAlbum(files[i]);
					if (!albums.ContainsKey(temp_album.Name))
					{
						albums.Add(temp_album.Name, temp_album);
					}
				}
			}

			if (MyConstants.user_preferences.music_directories != null)
			{
				int directory_count = MyConstants.user_preferences.music_directories.Count;

				if (worker.CancellationPending)
				{
					e.Cancel = true;
				}
				else
				{
					List<string> song_paths = new List<string>();
					for (int i = 0; i < directory_count; i++)
					{
						//Get all subfolders from path  and add path argument
						song_paths.AddRange(UsefulFunctions.GetAllFilePaths(MyConstants.user_preferences.music_directories[i]));
					}

					//Go through every found files
					for (int i = 0; i < song_paths.Count; i++)
					{
						if (ValidMediaFormat(song_paths[i]))
						{
							//Create tag file to get album name
							var tfile = TagLib.File.Create(song_paths[i]);
							string albumName = tfile.Tag.Album;

							//TODO - Add unknown album
							if (albumName != null)
							{
								//If album has not been found create new object
								if (!albums.ContainsKey(albumName))
								{
									//
									//CHANGE
									string album_artist = tfile.Tag.Performers[0];
									if (album_artist == null)
									{
										album_artist = MediaTools.DefaultSongArtist;
									}

									AlbumData new_album = new AlbumData(albumName, album_artist);
									albums.Add(albumName, new_album);
								}

								SongData new_song = new SongData(tfile, song_paths[i]);

								//Check to see if album already contains song
								bool existing_song = albums[albumName].ContainsSong(new_song);

								if (!existing_song)
								{
									//Add song to new Album's track listing
									new_song.AlbumGUID = albums[albumName].GUID;
									albums[albumName].tracks.Add(new_song);

									//If there hasnt been art found for the album check new song for art
									if (!albums[albumName].HasArt && tfile.Tag.Pictures.Length > 0)
									{
										//Make sure it doesnt save art again
										albums[albumName].HasArt = true;

										//Get picture, save it with albums GUID
										BitmapImage album_art = GetAlbumArt(tfile);
										UsefulFunctions.SaveBitmapImage(album_art, cache_directory + albums[albumName].GUID);
									}
								}
							}
							float percent = ((float)i / (float)song_paths.Count);
							int percentComplete = (int)(percent * 100);

							Console.WriteLine("Progress:" + percentComplete);
							if (percentComplete > highestPercentageReached)
							{
								highestPercentageReached = percentComplete;
								worker.ReportProgress(percentComplete);
							}
						}
					}
				}
			}

			foreach (KeyValuePair<string, AlbumData> t in albums)
			{
				if (!File.Exists(cache_directory + t.Value.GUID + ".bdb"))
				{
					SaveAlbum(t.Value, cache_directory);
				}
			}
		}

		public static List<AlbumData> GetAlbumByArtist(string artist)
		{
			List<AlbumData> found_albums = new List<AlbumData>();

			foreach (KeyValuePair<string, AlbumData> t in albums)
			{
				string temp_artist = t.Value.Artist;
				temp_artist = temp_artist.ToLower();
				if (temp_artist == artist)
				{
					found_albums.Add(t.Value);
				}
			}

			return found_albums;
		}

		public static void ClearCache()
		{
			string[] cache_files = Directory.GetFiles(MyConstants.CacheDirectory);
			for (int i = 0; i < cache_files.Length; i++)
			{
				System.IO.File.Delete(cache_files[i]);
			}
			albums = new Dictionary<string, AlbumData>();
		}

		//New New version of GetAlbum
		public static Dictionary<string, AlbumData> GetSongDataFromPath(string path, Dictionary<string, AlbumData> albums = null)
		{
			if (albums == null)
				albums = new Dictionary<string, AlbumData>();

			string current_directory = Directory.GetCurrentDirectory();
			string cache_directory = current_directory + @"\" + MyConstants.CacheDirectory + @"\";

			//Get all subfolders from path  and add path argument
			List<string> song_paths = UsefulFunctions.GetAllFilePaths(path);
			for (int i = 0; i < song_paths.Count; i++)
			{
				if (ValidMediaFormat(song_paths[i]))
				{
					//Create tag file to get album name
					var tfile = TagLib.File.Create(song_paths[i]);
					string albumName = tfile.Tag.Album;

					//TODO - Add unknown album
					if (albumName != null)
					{
						//If album has not been found create new object
						if (!albums.ContainsKey(albumName))
						{
							//
							//CHANGE
							string album_artist = tfile.Tag.Performers[0];
							if (album_artist == null)
							{
								album_artist = MediaTools.DefaultSongArtist;
							}

							AlbumData new_album = new AlbumData(albumName, album_artist);
							albums.Add(albumName, new_album);
						}

						SongData new_song = new SongData(tfile, song_paths[i]);

						//Check to see if album already contains song
						bool existing_song = albums[albumName].ContainsSong(new_song);

						if (!existing_song)
						{
							//Add song to new Album's track listing
							new_song.AlbumGUID = albums[albumName].GUID;
							albums[albumName].tracks.Add(new_song);

							//If there hasnt been art found for the album check new song for art
							if (!albums[albumName].HasArt && tfile.Tag.Pictures.Length > 0)
							{
								//Make sure it doesnt save art again
								albums[albumName].HasArt = true;

								//Get picture, save it with albums GUID
								BitmapImage album_art = GetAlbumArt(tfile);
								UsefulFunctions.SaveBitmapImage(album_art, cache_directory + albums[albumName].GUID);
							}
						}
					}
				}
			}

			foreach (KeyValuePair<string, AlbumData> t in albums)
			{
				if (!File.Exists(cache_directory + t.Value.GUID + ".bdb"))
				{
					SaveAlbum(t.Value, cache_directory);
				}
			}

			return albums;
		}

		public static BitmapImage GetAlbumArt(TagLib.File tfile, int width = 512, int height = 512)
		{
			try
			{
				MemoryStream ms = new MemoryStream(tfile.Tag.Pictures[0].Data.Data);
				BitmapImage bitmap = UsefulFunctions.SaveBitmapImageFromStream(ms, width, height);
				return bitmap;
			}
			catch (System.Exception)
			{
				return null;
			}
		}

		public static void SaveAlbum(AlbumData a, string path)
		{
			string full_path = path + @"\" + a.GUID + ".bdb";
			FileStream outFile = File.Create(full_path);
			XmlSerializer formatter = new XmlSerializer(a.GetType());
			formatter.Serialize(outFile, a);
		}

		public static AlbumData LoadAlbum(string p)
		{
			AlbumData sd = new AlbumData();
			XmlSerializer formatter = new XmlSerializer(sd.GetType());
			FileStream songDataFile = new FileStream(p, FileMode.Open);
			byte[] buffer = new byte[songDataFile.Length];
			songDataFile.Read(buffer, 0, (int)songDataFile.Length);
			MemoryStream stream = new MemoryStream(buffer);
			return (AlbumData)formatter.Deserialize(stream);
		}

		public static bool ValidMediaFormat(string path)
		{
			string file_extension = Path.GetExtension(path);
			switch (file_extension)
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
		public AlbumData()
		{
			SetGuid();
		}

		public AlbumData(string n, string a, List<SongData> t = null)
		{
			Name = n;
			Artist = a;
			if (t == null)
			{
				tracks = new List<SongData>();
			}
			else
			{
				tracks = t;
			}
			SetGuid();
		}

		public bool ContainsSong(SongData new_song)
		{
			bool existing_song = false;
			for (int si = 0; si < this.tracks.Count; si++)
			{
				string temp_song_name = this.tracks[si].Title;
				if (new_song.Title == temp_song_name)
				{
					existing_song = true;
					break;
				}
			}
			return existing_song;
		}

		private void SetGuid()
		{
			this.GUID = Guid.NewGuid().ToString();
		}

		public void Save(string path, string file_name)
		{
			string full_path = path + @"\" + file_name;
			FileStream outFile = File.Create(full_path);
			XmlSerializer formatter = new XmlSerializer(this.GetType());
			formatter.Serialize(outFile, this);
		}
	}

	public class SongData
	{
		public string Title { get; set; }
		public string AlbumName { get; set; }
		public string Artist { get; set; }
		public double Length { get; set; }
		public string LengthDisplay { get; set; }
		public string path { get; set; }
		public string GUID { get; set; }
		public string AlbumGUID { get; set; }
		public bool contains_art { get; set; }
		public SongData()
		{
		}

		public SongData(TagLib.File tfile, string song_path)
		{
			path = song_path;
			GetDataFromTagFile(tfile);
			SetGuid();
		}

		public SongData(string song_path = "")
		{
			if (song_path != "")
			{
				path = song_path;

				TagLib.File tag_file = TagLib.File.Create(path);
				GetDataFromTagFile(tag_file);
			}
			SetGuid();
		}
		private void SetGuid()
		{
			this.GUID = Guid.NewGuid().ToString();
		}

		private void GetDataFromTagFile(TagLib.File tag_file)
		{
			//Set default details if stuff dont exist - (Taken from Redd)
			string temp_title = tag_file.Tag.Title;
			if (temp_title == null)
			{
				Title = Path.GetFileName(path);
				//Title = MediaTools.DefaultSongTitle;
			}
			else
			{
				Title = temp_title;
			}

			string temp_artist = tag_file.Tag.Performers[0];
			if (temp_artist == null)
			{
				Artist = MediaTools.DefaultSongArtist;
			}
			else
			{
				Artist = temp_artist;
			}

			string temp_album = tag_file.Tag.Album;
			if (temp_album == null)
			{
				AlbumName = MediaTools.DefaultSongAlbum;
			}
			else
			{
				AlbumName = temp_artist;
			}

			if (tag_file.Tag.Pictures.Length > 0)
			{
				contains_art = true;
			}

			Length = tag_file.Properties.Duration.TotalSeconds;
			LengthDisplay = tag_file.Properties.Duration.ToString().Substring(0, 8);
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
