namespace Figment

type ReaderBuilder() =
    member x.Bind(m, f) = fun c -> f (m c) c
    member x.Return a = fun _ -> a
    member x.ReturnFrom a = a
