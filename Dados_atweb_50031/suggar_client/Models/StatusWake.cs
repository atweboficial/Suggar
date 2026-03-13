using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class StatusWake
    {
        public int situacaoPedidoId { get; set; }
        public string nome { get; set; }
        public string descricao { get; set; }
        public string observacao { get; set; }
    }


}
