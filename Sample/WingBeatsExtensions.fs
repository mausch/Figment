namespace Figment

open WingBeats.Xml
open System.Web.Mvc

module Result =
    let wbview (n: Node list) =
        {new ActionResult() with
            override x.ExecuteResult ctx =
                Renderer.Render(n, ctx.HttpContext.Response.Output) }

module Actions =
    let wbview (n: Node list) (ctx: ControllerContext) =
        Result.wbview n

[<AutoOpen>]
module XhtmlElementExtensions = 
    type WingBeats.Xhtml.XhtmlElement with
        member x.DocTypeTransitional = 
            DocType({ name   = "html"
                      pubid  = "-//W3C//DTD XHTML 1.0 Transitional//EN"
                      sysid  = "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
                      subset = null })

        member x.DocTypeHTML5 =
            DocType({ name   = "html"
                      pubid  = null
                      sysid  = null
                      subset = null })
    //<!DOCTYPE html>