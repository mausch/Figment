namespace Figment

type ReaderBuilder() =
    member x.Bind(m, f) = fun c -> f (m c) c
    member x.Return a = fun _ -> a
    member x.ReturnFrom a = a
    member x.map f m = x.Bind(m, fun a -> x.Return (f a))

[<AutoOpen>]
module ReaderOperators =
    let internal r = ReaderBuilder()
    let (>>.) m f = r.Bind(m, fun _ -> f)
    let (>>=) m f = r.Bind(m,f)