module FSharpMvc.Result

open System.Web.Mvc

let empty = EmptyResult() :> ActionResult

let view model = 
    let viewData = ViewDataDictionary(Model = model)
    ViewResult(ViewData = viewData) :> ActionResult

let content s = 
    ContentResult(Content = s) :> ActionResult

let redirect url =
    RedirectResult(url) :> ActionResult