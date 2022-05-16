using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Utilidades
{
    public class Utilis
    {

        public enum VariablesLocal
        {
            [EnumMember]
            ValorTope = 1000,
            [Description("Retiro")]
            Retiro,
        }

    }
}
