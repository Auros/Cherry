using CatCore;
using CatCore.Models.Twitch;
using CatCore.Models.Twitch.IRC;
using CatCore.Services.Twitch.Interfaces;
using Cherry.Interfaces;
using Cherry.Models;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class TwitchRequestSource : ICherryRequestSource
    {
        public event EventHandler<RequestEventArgs>? SongRequested;
        public event EventHandler<RequestEventArgs>? RequestCancelled;

        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private readonly CatCoreInstance _chatCoreInstance;
        private readonly Dictionary<string, RequestEventArgs> _lazyTinyRequestCache = new Dictionary<string, RequestEventArgs>();
        private ITwitchService? _twitchService;

        public TwitchRequestSource(Config config, SiraLog siraLog, UBinder<Plugin, CatCoreInstance> chatCoreInstance)
        {
            _config = config;
            _siraLog = siraLog;
            _chatCoreInstance = chatCoreInstance.Value;
        }

        public async Task Run()
        {
            _twitchService = _chatCoreInstance.RunTwitchServices();
            _twitchService.OnTextMessageReceived += ChatMessageReceived;
            while (!_twitchService.LoggedIn)
                await Task.Yield();
        }

        private void ChatMessageReceived(ITwitchService service, TwitchMessage message)
        {
            if (message.Message.StartsWith(_config.RequestCommand))
            {
                string[] splitMessage = message.Message.Split(' ');
                if (2 > splitMessage.Length)
                    return;

                string key = splitMessage[1];
                if (!int.TryParse(key, NumberStyles.HexNumber, null, out int _))
                {
                    SendMessage(message.Channel, "Invalid Key");
                    return;
                }

                TwitchUser user = (message.Sender as TwitchUser)!;

                Power elevationLevel = Power.None;
                if (message.Sender.IsBroadcaster)
                    elevationLevel = Power.Level4;
                else if (message.Sender.IsModerator)
                    elevationLevel = Power.Level3;
                else if (user.IsVip)
                    elevationLevel = Power.Level2;
                else if (user.IsSubscriber)
                    elevationLevel = Power.Level1;

                TwitchRequester requester = new TwitchRequester(message.Sender.Id, message.Sender.UserName, elevationLevel);
                RequestEventArgs args = new RequestEventArgs(key, requester, DateTime.Now);
                SongRequested?.Invoke(new TwitchSender(message.Channel, this), args);

                if (_lazyTinyRequestCache.ContainsKey(message.Sender.Id))
                {
                    _lazyTinyRequestCache[message.Sender.Id] = args;
                }
                else
                {
                    _lazyTinyRequestCache.Add(message.Sender.Id, args);
                }
            }
            else if (message.Message.StartsWith(_config.CancelCommand))
            {
                if (_lazyTinyRequestCache.TryGetValue(message.Sender.Id, out RequestEventArgs request))
                {
                    RequestCancelled?.Invoke(new TwitchSender(message.Channel, this), request);
                    SendMessage(message.Channel, $"Removed {request.Key} from the queue.");
                    _lazyTinyRequestCache.Remove(message.Sender.Id);
                }
            }
        }

        public Task Stop()
        {
            if (!(_twitchService is null))
                _twitchService.OnTextMessageReceived -= ChatMessageReceived;
            _chatCoreInstance?.StopTwitchServices();
            _twitchService = null;

            return Task.CompletedTask;
        }

        public void SendMessage(object sender, string message)
        {
            if (sender is TwitchChannel channel)
            {
                if (!_config.AddTwitchTTSPrefix)
                    channel.SendMessage(message);
                else
                    channel?.SendMessage($"! {message}");
            }
        }

        private class TwitchSender : DynamicSender
        {
            private readonly TwitchChannel _channel;
            private readonly ICherryRequestSource _source;

            public TwitchSender(TwitchChannel channel, ICherryRequestSource source)
            {
                _source = source;
                _channel = channel;
            }

            public override void SendMessage(string text)
            {
                _source.SendMessage(_channel, text);
            }
        }
    }
}