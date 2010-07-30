namespace Figment

open WingBeats.Xml
open System.Web.Mvc

module Result =
    let wbview (n: Node) =
        {new ActionResult() with
            override x.ExecuteResult ctx =
                Renderer.Render(n, ctx.HttpContext.Response.Output) }

module Actions =
    let wbview (n: Node) (ctx: ControllerContext) =
        Result.wbview n
