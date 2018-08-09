using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using SpotifyClasses;

namespace SpotifyAPI_GUI_v2
{
    class SQLServer
    {
        public SqlConnection connection { get; set; }

        public void openConnection()
        {
            string connectionString = Environment.GetEnvironmentVariable("SpotifyDBConnectionString");
            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void closeConnection()
        {
            try
            {
                connection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private SqlDataReader ReadCommand(string cmd)
        {
            SqlCommand command = new SqlCommand(cmd, connection);

            SqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return reader;
        }

        public HashSet<string> GetPlaylistNames()
        {
            HashSet<string> set = new HashSet<string>();

            string cmd = "SELECT [Name] FROM dbo.PlaylistID_Name";
            SqlDataReader reader = ReadCommand(cmd);

            while (reader.Read())
            {
                set.Add(reader[0].ToString());
            }
            reader.Close();
            return set;
        }

        public List<object> GetHighestOrLowest(string playlist_name, string column_name, bool descending)
        {
            List<object> trackData = new List<object>();

            string playlist_choice = (playlist_name == "All Playlists") ? "" : playlist_name;
            string asc_or_desc = (descending) ? "DESC" : "ASC";

            string cmd = String.Format("EXEC dbo.sortTable @playlist_name = '{0}', @column_name = '{1}', @asc_or_desc = '{2}'", playlist_choice, column_name, asc_or_desc);
            SqlDataReader reader = ReadCommand(cmd);

            if (reader != null && reader.HasRows && reader.Read())
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    trackData.Add(reader[col]);
                }
            }
            reader.Close();
            return trackData;
        }


        public Dictionary<string, HashSet<string>> GetAllArtistSongs()
        {
            Dictionary<string, HashSet<string>> artistTracks = new Dictionary<string, HashSet<string>>();

            string cmd = "SELECT * FROM dbo.Playlists";
            SqlDataReader reader = ReadCommand(cmd);
            while (reader.Read())
            {
                string[] artists = reader[2].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string a in artists)
                {
                    string artist = a.Trim();
                    string songTitle = reader[1].ToString();
                    if (artistTracks.ContainsKey(artist))
                    {
                        HashSet<string> songs = artistTracks[artist];
                        songs.Add(songTitle);
                    }
                    else
                    {
                        artistTracks.Add(artist, new HashSet<string> { songTitle });
                    }
                }
            }
            reader.Close();
            return artistTracks;
        }

        public Dictionary<string, string> GetPlaylistNameID()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string cmd = "SELECT * FROM dbo.PlaylistID_Name";
            SqlDataReader reader = ReadCommand(cmd);
            while (reader.Read())
            {
                dict.Add(reader[0].ToString(), reader[1].ToString());
            }
            reader.Close();
            return dict;
        }

        public void AddPlaylistToTable(string playlist_name, string playlistId)
        {
            SpotifyAPIGetData p = new SpotifyAPIGetData();
            spotifyPlaylist playlist = p.GetPlaylist(playlistId);

            string d = "DELETE FROM dbo.Playlists WHERE Playlist = '" + playlist_name + "'";
            SqlCommand del = new SqlCommand(d, connection);
            try
            {
                del.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("Something Went Wrong, Please Try Again Later ://");
            }

            foreach (songInfo song in playlist.items)
            {
                spotifyTrack t = song.track;

                string title = t.name.Replace("'", "''");
                List<string> artists = new List<string>();
                foreach (artistInfo a in t.artists)
                {
                    artists.Add(a.name);
                }
                string artistString = String.Join(", ", artists).Replace("'", "''");
                string album = t.album.name.Replace("'", "''");
                int @explicit = (t.@explicit) ? 1 : 0;
                TimeSpan duration = TimeSpan.FromMilliseconds(t.duration_ms);
                int popularity = t.popularity;
                string trackId = t.id.Replace("'", "''");
                string release_date = t.album.release_date;

                string c = String.Format("EXEC dbo.insertPlaylistData @Playlist = '{0}', @Title = '{1}', @Artist = '{2}', @Album = '{3}', @Explicit = {4}, @Duration = '{5}', @Popularity = {6}, @Release_Date = '{7}', @ID = '{8}'", playlist_name, title, artistString, album, @explicit, duration, popularity, release_date, trackId);
                SqlCommand command = new SqlCommand(c, connection);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something Went Wrong, Please Try Again Later ://");
                }
            }
        }

        public void LinkNameAndID(string name, string id)
        {
            string n = name.Replace("'", "''");
            string c = String.Format("EXEC dbo.InsertNameID @Name = '{0}', @ID = '{1}'", n, id);
            SqlCommand command = new SqlCommand(c, connection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("Something Went Wrong, Please Try Again Later ://");
            }
        }
    }
}
