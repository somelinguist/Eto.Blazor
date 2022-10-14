using Eto.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Eto.Blazor.Mac
{
    public class EtoBlazorWebViewAdder : IEtoBlazorWebViewAdder
    {
        void IEtoBlazorWebViewAdder.AddBlazorWebView(IServiceCollection services)
        {
            services.TryAddSingleton(new BlazorWebViewDeveloperTools { Enabled = false });
            services.AddEtoDefaultBlazorWebView();
        }

        public IServiceCollection AddBlazorWebViewDeveloperTools(IServiceCollection services)
        {
            return services.AddSingleton<BlazorWebViewDeveloperTools>(new BlazorWebViewDeveloperTools { Enabled = true });
        }
    }

    internal class BlazorWebViewDeveloperTools
    {
        public bool Enabled { get; set; } = false;
    }
}