namespace HelloBolero

module Counter =
    open Microsoft.AspNetCore.Components
    open Elmish
    open Bolero
    open Bolero.Html

    type Model = { counter: int }

    let initModel = { counter = 0 }

    type Message =
        | Increment
        | Decrement
        //| GetValue
        | CounterChanged of int
        | Error of exn

    type CounterService =
        {
            increment: unit -> Async<unit>
            decrement: unit -> Async<unit>
            subscribe: (int -> unit) -> unit
        }

    let update counterService message model =
        match message with
        | Increment ->
            model,
            Cmd.OfAsync.attempt
                counterService.increment
                ()
                Error
        | Decrement ->
            model,
            Cmd.OfAsync.attempt
                counterService.decrement
                ()
                Error
        // | GetValue
        //     model,
        //     Cmd.ofAsync
        //         counterService.getValue
        //         (fun counter -> CounterChanged counter)
        //         Error
        | CounterChanged counter -> { model with counter = counter }, []
        | Error _ -> model, []

    let view model dispatch =
        div {
            button {
                on.click (fun _ -> dispatch Decrement) 
                text "-"
            }
            text $"{model.counter}"
            button {
                on.click (fun _ -> dispatch Increment) 
                text "+"
            }
        }

    type CounterApp() =
        inherit ProgramComponent<Model, Message>()

        [<Parameter>]
        member val CounterService = Unchecked.defaultof<CounterService> with get, set

        override this.Program =
            let appSub _ =
                let sub dispatch =
                    this.CounterService.subscribe(fun value -> dispatch (CounterChanged value))
                Cmd.ofSub sub

            Program.mkProgram (fun _ -> initModel, []) (update this.CounterService) view
            |> Program.withSubscription appSub