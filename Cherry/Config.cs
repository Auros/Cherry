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
        public virtual bool QueueOpened { get; set; } = false;
        public virtual string RequestCommand { get; set; } = "!bsr";
        public virtual string CancelCommand { get; set; } = "!oops";

        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public virtual List<string> BannedSongs { get; set; } = new List<string>();

        [NonNullable, UseConverter(typeof(ListConverter<UserBan>))]
        public virtual List<UserBan> BannedUsers { get; set; } = new List<UserBan>();

        public virtual float SesssionLengthInHours { get; set; } = 6f;


        public class UserBan
        {
            public virtual string ID { get; set; } = null!;
            public virtual bool Permanent { get; set; }
            public virtual DateTime Until { get; set; }
        }
    }
}