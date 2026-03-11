using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Data
    {
        public int tipoparceiroid { get; set; }
        public string descricaotipoparceiro { get; set; }
        public string numeroidentificacao { get; set; }
        public string nome { get; set; }
        public object razaosocial { get; set; }
        public object referencia { get; set; }
        public string telefone { get; set; }
        public object telefone2 { get; set; }
        public object telefone3 { get; set; }
        public object rginscricaoestadual { get; set; }
        public string cep { get; set; }
        public string endereco { get; set; }
        public string numero { get; set; }
        public object complemento { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string pais { get; set; }
        public object regiao { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string email { get; set; }
        public List<object> enderecosAdicionaisParceiro { get; set; }
    }

    public class ParceiroATweb
    {
        public Data data { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        public object errors { get; set; }
    }
}
