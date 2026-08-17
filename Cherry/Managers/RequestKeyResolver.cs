using Cherry.Models;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal sealed class RequestKeyResolver
    {
        private static readonly Regex LinkPattern = new Regex(
            @"(https?://[^\s<>""]+)|(?:\b(?:bsr|bsid)[:\s#-]*([0-9a-f]{1,8})\b)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BeatSaverIdPattern = new Regex(@"^[0-9a-f]{1,8}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HashPattern = new Regex(@"^[0-9a-f]{40}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HashSearchPattern = new Regex(@"\b[0-9a-f]{40}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly SiraLog _siraLog;
        private readonly IHttpService _httpService;
        private readonly MapStore _mapStore;

        public RequestKeyResolver(SiraLog siraLog, IHttpService httpService, MapStore mapStore)
        {
            _siraLog = siraLog;
            _httpService = httpService;
            _mapStore = mapStore;
        }

        internal async Task<string?> ResolveAsync(string input, CancellationToken cancellationToken)
        {
            if (FirstTarget(input) is not RequestTarget target)
                return null;

            switch (target.Kind)
            {
                case RequestTargetKind.BeatSaverId:
                    return target.Value;
                case RequestTargetKind.BeatSaverHash:
                    return (await _mapStore.GetMapByHashAsync(target.Value, cancellationToken))?.Key;
                case RequestTargetKind.ScoreSaberMapId:
                    return await ResolveScoreSaberMapAsync(target.Value, cancellationToken);
                default:
                    return null;
            }
        }

        private async Task<string?> ResolveScoreSaberMapAsync(string mapId, CancellationToken cancellationToken)
        {
            var response = await _httpService.GetAsync($"https://scoresaber.com/api/v2/maps/{mapId}", cancellationToken: cancellationToken);
            if (!response.Successful)
            {
                _siraLog.Warn($"ScoreSaber could not resolve map {mapId}: {response.Code}");
                return null;
            }

            var map = JsonConvert.DeserializeObject<ScoreSaberMap>(await response.ReadAsStringAsync());
            if (map == null)
                return null;

            if (BeatSaverIdPattern.IsMatch(map.BeatSaverId))
                return map.BeatSaverId;

            if (HashPattern.IsMatch(map.Hash))
                return (await _mapStore.GetMapByHashAsync(map.Hash, cancellationToken))?.Key;

            return null;
        }

        private static RequestTarget? FirstTarget(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            foreach (Match match in LinkPattern.Matches(input))
            {
                if (match.Groups[2].Success)
                    return new RequestTarget(RequestTargetKind.BeatSaverId, match.Groups[2].Value);

                if (TryParseUrl(TrimToken(match.Value), out var target))
                    return target;
            }

            var token = TrimToken(input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty);
            if (BeatSaverIdPattern.IsMatch(token))
                return new RequestTarget(RequestTargetKind.BeatSaverId, token);
            if (HashPattern.IsMatch(token))
                return new RequestTarget(RequestTargetKind.BeatSaverHash, token);
            return null;
        }

        private static bool TryParseUrl(string token, out RequestTarget target)
        {
            target = default;
            if (!Uri.TryCreate(token, UriKind.Absolute, out var uri))
                return false;

            var host = uri.Host.ToLowerInvariant();
            if (host == "beatsaver.com" || host.EndsWith(".beatsaver.com", StringComparison.Ordinal))
                return TryParseBeatSaverUrl(uri, out target);

            if (host == "scoresaber.com" || host.EndsWith(".scoresaber.com", StringComparison.Ordinal))
            {
                if (TryParseScoreSaberUrl(uri, out target))
                    return true;

                var hash = HashSearchPattern.Match(uri.ToString());
                if (hash.Success)
                {
                    target = new RequestTarget(RequestTargetKind.BeatSaverHash, hash.Value);
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseBeatSaverUrl(Uri uri, out RequestTarget target)
        {
            target = default;
            var parts = uri.AbsolutePath.Trim('/').Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].Equals("map", StringComparison.OrdinalIgnoreCase)
                    && !parts[i].Equals("maps", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 2 < parts.Length && parts[i + 1].Equals("hash", StringComparison.OrdinalIgnoreCase) && HashPattern.IsMatch(parts[i + 2]))
                {
                    target = new RequestTarget(RequestTargetKind.BeatSaverHash, parts[i + 2]);
                    return true;
                }

                if (i + 2 < parts.Length && parts[i + 1].Equals("id", StringComparison.OrdinalIgnoreCase) && BeatSaverIdPattern.IsMatch(parts[i + 2]))
                {
                    target = new RequestTarget(RequestTargetKind.BeatSaverId, parts[i + 2]);
                    return true;
                }

                if (i + 1 < parts.Length && BeatSaverIdPattern.IsMatch(parts[i + 1]))
                {
                    target = new RequestTarget(RequestTargetKind.BeatSaverId, parts[i + 1]);
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseScoreSaberUrl(Uri uri, out RequestTarget target)
        {
            target = default;
            var parts = uri.AbsolutePath.Trim('/').Split('/');
            for (int i = 0; i + 1 < parts.Length; i++)
            {
                if (parts[i].Equals("map", StringComparison.OrdinalIgnoreCase) && int.TryParse(parts[i + 1], out _))
                {
                    target = new RequestTarget(RequestTargetKind.ScoreSaberMapId, parts[i + 1]);
                    return true;
                }
            }

            return false;
        }

        private static string TrimToken(string token) => token.TrimEnd('.', ',', ';', ':', ')', ']', '}');

        private enum RequestTargetKind
        {
            BeatSaverId,
            BeatSaverHash,
            ScoreSaberMapId
        }

        private readonly struct RequestTarget
        {
            internal RequestTargetKind Kind { get; }
            internal string Value { get; }

            internal RequestTarget(RequestTargetKind kind, string value)
            {
                Kind = kind;
                Value = value;
            }
        }

        private sealed class ScoreSaberMap
        {
            [JsonProperty("hash")]
            internal string Hash { get; set; } = string.Empty;

            [JsonProperty("bsid")]
            internal string BeatSaverId { get; set; } = string.Empty;
        }
    }
}
