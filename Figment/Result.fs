namespace Figment

[<AutoOpen>]
module Result =

    open System
    open System.Linq
    open System.Web
    open System.Web.UI
    open System.Web.Mvc
    open System.Web.Routing
    open Figment.Helpers
    open Figment.Extensions
    open FSharpx
    open FSharpx.Reader

    let result = ReaderBuilder()

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

    let getQueryString (key: string) (ctx: ControllerContext): string option = 
        match ctx.Request.QueryString.[key] with
        | null -> None
        | a -> Some a

    let getQueryStringOrFail (key: string) (ctx: ControllerContext) = 
        getQueryString key ctx
        |> Option.getOrElseF (fun () -> failwithf "Missing required querystring key '%s'" key)

    let setInContext k v (ctx: ControllerContext) =
        ctx.[k] <- v

    let getFromContext k (ctx: ControllerContext) =
        unbox ctx.[k]

    let xml data = 
        // charset?
        contentType "text/xml" >>.
        (fun ctx ->
            let serializer = System.Xml.Serialization.XmlSerializer(data.GetType())
            serializer.Serialize(ctx.HttpContext.Response.Output, data))

    let noCache (ctx: ControllerContext) =
        let cache = ctx.Response.Cache
        cache.SetExpires(DateTime.UtcNow.AddDays(-1.0))
        cache.SetValidUntilExpires(false)
        cache.SetRevalidation(HttpCacheRevalidation.AllCaches)
        cache.SetCacheability(HttpCacheability.NoCache)
        cache.SetNoStore()

    let setCache (settings: OutputCacheParameters) (ctx: ControllerContext) =
        let cachePolicy = ctx.HttpContext.Response.Cache
        cachePolicy.SetExpires(ctx.HttpContext.Timestamp.AddSeconds(float settings.Duration))
        // TODO set the other cache parameters

    let setHttpCookie (c: HttpCookie) (ctx: ControllerContext) =
        ctx.Response.SetCookie c
        
    let getHttpCookie (name: string) (ctx: ControllerContext) =
        match ctx.Request.Cookies.[name] with
        | null -> None
        | a -> Some a

    let removeHttpCookie (name: string) (ctx: ControllerContext) =
        match getHttpCookie name ctx with
        | None -> ()
        | Some cookie ->
            let expiration = 
                if cookie.Expires = DateTime.MinValue
                    then cookie.Expires
                    else cookie.Expires.AddYears(-1)
            let expiredCookie = HttpCookie(name = cookie.Name, 
                                       Domain = cookie.Domain, 
                                       Expires = expiration,
                                       HttpOnly = cookie.HttpOnly,
                                       Path = cookie.Path,
                                       Secure = cookie.Secure)
            setHttpCookie expiredCookie ctx
                 