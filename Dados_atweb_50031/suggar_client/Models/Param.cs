using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Models
{
    public class Param
    {

        private string _chave;
        public string chave
        {
            get { return _chave; }
            set { _chave = value; }
        }

        private string _valor;
        public string valor
        {
            get { return _valor; }
            set { _valor = value; }
        }

    }
}
