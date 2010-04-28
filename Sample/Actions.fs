module Actions

open System.Web
open System.Web.Mvc
open System.Web.Routing
open FSharpMvc.Result

let action1 (ctx: ControllerContext) = 
    empty

let action2 (ctx: ControllerContext) = 
    view [1; 2]

let action3 () = "<h1>Hello World</h1>"