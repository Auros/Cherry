using System;
using System.Threading;
using IPA.Utilities.Async;

namespace Cherry
{
    internal class MainThreadInvoker
    {
        private static CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        public static void ClearQueue()
        {
            _cancellationToken.Cancel();
            _cancellationToken = new CancellationTokenSource();
        }

        public static void Invoke(Action? action)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(action, _cancellationToken.Token);
            }
        }

        public static void Invoke<TA>(Action<TA>? action, TA a)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => action(a), _cancellationToken.Token);
            }
        }

        public static void Invoke<TA, TB>(Action<TA, TB>? action, TA a, TB b)
        {
            if (action != null)
            {
                UnityMainThreadTaskScheduler.Factory.StartNew(() => action.Invoke(a, b), _cancellationToken.Token);
            }
        }
    }
}