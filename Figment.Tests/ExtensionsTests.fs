module ExtensionsTests

open System
open Xunit
open Microsoft.FSharp.Reflection
open Figment.Extensions

let assertThrows<'e when 'e :> exn> f = 
    Assert.Throws<'e>(Assert.ThrowsDelegate(f)) |> ignore

[<Fact>]
let InvokeFunction() =
    let f a b c = a + b + c
    let r = FSharpValue.InvokeFunction f [1;2;3]
    Assert.NotNull r
    let r = Assert.IsType<int> r
    Assert.Equal(6, r)

[<Fact>]
let GetFlattenedFunctionElements_non_function() =
    let a = 2
    assertThrows<ArgumentException>(fun () -> FSharpType.GetFlattenedFunctionElements (a.GetType()) |> ignore)

[<Fact>]
let GetFlattenedFunctionElements_unit_int() =
    let f() = 2
    let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
    Assert.Equal(2, t.Length)
    Assert.Equal(typeof<unit>, t.[0])
    Assert.Equal(typeof<int>, t.[1])

[<Fact>]
let GetFlattenedFunctionElements_int_float_string() =
    let f (i: int) (j: float) = "bla"
    let t = FSharpType.GetFlattenedFunctionElements(f.GetType())
    Assert.Equal(3, t.Length)
    Assert.Equal(typeof<int>, t.[0])
    Assert.Equal(typeof<float>, t.[1])
    Assert.Equal(typeof<string>, t.[2])

open System.Collections.Specialized
open System.Linq
    
[<Fact>]
let ``NameValueCollection as ILookup``() =
    let nv = NameValueCollection()
    nv.Add("1", "one")
    nv.Add("1", "uno")
    let l = nv.AsLookup()
    Assert.True(l.Contains "1")
    Assert.False(l.Contains "2")
    Assert.True(Seq.isEmpty l.["2"])
    Assert.Equal(2, Enumerable.Count l.["1"])
    ()

[<Fact>]
let ``NameValueCollection as IDictionary``() =
    let nv = NameValueCollection()
    nv.Add("1", "one")
    nv.Add("1", "uno")
    let l = nv.AsDictionary()
    Assert.True(l.ContainsKey "1")
    Assert.False(l.ContainsKey "2")
    match l.TryGetValue "2" with
    | true, _ -> failwith "key should not have been found"
    | _ -> ()
    Assert.Equal(2, l.["1"].Length)
    match l.TryGetValue "1" with
    | false, _ -> failwith "key should have been found"
    | _, [|"one";"uno"|] -> ()
    | _ -> failwith "values not matched"
    l.["2"] <- [|"dos";"two"|]
    match l.TryGetValue "2" with
    | false, _ -> failwith "key should have been found"
    | _, [|"dos";"two"|] -> ()
    | _ -> failwith "values not matched"

