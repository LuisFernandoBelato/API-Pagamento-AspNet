using MySql.Data.MySqlClient;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO
{
    public class BD
    {
        public MySql.Data.MySqlClient.MySqlConnection CriarConexao()
        {
            string strCon = Environment.GetEnvironmentVariable("STRING_CONEXAO");
            MySqlConnection conexao = new MySqlConnection(strCon);
            return conexao;
        }
    }
}
