module FSharpMvc.Binding

open System
open System.Web.Mvc
open System.Globalization
open System.Collections.Specialized
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

let ignoreContext (action: unit -> 'a) (ctx: ControllerContext) =
    action()

let bind (parameter: string) (f: 'a -> 'b) (ctx: ControllerContext) = 
    let binder = ModelBinders.Binders.GetBinder typeof<'a>
    let bindingContext = ModelBindingContext(
                            ModelName = parameter,
                            ModelState = ctx.Controller.ViewData.ModelState, 
                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof<'a>),
                            ValueProvider = ctx.Controller.ValueProvider)
    let r = binder.BindModel(ctx, bindingContext)
    if not bindingContext.ModelState.IsValid
        then failwith "Binding failed"
    f (r :?> 'a)


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

    let binder = ModelBinders.Binders.GetBinder typeof<'a>
    let bindingContext = ModelBindingContext(
                            ModelName = pname, 
                            ModelState = ctx.Controller.ViewData.ModelState, 
                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof<'a>),
                            ValueProvider = ctx.Controller.ValueProvider)
    let r = binder.BindModel(ctx, bindingContext)
    if not bindingContext.ModelState.IsValid
        then failwith "Binding failed"
    action (r :?> 'a)

let bindFormToRecord (action: 'a -> 'b) =
    ()