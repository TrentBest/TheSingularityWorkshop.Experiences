# Galactic War — Finite Provenance, Trophy Objects, and Ownership

**Status:** candidate design contract. This document defines the intended semantics of genuinely scarce, historically attributable objects. It does not claim an implemented inventory, ownership ledger, combat-loot system, or anti-cheat service.

## The promise

Some objects in the galaxy are not interchangeable rewards generated from an endless table. They are particular things with a history.

A ceremonial blade, a commander's insignia, a one-of-a-kind prototype, a limited-run uniform decoration, or a famous ship component can have a finite number of legitimate instances. If a decoration was issued in a run of 200, there are at most 200 legitimate instances in existence. The world should be able to answer:

- Which instance is this?
- How many legitimate instances were created?
- Who or what has held it?
- How did it change hands?
- Where is it now, if known?
- Is it intact, damaged, sealed, lost, destroyed, or retired?
- What event made its current ownership legitimate?

Rarity is a fact about the world's history and rules, not merely a color or probability in an item tooltip.

## Object classes

The Experience should distinguish several ownership and scarcity models rather than applying one loot rule to everything.

### Finite issue

A finite issue has a declared maximum quantity. For example, a decoration may have exactly 200 authorized instances. Each instance has its own persistent identity and provenance. The issue may be closed permanently or have an explicitly documented release schedule; the system must never silently mint replacement copies.

### Unique object

A unique object has one authorized instance. It may be a singular artifact, prototype, historic weapon, or named vessel. Its uniqueness is enforced by identity and issuance rules, not by a low random drop chance.

### Reproducible equipment

Ordinary equipment can be manufactured, purchased, repaired, or replaced under the world's economic and production rules. It may still have individual serial identity when maintenance, ownership, or history matters, but it is not scarce merely because it has a serial number.

### Bound or non-transferable award

Some awards represent a person's qualification, achievement, service, or personal history. Their meaning may depend on the recipient, so transfer is not permitted. The object can be displayed, lost temporarily through account or character state, or retired under explicit rules, but it cannot become another person's trophy.

The item's class and transfer policy must be visible and machine-enforceable. Do not imply that every cosmetic can be stolen or that every rare object can be traded.

## Instance identity and provenance

Every individually tracked finite or unique instance needs a stable identity that survives display changes, repairs, ownership changes, and movement between containers.

Its provenance is an append-only sequence of authoritative events, such as:

- authorized creation or issuance;
- initial assignment or custody;
- transfer, trade, gift, or inheritance;
- capture or recovery under a declared game rule;
- deposit into secure storage or escrow;
- repair, modification, or restoration;
- loss, theft, seizure, or reclamation;
- destruction or permanent retirement.

An event records the actor or authority, the instance, the prior and resulting custody/ownership state, the in-world time and location where relevant, the governing rule, and evidence sufficient to explain the transition. A current-owner field can be a projection of this history; it must not replace the history.

A claim of ownership is not proof by itself. The authoritative state must be derived from validated transitions. Conflicting reports can exist in the story, but they must not create two authoritative owners.

## The trophy rule

For designated trophy objects, the intended fantasy is that possession can be earned by overcoming the current holder. A visitor may need to defeat the person currently carrying a legendary insignia, recover it, and take possession through the world's rules.

**Defeat is not automatically synonymous with killing.** Depending on the encounter and the object's rules, defeat could mean surrender, capture, disarmament, a completed duel, seizure of a secured objective, or another explicitly recognized resolution. The encounter's declared rules determine whether the item changes hands.

The rule must answer, before an encounter begins:

1. Is this object actually transferable through defeat?
2. What constitutes a valid defeat or capture?
3. Does the holder retain it if they retreat, are rescued, or are only temporarily disarmed?
4. What must the victor do to secure and claim it?
5. Can an ally recover it, and under what attribution?
6. What happens if the object is destroyed, inaccessible, or carried away by a third party?
7. What evidence is retained if the outcome is disputed?

Do not make an item transfer merely because a combatant's health reaches zero. The world must resolve custody and ownership from the encounter's actual outcome.

## Ownership, custody, and location are different

The design should not collapse these concepts:

- **Ownership:** the entity recognized as having the legitimate title or claim under the object's rules.
- **Custody:** the person, organization, or system currently responsible for physically holding or securing it.
- **Location:** where the instance is believed or authoritatively known to be.
- **Condition:** whether it is intact, damaged, disabled, sealed, or destroyed.
- **Display:** how it is presented in a character's appearance, a ship, a museum, or an archive.

A museum can hold an object without owning it. A character can own an object held in secure storage. A trophy can be displayed by its owner or loaned to a faction. A lost object can have a last-known location without a current custodian.

## Fairness and exploit resistance

Finite scarcity makes mistakes and exploits unusually consequential. The implementation must fail closed on ambiguous transfers and must never duplicate an instance to repair a bookkeeping problem.

Candidate safeguards include:

- globally unique instance identity and issuance limits;
- atomic, validated ownership/custody transitions;
- idempotent handling of repeated event delivery;
- explicit trade, escrow, and recovery states;
- evidence for contested or exceptional transfers;
- protection against disconnects and simultaneous claims during an encounter;
- no automatic transfer from a temporary incapacitation unless the rule explicitly says so;
- no silent replacement after destruction or loss;
- auditability for administrative correction, with the correction itself recorded.

The precise controls against collusion, repeated farming, staged defeats, account sharing, or abuse of recovery mechanics need a threat model and playtesting. They should not be guessed into existence as arbitrary cooldowns. The goal is to protect meaningful scarcity while preserving legitimate stories, including a real loss to a better opponent.

## Player-facing meaning

The interface should communicate more than a rarity tier. A finite object can show its issue number, total authorized issue, origin, notable prior holders, current status, and a concise provenance history. A unique object can explain why it is unique. A bound award can explain why it cannot be transferred.

A visitor should be able to inspect an object and understand why another character values it. A famous object should create social consequences: recognition, suspicion, respect, rivalry, political interest, or a challenge from someone who believes they have a stronger claim.

Do not expose private information or reveal hidden holders and locations when the world rules treat them as secret. Provenance visibility is itself a policy.

## Architecture boundaries

- **Galactic War / Experiences** owns object meaning, scarcity classes, transfer policy, trophy rules, and the narrative consequences of possession.
- **FSM_API** can provide stable identity and state-transition primitives for tracked instances; it does not decide what counts as a legitimate victory or transfer.
- **FSM_COS** composes the capabilities needed by the Experience; **FSM_Foundry** executes the composed behavior.
- **Economy** can supply reusable valuation, trade, and transaction capabilities where appropriate. Market price is not proof of ownership.
- **DataWarehouse** can retain and query provenance history as internal memory; a derived view must not become an alternate authority for ownership.
- **Renderer / Visualization** can present identity and provenance as MicroBundle experiences. A badge, icon, or rendered display is not authoritative ownership state.

Do not create a parallel game-specific identity system if the existing FSM/API primitives can supply stable instance identity. Verify actual contracts before implementation.

## Lifecycle contract and invariants

The issue and each instance have separate lifecycles. Do not represent both with one generic "rarity" or "owned" flag.

### Issue-level lifecycle

| State | Meaning | Permitted next states |
|---|---|---|
| Draft | Proposed, not authorized for creation | Authorized, Cancelled |
| Authorized | Rules, cap, and issuing authority are recorded; no implication that every instance exists | Open, Closed |
| Open | Authorized instances may be minted up to the current cap | Exhausted, Closed |
| Exhausted | The full authorized cap has been minted | Closed |
| Closed | No further minting under this issue revision | — |
| Cancelled | Issue was never opened; no instances may have been minted | — |

Closing an issue does not destroy, revoke, or invalidate already minted instances. Exhausted and Closed are not synonyms: an issue can close early with some authorized instances never minted. A cancelled issue must have zero minted instances.

If the authorized cap is ever changed, that is a separately authorized, auditable issue amendment with its own effective time and reason. It must not rewrite the original cap or make previously invalid instances appear legitimate. The default policy should be that a finite issue's cap never increases after opening; any exception must be visible to players and historical records.

### Instance lifecycle

An instance's physical condition and legal ownership are orthogonal to whether it is in circulation. Use explicit events to move between lifecycle states:

| State | Meaning | Example event that may enter it |
|---|---|---|
| Minted | Legitimately created, not yet assigned | Authorized issuance |
| In circulation | Available to be held, displayed, stored, or transferred under its policy | Initial assignment or release |
| Secured | Held in validated escrow, institutional custody, or a protected objective | Deposit or seizure |
| Lost | No current custodian is known; last-known location/evidence remains | Confirmed loss |
| Destroyed | The instance no longer exists as a usable object | Validated destruction |
| Retired | Permanently removed from circulation but retained in the historical record | Authorized retirement |

"Lost" is not "destroyed"; recovery may return a lost instance to circulation through a recorded recovery event. "Retired" is not a hidden deletion. Destruction and retirement are terminal unless a separately defined restoration rule exists; restoration must preserve the original instance identity and record the restoration rather than minting a replacement.

### Transition acceptance

Every state-changing command should be evaluated against the current authoritative version of the instance and issue. A transition is accepted only if all of these are true:

1. The instance identity exists and was minted under a valid issue.
2. The requested transition is permitted by the object's class and current state.
3. The actor or issuing authority is authorized for this transition.
4. Required encounter, custody, consent, transaction, or evidence conditions are satisfied.
5. The expected prior state/version still matches the authoritative state.
6. The transition has a unique operation/event identity, so retries are idempotent.
7. The event can be appended without violating the issue cap or creating conflicting ownership/custody.

If any check fails, the authoritative state remains unchanged and the rejection is explainable. A retry of the same accepted operation returns the prior result; it does not append a second transfer. Two competing claims against the same prior version cannot both succeed. The losing claimant receives a conflict result and must re-evaluate against the new state.

### Minimum invariant suite

These are testable invariants, not UI conventions:

- **Identity:** one instance ID identifies at most one legitimate instance for its entire lifetime.
- **Cap:** minted count never exceeds the issue's authorized cap at the time of minting.
- **No resurrection by duplication:** destruction, loss, or retirement never authorizes a second instance with the same identity.
- **Single authoritative title:** at most one current owner exists when the item's policy defines ownership.
- **Single physical custody:** at most one authoritative custodian exists at a time, except for explicitly modeled shared custody.
- **Custody/title separation:** moving an object into a museum or escrow does not silently change title.
- **Transfer policy:** bound awards reject transfer even when an encounter is won.
- **Trophy conditions:** a transfer occurs only after the configured victory, recovery, and claim conditions are all met.
- **Append-only history:** corrections append a compensating or superseding event; they do not erase the original event.
- **Projection consistency:** current owner, custodian, location, and condition can be rebuilt from accepted history and agree with the authoritative projection.
- **Privacy:** public provenance views reveal only fields the disclosure policy permits; hidden custody does not become public through a UI projection.

### Adversarial test cases

| Test | Setup | Expected result |
|---|---|---|
| Cap race | Two issuance requests compete for the final authorized instance | Exactly one succeeds |
| Duplicate retry | Same transfer operation is submitted twice | One transfer event and one resulting state |
| Competing capture | Two actors claim the same trophy from the same encounter state | At most one claim succeeds |
| Disconnect at claim | Holder disconnects between defeat and secured recovery | No ownership change until the declared claim conditions complete |
| Temporary disarm | Holder is disarmed but retains encounter control or recovers the item | No transfer unless the rule explicitly treats disarmament as final |
| Bound award capture | A bound award holder is defeated | Transfer rejected; award remains bound |
| Escrow deposit | Owner deposits trophy with a neutral custodian | Custody changes; title does not |
| Lost-item recovery | Lost item is found by a new actor | Recovery/custody event recorded; title follows the declared recovery rule |
| Destruction replay | Destruction event is delivered repeatedly | One terminal transition; no replacement instance |
| History correction | An administrator corrects a mistaken transfer | Original event remains; correction is auditable |
| Unauthorized mint | Actor attempts issuance outside the authorized issue | Rejected; minted count unchanged |
| Closed issue mint | Mint request arrives after closure | Rejected, even if the nominal cap has unused capacity |

A test harness should assert both the resulting projection and the event history. Testing only the current-owner field would miss provenance corruption.

## First proof

The smallest useful proof is a finite issue of three visibly distinct trophy insignia, each with a stable instance identity.

1. Issue exactly three instances under one declared finite issue.
2. Assign one instance to an NPC holder and one to a player character; leave the third in a known secured location.
3. Record the initial issuance and custody events.
4. Resolve one encounter in which the player defeats the NPC by a declared non-lethal capture or surrender condition.
5. Secure and claim the trophy through the explicit transfer rule.
6. Verify that the previous holder no longer has authoritative ownership or custody, the new holder does, and the provenance history contains both the original issuance and the transfer.
7. Replay the same transfer event and verify it does not create a duplicate or a second ownership transition.
8. Attempt an invalid or ambiguous transfer and verify that the authoritative state remains unchanged.
9. Confirm that the total authorized issue remains three throughout.

This proof tests the contract, not the entire combat system, economy, or multiplayer anti-cheat architecture.

## Decisions still requiring illumination

- Which objects are genuinely finite, and which are simply difficult to obtain?
- Can a finite issue ever be expanded, and who has authority to authorize that change?
- Which trophy classes transfer on defeat, and which remain bound?
- What happens to a trophy when a holder permanently leaves the world?
- Can an object be destroyed, and is destruction reversible?
- Are historical owners public, private, or selectively discoverable?
- How do faction custody, museums, inheritance, salvage, and legal seizure interact?
- What are the threat models for multiplayer transfer exploits and collusion?

The governing principle is simple: **if an object is said to be one of 200, the galaxy must be able to account for all 200—and every legitimate change of hands.**
