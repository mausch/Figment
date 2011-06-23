namespace Figment

type ReaderBuilder() =
    member x.Bind(m: 'a -> 'b, f: 'b -> 'a -> 'c) =
        fun c ->
            let x = m c
            f x c
    member x.Return a = fun c -> a
    member x.ReturnFrom a = a

module ReaderOperators =
    let internal r = ReaderBuilder()
    let (>>>) m f = r.Bind(m, fun _ -> f)