using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.ViewModel;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain
{
    public class Pagamento
    {
        public double _ValorTotal { get; set; }
        public double _TaxaDeJuros { get; set; }
        public int _QtdeParcelas { get; set; }
        public string _Cartao { get; set; }
        public string _CVV { get; set; }
        public Situacao _Situacao { get; set; }
    }

    public enum Situacao
    {
        PENDENTE = 1,
        CONFIRMADO = 2,
        CANCELADO = 3
    }

}
