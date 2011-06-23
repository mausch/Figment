module Figment.Actions

open System.Web.Mvc
open Figment.Helpers
open Figment.Result
open Figment.ReaderOperators

// actions

let content str : FAction =
    fun _ -> Result.content str

let contentf fmt : FAction = 
    fun _ -> Printf.kprintf Result.content fmt    

let redirect str : FAction =
    fun _ -> Result.redirect str

let view str model : FAction = 
    fun _ -> Result.view str model

let empty : FAction =
    fun _ -> Result.empty

let notFound : FAction =
    fun _ -> Result.notFound()
    
let status code : FAction =
    fun _ -> Result.status code

let contentType t : FAction =
    fun _ -> Result.contentType t