using System;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using Microsoft.AspNetCore.Components;

namespace Eto.Blazor
{
    public sealed class EtoDispatcher : Dispatcher
    {
        readonly Application _dispatcher;

        public EtoDispatcher(Application dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public override bool CheckAccess()
        {
            try
            {
                _dispatcher.EnsureUIThread();
            }
            catch (UIThreadAccessException)
            {
                return false;
            }
            return true;
        }

        public override Task InvokeAsync(Action workItem)
        {
            return _dispatcher.InvokeAsync(workItem);
        }

        public override Task InvokeAsync(Func<Task> workItem)
        {
            return _dispatcher.InvokeAsync(workItem);
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
        {
            return _dispatcher.InvokeAsync(workItem);
        }

        public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
        {
            var tcs = new TaskCompletionSource<TResult>();
            _dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var ret = await workItem().ConfigureAwait(false);
                    tcs.SetResult(ret);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }
    }
}