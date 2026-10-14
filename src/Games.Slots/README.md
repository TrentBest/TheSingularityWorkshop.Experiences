# TheSingularityWorkshop.Games.Slots

A host-neutral first slice for **deterministic slot outcome evaluation** and exact probability analysis in Workshop simulations.

## What exists

- Independent cyclic reel strips, where repeated symbols represent repeated stops and therefore affect probability.
- A configurable total-return paytable for a simple single-line, all-reels-match win.
- Evaluation of caller-supplied stops; this library does not hide random selection inside the rules engine.
- Exact analytical expected payout and return-to-player (RTP) for the supported rule set.
- Input validation and immutable/read-only result surfaces.

## What does not exist yet

No reel animation, paylines, wild/scatter symbols, bonus rounds, random-stop generator, player wallet, chip accounting, wager settlement, casino UI, or MicroBundle identity/adapter. Do not interpret this library as a complete slot game or a financial system.

## Quick example

```csharp
using TheSingularityWorkshop.Games.Slots;

var machine = new SlotMachineDefinition(
    new[]
    {
        new[] { "Cherry", "Cherry", "Bell" },
        new[] { "Cherry", "Bell", "Bell" },
        new[] { "Cherry", "Bell", "Seven" }
    },
    new Dictionary<string, decimal>
    {
        ["Cherry"] = 2m,
        ["Bell"] = 4m,
        ["Seven"] = 10m
    });

var spin = machine.Evaluate(new[] { "Cherry", "Cherry", "Cherry" }, stake: 1m);
Console.WriteLine(spin.GrossPayout);                  // 2
Console.WriteLine(machine.CalculateReturnToPlayer(1m)); // exact expected gross payout per 1-unit stake
```

A multiplier is **total gross return**, including returned stake—not profit-only. A 2x return on a 1-unit stake returns 2 units total.

See [the design note](../../docs/GAMES_SLOTS_DESIGN.md) for the probability derivation, separation from accounting, and next milestones.

## Probability contract

For each paying symbol, its probability is the product of that symbol's frequency on each independent reel divided by that reel's number of stops. The exact RTP is the sum of each paying symbol's probability multiplied by its configured total-return multiplier. This contract applies only to this package's simple single-line, all-reels-match rule.

RTP is an expectation over many spins, not a promise for a finite session. It says nothing by itself about variance, maximum loss, or player outcomes.

## Boundaries

This is a simulation-only game-rule library. It does not implement real-money gambling, deposits, withdrawals, payments, account balances, or settlement. It has no GUI or host dependency. Random stop selection is deliberately outside the evaluator so tests and experiments can provide reproducible outcomes.

The package version and package ID are source metadata, not proof of an allocated MicroBundle ID or publication readiness. Package-on-build and publishing remain disabled.
