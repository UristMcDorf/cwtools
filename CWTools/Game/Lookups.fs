namespace CWTools.Games
open CWTools.Process
open FSharp.Collections.ParallelSeq
open FParsec
open System.IO
open CWTools.Parser
open CWTools.Process.STLProcess
open CWTools.Common.STLConstants
open CWTools.Common
// open CWTools.Process.STLScopes
open CWTools.Parser.ConfigParser
open CWTools.Process.Scopes
open CWTools.Utilities.Position
open CWTools.Utilities.Utils
open Microsoft.FSharp.Collections.Tagged



type Lookup<'S, 'M when 'S : comparison and 'S :> IScope<'S> and 'M :> IModifier>() =
    let mutable _scriptedTriggers : Effect<'S> list = []

    let mutable _scriptedTriggersMap : Lazy<Map<string,Effect<'S>,InsensitiveStringComparer>> = lazy ( Map<string,Effect<'S>,InsensitiveStringComparer>.Empty (InsensitiveStringComparer()) )
    let resetTriggers() =
        _scriptedTriggersMap <- lazy (_scriptedTriggers|> List.map (fun e -> e.Name, e) |> (fun l -> EffectMap<'S>.FromList(InsensitiveStringComparer(), l)))

    let mutable _scriptedEffects : Effect<'S> list = []

    let mutable _scriptedEffectsMap : Lazy<Map<string,Effect<'S>,InsensitiveStringComparer>> = lazy ( Map<string,Effect<'S>,InsensitiveStringComparer>.Empty (InsensitiveStringComparer()) )
    let resetEffects() =
        _scriptedEffectsMap <- lazy (_scriptedEffects|> List.map (fun e -> e.Name, e) |> (fun l -> EffectMap<'S>.FromList(InsensitiveStringComparer(), l)))
    member __.scriptedTriggers
        with get () = _scriptedTriggers
        and set (value) = resetTriggers(); _scriptedTriggers <- value
    member this.scriptedTriggersMap with get() = _scriptedTriggersMap.Force()
    member __.scriptedEffects
        with get () = _scriptedEffects
        and set (value) = resetEffects(); _scriptedEffects <- value
    member this.scriptedEffectsMap with get() = _scriptedEffectsMap.Force()
    member val onlyScriptedEffects : Effect<'S> list = [] with get, set
    member val onlyScriptedTriggers : Effect<'S> list = [] with get, set

    member val staticModifiers : 'M list = [] with get, set
    member val coreModifiers : 'M list = [] with get, set
    member val HOI4provinces : string list = [] with get, set
    member val EU4ScriptedEffectKeys : string list = [] with get, set
    member val EU4TrueLegacyGovernments : string list = [] with get, set
    member val definedScriptVariables : string list = [] with get, set
    member val scriptedLoc : string list = [] with get, set
    member val proccessedLoc : (Lang * Collections.Map<string, LocEntry<'S>>) list = [] with get, set
    member val technologies : (string * (string list)) list =  [] with get, set
    member val configRules : RootRule<'S> list = [] with get, set
    member val typeDefs : TypeDefinition<'S> list = [] with get, set
    member val enumDefs : Collections.Map<string, string * string list> = Map.empty with get, set
    member val typeDefInfo : Collections.Map<string, (string * range) list> = Map.empty with get, set
    member val typeDefInfoForValidation : Collections.Map<string, (string * range) list> = Map.empty with get, set
    member val varDefInfo : Collections.Map<string, (string * range) list> = Map.empty with get, set
    member val globalScriptedVariables : string list = [] with get, set
