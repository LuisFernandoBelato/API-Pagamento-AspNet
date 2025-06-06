using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain;
using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.ViewModel;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Controllers
{
    /// <summary>
    /// Rotas - Pagamentos
    /// </summary>
    [Authorize("APIAuth")]
    [Route("api/[controller]")]
    [ApiController]
    public class PagamentosController : ControllerBase
    {
        private readonly Services.PagamentoService _pagamentoService;
        private readonly Services.CartaoService _cartaoService;
        public PagamentosController (Services.PagamentoService pagamentoService, Services.CartaoService cartaoService)
        {
            _pagamentoService = pagamentoService;
            _cartaoService = cartaoService;
        }


        /// <summary>
        /// Calcula o valor das parcelas de uma Transação com base no Valor Total, Quantidade de Parcelas e Taxa de Juros
        /// </summary>
        /// <param name="pagto">Transação</param>
        [HttpPost]
        [Route("/pagamentos/calcular-parcelas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CalculaParcelas (CalculodeParcelasViewModel pagto)
        {
            if (pagto._ValorTotal < 0 || pagto._TaxaDeJuros < 0 || pagto._QtdeParcelas < 0)
                return StatusCode(400,"Valores Negativos são Inválidos");

            List<Parcela> parcelas = _pagamentoService.CalcularParcelas(
                                                        pagto._ValorTotal, 
                                                        pagto._TaxaDeJuros, 
                                                        pagto._QtdeParcelas);

            return StatusCode(200,parcelas);
        }


        /// <summary>
        /// Gera a Transação conforme os dados repassados no Corpo da Requisição
        /// </summary>
        /// <param name="pagto">Transação</param>
        [HttpPost]
        [Route("/pagamentos")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Pagamentos (PagamentoViewModel pagto)
        {
            bool cartaoValido = false;

            try
            {
                if (_cartaoService.ValidarCartao(pagto._Cartao))
                    cartaoValido = true;
            }
            catch (Exception ex)
            {
                cartaoValido = false;
                return BadRequest(ex.Message);
            }
            if (!cartaoValido)
                return BadRequest("Cartão Inválido");


            int idTransacao = -1;
            bool pagamentoGravado = false;

            Pagamento pagamento = new Pagamento();
            pagamento._ValorTotal = pagto._ValorTotal;
            pagamento._Cartao = pagto._Cartao;
            pagamento._TaxaDeJuros = pagto._TaxaDeJuros;
            pagamento._CVV = pagto._CVV.ToString();
            pagamento._Situacao = (Situacao) 1;

            try
            {
                idTransacao = _pagamentoService.GravarPagamento(pagamento);
                if (idTransacao != -1)
                    pagamentoGravado = true;
            }
            catch (Exception ex)
            {
                pagamentoGravado = false;
                return BadRequest(ex.Message);
            }
            if (!pagamentoGravado)
                return BadRequest("Transação não Concluida");

            var retorno = new {message = "Transação Concluída", id = idTransacao};
            return StatusCode(201, retorno);
        }


        /// <summary>
        /// Retorna a situação de uma Transação (PENDENTE (1), CONFIRMADO (2) e CANCELADO (3))
        /// </summary>
        /// <param name="id">ID da Transação</param>
        [HttpGet]
        [Route("/pagamentos/{id}/situacao")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SituacaoPagamento (int id)
        {
            bool sucesso = false;
            Situacao situacao;
            try
            {
                situacao = _pagamentoService.ConsultaSituacaoPagamento(id);
                if (situacao != 0)
                    sucesso = true;
            }
            catch (Exception ex) 
            {
                sucesso = false;
                return BadRequest(ex.Message);
            }

            if (sucesso)
            {
                var retorno = new { tipo = situacao, status = situacao.ToString()};
                return StatusCode(200, retorno);
            }
            return BadRequest("Consulta Não Concluida");

        }

        /// <summary>
        /// Altera a situação da Transação para "Confirmado"
        /// </summary>
        /// <param name="id">ID da Transação</param>
        [HttpPut]
        [Route("/pagamentos/{id}/confirmar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ConfirmarPagamento (int id)
        {
            bool sucesso = false;
            try
            {
                if (_pagamentoService.ConfirmarPagamento(id))
                    sucesso = true;
            }
            catch (Exception ex)
            {
                sucesso = false;
                return BadRequest(ex.Message);
            }

            if (sucesso)
                return StatusCode(200,"Pagamento Confirmado com Sucesso");
            return BadRequest("Consulta Não Concluida");
        }


        /// <summary>
        /// Altera a situação da Transação para "Cancelado"
        /// </summary>
        /// <param name="id">ID da Transação</param>
        [HttpPut]
        [Route("/pagamentos/{id}/cancelar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CancelarPagamento (int id)
        {
            bool sucesso = false;
            try
            {
                if (_pagamentoService.CancelarPagamento(id))
                    sucesso = true;
            }
            catch (Exception ex)
            {
                sucesso = false;
                return BadRequest(ex.Message);
            }

            if (sucesso)
                return StatusCode(200, "Pagamento Cancelado com Sucesso");
            return BadRequest("Consulta Não Concluida");
        }
    }
}
