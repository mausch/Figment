module Extensions

open System.Text.RegularExpressions

type CaptureCollection with
    member this.Captures 
        with get() =
            this |> Seq.cast<Capture> |> Seq.toArray

type GroupCollection with
    member this.Groups
        with get() =
            this |> Seq.cast<Group> |> Seq.toArray