namespace CWTools.Validation.Stellaris

open CWTools.Parser.ConfigParser
open CWTools.Process
open CWTools.Utilities.Utils
open CWTools.Validation.ValidationCore
open CWTools.Common.STLConstants
open CWTools.Utilities.Position
open CWTools.Games
open CWTools.Validation.Stellaris.STLValidation
open FParsec
open CWTools.Parser.Types
open CWTools.Utilities
open CWTools.Process.STLScopes
open CWTools.Common
open CWTools.Validation.Stellaris.STLLocalisationValidation
open CWTools.Validation.Stellaris.ScopeValidation
open Microsoft.FSharp.Collections.Tagged
open System.IO
open FSharp.Data.Runtime
open QuickGraph
open System
open FSharp.Collections.ParallelSeq
open System
open CWTools.Process.Scopes
open CWTools.Validation.Rules
open CWTools.Validation


module rec STLRules =
    type NewField = NewField<Scope>
    type NewRule = NewRule<Scope>
    type Options = Options<Scope>
    type TypeDefinition = TypeDefinition<Scope>
    type RootRule = RootRule<Scope>
    type SubTypeDefinition = SubTypeDefinition<Scope>
    let scopes = (scopedEffects |> List.map (fun se -> se.Name)) @ (oneToOneScopes |> List.map fst)



    // let inline checkLocalisationField< ^a when ^a : (member Position : range) and ^a : (member Key : string)> (keys : (Lang * Collections.Set<string>) list) (synced : bool) (key : string) (leafornode : ^a) =
    let inline checkLocalisationField(keys : (Lang * Collections.Set<string>) list) (synced : bool) (key : string) (leafornode : ^a) =
        match synced with
        |true ->
            let defaultKeys = keys |> List.choose (fun (l, ks) -> if l = STL STLLang.Default then Some ks else None) |> List.tryHead |> Option.defaultValue Set.empty
            //let key = leaf.Value |> (function |QString s -> s |s -> s.ToString())
            checkLocName leafornode defaultKeys (STL STLLang.Default) key
        |false ->
            checkLocKeysLeafOrNode keys key leafornode

//(Lang * Set<string> list -> bool -> string -> ^a -> ValidationResult)

    // let inline ruleApplicatorCreator(rootRules : RootRule list, typedefs : TypeDefinition list , types : Collections.Map<string, StringSet>, enums : Collections.Map<string, StringSet>, localisation : (Lang * Collections.Set<string>) list, files : Collections.Set<string>, triggers : Map<string,Effect,InsensitiveStringComparer>, effects : Map<string,Effect,InsensitiveStringComparer>, anyScope, checkLocField : (Lang * Set<string> list -> bool -> string -> ^a -> ValidationResult)) =
    //     let triggerMap = triggers //|> List.map (fun e -> e.Name, e) |> (fun l -> EffectMap.FromList(InsensitiveStringComparer(), l))
    //     let effectMap = effects //|> List.map (fun e -> e.Name, e) |> (fun l -> EffectMap.FromList(InsensitiveStringComparer(), l))


    //     let aliases =
    //         rootRules |> List.choose (function |AliasRule (a, rs) -> Some (a, rs) |_ -> None)
    //                     |> List.groupBy fst
    //                     |> List.map (fun (k, vs) -> k, vs |> List.map snd)
    //                     |> Collections.Map.ofList
    //     let typeRules =
    //         rootRules |> List.choose (function |TypeRule (k, rs) -> Some (k, rs) |_ -> None)
    //     let typesMap = types //|> Map.toSeq |> PSeq.map (fun (k, s) -> k, StringSet.Create(InsensitiveStringComparer(), (s |> List.map fst))) |> Map.ofSeq
    //     let enumsMap = enums //|> Map.toSeq |> PSeq.map (fun (k, s) -> k, StringSet.Create(InsensitiveStringComparer(), s)) |> Map.ofSeq


    //     let isValidValue (value : Value) =
    //         let key = value.ToString().Trim([|'"'|])
    //         function
    //         |ValueType.Bool ->
    //             key = "yes" || key = "no"
    //         |ValueType.Enum e ->
    //             match enumsMap.TryFind e with
    //             |Some es -> es.Contains key
    //             |None -> true
    //         |ValueType.Float (min, max)->
    //             match value with
    //             |Float f -> true
    //             |Int _ -> true
    //             |_ -> false
    //         |ValueType.Specific s -> key = s
    //         |ValueType.Percent -> key.EndsWith("%")
    //         |_ -> true


    //     let rec applyClauseField (enforceCardinality : bool) (nodeSeverity : Severity option) (ctx : RuleContext<Scope>) (rules : NewRule list) (startNode : Node) =
    //         let severity = nodeSeverity |> Option.defaultValue (if ctx.warningOnly then Severity.Warning else Severity.Error)
    //         let subtypedrules =
    //             rules |> List.collect (fun (r,o) -> r |> (function |SubtypeRule (key, shouldMatch, cfs) -> (if (not shouldMatch) <> List.contains key ctx.subtypes then cfs else []) | x -> [(r, o)]))
    //         let expandedrules =
    //             subtypedrules |> List.collect (
    //                 function
    //                 | (LeafRule((AliasField a),_), _) -> (aliases.TryFind a |> Option.defaultValue [])
    //                 | (NodeRule((AliasField a),_), _) -> (aliases.TryFind a |> Option.defaultValue [])
    //                 |x -> [x])
    //         let valueFun (leaf : Leaf) =
    //             match expandedrules |> List.choose (function |(LeafRule (l, r), o) when checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l leaf.Key leaf -> Some (l, r, o) |_ -> None) with
    //             |[] ->
    //                 if enforceCardinality && ((leaf.Key |> Seq.tryHead |> Option.map ((=) '@') |> Option.defaultValue false) |> not) then Invalid [inv (ErrorCodes.ConfigRulesUnexpectedProperty (sprintf "Unexpected node %s in %s" leaf.Key startNode.Key) severity) leaf] else OK
    //             |rs -> rs <&??&> (fun (l, r, o) -> applyLeafRule ctx o r leaf) |> mergeValidationErrors "CW240"
    //         let nodeFun (node : Node) =
    //             match expandedrules |> List.choose (function |(NodeRule (l, rs), o) when checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l node.Key node -> Some (l, rs, o) |_ -> None) with
    //             | [] ->
    //                 if enforceCardinality then Invalid [inv (ErrorCodes.ConfigRulesUnexpectedProperty (sprintf "Unexpected node %s in %s" node.Key startNode.Key) severity) node] else OK
    //                 //|rs -> rs <&??&> (fun (_, o, f) -> applyNodeRule root enforceCardinality ctx o f node)
    //             | matches -> matches <&??&> (fun (l, rs, o) -> applyNodeRule enforceCardinality ctx o l rs node)
    //         let leafValueFun (leafvalue : LeafValue) =
    //             match expandedrules |> List.choose (function |(LeafValueRule (l), o) when checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l leafvalue.Key leafvalue -> Some (l, o) |_ -> None) with
    //             | [] ->
    //                 if enforceCardinality then Invalid [inv (ErrorCodes.ConfigRulesUnexpectedProperty (sprintf "Unexpected node %s in %s" leafvalue.Key startNode.Key) severity) leafvalue] else OK
    //             |rs -> rs <&??&> (fun (l, o) -> applyLeafValueRule ctx o l leafvalue) |> mergeValidationErrors "CW240"
    //         let checkCardinality (node : Node) (rule : NewRule) =
    //             match rule with
    //             |NodeRule(ValueField (ValueType.Specific key), _), opts
    //             |LeafRule(ValueField (ValueType.Specific key), _), opts ->
    //                 let leafcount = node.Values |> List.filter (fun leaf -> leaf.Key == key) |> List.length
    //                 let childcount = node.Children |> List.filter (fun child -> child.Key == key) |> List.length
    //                 let total = leafcount + childcount
    //                 if opts.min > total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Missing %s, expecting at least %i" key opts.min) (opts.severity |> Option.defaultValue severity)) node]
    //                 else if opts.max < total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Too many %s, expecting at most %i" key opts.max) Severity.Warning) node]
    //                 else OK
    //             |NodeRule(AliasField(_), _), _
    //             |LeafRule(AliasField(_), _), _
    //             |LeafValueRule(AliasField(_)), _ -> OK
    //             |NodeRule(l, _), opts ->
    //                 let total = node.Children |> List.filter (fun child -> checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l child.Key child) |> List.length
    //                 if opts.min > total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Missing %A, expecting at least %i" l opts.min) (opts.severity |> Option.defaultValue severity)) node]
    //                 else if opts.max < total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Too many n %A, expecting at most %i" l opts.max) Severity.Warning) node]
    //                 else OK
    //             |LeafRule(l, r), opts ->
    //                 let total = node.Values |> List.filter (fun leaf -> checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l leaf.Key leaf) |> List.length
    //                 if opts.min > total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Missing %A, expecting at least %i" l opts.min) (opts.severity |> Option.defaultValue severity)) node]
    //                 else if opts.max < total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Too many l %A %A, expecting at most %i" l r opts.max) Severity.Warning) node]
    //                 else OK
    //             |LeafValueRule(l), opts ->
    //                 let total = node.LeafValues |> List.ofSeq |> List.filter (fun leafvalue -> checkLeftField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope ctx l leafvalue.Key leafvalue) |> List.length
    //                 if opts.min > total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Missing %A, expecting at least %i" l opts.min) (opts.severity |> Option.defaultValue severity)) node]
    //                 else if opts.max < total then Invalid [inv (ErrorCodes.ConfigRulesWrongNumber (sprintf "Too many lv %A, expecting at most %i" l opts.max) Severity.Warning) node]
    //                 else OK
    //             |_ -> OK
    //         startNode.Leaves <&!&> valueFun
    //         <&&>
    //         (startNode.Children <&!&> nodeFun)
    //         <&&>
    //         (startNode.LeafValues <&!&> leafValueFun)
    //         <&&>
    //         (rules <&!&> checkCardinality startNode)

    //     and applyValueField severity (vt : CWTools.Parser.ConfigParser.ValueType) (leaf : Leaf) =
    //         checkValidValue enumsMap severity vt (leaf.Value.ToRawString()) leaf

    //     and applyLeafValueRule (ctx : RuleContext<_>) (options : Options) (rule : NewField) (leafvalue : LeafValue) =
    //         let severity = options.severity |> Option.defaultValue (if ctx.warningOnly then Severity.Warning else Severity.Error)

    //         checkField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope severity ctx rule (leafvalue.Value.ToRawString()) leafvalue

    //     and applyLeafRule (ctx : RuleContext<_>) (options : Options) (rule : NewField) (leaf : Leaf) =
    //         let severity = options.severity |> Option.defaultValue (if ctx.warningOnly then Severity.Warning else Severity.Error)
    //         (match options.requiredScopes with
    //         |[] -> OK
    //         |xs ->
    //             match ctx.scopes.CurrentScope with
    //             |Scope.Any -> OK
    //             |s -> if List.contains s xs then OK else Invalid [inv (ErrorCodes.CustomError (sprintf "Wrong scope, in %O but expected %A" s xs) Severity.Error) leaf])
    //         <&&>
    //         checkField enumsMap typesMap effectMap triggerMap localisation files changeScope anyScope severity ctx rule (leaf.Value.ToRawString()) leaf
    //     and applyNodeRule (enforceCardinality : bool) (ctx : RuleContext<_>) (options : Options) (rule : NewField) (rules : NewRule list) (node : Node) =
    //         let severity = options.severity |> Option.defaultValue (if ctx.warningOnly then Severity.Warning else Severity.Error)
    //         let newCtx =
    //             match options.pushScope with
    //             |Some ps ->
    //                 {ctx with scopes = {ctx.scopes with Scopes = ps::ctx.scopes.Scopes}}
    //             |None ->
    //                 match options.replaceScopes with
    //                 |Some rs ->
    //                     let newctx =
    //                         match rs.this, rs.froms with
    //                         |Some this, Some froms ->
    //                             {ctx with scopes = {ctx.scopes with Scopes = this::(ctx.scopes.PopScope); From = froms}}
    //                         |Some this, None ->
    //                             {ctx with scopes = {ctx.scopes with Scopes = this::(ctx.scopes.PopScope)}}
    //                         |None, Some froms ->
    //                             {ctx with scopes = {ctx.scopes with From = froms}}
    //                         |None, None ->
    //                             ctx
    //                     match rs.root with
    //                     |Some root ->
    //                         {ctx with scopes = {ctx.scopes with Root = root}}
    //                     |None -> newctx
    //                 |None ->
    //                     if node.Key.StartsWith("event_target:", System.StringComparison.OrdinalIgnoreCase) || node.Key.StartsWith("parameter:", System.StringComparison.OrdinalIgnoreCase)
    //                     then {ctx with scopes = {ctx.scopes with Scopes = Scope.Any::ctx.scopes.Scopes}}
    //                     else ctx
    //         (match options.requiredScopes with
    //         |[] -> OK
    //         |xs ->
    //             match newCtx.scopes.CurrentScope with
    //             |Scope.Any -> OK
    //             |s -> if List.contains s xs then OK else Invalid [inv (ErrorCodes.CustomError (sprintf "Wrong scope, in %O but expected %A" s xs) Severity.Error) node])
    //         <&&>
    //         match rule with
    //         |ScopeField s ->
    //             let scope = newCtx.scopes
    //             let key = node.Key
    //             match changeScope true effectMap triggerMap key scope with
    //             |NewScope (newScopes ,_) ->
    //                 let newCtx = {newCtx with scopes = newScopes}
    //                 applyClauseField enforceCardinality options.severity newCtx rules node
    //             |NotFound _ ->
    //                 Invalid [inv (ErrorCodes.CustomError "This scope command is not valid" Severity.Error) node]
    //             |WrongScope (command, prevscope, expected) ->
    //                 Invalid [inv (ErrorCodes.ConfigRulesErrorInTarget command (prevscope.ToString()) (sprintf "%A" expected) ) node]
    //             |_ -> Invalid [inv (ErrorCodes.CustomError "Something went wrong with this scope change" Severity.Hint) node]

    //         |_ -> applyClauseField enforceCardinality options.severity newCtx rules node

    //     let testSubtype (subtypes : SubTypeDefinition list) (node : Node) =
    //         let results =
    //             subtypes |> List.filter (fun st -> st.typeKeyField |> function |Some tkf -> tkf == node.Key |None -> true)
    //                     |> List.map (fun s -> s.name, s.pushScope, applyClauseField false None {subtypes = []; scopes = defaultContext; warningOnly = false } (s.rules) node)
    //         let res = results |> List.choose (fun (s, ps, res) -> res |> function |Invalid _ -> None |OK -> Some (ps, s))
    //         res |> List.tryPick fst, res |> List.map snd

    //     let applyNodeRuleRoot (typedef : TypeDefinition) (rules : NewRule list) (options : Options) (node : Node) =
    //         let pushScope, subtypes = testSubtype (typedef.subtypes) node
    //         let startingScopeContext =
    //             match Option.orElse pushScope options.pushScope with
    //             |Some ps -> { Root = ps; From = []; Scopes = [] }
    //             |None -> defaultContext
    //         let context = { subtypes = subtypes; scopes = startingScopeContext; warningOnly = typedef.warningOnly }
    //         applyNodeRule true context options (ValueField (ValueType.Specific "root")) rules node

    //     let validate ((path, root) : string * Node) =
    //         let pathDir = (Path.GetDirectoryName path).Replace("\\","/")
    //         let file = Path.GetFileName path
    //         let inner (node : Node) =

    //             let typekeyfilter (td : TypeDefinition) (n : Node) =
    //                 match td.typeKeyFilter with
    //                 |Some (filter, negate) -> n.Key == filter <> negate
    //                 |None -> true
    //             let skiprootkey (td : TypeDefinition) (n : Node) =
    //                 match td.skipRootKey with
    //                 |Some (SpecificKey key) -> n.Key == key
    //                 |Some (AnyKey) -> true
    //                 |None -> false
    //             let validateType (typedef : TypeDefinition) (n : Node) =
    //                 let typerules = typeRules |> List.choose (function |(name, r) when name == typedef.name -> Some r |_ -> None)
    //                 //let expandedRules = typerules |> List.collect (function | (LeafRule (AliasField a, _),_) -> (aliases.TryFind a |> Option.defaultValue []) |x -> [x])
    //                 //let expandedRules = typerules |> List.collect (function | _,_,(AliasField a) -> (aliases.TryFind a |> Option.defaultValue []) |x -> [x])
    //                 //match expandedRules |> List.choose (function |(NodeRule (l, rs), o) when checkLeftField enumsMap typesMap effectMap triggerMap localisation files ctx l node.Key node -> Some (l, rs, o) |_ -> None) with
    //                 //match expandedRules |> List.tryFind (fun (n, _, _) -> n == typedef.name) with
    //                 match typerules |> List.tryHead with
    //                 |Some ((NodeRule ((ValueField (ValueType.Specific (x))), rs), o)) when x == typedef.name->
    //                     match typedef.typeKeyFilter with
    //                     |Some (filter, negate) -> if n.Key == filter <> negate then applyNodeRuleRoot typedef rs o n else OK
    //                     |None -> applyNodeRuleRoot typedef rs o n
    //                 |_ ->
    //                     OK

    //             let skipres =
    //                 match typedefs |> List.filter (fun t -> checkPathDir t pathDir file && skiprootkey t node) with
    //                 |[] -> OK
    //                 |xs ->
    //                     node.Children <&!&>
    //                         (fun c ->
    //                             match xs |> List.tryFind (fun t -> checkPathDir t pathDir file && typekeyfilter t c) with
    //                             |Some typedef -> validateType typedef c
    //                             |None -> OK
    //                         )

    //             let nonskipres =
    //                 match typedefs |> List.tryFind (fun t -> checkPathDir t pathDir file && typekeyfilter t node && t.skipRootKey.IsNone) with
    //                 |Some typedef -> validateType typedef node
    //                 |None -> OK
    //             skipres <&&> nonskipres

    //         let res = (root.Children <&!&> inner)
    //         res
    //     {
    //         applyNodeRule = (fun (rule, node) -> applyNodeRule true {subtypes = []; scopes = defaultContext; warningOnly = false } defaultOptions (ValueField (ValueType.Specific "root")) rule node)
    //         testSubtype = (fun ((subtypes : SubTypeDefinition list), (node : Node)) -> testSubtype subtypes node)
    //         ruleValidate = (fun () -> (fun _ es -> es.Raw |> List.map (fun struct(e, _) -> e.logicalpath, e.entity) <&!!&> validate))
    //     }
        // interface IRuleApplicator<Scope> with
        //     member __.ApplyNodeRule(rule, node) = applyNodeRule true {subtypes = []; scopes = defaultContext; warningOnly = false } defaultOptions (ValueField (ValueType.Specific "root")) rule node
        //     member __.TestSubtype((subtypes : SubTypeDefinition list), (node : Node)) =
        //         testSubtype subtypes node
        //     //member __.ValidateFile(node : Node) = validate node
        //     member __.RuleValidate<'T when 'T :> ComputedData>() : StructureValidator<'T> =
        //         fun _ es -> es.Raw |> List.map (fun struct(e, _) -> e.logicalpath, e.entity) <&!!&> validate