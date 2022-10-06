using System;
using System.Threading.Tasks;
using Eto.Forms;

namespace Eto.Blazor;

public class BlazorWebViewInitializedEventArgs : EventArgs
{
#nullable disable
    public WebView WebView { get; internal set; }
    // TODO: This isn't used because the internal webview varies from platform to platform.
}