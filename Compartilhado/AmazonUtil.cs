using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Compartilhado.Model;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Compartilhado.Enuns;
using Amazon.SQS;
using Amazon;
using Amazon.SQS.Model;
using Newtonsoft.Json;

namespace Compartilhado;

public static class AmazonUtil
{
    public static async Task SalvarAsync(this Pedido pedido)
    {		
        var client = new AmazonDynamoDBClient();
		var context = new DynamoDBContext(client);
        await context.SaveAsync(pedido);		       
    }

    public static T ToObject<T>(this Dictionary<string, AttributeValue> dictionary)
    {
        var client = new AmazonDynamoDBClient();
        var context = new DynamoDBContext(client);

        var doc = Document.FromAttributeMap(dictionary);
        return context.FromDocument<T>(doc);
    }

    public static async Task EnviarParaFila(EnumFilasSQS fila, Pedido pedido)
    {
        var json = JsonConvert.SerializeObject(pedido);
        var client = new AmazonSQSClient(RegionEndpoint.SAEast1);
        var request = new SendMessageRequest
        {
            QueueUrl = $"https://sqs.sa-east-1.amazonaws.com/884957621427/{fila}",
            MessageBody = json
        };

        await client.SendMessageAsync(request);
    }

    public static async Task EnviarParaFila(EnumFilasSNS fila, Pedido pedido)
    {
        // Implementar depois
        await Task.CompletedTask;
    }
}
