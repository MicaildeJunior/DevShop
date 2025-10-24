using Compartilhado.Model;
using Microsoft.AspNetCore.Mvc;
using Compartilhado;

namespace Cadastrador.Controllers;

[Route("api/[controller]")]
public class PedidoController : ControllerBase
{
    [HttpPost]
    public async Task PostAsync([FromBody] Pedido pedido)
    {
        pedido.Id = Guid.NewGuid().ToString();
        pedido.DataDeCriacao = DateTime.Now;

        await pedido.SalvarAsync();

        Console.WriteLine($"Pedido salvo com suceddo Id: {pedido.Id}");
    }
}
