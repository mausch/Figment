namespace Figment

open WingBeats.Xml
open System.Web.Mvc

module Result =
    let wbview (n: Node) =
        Result.content (Renderer.RenderToString n)

module Actions =
    let wbview (n: Node) (ctx: ControllerContext) =
        Result.wbview n
