using Legato.Beatmaps;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Cherry.Models.Map;

namespace Cherry.Managers
{
    internal class CherryLevelManager
    {
        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly BeatmapLevelsModel _beatmapLevelsModel;

        public CherryLevelManager(SiraLog siraLog, IHttpService httpService, BeatmapLevelsModel beatmapLevelsModel)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _beatmapLevelsModel = beatmapLevelsModel;
        }

        public BeatmapLevel? TryGetLevel(string hash, State state)
        {
            var levelId = $"custom_level_{hash.ToUpperInvariant()}";
            return _beatmapLevelsModel.GetLoadedBeatmapLevels().FirstOrDefault(level =>
                state != State.Published
                    ? level.levelID.StartsWith(levelId, StringComparison.OrdinalIgnoreCase)
                    : level.levelID.Equals(levelId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<BeatmapLevel?> DownloadLevel(string name, string hash, string url, State state, CancellationToken token, IProgress<float>? downloadProgress = null)
        {
            var response = await _httpService.GetAsync(url, downloadProgress, token);
            if (!response.Successful)
            {
                _siraLog.Error(response.Code);
                return null;
            }

            // SongCore constructs the WIP path itself and doesn't expose it
            var extractPath = await ExtractZipAsync(await response.ReadAsByteArrayAsync(), name, state == State.Published ? CustomLevelPathHelper.customLevelsDirectoryPath : Path.Combine(Application.dataPath, "CustomWIPLevels"));
            if (string.IsNullOrEmpty(extractPath))
                return null;

            if (!await SongCore.Loader.Instance.RefreshSongsAsync(false, TimeSpan.FromSeconds(30), token))
            {
                _siraLog.Warn($"Timed out waiting for SongCore to load {name}.");
                return null;
            }

            return TryGetLevel(hash, state);
        }

        private async Task<string> ExtractZipAsync(byte[] zip, string name, string customSongsPath, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));

                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                string basePath = name;
                string path = customSongsPath + "/" + r.Replace(basePath, "");
                if (!overwrite && Directory.Exists(path))
                {
                    int pathNum = 1;
                    while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                    path += $" ({pathNum})";
                }
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        var entryPath = Path.Combine(path, entry.Name);
                        if (overwrite || !File.Exists(entryPath))
                            entry.ExtractToFile(entryPath, overwrite);
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
                return path;
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
                zipStream.Close();
                return "";
            }
        }
    }
}
