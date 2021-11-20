using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;

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

        [JsonProperty("id")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonProperty("uploader")]
        public User Uploader { get; set; }

        [JsonProperty("automapper")]
        public bool Automapper { get; set; }

        [JsonProperty("versions")]
        public Version[] Versions { get; set; }

        [JsonIgnore]
        public Version LatestVersion => Versions.LastOrDefault();

        internal struct Metadata
        {
            [JsonProperty("levelAuthorName")]
            public string LevelAuthorName { get; set; }

            [JsonProperty("songAuthorName")]
            public string SongAuthorName { get; set; }

            [JsonProperty("songSubName")]
            public string SongSubName { get; set; }

            [JsonProperty("songName")]
            public string SongName { get; set; }

            [JsonProperty("duration")]
            public int Duration { get; set; }
        }

        internal struct Stats
        {
            [JsonProperty("downloads")]
            public int Downloads { get; set; }

            [JsonProperty("downvotes")]
            public int Downvotes { get; set; }

            [JsonProperty("upvotes")]
            public int Upvotes { get; set; }
            
            [JsonProperty("score")]
            public float Rating { get; set; }
        }

        internal struct Difficulty
        {
            [JsonProperty("njs")]
            public float NJS { get; set; }

            [JsonProperty("seconds")]
            public float Seconds { get; set; }
        }

        internal struct User
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        internal enum State
        {
            Uploaded,
            Testplay,
            Published,
            Feedback
        }

        internal struct Version
        {

            [JsonProperty("downloadURL")]
            public string DownloadURL { get; set; }

            [JsonProperty("coverURL")]
            public string CoverURL { get; set; }

            [JsonProperty("diffs")]
            public Difficulty[] Difficulties { get; set; }

            [JsonProperty("hash")]
            public string Hash { get; set; }

            [JsonProperty("state")]
            [JsonConverter(typeof(StringEnumConverter))]
            public State State { get; set; }

            [JsonIgnore]
            private float? _min;

            [JsonIgnore]
            private float? _max;

            [JsonIgnore]
            public float MinNJS
            {
                get
                {
                    if (_min.HasValue)
                        return _min.Value;
                    float[] minPool = Difficulties.Select(d => d.NJS).ToArray();

                    _min = minPool.Min();
                    return _min.GetValueOrDefault();
                }
            }

            [JsonIgnore]
            public float MaxNJS
            {
                get
                {
                    if (_max.HasValue)
                        return _max.Value;
                    float[] maxPool = Difficulties.Select(d => d.NJS).ToArray();

                    _max = maxPool.Min();
                    return _max.GetValueOrDefault();
                }
            }
        }
    }
}