module SampleApp.Actions

open System.Web
open System.Web.Mvc
open System.Web.Routing
open FSharpMvc.Result

let action1 (ctx: ControllerContext) = empty

let action2 (ctx: ControllerContext) = 
    [1; 2] |> Seq.toArray |> view "action2viewName"

let action3 () = "<h1>Hello World</h1>"

type Model = { One: int; Two: int; Three: string }

let action4 (ctx: ControllerContext) =
    {One = 1; Two = 2; Three = "pepe"} |> view "action4view"

let action5 (ctx: ControllerContext) =
    redirect "/"

let action6 (ctx: ControllerContext) =
    redirectToAction action2