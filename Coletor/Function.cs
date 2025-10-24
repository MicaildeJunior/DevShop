using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Compartilhado;
using Compartilhado.Extensions;
using Compartilhado.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Coletor;

public class Function
{
    public static async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        foreach (var record in dynamoEvent.Records)
        {
            if(record.EventName == "INSERT")
            {
                var pedido = record.Dynamodb.NewImage.ToObject<Pedido>();
                pedido.Status = StatusPedido.Coletado;
                try
                {
                    await ProcessarValorDoPedido(pedido);

                }
                catch (Exception ex)
                {
                    pedido.JustificativaDeCancelamento = ex.Message;
                    pedido.Cancelado = true;
                    // Adicionar fila de falha
                }

                await pedido.SalvarAsync();
            }                       
        }
    }

    private static async Task ProcessarValorDoPedido(Pedido pedido)
    {
        foreach (var produto in pedido.Produtos!)
        {
            var produtoDoEstoque = await ObterProdutoDoBynamoDbAsync(produto.Id)
                ?? throw new InvalidOperationException($"O produto não encontrado na tabela estoque { produto.Nome}");

            produto.Valor = produtoDoEstoque.Valor;
            produto.Nome = produtoDoEstoque.Nome;

            var valorTotal = pedido.Produtos.Sum(p => p.Valor * p.Quantidade);
            if (pedido.ValorTotal != 0 && pedido.ValorTotal != valorTotal)
                throw new InvalidOperationException($"O valor esperado do pedido é de R$ {pedido.ValorTotal}");

            pedido.ValorTotal = valorTotal;            
        }
    }

    private static async Task<Produto?> ObterProdutoDoBynamoDbAsync(string id)
    {
        var client = new AmazonDynamoDBClient();

        var request = new QueryRequest
        {
            TableName = "estoque",
            KeyConditionExpression = "Id = :v_id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { "v_id" , new AttributeValue { S = id } } } 
        };

        var response = await client.QueryAsync(request);
        var item = response.Items.FirstOrDefault();

        if (item is null)
            return null;

        return item.ToObject<Produto?>();
    }
}