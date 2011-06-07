module ResultTests

open System
open System.Text
open System.Web
open System.Web.Routing
open System.Web.Mvc
open Figment.Result
open Xunit

[<Fact>]
let ``JSONP content type is application/javascript``() =
    //let callback (ctx: ControllerContext) = ctx.Params.["jsoncallback"]
    let callback (ctx: ControllerContext) = "callback"
    let sb = StringBuilder()
    let ctx = 
        let ctx = { new HttpContextBase() with 
                        member x.Response = 
                            { new HttpResponseBase() with
                                member x.Write(s: string) = 
                                    sb.Append s |> ignore } }
        let req = RequestContext(ctx, RouteData())
        let controller = { new ControllerBase() with member x.ExecuteCore() = () }
        ControllerContext(req, controller)
    jsonp callback "something" |> exec ctx
    printfn "%s" (sb.ToString())
    ()