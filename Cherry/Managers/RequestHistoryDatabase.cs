using Cherry.Interfaces;
using Cherry.Models;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zenject;

namespace Cherry.Managers
{
    internal class RequestHistoryDatabase : IRequestHistory, IInitializable, IDisposable
    {
        private readonly SiraLog _siraLog;
        private readonly IRequestManager _requestManager;

        private bool _didLoadFileHistory;
        private readonly Stack<CachedRequest> _cachedRequests = new Stack<CachedRequest>();

        private static readonly DirectoryInfo _historyFolder = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, nameof(Cherry)));
        private static readonly FileInfo _cacheFile = new FileInfo(Path.Combine(_historyFolder.FullName, "cache.json"));

        public RequestHistoryDatabase(SiraLog siraLog, IRequestManager requestManager)
        {
            _siraLog = siraLog;
            _requestManager = requestManager;
        }

        public void Initialize()
        {
            _requestManager.SongRequested += SongRequested;
            if (!_historyFolder.Exists)
                _historyFolder.Create();
        }

        private void SongAccepted(object sender, RequestEventArgs e)
        {
            _ = SongAcceptedCallbackAsync(e);
        }

        private void SongRequested(object sender, RequestEventArgs e)
        {
            _ = SongRequestedCallbackAsync(e);
        }
        
        private async Task SongAcceptedCallbackAsync(RequestEventArgs e)
        {
            await History();
            var req = _cachedRequests.Where(c => c.Args == e).FirstOrDefault();
            if (req != null)
                req.WasPlayed = true;
        }

        private async Task SongRequestedCallbackAsync(RequestEventArgs e)
        {
            await History();
            _cachedRequests.Push(new CachedRequest
            {
                Args = e,
                WasPlayed = false,
                SaveTime = DateTime.Now,
            });
        }

        public void Dispose()
        {
            _requestManager.SongRequested -= SongRequested;
        }

        public async Task<IEnumerable<CachedRequest>> History()
        {
            if (_didLoadFileHistory)
            {
                return _cachedRequests;
            }
            if (_cacheFile.Exists)
            {
                using FileStream fs = _cacheFile.OpenRead();
                using StreamReader sr = new StreamReader(fs);
                string json = await sr.ReadToEndAsync();
                try
                {
                    var requests = JsonConvert.DeserializeObject<CachedRequest[]>(json);
                    for (int i = requests.Length - 1; i >= 0; i--)
                        _cachedRequests.Push(requests[i]);
                }
                catch (Exception e)
                {
                    _siraLog.Error("Error occured while deserializing the cache.");
                    _siraLog.Logger.Error(e);
                }
            }
            _didLoadFileHistory = true;
            return _cachedRequests;
        }
    }
}