using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace _OLC1_Proyecto2_201404278.interprete
{
    class simbolo
    {
        public String identificador;
        public enum Tipo {ENTERO, DECIMAL, CADENA, BOOLEANO, CARACTER};
        public String tipo;
        public int linea, columna;
        public Object valor;
        //para metodos y funciones
        public ParseTreeNode parametrosF;
        public ParseTreeNode instrucciones;
        //para clase
        public List<ParseTreeNode> l_funciones_metodos;
        public List<ParseTreeNode> l_declaraciones;
        public Dictionary<String,Object> dec; 
        //para arreglos
        public Object o_array;
        public String dimension;

        //constructor para guardar variables
        public simbolo(String identificador, Object valor, String tipo, int linea, int columna) 
        {
            this.identificador = identificador;
            this.tipo = tipo;
            this.linea = linea;
            this.columna = columna;
            this.valor = valor;
        }

        //constructor para funciones
        public simbolo(String identificador, Object valor, String tipo, int linea, int columna, ParseTreeNode parametrosFormales, ParseTreeNode instrucciones)
        {
            this.identificador = identificador;
            this.tipo = tipo;
            this.linea = linea;
            this.columna = columna;
            this.valor = valor;
            this.parametrosF = parametrosFormales;
            this.instrucciones = instrucciones;
        }

        //contructor para clases
        public simbolo(String identificador, Object valor, String tipo, int linea, int columna, Dictionary<String,Object> declaraciones, List<ParseTreeNode> func_met, List<ParseTreeNode> l_dec )
        {
            this.identificador = identificador;
            this.tipo = tipo;
            this.linea = linea;
            this.columna = columna;
            this.valor = valor;
            this.dec = declaraciones;
            this.l_declaraciones = l_dec;
            this.l_funciones_metodos = func_met;
        }

        //para arreglos
        public simbolo(String identificador, Object valor, String tipo, int linea, int columna, Object array, String dim)
        {
            this.identificador = identificador;
            this.tipo = tipo;
            this.linea = linea;
            this.columna = columna;
            this.valor = valor;
            this.o_array = array;
            this.dimension = dim;
        }

    }
}
