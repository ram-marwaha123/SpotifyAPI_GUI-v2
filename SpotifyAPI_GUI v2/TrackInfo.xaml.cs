using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SpotifyClasses;

namespace SpotifyAPI_GUI_v2
{
    /// <summary>
    /// Interaction logic for TrackInfo.xaml
    /// </summary>
    public partial class TrackInfo : Window
    {
        private SpotifyAPIGetData spotify { get; set; }

        public TrackInfo()
        {
            InitializeComponent();
            spotify = new SpotifyAPIGetData();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RecommendedTracks recommended = new RecommendedTracks();

            string track_id = trackID_label.Content.ToString();
            spotifyRecommened tracks = spotify.GetRecommended(track_id);
            foreach (spotifyTrack t in tracks.tracks)
            {
                recommended.Recommended_ListBox.Items.Add(t.name);
            }
            recommended.Show();
        }
    }
}
