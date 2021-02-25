using Cherry.Interfaces;
using System;
using System.Linq;

namespace Cherry.Managers
{
    internal class CherryDenylist : IDenier
    {
        private readonly Config _config;

        public CherryDenylist(Config config)
        {
            _config = config;
        }

        public void DenySong(string key)
        {
            if (!_config.BannedSongs.Contains(key))
                _config.BannedSongs.Add(key);
        }

        public void DenyUser(string id, DateTime? expiration = null)
        {
            var lid = id.ToLower();
            if (!_config.BannedUsers.Any(u => u.ID.ToLower() == lid))
                _config.BannedUsers.Add(new Config.UserBan { ID = lid, Until = expiration.GetValueOrDefault(), Permanent = !expiration.HasValue });
        }

        public bool IsSongBanned(string key)
        {
            return _config.BannedSongs.Contains(key);
        }

        public bool IsUserBanned(string id)
        {
            var lid = id.ToLower();
            var user = _config.BannedUsers.FirstOrDefault(u => u.ID.ToLower() == lid);
            if (user != null)
            {
                if (user.Permanent) return true;
                bool banned = user.Until > DateTime.Now;

                if (banned) return true;
                _config.BannedUsers.Remove(user);
            }
            return false;
        }
    }
}