using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Controllers
{
    /// <summary>
    /// Rotas - Cartões
    /// </summary>
    [Authorize("APIAuth")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartoesController : ControllerBase
    {
        private readonly Services.CartaoService _cartaoService;
        public CartoesController(Services.CartaoService cartaoService) 
        {
            _cartaoService = cartaoService;
        }

        /// <summary>
        /// Retorna qual é a bandeira do cartão pelo número
        /// </summary>
        /// <param name="cartao">Nº. Cartão</param>
        [HttpGet]
        [Route("/cartoes/{cartao}/obter-bandeira")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObterBandeira (string cartao)
        {
            cartao = cartao.Replace("-", "");
            if (cartao.Substring(0, 4).Equals("1111") && cartao[8].Equals('1'))
                return StatusCode(200, "VISA");
            if (cartao.Substring(0, 4).Equals("2222") && cartao[8].Equals('2'))
                return StatusCode(200, "MASTERCARD");
            if (cartao.Substring(0, 4).Equals("3333") && cartao[8].Equals('3'))
                return StatusCode(200, "ELO");

            return StatusCode(404, "Bandeira Inválida ou Não Encontrada");
        }


        /// <summary>
        /// Retorna se o cartão é válido pelo número
        /// </summary>
        /// <param name="cartao">Nº. Cartão</param>
        [HttpGet]
        [Route("/cartoes/{cartao}/valido")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ValidaCartao (string cartao)
        {
            cartao = cartao.Replace("-", "");
            try
            {
                if (_cartaoService.ValidarCartao(cartao))
                    return Ok(true);
            }
            catch
            {
                return BadRequest("Cartão não Encontrado ou Erro na Busca");
            }
            return BadRequest("Cartão Inválido");
        }
    }
}
