module FSharpMvc.Binding

open System
open System.Web.Mvc
open System.Globalization
open System.Collections.Specialized
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

let ignoreContext (action: unit -> 'a) (ctx: ControllerContext) =
    action()

let bindOne<'a> (parameter: string) (ctx: ControllerContext) = 
    let binder = ModelBinders.Binders.GetBinder typeof<'a>
    let bindingContext = ModelBindingContext(
                            ModelName = parameter,
                            ModelState = ctx.Controller.ViewData.ModelState, 
                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof<'a>),
                            ValueProvider = ctx.Controller.ValueProvider)
    let r = binder.BindModel(ctx, bindingContext)
    if not bindingContext.ModelState.IsValid
        then failwithf "Binding failed for model name %s, value provider %s" parameter (ctx.Controller.ValueProvider.GetType().Name)
    r :?> 'a
    
let bind (parameter: string) (f: 'a -> 'b) (ctx: ControllerContext) = 
    let r = bindOne<'a> parameter ctx
    f r

let bind2 (parameter1: string) (parameter2: string) (f: 'a -> 'b -> 'c) (ctx: ControllerContext) = 
    let v1 = bindOne<'a> parameter1 ctx
    let v2 = bindOne<'b> parameter2 ctx
    f v1 v2

/// bind2 implemented on top of bind (instead of bindOne)
let bind2alt (parameter1: string) (parameter2: string) (f: 'a -> 'b -> 'c) (ctx: ControllerContext) = 
    let b1 = bind parameter1 f
    let b1 b a = b1 a b
    let b1 = bind parameter2 b1
    let b1 b a = b1 a b
    b1 ctx ctx

let contentResult (action: 'a -> string) a =
    action a |> Result.content

let formAction (action: NameValueCollection -> 'a) (ctx: ControllerContext) =
    action ctx.HttpContext.Request.Form

let querystringAction (action: NameValueCollection -> 'a) (ctx: ControllerContext) = 
    action ctx.HttpContext.Request.QueryString

let bindForm (action: 'a -> 'b) (e: Expr<'a -> 'b>) (ctx: ControllerContext) = 
    let pname = match e with
                | Lambda(var, body) -> var.Name
                | x -> failwithf "Expected lambda, actual %A" x

    let r = bindOne<'a> pname ctx
    action r

let bindFormToRecord (action: 'a -> 'b) =
    ()