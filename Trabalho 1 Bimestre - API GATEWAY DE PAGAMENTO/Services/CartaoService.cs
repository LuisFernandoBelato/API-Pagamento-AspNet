using MySql.Data.MySqlClient;
using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Services
{
    public class CartaoService
    {
        private readonly ILogger<CartaoService> _logger;
        private readonly Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.BD _bd;
        public CartaoService(ILogger<CartaoService> logger, BD bd) 
        {
            _logger = logger;
            _bd = bd;
        }

        public bool ValidarCartao (string cartao)
        {
            bool Existe;
            MySqlConnection conexao = _bd.CriarConexao();
            try
            {
                conexao.Open ();

                MySqlCommand cmd = conexao.CreateCommand ();

                cmd.CommandText = "SELECT Numero FROM Cartao C WHERE C.Numero = @Cartao AND C.Validade > SYSDATE();";

                cmd.Parameters.AddWithValue("@Cartao", cartao);

                cmd.ExecuteNonQuery ();

                var dr = cmd.ExecuteReader();

                if (dr.Read())
                    Existe = true;
                else
                    Existe = false;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Erro Ao Validar o Cartão");
                throw new Exception(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            return Existe;
        }
    }
}
