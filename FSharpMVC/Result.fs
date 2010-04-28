module FSharpMvc.Result

open System.Web.Mvc

let empty = EmptyResult() :> ActionResult

let view viewName model = 
    let viewData = ViewDataDictionary(Model = model)
    ViewResult(ViewData = viewData, ViewName = viewName) :> ActionResult

let content s = 
    ContentResult(Content = s) :> ActionResult

let redirect url =
    RedirectResult(url) :> ActionResult