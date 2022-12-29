namespace HelloBolero

open System
open Eto.Forms
open Eto.Drawing
open Eto.Blazor
open Microsoft.Extensions.DependencyInjection
open Counter

type MainForm() as this =
    inherit Form()

    let blazorWebView = new BlazorWebView()
    let services = new ServiceCollection()

    let mutable counter = 0
    let counterChanged = new Event<int>()

    let counterService =
        {
            increment = fun () -> async { 
                counter <- counter + 1 
                counterChanged.Trigger(counter)
                }
            decrement = fun () -> async { 
                counter <- counter - 1 
                counterChanged.Trigger(counter)
                }
            subscribe = fun f -> counterChanged.Publish.Add(f)
        }

    do
        base.Title <- "My Eto Form"
        base.MinimumSize <- new Size(200, 200)

        services.AddEtoBlazorWebView() |> ignore

        #if DEBUG
        services.AddEtoBlazorWebViewDeveloperTools() |> ignore
        #endif

        blazorWebView.HostPage <- "wwwroot/index.html"
        blazorWebView.Services <- services.BuildServiceProvider()
        blazorWebView.RootComponents.Add<CounterApp>("#app", ([("CounterService", box counterService)] 
                                                                         |> dict))
        blazorWebView.Size <- new Size(500, 500)

        let counterText = new Label(Text = $"{counter}")
        counterService.subscribe(fun value -> 
            Application.Instance.AsyncInvoke( fun _ -> counterText.Text <- $"{value}"))

        let btnIncrement = new Button(Text="+")
        btnIncrement.Click.Add(fun _ -> counterService.increment() |> Async.RunSynchronously)

        let btnDecrement = new Button(Text="-")
        btnDecrement.Click.Add(fun _ -> counterService.decrement() |> Async.RunSynchronously)

        let layout = 
            new TableLayout(
                Spacing = Size(5,5),
                Padding = Padding(10)
            )

        [
            TableRow(
                TableCell(
                    new StackLayout(
                        StackLayoutItem(btnDecrement),
                        StackLayoutItem(counterText),
                        StackLayoutItem(btnIncrement),
                        Orientation=Orientation.Horizontal
                    )
                )
            )
            TableRow(
                TableCell(blazorWebView, true)
            )
        ]
        |> List.iter layout.Rows.Add
        
        base.Content <- layout

        let quitCommand = new Command(MenuText = "Quit")
        quitCommand.Shortcut <- Application.Instance.CommonModifier ||| Keys.Q
        quitCommand.Executed.Add(fun e -> Application.Instance.Quit())

        let aboutCommand = new Command(MenuText = "About...")
        aboutCommand.Executed.Add(fun e ->
            let dlg = new AboutDialog()
            dlg.ShowDialog(this) |> ignore
            )

        base.Menu <- new MenuBar()
        let fileItem = new ButtonMenuItem(Text = "&File")
        base.Menu.ApplicationItems.Add(new ButtonMenuItem(Text = "&Preferences..."))
        base.Menu.QuitItem <- quitCommand.CreateMenuItem()
        base.Menu.AboutItem <- aboutCommand.CreateMenuItem()

        base.ToolBar <- new ToolBar()

    member _.BlazorWebView with get() = blazorWebView