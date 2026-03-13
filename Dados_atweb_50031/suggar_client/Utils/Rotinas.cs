using suggar_client.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suggar_client.Utils
{
    public class Rotinas
    {
        public static int NumeroPedido(Dados_atweb_50031 _contexto = null)
        {
            int numero = 0;

            Dados_atweb_50031 contexto;
            if (_contexto != null)
                contexto = _contexto;
            else
                contexto = new Dados_atweb_50031();

            ParametrosGlobais registro = contexto.ParametrosGlobais.Find(1); //sempre busca registro de id=1 para os parametros

            //Verifica se ID já existe na tabela
            bool ok = false;
            do
            {
                registro.numeropedido = registro.numeropedido + 1;
                numero = (int)registro.numeropedido;

                Pedidos Pedidos = contexto.Pedidos.FirstOrDefault(m => m.numeropedido == registro.numeropedido);
                if (Pedidos == null)
                    ok = true;

            } while (ok == false);

            //Atualiza numeracao
            contexto.Entry(registro).State = System.Data.Entity.EntityState.Modified;
            contexto.SaveChanges();

            return numero;
        }
        public static int? ParseNullableInt(string value)
        {
            int intValue;
            if (int.TryParse(value, out intValue))
                return intValue;
            return null;
        }

        public static string DeixaNumerico(string Valor)
        {
            int v;
            int t = Valor.Length;
            string str = "";
            for (int x = 0; x <= t - 1; x++)
            {
                if (Int32.TryParse(Valor.Substring(x, 1), out v))
                {
                    str = str + Valor.Substring(x, 1);
                }
            }
            return str;
        }

        public static string AcertaRetornoJson(string retorno)
        {
            retorno = retorno.Replace(@"\", "").Replace(@"\", "");   //remove barras '\'
            retorno = retorno.Substring(1);                          //remove aspas duplas do inicio  
            retorno = retorno.Substring(0, retorno.Length - 1);      //remove aspas duplas do fim
            retorno = retorno.Replace("\"", "'");                    //substitui aspas duplas por aspas simples

            return retorno;
        }

        public static string GetException(Exception ex)
        {

            //Esta rotina retorna o erro real para o usuario, ao invés de erros do tipo "See InnerException, if present, for more details."

            string erros = "A aplicação retornou o(s) seguinte(s) erro(s): " + Environment.NewLine;

            DbEntityValidationException EntityValidation = null;
            try
            {
                EntityValidation = (DbEntityValidationException)ex;
            }
            catch { }

            if (ex.InnerException != null)
            {
                erros = erros + GetInnerException(ex.InnerException);
            }
            else if (EntityValidation != null)
            {
                foreach (var eve in EntityValidation.EntityValidationErrors)
                {
                    //erros = erros + string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    //    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        erros = erros + string.Format("Campo: \"{0}\" - Erro: \"{1}\" ",
                            ve.PropertyName, ve.ErrorMessage);
                        erros = erros + Environment.NewLine;
                    }

                }
            }
            else
            {
                erros = ex.Message;
            }

            return erros;
        }

        private static string GetInnerException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return string.Format("{0} > {1} ", ex.InnerException.Message, GetInnerException(ex.InnerException));
            }
            return ex.Message;
        }

        public static void Log(string mensagem)
        {
            //(Tenta gravar log 3 vezes)
            StreamWriter objWriter = null;

            int count = 0;
            do
            {
                count++;
                try
                {
                    string LogPath = "";

                    string exe = Process.GetCurrentProcess().MainModule.FileName;
                    string path = Path.GetDirectoryName(exe);
                    LogPath = path + @"\log.txt";

                    objWriter = new StreamWriter(LogPath, true);
                    objWriter.WriteLine(mensagem);
                    objWriter.Close();
                    break;
                }
                catch
                {
                    try
                    {
                        objWriter.Close();
                    }
                    catch { }
                }
            } while (count < 3);
        }
    }
}
