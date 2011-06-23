namespace Figment

type ReaderBuilder() =
    member x.Bind(m: 'a -> 'b, f: 'b -> 'a -> 'c) =
        fun c ->
            let x = m c
            f x c
    member x.Return a = fun c -> a
    member x.ReturnFrom a = a
    member x.map f m = x.Bind(m, fun a -> x.Return (f a))

module ReaderOperators =
    let internal r = ReaderBuilder()
    let (>>.) m f = r.Bind(m, fun _ -> f)
    let (>>=) m f = r.Bind(m,f)