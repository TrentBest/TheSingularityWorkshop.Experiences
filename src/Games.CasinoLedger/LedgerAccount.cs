namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>Defines an account and whether its balance may become negative.</summary>
/// <param name="AccountId">A stable account identifier.</param>
/// <param name="AllowNegativeBalance">Whether postings may drive this account below zero.</param>
/// <remarks>
/// Negative balances are opt-in, for explicit counterparty or clearing accounts. Player chip accounts and
/// cage inventories should normally disallow them. This is a simulation ledger, not a regulated accounting system.
/// </remarks>
public sealed record LedgerAccount(string AccountId, bool AllowNegativeBalance = false);
