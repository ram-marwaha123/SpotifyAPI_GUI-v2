using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using SpotifyClasses;

namespace SpotifyAPI_GUI_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SQLServer server { get; set; }
        private SpotifyAPIGetData spotify { get; set; }
        private Dictionary<string, HashSet<string>> artistTracks { get; set; }

        private Dictionary<string, string> PlaylistNameID { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            start();
        }

        private void start()
        {
            server = new SQLServer();
            server.openConnection();

            spotify = new SpotifyAPIGetData();

            artistTracks = server.GetAllArtistSongs();
            
            AddPlaylistGrid.Visibility = Visibility.Collapsed;
            Add_PlaylistID_TextBox.Text = "";
            Add_PlaylistName_Label.Content = "";

            UpdatePlaylistGrid.Visibility = Visibility.Collapsed;
            PlaylistNameID = server.GetPlaylistNameID();

            GetPlaylistGrid.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server.closeConnection();
        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            AddPlaylistGrid.Visibility = Visibility.Visible;
            UpdatePlaylistGrid.Visibility = Visibility.Collapsed;
            GetPlaylistGrid.Visibility = Visibility.Collapsed;
        }

        private void FindPlaylist(object sender, RoutedEventArgs e)
        {
            Add_PlaylistName_Label.Content = spotify.GetPlaylistName(Add_PlaylistID_TextBox.Text);
        }

        private void AddPlaylistToTable(object sender, RoutedEventArgs e)
        {
            string name = Add_PlaylistName_Label.Content.ToString();
            
            if (!PlaylistNameID.ContainsKey(name))
            {
                string id = Add_PlaylistID_TextBox.Text;
                PlaylistNameID.Add(name, id);
                server.AddPlaylistToTable(name, id);
                server.LinkNameAndID(name, id);
            }
            MessageBox.Show("Playlist Added! :)");
        }


        private void UpdatePlaylist(object sender, RoutedEventArgs e)
        {
            UpdatePlaylistGrid.Visibility = Visibility.Visible;
            AddPlaylistGrid.Visibility = Visibility.Collapsed;
            GetPlaylistGrid.Visibility = Visibility.Collapsed;

            Update_Playlist_ComboBox.Items.Clear();

            HashSet<string> playlists = server.GetPlaylistNames();
            foreach (string p in playlists)
            {
                Update_Playlist_ComboBox.Items.Add(p);
            }
            Update_Playlist_ComboBox.SelectedIndex = 0;
        }

        private void Update(object sender, RoutedEventArgs e)
        {
            string name = Update_Playlist_ComboBox.Text.ToString();
            server.AddPlaylistToTable(name, PlaylistNameID[name]);
            MessageBox.Show("Playlist Added! :)");
        }


        private void GetPlaylist(object sender, RoutedEventArgs e)
        {
            GetPlaylistGrid.Visibility = Visibility.Visible;
            AddPlaylistGrid.Visibility = Visibility.Collapsed;
            UpdatePlaylistGrid.Visibility = Visibility.Collapsed;

            Get_Playlists_ComboBox.Items.Clear();
            HashSet<string> PlaylistNames = server.GetPlaylistNames();
            if (PlaylistNames.Count > 1)
            {
                Get_Playlists_ComboBox.Items.Add("All Playlists");
            }
            foreach (string name in PlaylistNames)
            {
                Get_Playlists_ComboBox.Items.Add(name);
            }
        }

        private void GetTracks(object sender, RoutedEventArgs e)
        {
            string name = Get_Playlists_ComboBox.SelectedItem.ToString();
            SqlDataAdapter adapter = server.GetDataForDataGrid(name);
            DataTable dt = new DataTable("Playlist");
            adapter.Fill(dt);
            Get_Tracks_DataGrid.ItemsSource = dt.DefaultView;
            Get_Tracks_DataGrid.Columns[3].Visibility = Visibility.Collapsed;
        }

        private void Get_Tracks_DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView item = Get_Tracks_DataGrid.SelectedItem as DataRowView;
            if (item != null)
            {
                string id = item["ID"].ToString();
                spotifyTrack track = spotify.GetTrack(id);

                TrackInfo TrackWindow = new TrackInfo();

                TrackWindow.Title = track.name;

                TrackWindow.Title_Label.Content = track.name;

                List<string> artists = new List<string>();
                foreach (artistInfo artist in track.artists)
                {
                    artists.Add(artist.name);
                }
                TrackWindow.Artist_Label.Content = String.Join(", ", artists);

                TrackWindow.Album_Label.Content = track.album.name;

                TrackWindow.Explicit_CheckBox.IsChecked = track.@explicit;

                TimeSpan duration = TimeSpan.FromMilliseconds(track.duration_ms);
                TrackWindow.Duration_Label.Content = String.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);

                TrackWindow.ReleaseDate_Label.Content = track.album.release_date;

                TrackWindow.Popularity_ProgressBar.Value = track.popularity;

                TrackWindow.trackID_label.Content = id;

                TrackWindow.Show();
            }
        }
        
    }

    class track
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string ID { get; set; }
    }
}