using ChatCore;
using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Services.Twitch;
using Cherry.Interfaces;
using Cherry.Models;
using IPA.Utilities;
using SiraUtil.Tools;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Cherry.Managers
{
    internal class TwitchRequestSource : ICherryRequestSource
    {
        public event EventHandler<RequestEventArgs>? SongRequested;
        public event EventHandler<CancelEventArgs>? RequestCancelled;
        private readonly FieldAccessor<TwitchService, IWebSocketService>.Accessor ServiceSocket = FieldAccessor<TwitchService, IWebSocketService>.GetAccessor("_websocketService");

        private readonly Config _config;
        private readonly SiraLog _siraLog;
        private TwitchService? _twitchService;
        private readonly ChatCoreInstance _chatCoreInstance;
        private readonly Dictionary<string, RequestEventArgs> _lazyTinyRequestCache = new Dictionary<string, RequestEventArgs>();

        public TwitchRequestSource(Config config, SiraLog siraLog, UBinder<Plugin, ChatCoreInstance> chatCoreInstance)
        {
            _config = config;
            _siraLog = siraLog;
            _chatCoreInstance = chatCoreInstance.Value;
        }

        public async Task Run()
        {
            _twitchService = _chatCoreInstance.RunTwitchServices();
            _twitchService.OnTextMessageReceived += ChatMessageReceived;

            if (!ServiceSocket(ref _twitchService).IsConnected)
            {
                // the black magic bit from eris
                SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);
                void ReleaseSemaphore(IChatService _)
                {
                    _twitchService.OnLogin -= ReleaseSemaphore;
                    semaphoreSlim?.Release();
                }
                try
                {
                    _twitchService.OnLogin += ReleaseSemaphore;
                    await semaphoreSlim.WaitAsync(CancellationToken.None);
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                    ReleaseSemaphore(null!);
                }
            }
        }

        private void ChatMessageReceived(IChatService service, IChatMessage message)
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
                RequestEventArgs args = new RequestEventArgs(key, requester);
                SongRequested?.Invoke(message.Channel, args);

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
                    CancelEventArgs args = new CancelEventArgs(request.Key, request.Requester);
                    SendMessage(message.Channel, $"Removed {request.Key} from the queue.");
                    _lazyTinyRequestCache.Remove(message.Sender.Id);
                    RequestCancelled?.Invoke(message.Channel, args);
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
            if (sender is IChatChannel channel)
            {
                _twitchService?.SendTextMessage(message, channel);
            }
        }
    }
}