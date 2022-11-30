using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace _OLC1_Proyecto2_201404278.sol.arbol
{
    class nodoArbol
    {
        private String tipo;
        private String value;
        private int columna;
        private int linea;
        private LinkedList<nodoArbol> hijos = new LinkedList<nodoArbol>();

        public nodoArbol(String tipo, String valor, LinkedList<nodoArbol> lista, int l, int c)
        {
            this.tipo = tipo;
            this.value = valor;
            this.hijos = lista;
            this.linea = l;
            this.columna = c;
        }

        public nodoArbol(String tipo, String valor, int l, int c)
        {
            this.tipo = tipo;
            this.value = valor;        
            this.linea = l;
            this.columna = c;
        } 

        public void addHijo(nodoArbol hijo)
        {
            this.hijos.AddLast(hijo);
        }

        public String getTipo()
        {
            return tipo;
        }

        public String getValue()
        {
            return value;
        }

        public int getColumna()
        {
            return columna;
        }

        public int getLinea()
        {
            return linea;
        }

        public LinkedList<nodoArbol> getHijos()
        {
            return hijos;
        }


    }
        
}
