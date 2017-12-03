using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fapl.AdoptionUpdates.Shared.AzureStorage
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
                    { "PetfinderID",  new EntityProperty(mapping.PetfinderID) },
                    { "PetPointReferenceNumbers", new EntityProperty(mapping.PetPointReferenceNumbers) }
                }
            };

        public async Task Upsert(IEnumerable<PetIDMapping> mappings)
        {
            await table.CreateIfNotExistsAsync();

            // Inserting duplicate PetPoint reference numbers in the same batch causes an exception because
            // it's the row key, so must be unique. Just simulate the behavior of the last row overwritting
            // previous rows by discarding all duplicates except for the last.
            mappings = mappings
                .GroupBy(m => m.PetPointReferenceNumber)
                .Select(g => g.Last())
                .ToArray();

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
