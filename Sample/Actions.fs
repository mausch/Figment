module SampleApp.Actions

open System.Web
open System.Web.Mvc
open System.Web.Routing
open System.Collections.Specialized
open Figment.Result

let action1 (ctx: ControllerContext) = empty

let action2 (ctx: ControllerContext) = 
    [1; 2] |> Seq.toArray |> view "action2viewName"

let action3 () = "<h1>Hello World</h1>"

type Model = { One: int; Two: int; Three: string }

let action4 (ctx: ControllerContext) =
    {One = 1; Two = 2; Three = "pepe"} |> view "action4view"

let action5 (ctx: ControllerContext) =
    redirect "/"

/// redirectToAction does not work
(*let action6 (ctx: ControllerContext) =
    redirectToAction action2*)

(*let postAction6 (ctx: ControllerContext) = 
    //redirect "/"
    content "<h1>posted action6</h1>"*)

let postAction6Easy (form: NameValueCollection) =
    content "<h1>posted action6</h1>"

let postAction6Easier (somefield: string) =
    sprintf "posted %s" somefield |> content

// how do I bind this?
let querystringAndForm (form: NameValueCollection) (qs: NameValueCollection) = 
    sprintf "form : %s<br/>querystring:%s" form.["formvalue"] qs.["qsvalue"]