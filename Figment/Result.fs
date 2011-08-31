namespace Figment

[<AutoOpen>]
module Result =

    open System.Linq
    open System.Web
    open System.Web.Mvc
    open System.Web.Routing
    open Figment.Helpers
    open Figment.Extensions

    let result = ReaderBuilder()

    let inline bind f m = result.Bind(m,f)
    let inline combine a b = result.Bind(a, fun _ -> b)
    let inline (>>=) m f = bind f m
    let (>>.) = combine
    let inline map f m = result.Bind(m, fun a -> result.Return (f a))

    let empty : FAction = EmptyResult() |> fromActionResult

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

    let unauthorized : FAction = HttpUnauthorizedResult() |> fromActionResult

    let status code : FAction =
        fun ctx -> ctx.Response.StatusCode <- code

    let contentType t : FAction =
        fun ctx -> ctx.Response.ContentType <- t

    let charset c : FAction  =
        fun ctx -> ctx.Response.Charset <- c

    let header name value : FAction  =
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

    let write (text: string) : FAction =
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

    let getQueryString (key: string) (ctx: ControllerContext) : string option = 
        ctx.Request.QueryString.[key] |> Option.fromNull

    let xml data = 
        // charset?
        contentType "text/xml" >>.
        (fun ctx ->
            let serializer = System.Xml.Serialization.XmlSerializer(data.GetType())
            serializer.Serialize(ctx.HttpContext.Response.Output, data))