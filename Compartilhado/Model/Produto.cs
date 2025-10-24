namespace Compartilhado.Model;

public class Produto
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
    public bool Reservado { get; set; }
}
