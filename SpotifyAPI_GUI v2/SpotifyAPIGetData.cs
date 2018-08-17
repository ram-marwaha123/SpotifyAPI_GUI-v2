using System;
using System.IO;
using System.Text;
using System.Net;
using System.Windows;
using System.Runtime.Serialization.Json;
using SpotifyClasses;

namespace SpotifyAPI_GUI_v2
{
    class SpotifyAPIGetData
    {
        public static string AuthorizationCode { get; set; }

        public void GetAccessToken()
        {
            string clientId = Environment.GetEnvironmentVariable("SpotifyClientId");
            string clientSecret = Environment.GetEnvironmentVariable("SpotifyClientSecret");

            var encodeIdSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://accounts.spotify.com/api/token");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";
            webRequest.Headers.Add("Authorization: Basic " + encodeIdSecret);

            var request = ("grant_type=client_credentials");
            byte[] requestBytes = Encoding.ASCII.GetBytes(request);
            webRequest.ContentLength = requestBytes.Length;

            Stream stream = webRequest.GetRequestStream();
            stream.Write(requestBytes, 0, requestBytes.Length);
            stream.Close();

            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            spotifyToken token;
            AuthorizationCode = "";
            using (Stream responseStream = response.GetResponseStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(spotifyToken));
                token = (spotifyToken)serializer.ReadObject(responseStream);
                AuthorizationCode = token.access_token;
            }
        }

        public String MakeRequest(string endPoint)
        {
            GetAccessToken();

            String r = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.PreAuthenticate = true;
            request.Headers.Add("Authorization", "Bearer " + AuthorizationCode);
            request.Accept = "application/json";
            request.Method = "GET";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show("ERROR:" + response.StatusCode);
                        r = "ERROR OCCURED :\\";
                    }
                    else
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(responseStream);
                            r = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                r = "ERROR OCCURED :\\";
                MessageBox.Show(e.ToString());
            }

            return r;
        }


        public spotifyTrack GetTrack(string id)
        {
            string endPoint = "https://api.spotify.com/v1/tracks/" + id;
            string response = MakeRequest(endPoint);
            byte[] byteArray = Encoding.ASCII.GetBytes(response);
            MemoryStream stream = new MemoryStream(byteArray);

            var serializer = new DataContractJsonSerializer(typeof(spotifyTrack));
            spotifyTrack track = (spotifyTrack)serializer.ReadObject(stream);

            return track;
        }

        public string GetPlaylistName(string id)
        {
            string endPoint = "https://api.spotify.com/v1/users/ram_marwaha/playlists/" + id + "?field=name";
            string response = MakeRequest(endPoint);
            if (response == "ERROR OCCURED :\\")
            {
                return "No Playlist with the given ID";
            }
            byte[] byteArray = Encoding.ASCII.GetBytes(response);
            MemoryStream stream = new MemoryStream(byteArray);

            var serializer = new DataContractJsonSerializer(typeof(spotifyName));
            spotifyName playlist = (spotifyName)serializer.ReadObject(stream);

            return playlist.name;
        }

        public spotifyPlaylist GetPlaylist(string id, int offset = 0, int limit = 0)
        {
            string endPoint = "https://api.spotify.com/v1/users/ram_marwaha/playlists/" + id + "/tracks?offset=" + offset + ((limit == 0) ? "" : "&limit=" + limit);
            string response = MakeRequest(endPoint);
            byte[] byteArray = Encoding.ASCII.GetBytes(response);
            MemoryStream stream = new MemoryStream(byteArray);

            var serializer = new DataContractJsonSerializer(typeof(spotifyPlaylist));
            spotifyPlaylist playlist = (spotifyPlaylist)serializer.ReadObject(stream);

            while (playlist.next != null)
            {
                string endPointNext = playlist.next;
                string responseNext = MakeRequest(endPointNext);
                byte[] byteArrayNext = Encoding.ASCII.GetBytes(responseNext);
                MemoryStream streamNext = new MemoryStream(byteArrayNext);


                var serializerNext = new DataContractJsonSerializer(typeof(spotifyPlaylist));
                spotifyPlaylist temp = (spotifyPlaylist)serializer.ReadObject(streamNext);
                playlist.items.AddRange(temp.items);

                playlist.next = temp.next;
            }

            return playlist;
        }

        public spotifyRecommened GetRecommended(string track_id)
        {
            string endPoint = "https://api.spotify.com/v1/recommendations?limit=10&seed_tracks=" + track_id + "&min_popularity=50";
            string response = MakeRequest(endPoint);
            byte[] byteArray = Encoding.ASCII.GetBytes(response);
            MemoryStream stream = new MemoryStream(byteArray);

            var serializer = new DataContractJsonSerializer(typeof(spotifyRecommened));
            spotifyRecommened tracks = (spotifyRecommened)serializer.ReadObject(stream);

            return tracks;
        }
    }
}