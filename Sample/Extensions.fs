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
    open Figment.Extensions
    open Formlets
    open Figment.RoutingConstraints

    let runPost formlet (ctx: ControllerContext) =
        let env = EnvDict.fromFormAndFiles ctx.Request
        run formlet env

    let runGet formlet (ctx: ControllerContext) =
        let env = EnvDict.fromNV ctx.QueryString
        run formlet env

    let runParams formlet (ctx: ControllerContext) =
        let env = EnvDict.fromNV ctx.Request.Params
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

    /// <summary>
    /// 'a : state
    /// 'b : form result
    /// 'c : new state
    /// 'd : formlet type
    /// </summary>
    type FormletAction<'a,'b,'c,'d> = ControllerContext -> 'a -> 'b -> ('c * 'd Formlet)

    let internal stateField = "_state"
    let internal getState (ctx: ControllerContext) =
        ctx.Request.Params.[stateField] |> losSerializer.Deserialize |> unbox
    let internal setState v (f: _ Formlet) =
        let v = losSerializer.Serialize v
        assignedHidden stateField v *> f
    let internal aform nexturl formlet = form "post" nexturl [] formlet

    let formletToAction nextUrl (f: _ Formlet) (a: FormletAction<_,_,_,_>) : Helpers.FAction =
        fun ctx ->
            let s = getState ctx
            match runParams f ctx with
            | Success v -> 
                let newState, formlet = a ctx s v
                let formlet = setState newState formlet
                let formlet = aform nextUrl formlet
                Result.formlet formlet
            | _ -> failwith "bla"

    let actionFormlet thisFormlet a url i =
        let s i = if i = 0 then "" else i.ToString()
        let thisUrl = url + s i
        let i = i+1
        let nextUrl = url + s i
        action (ifPathIs thisUrl) (formletToAction nextUrl thisFormlet a)
        url,i