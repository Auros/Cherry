using Newtonsoft.Json;

namespace Cherry.Models
{
    internal struct Map
    {
        [JsonProperty("metadata")]
        public Metadata MapMetadata { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("downloadURL")]
        public string DownloadURL { get; set; }

        internal struct Metadata
        {
            [JsonProperty("levelAuthorName")]
            public string LevelAuthorName { get; set; }

            [JsonProperty("songAuthorName")]
            public string SongAuthorName { get; set; }

            [JsonProperty("automapper")]
            public string? Automapper { get; set; }

            [JsonProperty("songSubName")]
            public string SongSubName { get; set; }

            [JsonProperty("songName")]
            public string SongName { get; set; }

            [JsonProperty("duration")]
            public int Duration { get; set; }
        }
    }
}
