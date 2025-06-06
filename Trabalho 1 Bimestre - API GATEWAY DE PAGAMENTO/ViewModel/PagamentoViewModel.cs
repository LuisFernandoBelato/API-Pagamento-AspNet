using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.ViewModel
{
    public class PagamentoViewModel
    {
        public double _ValorTotal { get; set; }
        public double _TaxaDeJuros { get; set; }
        public int _QtdeParcelas { get; set; }
        public string _Cartao { get; set; }
        public int _CVV { get; set; }
    }
}
