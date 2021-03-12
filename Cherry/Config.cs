using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Cherry
{
    internal class Config
    {
        public event Action<Config>? Updated;

        public virtual bool QueueOpened { get; set; } = false;
        public virtual string RequestCommand { get; set; } = "!bsr";    
        public virtual string CancelCommand { get; set; } = "!oops";

        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> BannedSongs { get; set; } = new List<string>();

        [NonNullable, UseConverter(typeof(ListConverter<UserBan>))]
        public virtual List<UserBan> BannedUsers { get; set; } = new List<UserBan>();

        public virtual bool AddTwitchTTSPrefix { get; set; } = false;
        public virtual float SesssionLengthInHours { get; set; } = 6f;

        [NonNullable]
        public virtual ConcurrentRequestPermissions GlobalConcurrent { get; set; } = new ConcurrentRequestPermissions { Enabled = true, MaxConcurrentRequests = 2 };

        [NonNullable]
        public virtual ConcurrentRequestPermissions Level1Concurrent { get; set; } = new ConcurrentRequestPermissions { Enabled = true, MaxConcurrentRequests = 5 };

        [NonNullable]
        public virtual ConcurrentRequestPermissions Level2Concurrent { get; set; } = new ConcurrentRequestPermissions { Enabled = true, MaxConcurrentRequests = 10 };

        [NonNullable]
        public virtual ConcurrentRequestPermissions Level3Concurrent { get; set; } = new ConcurrentRequestPermissions { Enabled = false, MaxConcurrentRequests = 10 };

        [NonNullable]
        public virtual ConcurrentRequestPermissions Level4Concurrent { get; set; } = new ConcurrentRequestPermissions { Enabled = false, MaxConcurrentRequests = 10 };

        public virtual bool DoMapAge { get; set; }
        public virtual string MinimumAgeSerializable { get; set; } = new DateTime(2018, 5, 1).ToString();

        [Ignore]
        public DateTime MinimumAge
        {
            get => MinimumAgeSerializable != null ? DateTime.Parse(MinimumAgeSerializable) : new DateTime(2018, 5, 1);
            set => MinimumAgeSerializable = value.ToString();
        }

        public virtual bool DoMapRating { get; set; }
        public virtual float MiniumMapRating { get; set; } = 0.5f;

        public virtual bool DoMaxSongLength { get; set; }
        public virtual float MaxSongLengthInMinutes { get; set; } = 10;

        public virtual bool AllowAutoMappedSongs { get; set; }

        public virtual bool DoMinNJS { get; set; }
        public virtual bool DoMaxNJS { get; set; }
        public virtual float MinNJS { get; set; } = 10f;
        public virtual float MaxNJS { get; set; } = 20f;

        public class UserBan
        {
            public virtual string ID { get; set; } = null!;
            public virtual bool Permanent { get; set; }
            public virtual string UntilSerializable { get; set; } = null!;

            [Ignore]
            public DateTime Until
            {
                get => UntilSerializable != null ? DateTime.Parse(UntilSerializable) : default;
                set => UntilSerializable = value.ToString();
            }
        }

        public class ConcurrentRequestPermissions
        {
            public virtual bool Enabled { get; set; }
            public virtual int MaxConcurrentRequests { get; set; }
        }

        public virtual void Changed()
        {
            Updated?.Invoke(this);
        }
    }
}