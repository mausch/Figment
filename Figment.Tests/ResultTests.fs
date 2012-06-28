module ResultTests

open System
open System.Text
open System.Web
open System.Web.Routing
open System.Web.Mvc
open Figment.Result
open Figment.Testing
open Xunit

[<Fact>]
let ``status result``() =
    let ctx = 
        let statusCode = ref 0
        buildResponse
            { new HttpResponseBase() with
                member x.StatusCode 
                    with get() = !statusCode
                    and set v = statusCode := v }
    let ctx = buildCtx ctx
    status 200 ctx
    Assert.Equal(200, ctx.HttpContext.Response.StatusCode)

[<Fact>]
let ``JSONP content type is application/javascript``() =
    //let callback (ctx: ControllerContext) = ctx.Params.["jsoncallback"]
    let callback (ctx: ControllerContext) = "callback"
    let sb = StringBuilder()
    let ctx = 
        let contentType = ref ""
        buildResponse
            { new HttpResponseBase() with
                member x.ContentType
                    with get() = !contentType
                    and set v = contentType := v
                member x.Write(s: string) = 
                    sb.Append s |> ignore }
    let ctx = buildCtx ctx
    jsonp callback "something" ctx
    Assert.Equal<string>("callback(\"something\")", sb.ToString())
    Assert.Equal<string>("application/javascript", ctx.HttpContext.Response.ContentType)