---
name: add-kanobu-ship
description: 'Use when adding, implementing, or changing a KaNoBu ship/figure type, its movement, combat outcomes, fleet availability, memory/visibility, Godot presentation, tests, or documentation. Trigger on requests such as "add a new ship", "добавить корабль", "новый корабль", or "изменить правила боя" in this repository.'
---

# Add a KaNoBu ship correctly

Use this workflow for new ship types and substantial changes to existing ship mechanics. Treat the current code and the user's exact rules as authoritative; never assume an earlier conversation or README matches the current checkout.

## 1. Confirm requirements before editing

Do not modify code, tests, or documentation until all required ship decisions are known. First inspect the request and relevant current code/docs to determine which answers are already explicit. Do not ask again for information the user already supplied. If any required item is missing or ambiguous, stop and ask the user before implementation; present concise selectable options plus an "Other / specify" option where possible. Ask all missing questions together.

Required decisions:

1. **Ship name/type:** display name and code/enum identifier. Suggest a repository-style identifier such as `ShipDreadnought` if the user gave a ship name but not an identifier.
2. **Movement:** stationary, one orthogonal cell, scout-style Manhattan distance up to two, or another explicitly stated movement rule. Clarify any special restrictions if needed.
3. **Combat:** outcomes when this ship attacks each relevant type and when each existing ship attacks it. State winners explicitly as attacker/defender. Include mine behavior (attacker only loses, defender only loses, both destroyed, or another rule), scout behavior (combat or reconnaissance only), flag interaction, and same-type encounter. Do not silently infer symmetry.
4. **Console representation:** the short printable symbol/letter shown for this ship by the console/log output. Inspect the existing console formatting and `FigureTypeInfo.PrintableName` conventions, then offer unused, readable letters as options and allow a custom symbol. Do not choose a symbol without user confirmation when it was not specified.

Use a short questionnaire, for example:

- Name/identifier: proposed value or custom value.
- Movement: stationary / one orthogonal cell / scout range / custom.
- Combat: provide a compact attacker-vs-defender matrix, or ask separately about each missing relationship and the mine/scout/flag cases.
- Console symbol: candidate unused letters / custom symbol.

If the request contains a complete battle matrix or explicit rules that determine it unambiguously, treat those rules as the answer. If the user only gives a broad phrase (for example, "beats all ships"), clarify whether that includes the mine, flag, scout reconnaissance, and same-type ship before coding.

## 2. Inspect current conventions

Before editing, read the current versions of:

- `TurnBase.KaNoBu/Figures/KaNoBuFigure.cs`, all relevant `Ship*KaNoBuFigure.cs` classes, and `FigureTypeInfo.cs`;
- `TurnBase.KaNoBu/Rules/KaNoBuRules.cs` and `KaNoBuFieldMemorization.cs`;
- `TurnBase.KaNoBu/Players/KaNoBuPlayerMedium.cs` when battle outcomes influence AI decisions;
- `TurnBase.KaNoBu.Godot/Presentation/Unit.cs` and the scene/texture atlas when ship icons are mapped;
- relevant tests and README files.

Check the working tree/current file contents before modifying anything. Preserve user edits and local style. Also inspect `TurnBase.KaNoBu.Console` output/formatting code to see how `PrintableName` is displayed. After this inspection, repeat the requirements gate: every required decision must have an explicit answer from the user or be unambiguously dictated by their request. If not, ask before proceeding.

## 3. Define the battle matrix explicitly

For each existing ship class, write down outcomes against the new type in both directions. `ResolveBattle(defender)` always describes the caller as the **attacker**. A common source of bugs is translating “ship X loses when attacked” into the wrong `BattleResolution`: when X is the defender and loses, the attacker wins (`AttackerWon(attacker)`), not `DefenderWon(defender)`.

Represent each case with the correct result and winner instance:

- attacker wins: `BattleResolution.AttackerWon(attackerFigure)`;
- defender wins: `BattleResolution.DefenderWon(defenderFigure)`;
- both perish: `BattleResolution.BothAreDestroyed()`;
- scout reveals without combat: `BattleResolution.Scouted()`;
- draw: `BattleResolution.Draw()`.

Preserve special cases already established by the game (unknown type cannot resolve combat; flag and mine cannot initiate combat; a scout's reconnaissance is not a normal loss/win). Follow the requested rule exactly rather than inferring symmetry. For example, a rule that a dreadnought wins when it attacks but loses to an attacking ship is intentionally asymmetric; a scout that only scouts is the stated exception. Mine behavior depends on whether it is the defender or an impossible attacker and must match existing rules and the request.

## 4. Integrate the type everywhere

Implement only the applicable items, but audit all of them rather than stopping when the new class compiles:

1. Add the enum member and `Create` factory case in `KaNoBuFigure`.
2. Add the concrete figure class: identifier, mobility, legal movement offsets, and `ResolveBattle` behavior. Use existing movement conventions; do not invent movement rules unless requested.
3. Update every existing figure's `ResolveBattle` switch for the new defender type, including regular ships, universal, scout, mine, flag, and any other exhaustive switch. Keep non-attacking pieces' exceptions and scout reconnaissance semantics correct.
4. Add the user-confirmed console abbreviation in `FigureTypeInfo.PrintableName`; verify it appears correctly in console/log output and does not collide with another type's symbol.
5. Check initial fleet generation in `KaNoBuRules`: decide whether/how often the new type is included, ensure the generated count still matches the placement area, and avoid accidentally removing any existing type from small starting fleets. Add or update composition tests for relevant field sizes.
6. Check `KaNoBuFieldMemorization`'s candidate-type list and battle-notification inference. Include the new type only where it is a valid candidate. Verify that a hidden losing/winning ship stays unknown when battle evidence is ambiguous and is revealed when uniquely inferable.
7. Review AI logic that predicts battle outcomes or retreat/approach behavior. It should rely on `ResolveBattle` where possible and respect scout behavior.
8. Review Godot/UI mappings such as the `UnitType` texture switch. Use a real, valid atlas region/icon, preserve scene conventions, and do not assume an unused coordinate is valid. If no suitable icon exists, use an intentional existing fallback or explain the missing art rather than silently mapping a blank area.
9. Review serialization/network/logging/console mappings if they enumerate figure types or require explicit names.

## 5. Test the rules, not just the class

Extend battle table tests to cover:

- the new ship attacking every relevant known defender;
- every relevant existing ship attacking the new ship;
- the mine exception, scout's non-combat reconnaissance, flag behavior, and same-type encounter if applicable;
- expected battle outcome **and** winner type/player identity.

Add focused tests for new movement, initial fleet availability/count, and memory/notification inference when affected. Do not change unrelated expectations merely to make tests pass: update a test only when the new rule makes its previous assumption genuinely invalid. Prefer deterministic AI tests; verify the test setup is actually the condition described by the test.

Run the targeted tests and then `dotnet test`. Fix failures caused by the change, and distinguish pre-existing/flaky failures from regressions. Inspect the final diff and check whitespace/errors.

## 6. Update documentation

Find which README files actually describe the changed concepts. At minimum, inspect:

- `TurnBase.KaNoBu/README.md` for the game-wide ship list and concise rule summary;
- `TurnBase.KaNoBu/Figures/README.md` for the figure catalog, movement, and attacker/defender battle behavior;
- `TurnBase.KaNoBu/Rules/README.md` if fleet composition, rule responsibilities, or initialization changed.

Update only relevant docs, following each file's language, headings, terminology, and bullet style. State asymmetric rules explicitly by attacker/defender role and name all important exceptions (especially mine and scout). Check parent/root README only if its architecture, subsystem inventory, public contract, or lifecycle description is affected; do not duplicate a detailed battle catalog there. Keep documentation consistent with the actual tested behavior and the current contents, not with stale prior notes.

## Completion checklist

- [ ] Before editing, the ship name/identifier, movement, complete combat exceptions, and console symbol were confirmed; missing details were asked with options.
- [ ] User's attacker/defender semantics are implemented literally.
- [ ] Every battle switch and new-type integration point was audited.
- [ ] Tests assert outcome and winner identity for both attack directions and exceptions.
- [ ] Fleet generation, memory inference, AI, and UI are reviewed where relevant.
- [ ] Relevant README files describe the same rules as the implementation.
- [ ] `dotnet test` and diff/error checks have been run; remaining issues are reported clearly.
