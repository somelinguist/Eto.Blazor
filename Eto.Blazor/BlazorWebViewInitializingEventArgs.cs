using Eto.Forms;

namespace Eto.Blazor
{
    public class BlazorWebViewInitializingEventArgs : EventArgs
    {
# nullable disable
        public string Stuff { get; set; }
        // TODO: BlazorWebViewInitializingEventArgs is suspuposd to give a chance for the program to configure the
        // internal webview used by BlazorWebView, but these configurate settings vary by platform.
    }
}

