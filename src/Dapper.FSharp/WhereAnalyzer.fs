﻿module internal Dapper.FSharp.WhereAnalyzer

type FieldWhereMetadata = {
    Key : string * ColumnComparison
    Name : string
    ParameterName : string
}

let extractWhereParams (meta:FieldWhereMetadata list) =
    let fn (m:FieldWhereMetadata) =
        match m.Key |> snd with
        | Eq p | Ne p | Gt p
        | Lt p | Ge p | Le p -> (m.ParameterName, p) |> Some
        | In p | NotIn p -> (m.ParameterName, p :> obj) |> Some
        | Like str -> (m.ParameterName, str :> obj) |> Some
        | IsNull | IsNotNull -> None
    meta
    |> List.choose fn

let normalizeParamName (s:string) =
    s.Replace(".","_")

let rec getWhereMetadata (meta:FieldWhereMetadata list) (w:Where)  =
    match w with
    | Empty -> meta
    | Column (field, comp) ->

        let parName =
            meta
            |> List.filter (fun x -> System.String.Equals(x.Name, field, System.StringComparison.OrdinalIgnoreCase))
            |> List.length
            |> fun l -> sprintf "Where_%s%i" field (l + 1)
            |> normalizeParamName

        meta @ [{ Key = (field, comp); Name = field; ParameterName = parName }]
    | Binary(w1, _, w2) -> [w1;w2] |> List.fold getWhereMetadata meta
    | Unary(_, w) -> w |> getWhereMetadata meta