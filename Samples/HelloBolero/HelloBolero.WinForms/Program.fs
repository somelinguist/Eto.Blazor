namespace HelloBolero.WinForms
module Program =
    open System
    open Eto.Forms
    open Eto.Blazor
    open Eto.Blazor.WinForms
    open HelloBolero

    [<EntryPoint>]
    [<STAThread>]
    let Main(args) = 
        let platform = new Eto.WinForms.Platform()
        platform.Add<BlazorWebView.IHandler>(fun () -> new EtoBlazorWebViewHandler())
        platform.Add<IEtoBlazorWebViewAdder>(fun () -> new EtoBlazorWebViewAdder())
        let app = new Application(platform)
        app.Run(new MainForm())
        0