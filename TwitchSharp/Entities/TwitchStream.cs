using System.Net;
using System.Text.Json;

namespace TwitchSharp.Entities
{
    /// <summary>
    /// Represents a live Twitch stream with its associated metadata
    /// </summary>
    public class TwitchStream
    {
        /// <summary>Unique identifier for the stream</summary>
        public string ID { get; private set; }
        /// <summary>The user broadcasting the stream</summary>
        public TwitchUser Broadcaster { get; private set; }
        /// <summary>ID of the game being played</summary>
        public string GameID { get; private set; }
        /// <summary>Name of the game being played</summary>
        public string GameName { get; private set; }
        /// <summary>Title of the stream</summary>
        public string Title { get; private set; }
        /// <summary>Stream tags set by the broadcaster</summary>
        public string[] Tags { get; private set; }
        /// <summary>Current number of viewers</summary>
        public int CurrentViewer { get; private set; }
        /// <summary>When the stream started</summary>
        public DateTime StartedAt { get; private set; }
        public bool IsMature { get; private set; }
        private string _ThumbnailUrl { get; set; }

        public TwitchStream(TwitchClient client, TwitchUser user)
        {
            using (var httpClient = new HttpClient())
            {
                string url = $"https://api.twitch.tv/helix/streams?user_login={user.LoginName}";
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {client.GetAppAccessTokenAsync().Result}");
                httpClient.DefaultRequestHeaders.Add("Client-Id", client.ClientID);
                var response = httpClient.GetAsync(url).Result;

                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new Exception("Code 400 - Bad Request!");
                    case HttpStatusCode.Unauthorized:
                        throw new Exception("Code 401 - Unauthorized!");
                    case HttpStatusCode.OK:
                        JsonElement dataArray = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result).RootElement.GetProperty("data");
                        if (dataArray.GetArrayLength() == 0)
                            throw new Exception("User is Offline!");
                        JsonElement data = dataArray[0];
                        ID = data.GetProperty("id").GetString()!;
                        Broadcaster = user;
                        GameID = data.GetProperty("game_id").GetString()!;
                        GameName = data.GetProperty("game_name").GetString()!;
                        Title = data.GetProperty("title").GetString()!;
                        Tags = data.GetProperty("tags").EnumerateArray().Select(x => x.GetString()).ToArray()!;
                        CurrentViewer = data.GetProperty("viewer_count").GetInt16();
                        DateTime.TryParse(data.GetProperty("started_at").GetString()!, out DateTime dt);
                        StartedAt = dt;
                        IsMature = data.GetProperty("is_mature").GetBoolean();
                        _ThumbnailUrl = data.GetProperty("thumbnail_url").GetString()!;
                        break;
                    default:
                        throw new Exception("Unknown Error");
                }
            }
        }

        public string GetThumbnailUrl(int width = 1920, int height = 1080)
        {
            return _ThumbnailUrl.Replace("{width}", width.ToString()).Replace("{height}", height.ToString());
        }
        public string GetPlainThumbnailUrl()
        {
            return _ThumbnailUrl;
        }
    }
}