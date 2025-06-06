namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain
{
    public class Parcela
    {
        public int _Parcela { get; set; }
        public double _Valor { get; set; }

        public Parcela(int Parcela, double Valor)
        {
            _Parcela = Parcela;
            _Valor = Valor;
        }
    }
}
