using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace yupisoft.ConfigServer.Core
{
    public static class TaskExtensions
    {
        public sealed class DisposableScope : IDisposable
        {
            private readonly Action _closeScopeAction;
            public DisposableScope(Action closeScopeAction)
            {
                _closeScopeAction = closeScopeAction;
            }
            public void Dispose()
            {
                _closeScopeAction();
            }
        }

        public static IDisposable CreateTimeoutScope(this IDisposable disposable, TimeSpan timeSpan)
        {
            var cancellationTokenSource = new CancellationTokenSource(timeSpan);
            var cancellationTokenRegistration = cancellationTokenSource.Token.Register(disposable.Dispose);
            return new DisposableScope(
                () =>
                {
                    cancellationTokenRegistration.Dispose();
                    cancellationTokenSource.Dispose();
                    disposable.Dispose();
                });
        }

        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout).ContinueWith(_ => default(TResult), TaskContinuationOptions.ExecuteSynchronously);
            return Task.WhenAny(task, timeoutTask).Unwrap();
        }

        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout).ContinueWith((t) => { throw new TimeoutException(); }, TaskContinuationOptions.ExecuteSynchronously);
            var taskResult = Task.WhenAny(task, timeoutTask).Unwrap();
            return taskResult;
        }
    }
}
