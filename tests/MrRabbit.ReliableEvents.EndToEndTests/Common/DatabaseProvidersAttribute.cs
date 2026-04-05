using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace MrRabbit.ReliableEvents.EndToEndTests.Common;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class DatabaseProvidersAttribute : DataAttribute
{
    private readonly object?[]? _data;

    public DatabaseProvidersAttribute(params object?[]? data)
    {
        _data = data;
    }

    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker) =>
        new(CreateTheoryDataRows(DatabaseProvider.SQLite, testMethod).Concat(CreateTheoryDataRows(DatabaseProvider.MsSql, testMethod)).ToArray());

    private TheoryDataRow[] CreateTheoryDataRows(DatabaseProvider provider, MethodInfo testMethod)
    {
        if (_data == null || _data.Length == 0)
            return new[]
            {
                new TheoryDataRow(new object[] { provider })
                {
                    TestDisplayName = $"{provider.ToString()} ",
                }
            };

        return new[]
        {
            new TheoryDataRow([provider, .._data ])
            {
                TestDisplayName = $"{provider.ToString()} "
            }
        };
    }

    public override bool SupportsDiscoveryEnumeration() => true;
}
