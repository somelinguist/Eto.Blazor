using Eto;
using Eto.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class EtoBlazorWebviewServiceCollectionExtension
    {
        public static IEtoBlazorWebViewBuilder AddEtoBlazorWebView(this IServiceCollection services)
        {
            var platform = Platform.Instance;
            if (platform == null)
                throw new InvalidOperationException("Platform instance is null. Have you created your application?");
            var blazorAdder = platform.Create<IEtoBlazorWebViewAdder>();
            if (blazorAdder == null)
                throw new InvalidOperationException("Type for 'IEtoBlazorWebViewAdder' could not be found in this platform");
            blazorAdder.AddBlazorWebView(services);
            return new EtoBlazorWebViewBuilder(services);
        }

        public static void AddEtoDefaultBlazorWebView(this IServiceCollection services)
        {
            services.AddBlazorWebView();
            //services.TryAddSingleton(new BlazorWebViewDeveloperTools { Enabled = false });
            services.TryAddSingleton<EtoBlazorMarkerService>();
        }
    }
}
