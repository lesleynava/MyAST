module MicroMLController

open Microsoft.AspNetCore.Http
open Giraffe
open System.Text.Json
open Parse
open Absyn

// A helper function to convert AST to bracketed notation
let rec toBracketed expr =
    match expr with
    | CstI i -> sprintf "[CstI \"%d\"]" i
    | CstB b             -> sprintf "[CstB %b]" b
    | Var x              -> sprintf "[Var %s]" x
    | Let(x, e1, e2)     -> sprintf "[Let [Var %s] %s %s]" x (toBracketed e1) (toBracketed e2)
    | Prim(op, e1, e2)   -> sprintf "[Prim \"%s\" %s %s]" op (toBracketed e1) (toBracketed e2)
    | If(e1, e2, e3)     -> sprintf "[If %s %s %s]" (toBracketed e1) (toBracketed e2) (toBracketed e3)
    | Letfun(f, x, b, c) -> sprintf "[Letfun %s %s %s %s]" f x (toBracketed b) (toBracketed c)
    | Call(e1, e2)       -> sprintf "[Call %s %s]" (toBracketed e1) (toBracketed e2)
    | _                  -> "[Unknown]"


// The API handler
let parseHandler : HttpHandler =
    fun next ctx ->
        task {
            use reader = new System.IO.StreamReader(ctx.Request.Body)
            let! body = reader.ReadToEndAsync()
            let json = JsonDocument.Parse(body)
            let code = json.RootElement.GetProperty("code").GetString()

            try
                let expr = Parse.fromString code
                let tree = toBracketed expr
                return! text tree next ctx
            with ex ->
                return! RequestErrors.badRequest (text $"Error: {ex.Message}") next ctx
        }
