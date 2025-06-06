using MySql.Data.MySqlClient;
using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Domain;
using Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.ViewModel;

namespace Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.Services
{
    public class PagamentoService
    {
        private readonly ILogger<PagamentoService> _logger;
        private readonly Trabalho_1_Bimestre___API_GATEWAY_DE_PAGAMENTO.BD _bd;
        public PagamentoService(ILogger<PagamentoService> logger, BD bd)
        {
            _logger = logger;
            _bd = bd;
        }

        public List<Parcela> CalcularParcelas (double ValorTotal, double TaxaDeJuros, int QtdeParcelas)
        {
            List<Parcela> parcelas = new List<Parcela>();
            
            double valorParcela = (ValorTotal * TaxaDeJuros) / QtdeParcelas;

            for (int i = 0; i < QtdeParcelas; i++)
                parcelas.Add(new Parcela(i+1,valorParcela));

            return parcelas;
        }

        public int GravarPagamento (Pagamento pagamento)
        {
            bool sucesso = false;
            int id;
            MySqlConnection conexao = _bd.CriarConexao();
            try
            {
                conexao.Open();

                MySqlCommand cmd = conexao.CreateCommand();

                cmd.CommandText = @"INSERT INTO Transacao (Valor, Cartao, CVV, Parcelas, Situacao)
                                  VALUES (@Valor, @Cartao, @CVV, @Parcelas, @Situacao)";

                cmd.Parameters.AddWithValue("@Valor", pagamento._ValorTotal);
                cmd.Parameters.AddWithValue("@Cartao", pagamento._Cartao);
                cmd.Parameters.AddWithValue("@CVV", pagamento._CVV);
                cmd.Parameters.AddWithValue("@Parcelas", pagamento._QtdeParcelas);
                cmd.Parameters.AddWithValue("@Situacao", pagamento._Situacao);

                cmd.ExecuteNonQuery();

                id = (int) cmd.LastInsertedId;
                sucesso = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Criar uma Nova Transacao");
                throw new Exception(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            if (sucesso)
                return id;
            return -1;
        }

        public Situacao ConsultaSituacaoPagamento (int id)
        {
            bool sucesso = false;
            Situacao situacao = 0;
            MySqlConnection conexao = _bd.CriarConexao();
            try
            {
                conexao.Open();

                MySqlCommand cmd = conexao.CreateCommand();

                cmd.CommandText = "SELECT Situacao FROM Transacao WHERE TransacaoId = @id";

                cmd.Parameters.AddWithValue("@id", id);

                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    situacao = (Situacao) Convert.ToUInt16(dr["Situacao"]);
                    sucesso = true;
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Consultar a Situacao da Transacao - ID: " + id);
                throw new Exception(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            if (sucesso)
                return situacao;
            return 0;
        }

        public bool ConfirmarPagamento (int id)
        {
            bool sucesso = false;
            short situacao;
            MySqlConnection conexao = _bd.CriarConexao();
            try
            {
                conexao.Open();

                MySqlCommand cmd = conexao.CreateCommand();

                cmd.CommandText = "SELECT Situacao FROM Transacao WHERE TransacaoId = @id";
                cmd.Parameters.AddWithValue("@id", id);

                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    situacao = (short) dr["Situacao"];
                    if (situacao != 3)
                    {
                        dr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Transacao SET Situacao = 2 WHERE TransacaoId = @id";
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                        sucesso = true;
                    }
                    else
                        throw new Exception("Impossível Confirmar - Transação já Cancelada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Consultar a Situacao da Transacao - ID: " + id + "\t" + ex.Message);
                throw new Exception(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            return sucesso;
        }


        public bool CancelarPagamento (int id)
        {
            bool sucesso = false;
            short situacao;
            MySqlConnection conexao = _bd.CriarConexao();
            try
            {
                conexao.Open();

                MySqlCommand cmd = conexao.CreateCommand();

                cmd.CommandText = "SELECT Situacao FROM Transacao WHERE TransacaoId = @id";
                cmd.Parameters.AddWithValue("@id", id);

                var dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    situacao = (short) dr["Situacao"];
                    if (situacao != 2)
                    {
                        dr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Transacao SET Situacao = 3 WHERE TransacaoId = @id";
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                        sucesso = true;
                    }
                    else
                        throw new Exception("Impossível Cancelar - Transação já Confirmada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro Consultar a Situacao da Transacao - ID: " + id + "\t" + ex.Message);
                throw new Exception(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            return sucesso;
        }
    }
}
