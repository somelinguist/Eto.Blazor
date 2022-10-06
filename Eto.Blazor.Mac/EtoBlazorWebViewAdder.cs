using Eto.Blazor;
using Microsoft.Extensions.DependencyInjection;

namespace Eto.Blazor.Mac
{
    public class EtoBlazorWebViewAdder : IEtoBlazorWebViewAdder
    {
        void IEtoBlazorWebViewAdder.AddBlazorWebView(IServiceCollection services)
        {
            services.AddEtoDefaultBlazorWebView();
        }
    }
}