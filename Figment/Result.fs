module Figment.Result

open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Figment.Helpers
open Figment.Extensions

let inline result r = 
    {new ActionResult() with
        override x.ExecuteResult ctx = 
            if ctx = null
                then raise <| System.ArgumentNullException("ctx")
                else r ctx }

let inline exec ctx (r: ActionResult) =
    r.ExecuteResult ctx

let concat a b =
    let c ctx = 
        exec ctx a
        exec ctx b
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
    result (fun ctx -> ctx.Response.StatusCode <- code)

let contentType t =
    result (fun ctx -> ctx.Response.ContentType <- t)

let charset c =
    result (fun ctx -> ctx.Response.Charset <- c)

let header name value =
    result (fun ctx -> ctx.Response.AppendHeader(name, value))

let vary = header "Vary"

let allow (methods: #seq<string>) = header "Allow" (System.String.Join(", ", methods))

let file contentType stream =
    FileStreamResult(stream, contentType) :> ActionResult

let json data =
    JsonResult(Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet) :> ActionResult

let jsonp callback data =
    result (fun ctx ->
                let cb : string = callback ctx
                ctx.Response.Write(cb)
                ctx.Response.Write("(")
                json data |> exec ctx |> ignore
                ctx.Response.Write(")"))
    >>. contentType "application/javascript"

let xml data = 
    // charset?
    contentType "text/xml" >>.
    result (fun ctx ->
                let serializer = System.Xml.Serialization.XmlSerializer(data.GetType())
                serializer.Serialize(ctx.HttpContext.Response.Output, data))