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
    open System
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

    let internal stateMgr =
        let stateField = "_state"
        let serializer = binSerializer
        let getState (ctx: ControllerContext) =
            ctx.Request.Params.[stateField] |> serializer.Deserialize |> unbox
        let setState v (f: _ Formlet) =
            let v = serializer.Serialize v
            assignedHidden stateField v *> f
        getState,setState
    let internal getState x = fst stateMgr x
    let internal setState x = snd stateMgr x
    let internal copyState (ctx: ControllerContext) =
        getState ctx |> setState
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

    let actionFormlet thisFormlet a (url, i) =
        let s i = if i = 0 then "" else i.ToString()
        let thisUrl = url + s i
        let i = i+1
        let nextUrl = url + s i
        action (ifPathIs thisUrl) (formletToAction nextUrl thisFormlet a)
        url,i

    type Web<'a> = ControllerContext -> 'a Formlet


    open System.Reflection

    type WebBuilder() =
        let makeCont (f: 'a -> Web<'b>) (formlet: 'a Formlet) : Web<'b> =
            fun ctx ->
                match runParams formlet ctx with
                | Success v -> f v ctx
                | _ -> failwith "booooo"

        let aform2 formlet = form "post" "" [] formlet
        
        member x.Bind(a: Web<'a>, f: 'a -> Web<'b>): Web<'a> = 
            fun (ctx: ControllerContext) ->
                let formlet = a ctx
                let cont = box (makeCont f formlet), box typeof<'b>
                formlet |> setState cont |> aform2

        member x.Return a : Web<_> = 
            fun ctx ->
                failwith "return"

        member x.ReturnFrom a = a

        member x.ShowFormlet (formlet: 'a Formlet) : Web<_> =
            fun ctx -> formlet

        member x.ToAction (w: Web<_>) : Helpers.FAction =
            fun ctx ->            
                let cont = getState ctx
                match cont with
                | null -> 
                    w ctx |> Result.formlet
                | _ -> 
                    let t: Type = cont.GetType().GetProperty("Item2").GetValue(cont, null) |> unbox
                    let c = cont.GetType().GetProperty("Item1").GetValue(cont, null)
                    let formlet = c.GetType().GetMethod("Invoke").Invoke(c, [|ctx|])
                    let figmentResultType = Type.GetType("Figment.Result, Sample")
                    let rftm = figmentResultType.GetMethod("formlet", BindingFlags.Static ||| BindingFlags.Public)
                    let rftmg = rftm.GetGenericMethodDefinition()
                    let rf = rftmg.MakeGenericMethod([|t|])                
                    let r = rf.Invoke(null, [|formlet|])
                    unbox r
