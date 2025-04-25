namespace AStar.Dev.Technical.Debt.Reporting;

/// <summary>
///     The <see cref="RefactorAttribute" /> attribute - can be applied multiple times to cover multiple issues
/// </summary>
/// <param name="hoursToResolve">The estimated hours to resolve</param>
/// <param name="painEstimate">The estimated pain level</param>
/// <param name="description">The description of the refactoring to be performed</param>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class RefactorAttribute(int painEstimate, int hoursToResolve, string description) : Attribute
{
    /// <summary>
    ///     The estimated hours to resolve
    /// </summary>
    public int HoursToResolve { get; } = hoursToResolve;

    /// <summary>
    ///     Gets the description of the refactoring to be performed
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    ///     The estimated pain level - quantity to be defined
    /// </summary>
    public int PainEstimate => painEstimate;

    /// <summary>
    ///     Gets the calculated relative benefit of fixing / refactoring
    /// </summary>
    public double RelativeBenefitToFix => (double)PainEstimate / HoursToResolve;
}
