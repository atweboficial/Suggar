using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Ajuste
    {
        public int tipo { get; set; }
        public double valor { get; set; }
        public object observacao { get; set; }
        public string nome { get; set; }
    }

    public class Atributo
    {
        public string produtoVarianteAtributoValor { get; set; }
        public string produtoVarianteAtributoNome { get; set; }
    }

    public class CartaoCredito
    {
        public string numeroCartao { get; set; }
        public string nomeTitular { get; set; }
        public string dataValidade { get; set; }
        public object codigoSeguranca { get; set; }
        public object documentoCartaoCredito { get; set; }
        public object token { get; set; }
        public object info { get; set; }
        public string bandeira { get; set; }
    }

    public class CentroDistribuicao
    {
        public int centroDistribuicaoId { get; set; }
        public int quantidade { get; set; }
        public int situacaoProdutoId { get; set; }
        public double valorFreteEmpresa { get; set; }
        public double valorFreteCliente { get; set; }
    }

    public class CentrosDistribuicao
    {
        public int freteContratoId { get; set; }
        public string freteContrato { get; set; }
        public double valorFreteEmpresa { get; set; }
        public double valorFreteCliente { get; set; }
        public double peso { get; set; }
        public double pesoCobrado { get; set; }
        public double volume { get; set; }
        public double volumeCobrado { get; set; }
        public int prazoEnvio { get; set; }
        public object prazoHorasEnvio { get; set; }
        public string prazoEnvioTexto { get; set; }
        public int centroDistribuicaoId { get; set; }
        public List<object> cotacoesFilhas { get; set; }
    }

    public class Frete
    {
        public int freteContratoId { get; set; }
        public string freteContrato { get; set; }
        public string referenciaConector { get; set; }
        public double valorFreteEmpresa { get; set; }
        public double valorFreteCliente { get; set; }
        public double peso { get; set; }
        public double pesoCobrado { get; set; }
        public double volume { get; set; }
        public double volumeCobrado { get; set; }
        public int prazoEnvio { get; set; }
        public object prazoHorasEnvio { get; set; }
        public string prazoEnvioTexto { get; set; }
        public int retiradaLojaId { get; set; }
        public List<CentrosDistribuicao> centrosDistribuicao { get; set; }
        public object servico { get; set; }
        public object retiradaAgendada { get; set; }
        public object agendamento { get; set; }
        public List<InformacoesAdicionai> informacoesAdicionais { get; set; }
        public object grupoFreteId { get; set; }
        public object grupoFreteNome { get; set; }
    }

    public class InformacoesAdicionai
    {
        public string chave { get; set; }
        public object valor { get; set; }
    }

    public class Integrador
    {
        public string nome { get; set; }
        public string pedidoId { get; set; }
        public string pedidoUrl { get; set; }
    }

    public class Iten
    {
        public int produtoVarianteId { get; set; }
        public string sku { get; set; }
        public object ean { get; set; }
        public string nome { get; set; }
        public int quantidade { get; set; }
        public double precoCusto { get; set; }
        public double precoVenda { get; set; }
        public double valorItem { get; set; }
        public double valorItemArredondado { get; set; }
        public bool isBrinde { get; set; }
        public double valorAliquota { get; set; }
        public bool isMarketPlace { get; set; }
        public double precoPor { get; set; }
        public double desconto { get; set; }
        public Totais totais { get; set; }
        public List<Ajuste> ajustes { get; set; }
        public List<CentroDistribuicao> centroDistribuicao { get; set; }
        public List<object> valoresAdicionais { get; set; }
        public List<Atributo> atributos { get; set; }
        public List<object> embalagens { get; set; }
        public List<object> personalizacoes { get; set; }
        public List<object> frete { get; set; }
        public object dadosProdutoEvento { get; set; }
        public List<object> formulas { get; set; }
        public object seller { get; set; }
        public List<object> metadados { get; set; }
        public object situacaoPedidoProdutoId { get; set; }
        public List<object> imagens { get; set; }
        public List<object> categorias { get; set; }
        public int etiqueta { get; set; }
        public string etiquetaDescricao { get; set; }
        public double peso { get; set; }
        public double altura { get; set; }
        public double largura { get; set; }
        public double comprimento { get; set; }
    }

    public class Metadado
    {
        public string chave { get; set; }
        public string valor { get; set; }
    }

    public class Observacao
    {
        public string observacao { get; set; }
        public string usuario { get; set; }
        public DateTime data { get; set; }
        public bool publica { get; set; }
    }

    public class Omnichannel
    {
        public string pedidoIdPublico { get; set; }
        public string pedidoIdPrivado { get; set; }
        public Integrador integrador { get; set; }
    }

    public class Pagamento
    {
        public int formaPagamentoId { get; set; }
        public int numeroParcelas { get; set; }
        public double valorParcela { get; set; }
        public double valorDesconto { get; set; }
        public double valorJuros { get; set; }
        public double valorTotal { get; set; }
        public object boleto { get; set; }
        public object pix { get; set; }
        public List<CartaoCredito> cartaoCredito { get; set; }
        public List<PagamentoStatus> pagamentoStatus { get; set; }
        public List<InformacoesAdicionai> informacoesAdicionais { get; set; }
    }

    public class PagamentoStatus
    {
        public object numeroAutorizacao { get; set; }
        public string numeroComprovanteVenda { get; set; }
        public DateTime dataAtualizacao { get; set; }
        public DateTime dataUltimoStatus { get; set; }
        public string adquirente { get; set; }
        public string tid { get; set; }
    }

    public class PedidoEndereco
    {
        public string tipo { get; set; } 
        public string nome { get; set; }
        public string endereco { get; set; }
        public string numero { get; set; }
        public string complemento { get; set; }
        public string referencia { get; set; }
        public string cep { get; set; }
        public string tipoLogradouro { get; set; }
        public string logradouro { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string pais { get; set; }
    }

    public class OrderList
    {
        public int pedidoId { get; set; }
        public int situacaoPedidoId { get; set; }
        public string tipoRastreamentoPedido { get; set; }
        public int transacaoId { get; set; }
        public DateTime data { get; set; }
        public DateTime? dataPagamento { get; set; }
        public DateTime dataUltimaAtualizacao { get; set; }
        public double valorFrete { get; set; }
        public double valorTotalPedido { get; set; }
        public double valorDesconto { get; set; }
        public double valorDebitoCC { get; set; }
        public string cupomDesconto { get; set; }
        public string marketPlacePedidoId { get; set; }
        public string marketPlacePedidoSiteId { get; set; }
        public int canalId { get; set; }
        public string canalNome { get; set; }
        public string canalOrigem { get; set; }
        public int retiradaLojaId { get; set; }
        public bool isPedidoEvento { get; set; }
        public Usuario usuario { get; set; }
        public List<PedidoEndereco> pedidoEndereco { get; set; }
        public Frete frete { get; set; }
        public List<Iten> itens { get; set; }
        public List<object> assinatura { get; set; }
        public List<Pagamento> pagamento { get; set; }
        public List<Observacao> observacao { get; set; }
        public double valorCreditoFidelidade { get; set; }
        public bool valido { get; set; }
        public double valorSubTotalSemDescontos { get; set; }
        public List<object> pedidoSplit { get; set; }
        public object usuarioMasterId { get; set; }
        public List<Metadado> metadados { get; set; }
        public int transacaoPaiId { get; set; }
        public List<object> detalhesContaCorrente { get; set; }
        public object pedidoPai { get; set; }
        public bool isChangeOrder { get; set; }
        public bool primeiraCompra { get; set; }
        public string identificador { get; set; }
        public Omnichannel omnichannel { get; set; }
    }

    public class Totais
    {
        public double precoCusto { get; set; }
        public double precoVenda { get; set; }
        public double precoPor { get; set; }
        public double desconto { get; set; }
    }

    public class Usuario
    {
        public int usuarioId { get; set; }
        public object grupoInformacaoCadastral { get; set; }
        public string tipoPessoa { get; set; }
        public string origemContato { get; set; }
        public string tipoSexo { get; set; }
        public string nome { get; set; }
        public string cpf { get; set; }
        public string email { get; set; }
        public string rg { get; set; }
        public string telefoneResidencial { get; set; }
        public string telefoneCelular { get; set; }
        public string telefoneComercial { get; set; }
        public DateTime dataNascimento { get; set; }
        public string razaoSocial { get; set; }
        public string cnpj { get; set; }
        public string inscricaoEstadual { get; set; }
        public string responsavel { get; set; }
        public DateTime dataCriacao { get; set; }
        public DateTime dataAtualizacao { get; set; }
        public bool revendedor { get; set; }
        public object listaInformacaoCadastral { get; set; }
    }


}
