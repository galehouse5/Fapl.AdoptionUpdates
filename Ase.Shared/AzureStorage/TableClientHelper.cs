using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ase.Shared.AzureStorage
{
    public static class TableClientHelper
    {
        public static async Task<IList<DynamicTableEntity>> ExecuteQueryAsync(this CloudTable table, TableQuery query)
        {
            TableContinuationToken token = null;
            var results = new List<DynamicTableEntity>();

            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(segment);
                token = segment.ContinuationToken;
            }
            while (token != null);

            return results;
        }
    }
}
