using Cherry.Interfaces;
using Cherry.Models;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            _requestManager.SongSkipped += SongSkipped;
            _requestManager.SongAccepted += SongAccepted;
            _requestManager.SongRequested += SongRequested;

            if (!_historyFolder.Exists)
                _historyFolder.Create();
        }

        private void SongSkipped(object sender, RequestEventArgs e)
        {
            var request = _cachedRequests.FirstOrDefault(c => c.Args.RequestTime == e.RequestTime && c.Args.Requester.ID == e.Requester.ID);
            var requests = _cachedRequests.Where(cr => cr != request).ToArray();
            _cachedRequests.Clear();
            for (int i = requests.Length - 1; i >= 0; i--)
                _cachedRequests.Push(requests[i]);
            Task.Run(Save);
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
            var req = _cachedRequests.Where(c => c.Args.RequestTime == e.RequestTime && c.Args.Requester.ID == e.Requester.ID).FirstOrDefault();
            if (req != null)
                req.WasPlayed = true;
            await Task.Run(Save);
        }

        private async Task SongRequestedCallbackAsync(RequestEventArgs e)
        {
            await History();
            _cachedRequests.Push(new CachedRequest
            {
                Args = e,
                WasPlayed = false,
                Key = e.Key,
                Priority = e.Priority,
                SaveTime = DateTime.Now,
                Modifiers = e.Modifiers,
                Difficulty = e.Difficulty,
                RequestTime = e.RequestTime,
                Requester = new GenericRequester(e.Requester.ID, e.Requester.Username, e.Requester.Elevation)
            });
            await Task.Run(Save);
        }

        public void Dispose()
        {
            _requestManager.SongSkipped -= SongSkipped;
            _requestManager.SongAccepted -= SongAccepted;
            _requestManager.SongRequested -= SongRequested;
            _ = Save();
        }

        private async Task Save()
        {
            using FileStream fs = File.Create(_cacheFile.FullName);
            using MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_cachedRequests)));
            await ms.CopyToAsync(fs);
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
                    _cachedRequests.Clear();
                    for (int i = 0; i < requests.Length; i++)
                    {
                        var req = requests[i];
                        req.Args = new RequestEventArgs(req.Key, req.Requester, req.RequestTime, req.Difficulty, req.Modifiers, req.Priority);
                        _cachedRequests.Push(requests[i]);
                    }
                }
                catch (Exception e)
                {
                    _siraLog.Error("Error occured while deserializing the cache.");
                    _siraLog.Error(e);
                }
            }
            _didLoadFileHistory = true;
            return _cachedRequests;
        }
    }
}