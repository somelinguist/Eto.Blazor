using Microsoft.Extensions.DependencyInjection;

namespace Eto.Blazor
{
    /// <summary>
    /// A builder for Eto Blazor WebViews.
    /// </summary>
    public interface IEtoBlazorWebViewBuilder
    {
        /// <summary>
        /// Gets the builder service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}