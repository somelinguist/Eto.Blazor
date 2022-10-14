using Eto.Blazor;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace Eto.Blazor.WinForms
{
    public class EtoBlazorWebViewAdder : IEtoBlazorWebViewAdder
    {
        void IEtoBlazorWebViewAdder.AddBlazorWebView(IServiceCollection services)
        {
            services.AddWindowsFormsBlazorWebView();
        }

        public IServiceCollection AddBlazorWebViewDeveloperTools(IServiceCollection services)
        {
            return services.AddBlazorWebViewDeveloperTools();
        }
    }
}
