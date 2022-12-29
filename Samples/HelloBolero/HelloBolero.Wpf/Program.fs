namespace HelloBolero.Wpf
module Program =
    open System
    open Eto.Forms
    open Eto.Blazor
    open Eto.Blazor.Wpf
    open HelloBolero

    [<EntryPoint>]
    [<STAThread>]
    let Main(args) = 
        let platform = new Eto.Wpf.Platform()
        platform.Add<BlazorWebView.IHandler>(fun () -> new EtoBlazorWebViewHandler())
        platform.Add<IEtoBlazorWebViewAdder>(fun () -> new EtoBlazorWebViewAdder())
        let app = new Application(platform)
        app.Run(new MainForm())
        0