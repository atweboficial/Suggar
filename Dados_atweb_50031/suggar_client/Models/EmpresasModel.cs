using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Models
{
    public class EmpresasModel
    {

        public int id { get; set; }
        public string nome { get; set; }
        public string razaosocial { get; set; }
        public string endereco { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string uf { get; set; }
        public string cep { get; set; }
        public string cnpj { get; set; }
        public string inscricaoestadual { get; set; }
        public string telefone { get; set; }
        public string email { get; set; }
        public string connectionstringname { get; set; }
        public string chave { get; set; }
        public byte[] logo { get; set; }
        public string contato { get; set; }
        public string ativo { get; set; }

    }
}
