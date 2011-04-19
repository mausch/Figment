namespace Figment

open WingBeats.Xml
open System.Web.Mvc

module Result =
    let wbview (n: Node list) =
        {new ActionResult() with
            override x.ExecuteResult ctx =
                Renderer.Render(n, ctx.HttpContext.Response.Output) }

    open Formlets

    let formlet (f: _ Formlet) = Result.content (render f)

module Actions =
    let wbview (n: Node list) (ctx: ControllerContext) =
        Result.wbview n

[<AutoOpen>]
module XhtmlElementExtensions = 
    open WingBeats.Xhtml

    let internal e = XhtmlElement()

    type XhtmlElement with
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

    type Shortcuts.XhtmlShortcut with
        member x.Link href text = e.A ["href",href] [&text]

[<AutoOpen>]
module FormletsExtensions =
    open System.Xml.Linq
    open System.Web.Mvc
    open WingBeats.Xml
    open Figment.Routing
    open Formlets

    let runPost formlet (ctx: ControllerContext) =
        let env = EnvDict.fromFormAndFiles ctx.HttpContext.Request
        run formlet env

    type 'a FormActionParameters = {
        Formlet: ControllerContext -> 'a Formlet
        Page: ControllerContext -> XNode list -> Node list
        Success: ControllerContext -> 'a -> ActionResult
    }

    /// <summary>
    /// Maps a page with a formlet and its handler
    /// </summary>
    /// <param name="url">URL</param>
    /// <param name="formlet">Formlet to show and process</param>
    /// <param name="page">Function taking an URL and rendered formlet and returning a wingbeats tree</param>
    /// <param name="successHandler">When formlet is successful, run this function</param>
    let formAction url (p: _ FormActionParameters) =
        get url 
            (fun ctx -> 
                let xml = p.Formlet ctx |> renderToXml
                p.Page ctx xml |> Result.wbview)
        post url
            (fun ctx -> 
                match runPost (p.Formlet ctx) ctx with
                | Success v -> p.Success ctx v
                | Failure(errorForm, _) -> p.Page ctx errorForm |> Result.wbview)

    type FormletAction<'a,'b> = ControllerContext -> 'a -> 'b Formlet

    let formletActionToFAction (a: FormletAction<_,_>) (f: _ Formlet) : Helpers.FAction =
        fun ctx ->
            match runPost f ctx with
            | Success v -> a ctx v |> Result.formlet
            | _ -> failwith "bla"

