using System;
using Eto.Blazor;
using Eto.Blazor.Wpf;
using Eto.Forms;

namespace HelloBlazor.Wpf
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var platform = new Eto.Wpf.Platform();
            platform.Add<BlazorWebView.IHandler>(() => new EtoBlazorWebViewHandler());
            platform.Add<IEtoBlazorWebViewAdder>(() => new EtoBlazorWebViewAdder());
            new Application(platform).Run(new MainForm());
        }
    }
}