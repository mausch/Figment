module ResultTests

open System
open System.Text
open System.Web
open System.Web.Routing
open System.Web.Mvc
open Figment.Result
open Xunit

[<Fact>]
let ``status result``() =
    let ctx = 
        let statusCode = ref 0
        let ctx = { new HttpContextBase() with 
                        member x.Response = 
                            { new HttpResponseBase() with
                                member x.StatusCode 
                                    with get() = !statusCode
                                    and set v = statusCode := v } }
        let req = RequestContext(ctx, RouteData())
        let controller = { new ControllerBase() with member x.ExecuteCore() = () }
        ControllerContext(req, controller)
    status 200 |> exec ctx
    Assert.Equal(200, ctx.HttpContext.Response.StatusCode)

[<Fact>]
let ``JSONP content type is application/javascript``() =
    //let callback (ctx: ControllerContext) = ctx.Params.["jsoncallback"]
    let callback (ctx: ControllerContext) = "callback"
    let sb = StringBuilder()
    let ctx = 
        let contentType = ref ""
        let ctx = { new HttpContextBase() with 
                        member x.Response = 
                            { new HttpResponseBase() with
                                member x.ContentType
                                    with get() = !contentType
                                    and set v = contentType := v
                                member x.Write(s: string) = 
                                    sb.Append s |> ignore } }
        let req = RequestContext(ctx, RouteData())
        let controller = { new ControllerBase() with member x.ExecuteCore() = () }
        ControllerContext(req, controller)
    jsonp callback "something" |> exec ctx
    Assert.Equal("callback(\"something\")", sb.ToString())
    Assert.Equal("application/javascript", ctx.HttpContext.Response.ContentType)