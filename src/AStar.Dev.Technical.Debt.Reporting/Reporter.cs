using System.Reflection;
using System.Text;

namespace AStar.Dev.Technical.Debt.Reporting;

/// <summary>
/// </summary>
public static class Reporter
{
    /// <summary>
    /// </summary>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static string GenerateReport(params Assembly[] assemblies)
    {
        try
        {
            IEnumerable<MemberInfo> typesWithTechDebt = FindTypesWithTechDebt(assemblies);
            MemberInfo[]            x                 = typesWithTechDebt.ToArray();
            IEnumerable<ReportLine> reportLines       = GenerateReportLines(x);

            return RenderReportLinesToTextReport(reportLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return "An error has occured";
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="assemblyToReportOn"></param>
    /// <param name="maxAllowableMain"></param>
    /// <exception cref="TechDebtPainExceededException"></exception>
    public static void AssertMaxPainNotExceeded(Assembly assemblyToReportOn, int maxAllowableMain)
    {
        IEnumerable<MemberInfo> typesWithDebt = FindAllTheTypesThatHaveTechDebt(assemblyToReportOn);

        int totalPain = (from t in typesWithDebt
                         let techDebtAttribute = (RefactorAttribute)t.GetCustomAttributes(typeof(RefactorAttribute), false)[0]
                         select techDebtAttribute).Sum(x => x.PainEstimate);

        if (totalPain > maxAllowableMain)
        {
            throw new TechDebtPainExceededException();
        }
    }

    private static IEnumerable<MemberInfo> FindTypesWithTechDebt(IEnumerable<Assembly> assemblies) =>
        assemblies.SelectMany(FindAllTheTypesThatHaveTechDebt);

    private static IEnumerable<MemberInfo> FindAllTheTypesThatHaveTechDebt(Assembly assembly)
    {
        return assembly.GetTypes()
                       .SelectMany(type => type.GetMembers())
                       .Union(assembly.GetTypes())
                       .Where(type => Attribute.IsDefined(type, typeof(RefactorAttribute)));
    }

    private static IEnumerable<ReportLine> GenerateReportLines(IEnumerable<MemberInfo> typesWithTechDebt) =>
        typesWithTechDebt.Select(type => new { type, techDebtAttribute = (RefactorAttribute) type.GetCustomAttributes(typeof (RefactorAttribute), false)[0], })
                         .Select(t => new ReportLine
                                      {
                                          Attribute = t.techDebtAttribute, TypeOrMemberName = t.type.ToString(), ReflectedType = t.type.ReflectedType?.Name,
                                          Assembly  = t.type.ReflectedType?.Assembly.GetName().FullName,
                                      }).ToList();

    private static string RenderReportLinesToTextReport(IEnumerable<ReportLine> reportLines)
    {
        var count = 0;
        var sb    = new StringBuilder();

        sb.AppendLine("***Start of Refactorings Report - finding all [Refactor] attribute usages");

        sb.AppendLine();

        foreach (ReportLine item in reportLines.OrderByDescending(x => x.Attribute.RelativeBenefitToFix))
        {
            sb.AppendLine(item.ToString());
            count++;
        }

        sb.AppendLine();

        sb.AppendLine("***End of Refactorings Report.");

        return count > 0 ? sb.ToString() : string.Empty;
    }

    private class ReportLine
    {
        public required string?           TypeOrMemberName { get;  init; }
        public required RefactorAttribute Attribute        { get;  init; }
        public          string?           ReflectedType    { get ; set ; }
        public          string?           Assembly         { get ; set ; }

        public override string ToString() =>
            $"Relative Benefit to fix: {Attribute.RelativeBenefitToFix:0.#} {Attribute.Description} {TypeOrMemberName} ({ReflectedType} - {Assembly}) Pain: {Attribute.PainEstimate} Effort to fix: {Attribute.HoursToResolve}";
    }
}

/// <summary>
/// </summary>
public class TechDebtPainExceededException : Exception
{
}
