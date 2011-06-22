module Figment.Actions

open System.Web.Mvc
open Figment.Helpers
open Figment.Result

// operators
let concat (a: FAction) (b: FAction) : FAction =
    fun ctx ->
        let x = a ctx
        let y = b ctx
        x >>> y

let (=>) = concat

// actions

let content str (ctx: ControllerContext) = 
    Result.content str

let contentf fmt (ctx: ControllerContext) = 
    Printf.kprintf Result.content fmt    

let redirect str (ctx: ControllerContext) = 
    Result.redirect str

let view str model (ctx: ControllerContext) = 
    Result.view str model

let empty (ctx: ControllerContext) = Result.empty

let notFound (ctx: ControllerContext) = Result.notFound()
    
let status code (ctx: ControllerContext) =
    Result.status code

let contentType t (ctx: ControllerContext) =
    Result.contentType t