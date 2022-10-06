using System;
using Eto.Forms;
using Eto.Blazor;
using Eto.Blazor.Mac;

namespace HelloBlazor.Mac
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Mac.Platform();
            platform.Add<BlazorWebView.IHandler>(() => new EtoBlazorWebViewHandler());
            platform.Add<IEtoBlazorWebViewAdder>(() => new EtoBlazorWebViewAdder());
            new Application(platform).Run(new MainForm());
        }
    }
}