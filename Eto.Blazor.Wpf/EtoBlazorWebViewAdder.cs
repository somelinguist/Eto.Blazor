using Eto.Blazor;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace Eto.Blazor.Wpf
{
    public class EtoBlazorWebViewAdder : IEtoBlazorWebViewAdder
    {
        void IEtoBlazorWebViewAdder.AddBlazorWebView(IServiceCollection services)
        {
            services.AddWpfBlazorWebView();
        }
    }
}
