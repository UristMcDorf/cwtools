namespace CWTools.Common

open System
open System.ComponentModel.Design
open CWTools.Utilities.Utils

module CK2Constants =
    type Scope =
        | Character
        | Title
        | Province
        | Offmap
        | War
        | Siege
        | Unit
        | Religion
        | Culture
        | Society
        | Artifact
        | Bloodline
        //Misc
        | Any
        | InvalidScope
        override x.ToString() =
            match x with
            | Any -> "Any/Unknown"
            | x -> sprintf "%A" x

        static member AnyScope = Scope.Any

        interface IScope<Scope> with
            member this.AnyScope = Scope.Any
            member this.MatchesScope target = this = target

    let allScopes = [
        Scope.Character;
        Scope.Title;
        Scope.Province;
        Scope.Offmap;
        Scope.War
        Scope.Siege;
        Scope.Unit;
        Scope.Religion;
        Scope.Culture;
        Scope.Society;
        Scope.Artifact;
        Scope.Bloodline
            ]
    let allScopesSet = allScopes |> Set.ofList
    let parseScope =
        (fun (x : string) ->
        x.ToLower()
        |>
            function
                | "character" -> Scope.Character
                | "title" -> Scope.Title
                | "province" -> Scope.Province
                | "offmap" -> Scope.Offmap
                | "war" -> Scope.War
                | "siege" -> Scope.Siege
                | "unit" -> Scope.Unit
                | "religion" -> Scope.Religion
                | "culture" -> Scope.Culture
                | "society" -> Scope.Society
                | "artifact" -> Scope.Artifact
                | "bloodline" -> Scope.Bloodline
                | "any" -> Scope.Any
                | "all" -> Scope.Any
                | "no_scope" -> Scope.Any
                | x -> log (sprintf "Unexpected scope %O" x); Scope.Any) //failwith ("unexpected scope" + x.ToString()))

    let parseScopes =
        function
        | "all" -> allScopes
        | x -> [parseScope x]

    type Effect = Effect<Scope>

    type DocEffect = DocEffect<Scope>
    type ScriptedEffect = ScriptedEffect<Scope>
    type ScopedEffect = ScopedEffect<Scope>
    type ModifierCategory =
        // |State
        // |Country
        // |Unit
        // |UnitLeader
        // |Air
        |Any

    type Modifier =
        {
            tag : string
            categories : ModifierCategory list
            /// Is this a core modifier or a static modifier?
            core : bool
        }
        interface IModifier with
            member this.Tag = this.tag

    // let categoryScopeList = [
    //     ModifierCategory.Country, [Scope.Country]
    //     ModifierCategory.UnitLeader, [Scope.UnitLeader; Scope.Country]
    //     ModifierCategory.Unit, [Scope.UnitLeader; Scope.Country]
    //     ModifierCategory.State, [Scope.Any]
    //     ModifierCategory.Air, [Scope.Air; Scope.Country]
    // ]
    // let modifierCategoryToScopesMap = categoryScopeList |> Map.ofList

    let scriptFolders = [
        "common";
        "common/alternate_start";
        "common/artifact_spawns";
        "common/artifacts";
        "common/bloodlines";
        "common/bookmarks";
        "common/buildings";
        "common/cb_types";
        "common/combat_tactics";
        "common/council_positions";
        "common/council_voting";
        "common/cultures";
        "common/death";
        "common/death_text";
        "common/defines";
        "common/disease";
        "common/dynasties";
        "common/event_modifiers";
        "common/execution_methods";
        "common/game_rules";
        "common/government_flavor";
        "common/governments";
        "common/graphicalculturetypes";
        "common/heir_text";
        "common/holding_types";
        "common/job_actions";
        "common/job_titles";
        "common/landed_titles";
        "common/laws";
        "common/mercenaries";
        "common/minor_titles";
        "common/modifier_definitions";
        "common/nicknames";
        "common/objectives";
        "common/offmap_powers";
        "common/offmap_powers/policies";
        "common/offmap_powers/statuses";
        "common/on_actions";
        "common/opinion_modifiers";
        "common/province_setup";
        "common/religion_features";
        "common/religion_modifiers";
        "common/religions";
        "common/religious_titles";
        "common/retinue_subunits";
        "common/save_conversion";
        "common/scripted_effects";
        "common/scripted_score_values";
        "common/scripted_triggers";
        "common/societies";
        "common/special_troops";
        "common/succession_voting";
        "common/trade_routes";
        "common/traits";
        "common/tributary_types";
        "common/triggered_modifiers";
        "decisions";
        "events";
        "gfx";
        "history";
        "history/characters";
        "history/offmap_powers";
        "history/provinces";
        "history/technology";
        "history/titles";
        "history/wars";
        "interface";
        "interface/coat_of_arms";
        "interface/portrait_offsets";
        "interface/portrait_properties";
        "interface/portrait";
        "localisation";
        "localisation/customizable_localisation";
        "map";
        "map/statics";
        "music";
        "tutorial"
    ]
