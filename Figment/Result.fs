module Figment.Result

open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Figment.Helpers

let inline result r = 
    {new ActionResult() with
        override x.ExecuteResult ctx = r ctx }

let inline exec (r: ActionResult) ctx =
    r.ExecuteResult ctx

let concat a b =
    let c ctx = 
        exec a ctx
        exec b ctx
    result c

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

let htmlcontent = htmlencode >> content
    
let contentf f = Printf.kprintf content f

let htmlcontentf f = Printf.kprintf htmlcontent f

let redirect url =
    RedirectResult(url) :> ActionResult

let redirectf f = Printf.kprintf redirect f

let redirectToRoute (routeValues: RouteValueDictionary) =
    RedirectToRouteResult(routeValues) :> ActionResult

let unauthorized = HttpUnauthorizedResult() :> ActionResult

let status code =
    result (fun ctx -> ctx.HttpContext.Response.StatusCode <- code)

let contentType t =
    result (fun ctx -> ctx.HttpContext.Response.ContentType <- t)

let charset c =
    result (fun ctx -> ctx.HttpContext.Response.Charset <- c)

let file contentType stream =
    FileStreamResult(stream, contentType) :> ActionResult

let json data =
    JsonResult(Data = data) :> ActionResult

let xml data = 
    // charset?
    contentType "text/xml" >>.
    if data = null
        then content ""
        else
            result (fun ctx ->
                        let serializer = System.Xml.Serialization.XmlSerializer(data.GetType())
                        serializer.Serialize(ctx.HttpContext.Response.Output, data))