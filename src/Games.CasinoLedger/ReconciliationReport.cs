using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Games.CasinoLedger;

/// <summary>Describes the net ledger balance for each currency at a reconciliation point.</summary>
public sealed class ReconciliationReport
{
    internal ReconciliationReport(IDictionary<string, decimal> totalsByCurrency)
    {
        TotalsByCurrency = new ReadOnlyDictionary<string, decimal>(
            new Dictionary<string, decimal>(totalsByCurrency, StringComparer.Ordinal));
    }

    /// <summary>Gets the sum of all account balances by currency. Every value must be zero.</summary>
    public IReadOnlyDictionary<string, decimal> TotalsByCurrency { get; }

    /// <summary>Gets whether all currency totals are exactly zero.</summary>
    public bool IsBalanced => TotalsByCurrency.Values.All(total => total == 0m);
}
