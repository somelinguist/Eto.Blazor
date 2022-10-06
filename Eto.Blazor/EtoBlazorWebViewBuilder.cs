using Microsoft.Extensions.DependencyInjection;

namespace Eto.Blazor
{
    internal class EtoBlazorWebViewBuilder : IEtoBlazorWebViewBuilder
    {
        public IServiceCollection Services { get; }

        public EtoBlazorWebViewBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }

    public interface IEtoBlazorWebViewAdder
    {
        void AddBlazorWebView(IServiceCollection services);
    }
}