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

            //O processamento deve ser feito dia por dia, pois a Wake limita o retorno a no méximo 30 paginas
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

                                                Rotinas.Log("Pedido " + PedidoEcom.pedidoId + " já integrado." + (updated > 0 ? " Url de rastreio atualizada." : ""));
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

                                                etapa = "Consulta ao endpoint do ATweb para buscar o parceiro";
                                                //Busca cadastro do consumidor via API
                                                var clientParceiro = new HttpClient();
                                                var requestParceiro = new HttpRequestMessage(HttpMethod.Get, "https://api.atweb.top/api/busca-parceiro/" + cpfcnpj);
                                                requestParceiro.Headers.Add("accept", "text/plain");
                                                requestParceiro.Headers.Add("Authorization", "Basic X2FwaSNqY3NicmFzaWw6Z1dsWmJtRVRIQlM0M29FNlE1OVd6a09wUldpRFdUTFY=");
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
                                                        ParceiroATweb.data.telefone = telefone;
                                                        ParceiroATweb.data.telefone2 = null;
                                                        ParceiroATweb.data.telefone3 = null;
                                                        try
                                                        {

                                                            string nomeEmail = Pedido.clientProfileData.email.Split('@')[0];
                                                            string dominioEmail = Pedido.clientProfileData.email.Split('@')[1];
                                                            dominioEmail = "@" + dominioEmail.Split('-')[0];
                                                            ParceiroATweb.data.email = nomeEmail + dominioEmail;
                                                        }
                                                        catch { }
                                                        ParceiroATweb.data.rginscricaoestadual = null;
                                                        ParceiroATweb.data.cep = Pedido.shippingData.address.postalCode;
                                                        ParceiroATweb.data.endereco = Pedido.shippingData.address.street;
                                                        ParceiroATweb.data.numero = Pedido.shippingData.address.number;
                                                        ParceiroATweb.data.complemento = Pedido.shippingData.address.complement;
                                                        ParceiroATweb.data.bairro = Pedido.shippingData.address.neighborhood;
                                                        ParceiroATweb.data.cidade = Pedido.shippingData.address.city;
                                                        ParceiroATweb.data.estado = Pedido.shippingData.address.state;
                                                        ParceiroATweb.data.pais = Pedido.shippingData.address.country == "BRA" ? "Brasil" : Pedido.shippingData.address.country;
                                                        try
                                                        {
                                                            ParceiroATweb.data.lat = Pedido.shippingData.address.geoCoordinates[0].ToString().Replace(",", ".");
                                                            ParceiroATweb.data.lng = Pedido.shippingData.address.geoCoordinates[1].ToString().Replace(",", ".");
                                                        }
                                                        catch { }

                                                        using (var clientCadParceiro = new HttpClient())
                                                        {
                                                            etapa = "Acesso ao endpoint ATweb de cadastro do parceiro";
                                                            jsonParceiro = JsonConvert.SerializeObject(ParceiroATweb.data);
                                                            var requestCadParceiro = new HttpRequestMessage(HttpMethod.Post, "https://api.atweb.top/api/cadastra-parceiro");
                                                            requestCadParceiro.Headers.Add("accept", "application/json"); // Alterado para application/json
                                                            requestCadParceiro.Headers.Add("Authorization", "Basic X2FwaSNqY3NicmFzaWw6Z1dsWmJtRVRIQlM0M29FNlE1OVd6a09wUldpRFdUTFY=");
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
                                                            requestParceiro.Headers.Add("Authorization", "Basic X2FwaSNqY3NicmFzaWw6Z1dsWmJtRVRIQlM0M29FNlE1OVd6a09wUldpRFdUTFY=");
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

                                                    if (orderType == OrderType.B2B || Pedido.clientProfileData.isCorporate)
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
                                                        decimal valorFrete = 0;
                                                        try { valorFrete = Convert.ToDecimal(Pedido.totals.FirstOrDefault(m => m.name == "Total do Frete").value / 100); } catch { }
                                                        ;

                                                        decimal valorDesconto = 0;
                                                        decimal percentualDesconto = 0;
                                                        try
                                                        {
                                                            valorDesconto = Math.Abs(Convert.ToDecimal(Pedido.totals.FirstOrDefault(m => m.name == "Total dos Descontos").value / 100));
                                                            if (valorDesconto > 0)
                                                            {
                                                                //Calcula percentual desconto
                                                                decimal totalItems = Convert.ToDecimal(Pedido.totals.FirstOrDefault(m => m.name == "Total dos Itens").value / 100);
                                                                percentualDesconto = (valorDesconto / totalItems) * 100;
                                                            }
                                                        }
                                                        catch { }
                                                        ;

                                                        string obs = "";
                                                        string transportador = "";
                                                        try
                                                        {
                                                            var logisticsInfo = Pedido.shippingData.logisticsInfo.FirstOrDefault();
                                                            if (logisticsInfo != null)
                                                            {
                                                                transportador = logisticsInfo.deliveryCompany;

                                                                obs = obs + "Estimativa de entrega: " + string.Format("{0:dd/MM/yyyy}", logisticsInfo.shippingEstimateDate) + Environment.NewLine;
                                                            }
                                                        }
                                                        catch { }


                                                        etapa = "Atribuição dos dados para cadastro da capa do pedido";
                                                        //Capa
                                                        Pedidos = new Pedidos
                                                        {
                                                            numeropedido = Rotinas.NumeroPedido(contexto),
                                                            data = Pedido.creationDate,
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
                                                            total = Convert.ToDecimal(Pedido.value / 100),
                                                            numeropedidoexterno = Pedido.orderId,
                                                            origempedidoexterno = "Wake",
                                                            obs = obs,
                                                            char1 = orderType == OrderType.B2B ? "B2B" : "B2C"
                                                        };


                                                        etapa = "Atribuição dos dados para cadastro dos produtos do pedido";
                                                        //Produtos
                                                        foreach (var Item in Pedido.items)
                                                        {
                                                            string obsItem = null;
                                                            Produtos Produtos = contexto.Produtos.FirstOrDefault(m => m.referencia == Item.refId);
                                                            if (Produtos == null)
                                                            {
                                                                //Produto nao cadastrado, usa cadastro "PEÇA DIVERSA"
                                                                Produtos = contexto.Produtos.FirstOrDefault(m => m.id == 1019);

                                                                //Insere a descricao da peça
                                                                obsItem = Item.name;
                                                            }
                                                            if (Produtos != null)
                                                            {
                                                                PedidosProdutos PedidosProdutos = new PedidosProdutos
                                                                {
                                                                    produtoid = Produtos.id,
                                                                    quantidade = Item.quantity,
                                                                    precotabela = Convert.ToDecimal(Item.price / 100),
                                                                    valorunitario = Convert.ToDecimal(Item.price / 100),
                                                                    valortotal = Convert.ToDecimal(Item.price / 100) * Item.quantity,
                                                                    obs = obsItem != null ? obsItem : null
                                                                };

                                                                Pedidos.PedidosProdutos.Add(PedidosProdutos);
                                                            }
                                                            else
                                                            {
                                                                Rotinas.Log("Produto não cadastrado no ATweb: " + Item.refId);
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

    }
}
