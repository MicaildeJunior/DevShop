using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Compartilhado;
using Compartilhado.Enuns;
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
                    await ProcessarValorDoPedido(pedido, context);
                    await AmazonUtil.EnviarParaFila(EnumFilasSQS.pedido, pedido);
                    context.Logger.LogLine($"Sucesso na coleta do pedido: '{pedido.Id}'");

                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Erro: '{ex.Message}'");
                    pedido.JustificativaDeCancelamento = ex.Message;
                    pedido.Cancelado = true;
                    await AmazonUtil.EnviarParaFila(EnumFilasSNS.falha, pedido);
                }

                await pedido.SalvarAsync();
            }                       
        }
    }

    private static async Task ProcessarValorDoPedido(Pedido pedido, ILambdaContext context)
    {
        foreach (var produto in pedido.Produtos!)
        {
            context.Logger.LogLine($"Buscando produto com Id='{produto.Id}' na tabela estoque");

            var produtoDoEstoque = await ObterProdutoDoDynamoDbAsync(produto.Id)
                ?? throw new InvalidOperationException($"O produto não foi encontrado na tabela estoque: { produto.Nome}");

            produto.Nome = produtoDoEstoque.Nome;
            produto.Valor = produtoDoEstoque.Valor;

        }
        
        var valorTotal = pedido.Produtos.Sum(p => p.Valor * p.Quantidade);

        if (pedido.ValorTotal > 0m && Math.Round(pedido.ValorTotal, 2) != Math.Round(valorTotal, 2))
            throw new InvalidOperationException(
                $"Valor do pedido divergente. Enviado: R$ {pedido.ValorTotal:F2}, Calculado: R$ {valorTotal:F2}");

        pedido.ValorTotal = Math.Round(valorTotal, 2);
    }    

    private static async Task<Produto?> ObterProdutoDoDynamoDbAsync(string id)
    {
        var client = new AmazonDynamoDBClient(Amazon.RegionEndpoint.SAEast1);

        var request = new GetItemRequest
        {
            TableName = "estoque",
            ConsistentRead = true,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id?.Trim() }
            }
        };

        var response = await client.GetItemAsync(request);
        if (response.Item == null || response.Item.Count == 0)
            return null;

        return response.Item.ToObject<Produto>();
    }

}