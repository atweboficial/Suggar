using Newtonsoft.Json;
using suggar_client.Models;
using suggar_client.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace suggar_client
{
    public partial class Form1 : Form
    {

        public class Global
        {
            public static string connectionStringName = "";
        }

        //Caso nao for informado periodo considera 'ontem'
        public DateTime dataI = DateTime.Today.AddDays(-1);
        public DateTime dataF = DateTime.Today.AddDays(-1);

        //Credenciais do WS
        public const string token = "sugga-9b5941a2-f9b2-40c7-bc37-02c7d19e0f14";
        //URL DOC: https://wakecommerce.readme.io/reference/retorna-uma-lista-de-pedido-na-ordem-decrescente-dentro-do-limite-de-datas-passadas

        Param Param = null;
        List<Param> Params = new List<Param>();

        string pathFile = "";
        private NotifyIcon TrayIcon;
        string mensagem;

        EmpresasModel Empresas;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            TrayIcon = new NotifyIcon();
            TrayIcon.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            TrayIcon.Visible = true;
            TrayIcon.Icon = this.Icon;
            Application.DoEvents();

            //Seta Connection String de acordo com o arquivo config.ini (que está na pasta bin\debug)
            string ArquivoIni = Environment.CurrentDirectory + @"\config.ini";
            System.Text.Encoding codutf = System.Text.Encoding.GetEncoding("iso-8859-15");
            StreamReader fluxoTexto = default(StreamReader);
            string linhaTexto = null;
            if (File.Exists(ArquivoIni))
            {
                fluxoTexto = new StreamReader(ArquivoIni, codutf);
                linhaTexto = fluxoTexto.ReadLine();
                while (!fluxoTexto.EndOfStream)
                {
                    if (linhaTexto.Trim() != "")
                    {

                        if (linhaTexto.Substring(0, 1) == "[")
                        {
                            //Pega nome do parametro
                            Param = new Param();
                            Param.chave = linhaTexto.Replace("[", "").Replace("]", "");
                        }
                        else
                        {
                            //Pega valor do parametro
                            if (Param != null)
                            {
                                Param.valor = linhaTexto;
                                Params.Add(Param);
                                Param = null;
                            }
                        }
                    }
                    //proxima linha do arquivo
                    linhaTexto = fluxoTexto.ReadLine();
                }
                fluxoTexto.Close();
            }
            else
            {
                MessageBox.Show("O arquivo config.ini não foi localizado.", "atweb_client", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            try
            {
                try
                {
                    dataI = Convert.ToDateTime(Params.FirstOrDefault(m => m.chave == "datainicial").valor);
                    dataF = Convert.ToDateTime(Params.FirstOrDefault(m => m.chave == "datafinal").valor);
                }
                catch { }

                //Processa -------------------------------------------------------------------------------

                Rotinas.Log("");
                Rotinas.Log("");
                Rotinas.Log("");
                Rotinas.Log("");
                Rotinas.Log("");
                Rotinas.Log("");
                Rotinas.Log("#### INICIO - " + DateTime.Now + " #############################################################################################################################################");
                Rotinas.Log("");
                Rotinas.Log("     Período: " + dataI.ToShortDateString() + " a " + dataF.ToShortDateString());
                Rotinas.Log("");

                //Busca dados da empresa via WS
                using (var client = new HttpClient())
                {
                    var conteudo = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("chaveEmpresa", Params.FirstOrDefault(m => m.chave == "chaveEmpresa").valor),
                        new KeyValuePair<string, string>("dadosBasicos", "Sim"),
                    });
                    var token = Encoding.ASCII.GetBytes("Atweb@services55968$:Atweb&APP%WSGH@12558#$");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(token));
                    var retWS = client.PostAsync("https://ws.atweb.top/WS/GetEmpresa", conteudo).Result;
                    string result = Rotinas.AcertaRetornoJson(retWS.Content.ReadAsStringAsync().Result);
                    Empresas = JsonConvert.DeserializeObject<EmpresasModel>(result);
                    Global.connectionStringName = Empresas.connectionstringname;
                }

                IntegracaoPedidosWake();
                AtualizacaoStatusPedidosWake();

            }
            catch (Exception e2)
            {
                Rotinas.Log(DateTime.Now + " - Erro - " + Rotinas.GetException(e2));
            }

            Rotinas.Log("-----------------------------------------------------------------------------------------------------------------------------");
            Rotinas.Log("");
            Rotinas.Log(mensagem);
            Rotinas.Log("");
            Rotinas.Log("#### FIM - " + DateTime.Now);
            Rotinas.Log("");
            Rotinas.Log("");
            Rotinas.Log("");

            //Envia log da  ltima execu  o para o(s) email(s) definidos no cadastro da empresa            
            try
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream, Encoding.UTF8))    // using UTF-8 encoding by default
                using (var mailClient = new SmtpClient("email-smtp.us-east-1.amazonaws.com"))
                using (var message = new MailMessage())
                {
                    bool gravaLine = false;
                    foreach (var line in System.IO.File.ReadLines(Environment.CurrentDirectory + @"\log.txt"))
                    {
                        if (line.Contains("#### INICIO - " + DateTime.Today.Date.ToShortDateString()) || gravaLine == true)
                        {
                            writer.WriteLine(line);
                            gravaLine = true;
                        }
                    }

                    writer.Flush();
                    stream.Position = 0;// read from the start of what was written

                    //Destinatarios                    
                    message.To.Add(new MailAddress("vinicius@atweb.com.br"));
                    message.To.Add(new MailAddress("enzo@atweb.com.br"));


                    //Comeplementa informa  es, anexa arquivo e envia email
                    message.From = new MailAddress("contato@atweb.top", "[ATweb]");
                    message.Subject = "[ATweb] Log " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                    message.Body = "Ver arquivo anexo.";
                    message.Attachments.Add(new Attachment(stream, "Log.txt", "text/plain"));
                    mailClient.EnableSsl = true;
                    mailClient.UseDefaultCredentials = false;
                    mailClient.Credentials = new System.Net.NetworkCredential("AKIAVESSLTG4VHJRKYZU", "BAxyljrwQ979FS3Sxqpo4SaE5Ugsu6aYclOzTIEEliM5");
                    mailClient.Port = 587;
                    mailClient.Send(message);
                }
            }
            catch (Exception ex)
            {
                Rotinas.Log(DateTime.Now + " Log não enviado: " + ex.Message);
            }

            TrayIcon.Visible = false;
            this.Close();
        }


        private void IntegracaoPedidosWake()
        {
            Rotinas.Log("-----------------------------------------------------------------------------------------------------------------------------");


            int pedidosIntegrados = 0;

            //Processa 5 dias pra trás
            DateTime dataIProcessar = dataI == dataF ? dataI.AddDays(-4) : dataI;
            DateTime dataFProcessar = dataF;
            //dataIProcessar = new DateTime(2026, 02, 15, 00, 00, 00); - Teste
            //dataFProcessar = new DateTime(2026, 02, 16, 00, 00, 00); - Teste

            //O processamento deve ser feito dia por dia, pois a Wake limita o retorno a no máximo 30 paginas
            do
            {
                dataFProcessar = dataIProcessar;

                Rotinas.Log("Início Integração Pedidos de " + dataIProcessar + " até " + dataFProcessar);

                string dataIFormatada = dataIProcessar.ToString("yyyy-MM-dd") + " 00:00:00";
                string dataFFormatada = dataFProcessar.ToString("yyyy-MM-dd") + " 23:59:59";
                string etapa = "";

                try
                {

                    string baseUrl = "";
                    string token = "";

                    //Caso tenha mais de 1 empresa já esta implementado o foreach
                    for (int empresa = 1; empresa <= 1; empresa++)
                    {

                        //Define configurações e credencias para a API de cada empresa na Wake
                        if (empresa == 1)
                        {
                            //Suggar
                            baseUrl = "https://api.fbits.net/pedidos";
                            token = "sugga-9b5941a2-f9b2-40c7-bc37-02c7d19e0f14";
                        }
                        else if (empresa == 2)
                        {
                            ////Outras empresas
                            //baseUrl = "...";                            
                            //token = "...";                            
                        }


                        //Lista paginada
                        int pages = 99999;
                        for (int page = 1; page < pages; page++)
                        {
                            etapa = "Consulta ao endpoint de lista de pedidos Wake";
                            var client = new HttpClient();
                            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + "?dataInicial=" + dataIFormatada + "&dataFinal=" + dataFFormatada + "&pagina=" + page);
                            request.Headers.Add("accept", "application/json");
                            request.Headers.Add("Authorization", "Basic " + token);
                            var json = client.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                            var obj = JsonConvert.DeserializeObject<List<OrderList>>(json);

                            Rotinas.Log("-----------------------------------------------------------------------------------------------------------------");
                            etapa = "Log de paginação Wake";
                            if (obj.Count == 0)
                            {
                                break;
                            }
                            else
                            {
                                Rotinas.Log("#### " + baseUrl + " ####  (" + dataIProcessar.Date.ToString("dd/MM/yyyy") + ")");
                                Rotinas.Log("Página " + page + " (máximo de 50 pedidos por página)");
                                Rotinas.Log("Total de pedidos retornados: " + obj.Count);
                            }

                            foreach (var PedidoEcom in obj)
                            {
                                bool passa = true;
                                //if (PedidoList.orderId != "1460620838070-01")
                                //{
                                //    passa = false;
                                //}

                                if (passa)
                                {
                                    Rotinas.Log("-----------------------------------------------------------------------------------------------------------------");
                                    Rotinas.Log("Integração pedido: " + PedidoEcom.pedidoId);

                                    try
                                    {
                                        using (Dados_atweb_50031 contexto = new Dados_atweb_50031())
                                        {
                                            Pedidos Pedidos = null;

                                            //Busca o pedido no ATweb
                                            etapa = "Consulta do pedido no ATweb";
                                            int pedidoid = contexto.Database.SqlQuery<int>("Select id from Pedidos Where numeropedidoexterno='" + PedidoEcom.pedidoId + "'").FirstOrDefault();
                                            if (pedidoid != 0)
                                            {
                                                ////Pedido ja existe no ATweb, Atualiza o link de rastreio caso possua
                                                //int updated = 0;
                                                //if (!string.IsNullOrEmpty(urlrastreio))
                                                //{
                                                //    updated = contexto.Database.ExecuteSqlCommand("UPDATE Pedidos SET urlrastreio='" + urlrastreio + "' WHERE id=" + pedidoid);
                                                //}

                                                Rotinas.Log("Pedido " + PedidoEcom.pedidoId + " já integrado.");
                                            }
                                            else
                                            {
                                                //Insere o pedido no ATweb

                                                int tipoparceiroid = 0;
                                                string cpfcnpj = "";
                                                string nome = "";
                                                string telefone = "";
                                                if (PedidoEcom.usuario.tipoPessoa == "Fisica")
                                                {
                                                    tipoparceiroid = 1; //Consumidor
                                                    cpfcnpj = Rotinas.DeixaNumerico(PedidoEcom.usuario.cpf);
                                                    nome = PedidoEcom.usuario.nome;
                                                    telefone = PedidoEcom.usuario.telefoneCelular;
                                                }
                                                else
                                                {
                                                    tipoparceiroid = 2; //Revendedor
                                                    cpfcnpj = Rotinas.DeixaNumerico(PedidoEcom.usuario.cnpj);
                                                    nome = !string.IsNullOrEmpty(PedidoEcom.usuario.nome) ? PedidoEcom.usuario.nome : PedidoEcom.usuario.razaoSocial;
                                                    telefone = PedidoEcom.usuario.telefoneCelular;
                                                }
                                                // B2B quando não for pessoa física; B2C quando tipoPessoa == "Fisica"
                                                bool isB2B = (PedidoEcom.usuario?.tipoPessoa != "Fisica");

                                                etapa = "Consulta ao endpoint do ATweb para buscar o parceiro";
                                                //Busca cadastro do consumidor via API
                                                var clientParceiro = new HttpClient();
                                                var requestParceiro = new HttpRequestMessage(HttpMethod.Get, "https://api.atweb.top/api/busca-parceiro/" + cpfcnpj);
                                                requestParceiro.Headers.Add("accept", "text/plain");
                                                requestParceiro.Headers.Add("Authorization", "Basic X2FwaSNzdWdnYXI6Y2E4OGMwNjdhZDI2OTFmMWYyMWY0OGJiODFkYjk2OWQ=");
                                                var jsonParceiro = clientParceiro.SendAsync(requestParceiro).Result.Content.ReadAsStringAsync().Result;
                                                var ParceiroATweb = JsonConvert.DeserializeObject<ParceiroATweb>(jsonParceiro);
                                                if (ParceiroATweb.data == null)
                                                {
                                                    //Tenta realizar o cadastro da pessoa 3x
                                                    for (int i = 0; i < 3; i++)
                                                    {

                                                        etapa = "Atribuição dos dados para cadastro do parceiro";
                                                        //Cadastra pessoa
                                                        ParceiroATweb.data = new Data();
                                                        ParceiroATweb.data.tipoparceiroid = tipoparceiroid;
                                                        ParceiroATweb.data.numeroidentificacao = cpfcnpj;
                                                        ParceiroATweb.data.nome = nome;
                                                        ParceiroATweb.data.razaosocial = nome;
                                                        ParceiroATweb.data.referencia = null;
                                                        ParceiroATweb.data.telefone = string.IsNullOrEmpty(telefone) ? "0" : telefone;
                                                        ParceiroATweb.data.telefone2 = null;
                                                        ParceiroATweb.data.telefone3 = null;
                                                        try
                                                        {

                                                            string nomeEmail = PedidoEcom.usuario.email.Split('@')[0];
                                                            string dominioEmail = PedidoEcom.usuario.email.Split('@')[1];
                                                            dominioEmail = "@" + dominioEmail.Split('-')[0];
                                                            ParceiroATweb.data.email = nomeEmail + dominioEmail;
                                                        }
                                                        catch { }
                                                        ParceiroATweb.data.rginscricaoestadual = null;
                                                        var enderecoEntrega = PedidoEcom.pedidoEndereco.FirstOrDefault(x => x.tipo == "Entrega");
                                                        if (enderecoEntrega == null)
                                                        {
                                                            enderecoEntrega = PedidoEcom.pedidoEndereco.FirstOrDefault();
                                                        }
                                                        if (enderecoEntrega != null)
                                                        {
                                                            ParceiroATweb.data.cep = enderecoEntrega.cep;
                                                            ParceiroATweb.data.endereco = enderecoEntrega.logradouro;
                                                            ParceiroATweb.data.numero = enderecoEntrega.numero;
                                                            ParceiroATweb.data.complemento = enderecoEntrega.complemento;
                                                            ParceiroATweb.data.bairro = enderecoEntrega.bairro;
                                                            ParceiroATweb.data.cidade = enderecoEntrega.cidade;
                                                            ParceiroATweb.data.estado = enderecoEntrega.pais;
                                                            ParceiroATweb.data.pais = enderecoEntrega.pais == "BRA" ? "Brasil" : enderecoEntrega.pais;
                                                        }
                                                        try
                                                        {
                                                            // Coordenadas não disponíveis no modelo OrderList/PedidoEndereco da Wake - lat/lng permanecem null
                                                            // Opção: geocodificar por CEP (ViaCEP, etc.) se precisar de coordenadas
                                                        }
                                                        catch { }

                                                        using (var clientCadParceiro = new HttpClient())
                                                        {
                                                            etapa = "Acesso ao endpoint ATweb de cadastro do parceiro";
                                                            jsonParceiro = JsonConvert.SerializeObject(ParceiroATweb.data);
                                                            var requestCadParceiro = new HttpRequestMessage(HttpMethod.Post, "https://api.atweb.top/api/cadastra-parceiro");
                                                            requestCadParceiro.Headers.Add("accept", "application/json"); // Alterado para application/json
                                                            requestCadParceiro.Headers.Add("Authorization", "Basic X2FwaSNzdWdnYXI6Y2E4OGMwNjdhZDI2OTFmMWYyMWY0OGJiODFkYjk2OWQ=");
                                                            var contentCadParceiro = new StringContent(jsonParceiro, System.Text.Encoding.UTF8, "application/json"); // Definindo Content-Type como application/json
                                                            requestCadParceiro.Content = contentCadParceiro;
                                                            var response = clientCadParceiro.SendAsync(requestCadParceiro).Result.Content.ReadAsStringAsync().Result;
                                                            ParceiroATweb = JsonConvert.DeserializeObject<ParceiroATweb>(response);

                                                            if (ParceiroATweb.data != null)
                                                            {
                                                                break;
                                                            }

                                                            System.Threading.Thread.Sleep(3000);

                                                            etapa = "Consulta ao endpoint do ATweb para buscar o parceiro (após o cadastro)";
                                                            //Busca (novamente) cadastro do consumidor via API
                                                            clientParceiro = new HttpClient();
                                                            requestParceiro = new HttpRequestMessage(HttpMethod.Get, "https://api.atweb.top/api/busca-parceiro/" + cpfcnpj);
                                                            requestParceiro.Headers.Add("accept", "text/plain");
                                                            requestParceiro.Headers.Add("Authorization", "Basic X2FwaSNzdWdnYXI6Y2E4OGMwNjdhZDI2OTFmMWYyMWY0OGJiODFkYjk2OWQ=");
                                                            jsonParceiro = clientParceiro.SendAsync(requestParceiro).Result.Content.ReadAsStringAsync().Result;
                                                            ParceiroATweb = JsonConvert.DeserializeObject<ParceiroATweb>(jsonParceiro);

                                                            if (ParceiroATweb.data != null)
                                                            {
                                                                break;
                                                            }

                                                            System.Threading.Thread.Sleep(5000);
                                                        }
                                                    }
                                                }

                                                if (ParceiroATweb.data != null)
                                                {
                                                    int? consumidorid = null;
                                                    int? entidadeid = null;

                                                    if (isB2B)
                                                    {
                                                        Entidades Entidades = contexto.Entidades.FirstOrDefault(m => m.cpfcnpj == cpfcnpj);
                                                        if (Entidades != null)
                                                        {
                                                            entidadeid = Entidades.id;

                                                            if (Entidades.disponivelmodulonegocios != "Sim")
                                                            {
                                                                //Disponibiliza a empresa no módulo de negócios
                                                                Entidades.disponivelmodulonegocios = "Sim";
                                                                contexto.Entry(Entidades).State = System.Data.Entity.EntityState.Modified;
                                                                contexto.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Consumidores Consumidores = contexto.Consumidores.FirstOrDefault(m => m.cpfcnpj == cpfcnpj);
                                                        if (Consumidores != null)
                                                        {
                                                            consumidorid = Consumidores.id;
                                                        }
                                                    }

                                                    etapa = "Consulta do consumidor/parceiro no ATweb";
                                                    //Cadastra pedido no ATweb

                                                    if (consumidorid != null || entidadeid != null)
                                                    {

                                                        etapa = "Busca de dados do retorno da Wake";
                                                        // API Wake já retorna valores em reais (double)
                                                        decimal valorFrete = 0;
                                                        try { valorFrete = Convert.ToDecimal(PedidoEcom.valorFrete); } catch { }

                                                        decimal valorDesconto = 0;
                                                        decimal percentualDesconto = 0;
                                                        try
                                                        {
                                                            valorDesconto = Convert.ToDecimal(PedidoEcom.valorDesconto);
                                                            if (valorDesconto > 0 && PedidoEcom.valorSubTotalSemDescontos > 0)
                                                            {
                                                                decimal totalItems = Convert.ToDecimal(PedidoEcom.valorSubTotalSemDescontos);
                                                                percentualDesconto = (valorDesconto / totalItems) * 100;
                                                            }
                                                        }
                                                        catch { }

                                                        string obs = "";
                                                        string transportador = "";
                                                        try
                                                        {
                                                            var logisticsInfo = PedidoEcom.frete?.informacoesAdicionais?.FirstOrDefault(x => string.Equals(x?.chave, "Forma de envio", StringComparison.OrdinalIgnoreCase));
                                                            if (logisticsInfo?.valor != null)
                                                                transportador = logisticsInfo.valor.ToString();
                                                            else
                                                                transportador = PedidoEcom.frete?.freteContrato ?? "";

                                                            if (!string.IsNullOrEmpty(PedidoEcom.frete?.prazoEnvioTexto))
                                                                obs = obs + "Estimativa de entrega: " + PedidoEcom.frete.prazoEnvioTexto + Environment.NewLine;
                                                        }
                                                        catch { }

                                                        // Busca URL de rastreio: GET pedidos/{pedidoId}/rastreamento
                                                        string urlrastreio = null;
                                                        try
                                                        {
                                                            var urlRastreio = baseUrl + "/" + PedidoEcom.pedidoId + "/rastreamento";
                                                            var clientRast = new HttpClient();
                                                            var requestRast = new HttpRequestMessage(HttpMethod.Get, urlRastreio);
                                                            requestRast.Headers.Add("accept", "application/json");
                                                            requestRast.Headers.Add("Authorization", "Basic " + token);
                                                            var jsonRast = clientRast.SendAsync(requestRast).Result.Content.ReadAsStringAsync().Result;
                                                            var rast = JsonConvert.DeserializeObject<PedidoRastreamentoResponse>(jsonRast);
                                                            if (!string.IsNullOrEmpty(rast?.urlRastreamento))
                                                                urlrastreio = rast.urlRastreamento;
                                                        }
                                                        catch { }

                                                        etapa = "Atribuição dos dados para cadastro da capa do pedido";
                                                        //Capa
                                                        Pedidos = new Pedidos
                                                        {
                                                            numeropedido = Rotinas.NumeroPedido(contexto),
                                                            data = PedidoEcom.data,
                                                            consumidorid = consumidorid,
                                                            entidadeid = entidadeid,
                                                            tipopedido = "Externo",
                                                            status = "Acesse o pedido para ver o status",
                                                            valorfrete = valorFrete,
                                                            tipofrete = "FOB",
                                                            transportador = transportador,
                                                            urlrastreio = urlrastreio,
                                                            valordesconto = valorDesconto,
                                                            percentualdesconto = percentualDesconto,
                                                            total = Convert.ToDecimal(PedidoEcom.valorTotalPedido),
                                                            numeropedidoexterno = PedidoEcom.pedidoId.ToString(),
                                                            origempedidoexterno = "Wake",
                                                            obs = obs,
                                                            char1 = isB2B ? "B2B" : "B2C"
                                                        };


                                                        etapa = "Atribuição dos dados para cadastro dos produtos do pedido";
                                                        //Produtos (API Wake: PedidoEcom.itens = List<Iten>, valores já em reais)
                                                        foreach (var Item in PedidoEcom.itens ?? new List<Iten>())
                                                        {
                                                            string obsItem = null;
                                                            Produtos Produtos = contexto.Produtos.FirstOrDefault(m => m.referencia == Item.sku);
                                                            if (Produtos == null)
                                                            {
                                                                //Produto nao cadastrado, usa cadastro "PEÇA DIVERSA"
                                                                Produtos = contexto.Produtos.FirstOrDefault(m => m.id == 1);

                                                                //Insere a descricao da peça
                                                                obsItem = Item.nome;
                                                            }
                                                            if (Produtos != null)
                                                            {
                                                                PedidosProdutos PedidosProdutos = new PedidosProdutos
                                                                {
                                                                    produtoid = Produtos.id,
                                                                    quantidade = Item.quantidade,
                                                                    precotabela = Convert.ToDecimal(Item.precoVenda),
                                                                    valorunitario = Convert.ToDecimal(Item.precoVenda),
                                                                    valortotal = Convert.ToDecimal(Item.valorItem),
                                                                    obs = obsItem != null ? obsItem : null
                                                                };

                                                                Pedidos.PedidosProdutos.Add(PedidosProdutos);
                                                            }
                                                            else
                                                            {
                                                                Rotinas.Log("Produto não cadastrado no ATweb: " + Item.sku);
                                                            }
                                                        }

                                                        try
                                                        {

                                                            etapa = "Gravação do pedido";
                                                            //Grava
                                                            contexto.Pedidos.Add(Pedidos);
                                                            contexto.SaveChanges();

                                                            pedidosIntegrados++;

                                                            Rotinas.Log("Pedido gravado com sucesso!");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            string erro = Rotinas.GetException(ex);
                                                            Rotinas.Log("Pedido não gravado. " + erro);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Rotinas.Log("Consumidor não encontrado: " + cpfcnpj);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string erro = Rotinas.GetException(ex);
                                        Rotinas.Log(DateTime.Now + " - Erro Integração pedidos Wake | Etapa: " + etapa + " | " + erro);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string erro = Rotinas.GetException(ex);
                    Rotinas.Log(DateTime.Now + " - Erro Integração pedidos Wake | Etapa: " + etapa + " | " + erro);
                }

                dataIProcessar = dataIProcessar.AddDays(1);

            } while (dataIProcessar <= dataF);


            Rotinas.Log("-----------------------------------------------------------------------------------------------------------------");
            Rotinas.Log("Fim Integração Pedidos Wake");
            Rotinas.Log("Pedidos integrados: " + pedidosIntegrados);
            Rotinas.Log("");
            Rotinas.Log("");

        }

        private void AtualizacaoStatusPedidosWake()
        {
            Rotinas.Log("-----------------------------------------------------------------------------------------------------------------------------");
            Rotinas.Log("Início atualização status pedidos Wake");

            int pedidosAtualizados = 0;

            using (Dados_atweb_50031 contexto = new Dados_atweb_50031())
            {
                string baseUrl = "https://api.fbits.net/pedidos";
                string token = "sugga-9b5941a2-f9b2-40c7-bc37-02c7d19e0f14";

                List<Pedidos> listaPedidosAtualizar = contexto.Pedidos
                    .Where(m => m.origempedidoexterno == "Wake" && m.status == "Acesse o pedido para ver o status")
                    .OrderByDescending(m => m.id)
                    .ToList();

                // Busca situações Wake para observação (ids 3-8 cancelados)
                var clientSit = new HttpClient();
                var requestSit = new HttpRequestMessage(HttpMethod.Get, "https://api.fbits.net/situacoesPedido");
                requestSit.Headers.Add("accept", "application/json");
                requestSit.Headers.Add("Authorization", "Basic " + token);
                var jsonSit = clientSit.SendAsync(requestSit).Result.Content.ReadAsStringAsync().Result;
                var listaStatusWake = JsonConvert.DeserializeObject<List<StatusWake>>(jsonSit);
                var dictObservacaoWake = listaStatusWake != null
                    ? listaStatusWake.ToDictionary(x => x.situacaoPedidoId, x => x.observacao ?? x.descricao ?? x.nome ?? ("Id " + x.situacaoPedidoId))
                    : new Dictionary<int, string>();

                foreach (var Pedidos in listaPedidosAtualizar)
                {
                    try
                    {
                        var clientDetail = new HttpClient();
                        var requestDetail = new HttpRequestMessage(HttpMethod.Get, baseUrl + "/" + Pedidos.numeropedidoexterno + "/status");
                        requestDetail.Headers.Add("accept", "application/json");
                        requestDetail.Headers.Add("Authorization", "Basic " + token);

                        var jsonDetail = clientDetail.SendAsync(requestDetail).Result.Content.ReadAsStringAsync().Result;
                        var PedidoStatus = JsonConvert.DeserializeObject<PedidoStatusResponse>(jsonDetail);

                        if (PedidoStatus != null)
                        {
                            int id = PedidoStatus.situacaoPedidoId;

                            // Status personalizado (Pedidos.statuspedidoid) por ocorrenciareferencia "id|"
                            StatusPedidos statusPedidos = contexto.StatusPedidos.FirstOrDefault(m =>
                                m.ocorrenciareferencia != null && m.ocorrenciareferencia.Contains(id.ToString() + "|"));

                            if (statusPedidos != null)
                            {
                                Pedidos.statuspedidoid = statusPedidos.id;
                            }

                            // Status padrão ATweb (Pedidos.Status): faturado, cancelado ou não alterar
                            bool ehFaturado = new[] { 1, 9, 11, 15, 16, 18, 20, 21, 23 }.Contains(id);
                            bool ehCancelado = id >= 3 && id <= 8;

                            if (ehFaturado)
                            {
                                Pedidos.status = "Faturado";
                                // datafaturamento a partir de dataAtualizacao da Wake
                                if (!string.IsNullOrEmpty(PedidoStatus.dataAtualizacao) && DateTime.TryParse(PedidoStatus.dataAtualizacao, out DateTime dataFaturamento))
                                {
                                    Pedidos.datafaturamento = dataFaturamento;
                                }

                                // Nota fiscal: capa com dados da Wake, itens do pedido
                                if (!string.IsNullOrEmpty(PedidoStatus.notaFiscal))
                                {
                                    NotasFiscais nf = contexto.NotasFiscais.FirstOrDefault(m => m.pedidoid == Pedidos.id && m.notafiscal == PedidoStatus.notaFiscal);
                                    if (nf == null)
                                    {
                                        nf = new NotasFiscais
                                        {
                                            pedidoid = Pedidos.id,
                                            notafiscal = PedidoStatus.notaFiscal,
                                            chaveacesso = PedidoStatus.chaveAcessoNFE,
                                            datafaturamento = Pedidos.datafaturamento ?? DateTime.Now,
                                            tipofaturamento = "Faturado",
                                            transportador = PedidoStatus.nomeTransportadora,
                                            total = Pedidos.total
                                        };

                                        var itensPedido = contexto.PedidosProdutos.Where(pp => pp.pedidoid == Pedidos.id).ToList();
                                        foreach (var item in itensPedido)
                                        {
                                            nf.NotasFiscaisProdutos.Add(new NotasFiscaisProdutos
                                            {
                                                produtoid = item.produtoid,
                                                quantidade = item.quantidade,
                                                precotabela = item.precotabela,
                                                valorunitario = item.valorunitario,
                                                valortotal = item.valortotal,
                                                obs = item.obs
                                            });
                                        }

                                        contexto.NotasFiscais.Add(nf);
                                    }
                                }
                            }
                            else if (ehCancelado)
                            {
                                Pedidos.status = "Cancelado";
                                string textoObs = dictObservacaoWake.ContainsKey(id) ? dictObservacaoWake[id] : ("Status Wake: " + id);
                                if (!string.IsNullOrEmpty(Pedidos.obs))
                                    Pedidos.obs = Pedidos.obs + " | " + textoObs;
                                else
                                    Pedidos.obs = textoObs;
                            }

                            if (!string.IsNullOrEmpty(PedidoStatus.urlRastreamento))
                            {
                                Pedidos.urlrastreio = PedidoStatus.urlRastreamento;
                            }

                            contexto.Entry(Pedidos).State = System.Data.Entity.EntityState.Modified;
                            try
                            {
                                contexto.SaveChanges();
                                pedidosAtualizados++;
                            }
                            catch (Exception ex)
                            {
                                Rotinas.Log("Erro ao gravar status do pedido " + Pedidos.numeropedidoexterno + ": " + Rotinas.GetException(ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Rotinas.Log("Erro ao atualizar status pedido " + Pedidos.numeropedidoexterno + ": " + Rotinas.GetException(ex));
                    }
                }
            }
        }
    }
}




