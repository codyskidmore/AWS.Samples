using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Customers.Api.Contracts.Data;

namespace Customers.Api.Repositories;

// NOTE!: Switch to CommandDefinition so the cancellation token can be passed to the query.
public class CustomerRepository : ICustomerRepository
{
    private readonly IAmazonDynamoDB _amazonDynamoDb;
    private readonly string _tableName = "aws-sample-nosqldb";
    
    public CustomerRepository(IAmazonDynamoDB amazonDynamoDb)
    {
        _amazonDynamoDb = amazonDynamoDb;
    }

    public async Task<bool> CreateAsync(CustomerDto customer, CancellationToken token)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        var customerAsJson = JsonSerializer.Serialize(customer);
        var customerAsAttributes = Document.FromJson(customerAsJson).ToAttributeMap();
        
        // DynamoDb doesn't have a "Create" concept. It uses PUT instead.
        var createItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = customerAsAttributes
        };

        var response = await _amazonDynamoDb.PutItemAsync(createItemRequest, token);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<CustomerDto?> GetAsync(Guid id, CancellationToken token)
    {
        var getItemRequest = new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"pk", new AttributeValue{ S = id.ToString()}},
                // Remember, we'd really use a value that guarantees
                // uniqueness between the primary key and the sort key
                {"sk", new AttributeValue{ S = id.ToString()}}
            }
        };
        // This is a point read operation, not a table scan which is why it scales.
        var response = await _amazonDynamoDb.GetItemAsync(getItemRequest, token);

        if (response.Item.Count == 0)
        {
            // There's a better way to do this in C# 8
            // while still not allocating memory.
            return null; 
        }

        // There's a mapper extension package for this in Nuget
        var itemAsDocument = Document.FromAttributeMap(response.Item);

        return JsonSerializer.Deserialize<CustomerDto>(itemAsDocument.ToJson());
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAsync(CustomerDto customer, CancellationToken token)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        var customerAsJson = JsonSerializer.Serialize(customer);
        var customerAsAttributes = Document.FromJson(customerAsJson).ToAttributeMap();
        
        var updateItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = customerAsAttributes
        };

        var response = await _amazonDynamoDb.PutItemAsync(updateItemRequest, token);
        return response.HttpStatusCode == HttpStatusCode.OK;    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token)
    {
        var deleteItemRequest = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {"pk", new AttributeValue{ S = id.ToString()}},
                // Remember, we'd really use a value that guarantees
                // uniqueness between the primary key and the sort key
                {"sk", new AttributeValue{ S = id.ToString()}}
            }
        };
        // This is a point read operation, not a table scan which is why it scales.
        var response = await _amazonDynamoDb.DeleteItemAsync(deleteItemRequest, token);

        return response.HttpStatusCode == HttpStatusCode.OK;    }
}
