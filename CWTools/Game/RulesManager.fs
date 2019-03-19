namespace CWTools.Games
open CWTools.Parser.ConfigParser
open CWTools.Common
open CWTools.Utilities
open CWTools.Utilities.Position
open FSharp.Collections.ParallelSeq
open CWTools.Process.Scopes
open CWTools.Utilities.Utils
open CWTools.Validation.Rules
type RulesSettings = {
    ruleFiles : (string * string) list
    validateRules : bool
    debugRulesOnly : bool
    debugMode : bool
}


type EmbeddedSettings<'S,'M when 'S : comparison> = {
    triggers : DocEffect<'S> list
    effects : DocEffect<'S> list
    embeddedFiles : (string * string) list
    modifiers : 'M list
    cachedResourceData : (Resource * Entity) list
    localisationCommands : (string * ('S list)) list
    eventTargetLinks : EventTargetLink<'S> list
}

type RuleManagerSettings<'S, 'M, 'T when 'S :> IScope<'S> and 'S : comparison and 'M :> IModifier and 'T :> ComputedData> = {
    rulesSettings : RulesSettings option
    parseScope : string -> 'S
    allScopes : 'S list
    anyScope : 'S
    changeScope : ChangeScope<'S>
    defaultContext : ScopeContext<'S>
    defaultLang : Lang
    oneToOneScopesNames : string list
    loadConfigRulesHook : RootRule<'S> list -> Lookup<'S,'M> -> EmbeddedSettings<'S, 'M> -> RootRule<'S> list
    refreshConfigBeforeFirstTypesHook : Lookup<'S, 'M> -> IResourceAPI<'T> -> EmbeddedSettings<'S, 'M> -> unit
    refreshConfigAfterFirstTypesHook : Lookup<'S, 'M> -> IResourceAPI<'T> -> EmbeddedSettings<'S, 'M> -> unit
}

type RulesManager<'T, 'S, 'M when 'T :> ComputedData and 'S :> IScope<'S> and 'S : comparison and 'M :> IModifier>
    (resources : IResourceAPI<'T>, lookup : Lookup<_,_>,
     settings : RuleManagerSettings<'S, 'M, 'T>,
     localisation : LocalisationManager<'S, 'T, 'M>,
     embeddedSettings : EmbeddedSettings<'S, 'M>) =

    let mutable tempEffects = []
    let mutable tempTriggers = []
    let mutable simpleEnums = []
    let mutable complexEnums = []
    let mutable tempTypes = []
    let mutable tempValues = Map.empty
    let mutable tempTypeMap = [("", StringSet.Empty(InsensitiveStringComparer()))] |> Map.ofList
    let mutable tempEnumMap = [("", ("", StringSet.Empty(InsensitiveStringComparer())))] |> Map.ofList
    let mutable rulesDataGenerated = false

    let loadBaseConfig(rulesSettings : RulesSettings) =
        let rules, types, enums, complexenums, values = rulesSettings.ruleFiles |> List.fold (fun (rs, ts, es, ces, vs) (fn, ft) -> let r2, t2, e2, ce2, v2 = parseConfig settings.parseScope settings.allScopes settings.anyScope fn ft in rs@r2, ts@t2, es@e2, ces@ce2, vs@v2) ([], [], [], [], [])
        // tempEffects <- updateScriptedEffects game rules
        // effects <- tempEffects
        // tempTriggers <- updateScriptedTriggers game rules
        // _triggers <- tempTriggers
        lookup.typeDefs <- types
        // let rulesWithMod = rules @ addModifiersWithScopes(game)
        let rulesPostHook = settings.loadConfigRulesHook rules lookup embeddedSettings
        // lookup.configRules <- rulesWithMod
        lookup.configRules <- rulesPostHook
        simpleEnums <- enums
        complexEnums <- complexenums
        tempTypes <- types
        tempValues <- values |> List.map (fun (s, sl) -> s, (sl |> List.map (fun s2 -> s2, range.Zero))) |> Map.ofList
        rulesDataGenerated <- false
        // log (sprintf "Update config rules def: %i" timer.ElapsedMilliseconds); timer.Restart()

    let refreshConfig() =
        /// Enums
        let complexEnumDefs = CWTools.Validation.Rules.getEnumsFromComplexEnums complexEnums (resources.AllEntities() |> List.map (fun struct(e,_) -> e))
        // let modifierEnums = { key = "modifiers"; values = lookup.coreModifiers |> List.map (fun m -> m.Tag); description = "Modifiers" }
        let allEnums = simpleEnums @ complexEnumDefs// @ [modifierEnums] @ [{ key = "provinces"; description = "provinces"; values = lookup.CK2provinces}]

        lookup.enumDefs <- allEnums |> List.map (fun e -> (e.key, (e.description, e.values))) |> Map.ofList

        settings.refreshConfigBeforeFirstTypesHook lookup |> ignore

        tempEnumMap <- lookup.enumDefs |> Map.toSeq |> PSeq.map (fun (k, (d, s)) -> k, (d, StringSet.Create(InsensitiveStringComparer(), (s)))) |> Map.ofSeq

        /// First pass type defs
        let loc = localisation.localisationKeys
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let files = resources.GetFileNames() |> Set.ofList
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let tempRuleApplicator = RuleApplicator<'S>(lookup.configRules, lookup.typeDefs, tempTypeMap, tempEnumMap, Collections.Map.empty, loc, files, lookup.triggersMap, lookup.effectsMap, lookup.eventTargetLinksMap, settings.anyScope, settings.changeScope, settings.defaultContext, settings.defaultLang)
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let allentities = resources.AllEntities() |> List.map (fun struct(e,_) -> e)
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let typeDefInfo = getTypesFromDefinitions tempRuleApplicator tempTypes allentities


        // let typeDefInfo = createLandedTitleTypes game typeDefInfo
        lookup.typeDefInfoRaw <- typeDefInfo// |> Map.map (fun _ v -> v |> List.map (fun (_, t, r) -> (t, r)))
        // lookup.typeDefInfo <- addModifiersAsTypes game lookup.typeDefInfo

        settings.refreshConfigAfterFirstTypesHook lookup |> ignore
        lookup.typeDefInfoForValidation <- typeDefInfo |> Map.map (fun _ v -> v |> List.choose (fun (v, t, r) -> if v then Some (t, r) else None))

        // lookup.scriptedEffects <- tempEffects @ addDataEventTargetLinks game
        // lookup.scriptedTriggers <- tempTriggers @ addDataEventTargetLinks game

        tempTypeMap <- lookup.typeDefInfo |> Map.toSeq |> PSeq.map (fun (k, s) -> k, StringSet.Create(InsensitiveStringComparer(), (s |> List.map fst))) |> Map.ofSeq
        let tempFoldRules = (FoldRules<'S>(lookup.configRules, lookup.typeDefs, tempTypeMap, tempEnumMap, Collections.Map.empty, loc, files, lookup.triggersMap, lookup.effectsMap, lookup.eventTargetLinksMap, tempRuleApplicator, settings.changeScope, settings.defaultContext, settings.anyScope, settings.defaultLang))

        let infoService = tempFoldRules
        // game.InfoService <- Some tempFoldRules
        if not rulesDataGenerated then resources.ForceRulesDataGenerate(); rulesDataGenerated <- true else ()

        let results = resources.AllEntities() |> PSeq.map (fun struct(e, l) -> (l.Force().Definedvariables |> (Option.defaultWith (fun () -> tempFoldRules.GetDefinedVariables e))))
                        |> Seq.fold (fun m map -> Map.toList map |>  List.fold (fun m2 (n,k) -> if Map.containsKey n m2 then Map.add n (k@m2.[n]) m2 else Map.add n k m2) m) tempValues

        lookup.varDefInfo <- results

        let varMap = lookup.varDefInfo |> Map.toSeq |> PSeq.map (fun (k, s) -> k, StringSet.Create(InsensitiveStringComparer(), (List.map fst s))) |> Map.ofSeq

        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let completionService = (CompletionService(lookup.configRules, lookup.typeDefs, tempTypeMap, tempEnumMap, varMap, loc, files, lookup.triggersMap, lookup.effectsMap, lookup.eventTargetLinksMap, [], settings.changeScope, settings.defaultContext, settings.anyScope, settings.oneToOneScopesNames, settings.defaultLang))
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let ruleApplicator =  (RuleApplicator<'S>(lookup.configRules, lookup.typeDefs, tempTypeMap, tempEnumMap, varMap, loc, files, lookup.triggersMap, lookup.effectsMap, lookup.eventTargetLinksMap, settings.anyScope, settings.changeScope, settings.defaultContext, settings.defaultLang))
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        let infoService = (FoldRules<'S>(lookup.configRules, lookup.typeDefs, tempTypeMap, tempEnumMap, varMap, loc, files, lookup.triggersMap, lookup.effectsMap, lookup.eventTargetLinksMap, ruleApplicator, settings.changeScope, settings.defaultContext, settings.anyScope, settings.defaultLang))
        // log "Refresh rule caches time: %i" timer.ElapsedMilliseconds; timer.Restart()
        // game.RefreshValidationManager()
        ruleApplicator, infoService, completionService

    member __.LoadBaseConfig(rulesSettings) = loadBaseConfig rulesSettings
    member __.RefreshConfig() = refreshConfig()