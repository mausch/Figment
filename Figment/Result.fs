module Figment.Result

open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Routing

let concat (a: ActionResult) (b: ActionResult) =
    {new ActionResult() with
        override x.ExecuteResult ctx =
            a.ExecuteResult ctx
            b.ExecuteResult ctx }    

let (>>.) = concat

let empty = EmptyResult() :> ActionResult

let view viewName model = 
    let viewData = ViewDataDictionary(Model = model)
    ViewResult(ViewData = viewData, ViewName = viewName) :> ActionResult

let notFound () = raise <| HttpException(404, "Not found")

let notFoundOrView viewName =    
    function
    | None -> notFound()
    | Some x -> view viewName x

let content s = 
    ContentResult(Content = s) :> ActionResult

let redirect url =
    RedirectResult(url) :> ActionResult

let redirectToRoute (routeValues: RouteValueDictionary) =
    RedirectToRouteResult(routeValues) :> ActionResult

/// doesn't work, can't compare IRouteHandlers
let redirectToAction action =    
    let routes = Enumerable.OfType<Route> RouteTable.Routes
    //let routes = RouteTable.Routes |> Enumerable.OfType
    let handler = FigmentRouteHandler(action) :> IRouteHandler
    let route = routes |> Seq.find (fun r -> r.RouteHandler = handler)
    redirect route.Url

let unauthorized = HttpUnauthorizedResult() :> ActionResult

let status code =
    {new ActionResult() with
        override x.ExecuteResult ctx =
            ctx.HttpContext.Response.StatusCode <- code }

let file contentType stream =
    FileStreamResult(stream, contentType) :> ActionResult

let json data =
    JsonResult(Data = data) :> ActionResult