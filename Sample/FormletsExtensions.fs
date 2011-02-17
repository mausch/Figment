namespace Figment

[<AutoOpen>]
module FormletsExtensions =
    open Figment.Routing
    open Formlets

    /// <summary>
    /// Maps a page with a formlet and its handler
    /// </summary>
    /// <param name="url">URL</param>
    /// <param name="formlet">Formlet to show and process</param>
    /// <param name="page">Function taking an URL and rendered formlet and returning a wingbeats tree</param>
    /// <param name="successHandler">When formlet is successful, run this function</param>
    let formAction url formlet page successHandler =
        get url 
            (fun ctx -> 
                let xml = formlet ctx |> renderToXml
                page url xml ctx |> Result.wbview)
        post url
            (fun ctx -> 
                let env = EnvDict.fromFormAndFiles ctx.HttpContext.Request
                match run (formlet ctx) env with
                | Success v -> successHandler ctx v
                | Failure(errorForm, _) -> page url errorForm ctx |> Result.wbview)

