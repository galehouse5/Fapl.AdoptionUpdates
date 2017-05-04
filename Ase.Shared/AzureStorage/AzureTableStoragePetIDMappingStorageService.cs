using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ase.Shared.AzureStorage
{
    public class AzureTableStoragePetIDMappingStorageService : IPetIDMappingStorageService
    {
        private string petfinderShelterID;
        private CloudTable table;

        public AzureTableStoragePetIDMappingStorageService(string petfinderShelterID, string connectionString, string tableName)
        {
            this.petfinderShelterID = petfinderShelterID;

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudTableClient client = account.CreateCloudTableClient();
            table = client.GetTableReference(tableName);
        }

        protected PetIDMapping CreateMapping(DynamicTableEntity entity)
            => new PetIDMapping
            {
                PetPointReferenceNumber = entity.RowKey,
                PetfinderID = entity.Properties["PetfinderID"].Int32Value.Value
            };

        public async Task<IReadOnlyCollection<PetIDMapping>> GetMappings(IEnumerable<string> petPointReferenceNumbers)
        {
            await table.CreateIfNotExistsAsync();

            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, petfinderShelterID);
            string rowKeyFilter = petPointReferenceNumbers
                .Select(n => TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, n))
                .Aggregate((c1, c2) => TableQuery.CombineFilters(c1, TableOperators.Or, c2));
            TableQuery query = new TableQuery()
                .Where(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter));

            var results = await table.ExecuteQueryAsync(query);
            return results.Select(CreateMapping).ToArray();
        }

        protected DynamicTableEntity CreateTableEntity(PetIDMapping mapping)
            => new DynamicTableEntity(petfinderShelterID, mapping.PetPointReferenceNumber)
            {
                Properties = new Dictionary<string, EntityProperty>
                {
                    { "PetfinderID",  new EntityProperty(mapping.PetfinderID) }
                }
            };

        public async Task Upsert(IEnumerable<PetIDMapping> mappings)
        {
            await table.CreateIfNotExistsAsync();

            while (mappings.Any())
            {
                TableBatchOperation batch = new TableBatchOperation();

                foreach (PetIDMapping mapping in mappings.Take(100))
                {
                    ITableEntity entity = CreateTableEntity(mapping);
                    batch.Add(TableOperation.InsertOrMerge(entity));
                }

                await table.ExecuteBatchAsync(batch);

                mappings = mappings.Skip(100);
            }
        }
    }
}
