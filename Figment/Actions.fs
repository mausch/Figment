module Figment.Actions

open System.Web.Mvc
open Figment.Helpers

// operators
let concat (a: FAction) (b: FAction) (ctx: ControllerContext) =
    let x = a ctx
    let y = b ctx
    Result.concat x y

let (=>) = concat

// actions

let content str (ctx: ControllerContext) = 
    Result.content str

let redirect str (ctx: ControllerContext) = 
    Result.redirect str

let view str model (ctx: ControllerContext) = 
    Result.view str model

let empty (ctx: ControllerContext) = Result.empty

let notFound (ctx: ControllerContext) = Result.notFound()
    
let status code (ctx: ControllerContext) =
    Result.status code