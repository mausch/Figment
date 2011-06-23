module Figment.Result

open System.Linq
open System.Web
open System.Web.Mvc
open System.Web.Routing
open Figment.Helpers
open Figment.Extensions

let inline toActionResult r = 
    {new ActionResult() with
        override x.ExecuteResult ctx = 
            if ctx = null
                then raise <| System.ArgumentNullException("ctx")
                else r ctx }


let inline exec ctx (r: ActionResult) =
    r.ExecuteResult ctx

let inline fromActionResult a : FResult =
    fun ctx -> exec ctx a

let result = ReaderBuilder()

let empty : FResult = EmptyResult() |> fromActionResult

let view viewName model = 
    let viewData = ViewDataDictionary(Model = model)
    ViewResult(ViewData = viewData, ViewName = viewName) |> fromActionResult

let notFound () = raise <| HttpException(404, "Not found")

let notFoundOrView viewName =    
    function
    | None -> notFound()
    | Some x -> view viewName x

let content s = 
    ContentResult(Content = s) |> fromActionResult

let htmlcontent x = x |> htmlencode |> content
    
let contentf f = Printf.kprintf content f

let htmlcontentf f = Printf.kprintf htmlcontent f

let redirect url =
    RedirectResult(url) |> fromActionResult

let redirectf f = Printf.kprintf redirect f

let redirectToRoute (routeValues: RouteValueDictionary) =
    RedirectToRouteResult(routeValues) |> fromActionResult

let unauthorized : FResult = HttpUnauthorizedResult() |> fromActionResult

let status code : FResult =
    fun ctx -> ctx.Response.StatusCode <- code

let contentType t : FResult  =
    fun ctx -> ctx.Response.ContentType <- t

let charset c : FResult =
    fun ctx -> ctx.Response.Charset <- c

let header name value : FResult =
    fun ctx -> ctx.Response.AppendHeader(name, value)

let vary x = header "Vary" x

let allow (methods: #seq<string>) = header "Allow" (System.String.Join(", ", methods))

let fileStream contentType name stream =
    FileStreamResult(stream, contentType, FileDownloadName = name) |> fromActionResult

let filePath contentType path =
    FilePathResult(path, contentType) |> fromActionResult

let fileContent contentType name bytes =
    FileContentResult(bytes, contentType, FileDownloadName = name) |> fromActionResult

let json data =
    JsonResult(Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet) |> fromActionResult

let write (text: string) : FResult =
    fun ctx ->
        ctx.Response.Write text

let writefn fmt = Printf.kprintf write fmt

let jsonp callback data =
    result {
        let! cb = callback
        do! write cb
        do! write "("
        do! json data
        do! write ")"
        do! contentType "application/javascript"
    }

open Figment.ReaderOperators

let xml data = 
    // charset?
    contentType "text/xml" >>.
    (fun ctx ->
        let serializer = System.Xml.Serialization.XmlSerializer(data.GetType())
        serializer.Serialize(ctx.HttpContext.Response.Output, data))