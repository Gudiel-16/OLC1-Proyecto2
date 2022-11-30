using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _OLC1_Proyecto2_201404278.interprete
{
    class resultado
    {
        public Object valor;
        public String tipo;
        public int linea, columna;

        public resultado(Object valor, String tipo, int linea, int columna) 
        {
            this.valor = valor;
            this.tipo = tipo;
            this.linea = linea;
            this.columna = columna;
        }
    }
}
