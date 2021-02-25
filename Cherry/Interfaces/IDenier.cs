using System;

namespace Cherry.Interfaces
{
    public interface IDenier
    {
        void DenySong(string key);
        void DenyUser(string id, DateTime? expiration = null);

        bool IsSongBanned(string key);
        bool IsUserBanned(string id);
    }
}