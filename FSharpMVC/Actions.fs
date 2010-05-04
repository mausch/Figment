module FSharpMvc.Actions

open System.Web.Mvc

let content str (ctx: ControllerContext) = 
    Result.content str

let redirect str (ctx: ControllerContext) = 
    Result.redirect str

let view str model (ctx: ControllerContext) = 
    Result.view str model

let empty (ctx: ControllerContext) = Result.empty