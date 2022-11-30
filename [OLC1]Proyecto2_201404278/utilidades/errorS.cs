using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _OLC1_Proyecto2_201404278.utilidades
{
    class errorS
    {
        public String descripcion;
        public int linea, columna;
        public String valor;

        public errorS(String descripcion, int linea, int columna, String valor) 
        {
            this.descripcion = descripcion;
            this.linea = linea;
            this.columna = columna;
            this.valor = valor;
        }

    }
}
