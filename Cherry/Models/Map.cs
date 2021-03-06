﻿using Newtonsoft.Json;
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

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonProperty("downloadURL")]
        public string DownloadURL { get; set; }

        [JsonProperty("coverURL")]
        public string CoverURL { get; set; }

        [JsonProperty("uploader")]
        public User Uploader { get; set; }

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

            [JsonProperty("difficulties")]
            public DifficultySet Difficulties { get; set; }
        }

        internal struct Difficulty
        {
            [JsonProperty("njs")]
            public float NJS { get; set; }

            [JsonProperty("length")]
            public int Length { get; set; }
        }

        internal struct User
        {
            [JsonProperty("username")]
            public string Name { get; set; }
        }

        internal struct DifficultySet
        {
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

            [JsonIgnore]
            public int AnyLength => ExpertPlus?.Length ?? Expert?.Length ?? Hard?.Length ?? Normal?.Length ?? Easy?.Length ?? 0;

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
                    float?[] minPool = new float?[] { ExpertPlus?.NJS, Expert?.NJS, Hard?.NJS, Normal?.NJS, Easy?.NJS };

                    _min = minPool.Where(njs => njs.HasValue).Min();
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
                    float?[] maxPool = new float?[] { ExpertPlus?.NJS, Expert?.NJS, Hard?.NJS, Normal?.NJS, Easy?.NJS };

                    _max = maxPool.Where(njs => njs.HasValue).Min();
                    return _max.GetValueOrDefault();
                }
            }
        }
    }
}