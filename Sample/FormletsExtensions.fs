namespace Figment

[<AutoOpen>]
module FormletsExtensions =
    open System.Xml.Linq
    open System.Web.Mvc
    open WingBeats.Xml
    open Figment.Routing
    open Formlets


    type 'a FormActionParameters = {
        Formlet: ControllerContext -> 'a Formlet
        Page: XNode list -> ControllerContext -> Node list
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
                p.Page xml ctx |> Result.wbview)
        post url
            (fun ctx -> 
                let env = EnvDict.fromFormAndFiles ctx.HttpContext.Request
                match run (p.Formlet ctx) env with
                | Success v -> p.Success ctx v
                | Failure(errorForm, _) -> p.Page errorForm ctx |> Result.wbview)

