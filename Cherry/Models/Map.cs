using Newtonsoft.Json;

namespace Cherry.Models
{
    internal struct Map
    {
        [JsonProperty("metadata")]
        public Metadata MapMetadata { get; set; }

        [JsonProperty("stats")]
        public Stats MapStats { get; set; }

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

            [JsonProperty("characteristics")]
            public Characteristic[] MapCharacteristics { get; set; }
        }

        internal struct Stats
        {
            [JsonProperty("downloads")]
            public int Downloads { get; set; }

            [JsonProperty("downVotes")]
            public int DownVotes { get; set; }

            [JsonProperty("upVotes")]
            public int UpVotes { get; set; }
            
            [JsonProperty("rating")]
            public double Rating { get; set; }
        }

        internal struct Characteristic
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("easy")]
            public Difficulty? Easy { get; set; }

            [JsonProperty("normal")]
            public Difficulty? Normal { get; set; }

            [JsonProperty("hard")]
            public Difficulty? Hard { get; set; }

            [JsonProperty("expert")]
            public Difficulty? Expert { get; set; }

            [JsonProperty("expertPlus")]
            public Difficulty? ExpertPlus { get; set; }
        }

        internal struct Difficulty
        {
            [JsonProperty("njs")]
            public float NJS { get; set; }
        }
    }
}