module ResultTests

open System
open System.Text
open System.Web
open System.Web.Routing
open System.Web.Mvc
open Figment.Result
open Xunit

let buildCtx = 
    let dummyController = { new ControllerBase() with member x.ExecuteCore() = () }
    fun ctx ->
        let req = RequestContext(ctx, RouteData())
        ControllerContext(req, dummyController)

let buildCtxFromResponse resp = 
    buildCtx 
        { new HttpContextBase() with
            member x.Response = resp }

[<Fact>]
let ``status result``() =
    let ctx = 
        let statusCode = ref 0
        buildCtxFromResponse
            { new HttpResponseBase() with
                member x.StatusCode 
                    with get() = !statusCode
                    and set v = statusCode := v }
    status 200 ctx
    Assert.Equal(200, ctx.HttpContext.Response.StatusCode)

[<Fact>]
let ``JSONP content type is application/javascript``() =
    //let callback (ctx: ControllerContext) = ctx.Params.["jsoncallback"]
    let callback (ctx: ControllerContext) = "callback"
    let sb = StringBuilder()
    let ctx = 
        let contentType = ref ""
        buildCtxFromResponse 
            { new HttpResponseBase() with
                member x.ContentType
                    with get() = !contentType
                    and set v = contentType := v
                member x.Write(s: string) = 
                    sb.Append s |> ignore }
    jsonp callback "something" ctx
    Assert.Equal("callback(\"something\")", sb.ToString())
    Assert.Equal("application/javascript", ctx.HttpContext.Response.ContentType)