open Giraffe
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

open Parse
open Fun
open MicroMLController 

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()
    app.UseStaticFiles() |> ignore

    let webApp =
        choose [
            route "/" >=> htmlFile "./Views/index.html"
            route "/api/parse" >=> POST >=> MicroMLController.parseHandler 
        ]

    app.UseGiraffe(webApp)

    // Debugging / example parsing output (optional)
    printfn "%A" Parse.e1

    printfn "%A" Parse.e2

    eval Parse.e1 [] |> printfn "%A"    
    eval Parse.e2 [] |> printfn "%A"

    print Parse.e1 |> printfn "%A"
    print Parse.e2 |> printfn "%A"

    app.Run()
    0
