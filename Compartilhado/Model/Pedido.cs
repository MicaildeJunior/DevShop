using Amazon.DynamoDBv2.DataModel;

namespace Compartilhado.Model;

public enum StatusPedido
{
    Coletado,
    Pago,
    Faturado
}

[DynamoDBTable("pedidos")]
public class Pedido
{
    [DynamoDBHashKey("Id")]
    public string Id { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataDeCriacao { get; set; }
    public List<Produto>? Produtos { get; set; }
    public Cliente? Cliente { get; set; }
    public Pagamento? Pagamento { get; set; }
    public string JustificativaDeCancelamento { get; set; } = string.Empty;
    public StatusPedido Status { get; set; }
    public bool Cancelado { get; set; }      
}
