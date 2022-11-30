using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2_201404278.sol.analizador;
using _OLC1_Proyecto2_201404278.utilidades;

namespace _OLC1_Proyecto2_201404278.interprete
{
    class interprete_import
    {
        entorno newEntorno;

        public interprete_import(entorno newEntorn)
        {
            //entorno que nos mandaran para guardar las clases, metodos y funciones
            this.newEntorno = newEntorn;
        }

        public void leerImport(String ruta)
        {
            //se crea nuevo arbol
            ParseTreeNode raiz = sintactico.analizar(ruta);
            if (raiz!=null)
            {
                ejecutar(raiz,newEntorno);
            }
            else
            {
                singlentonError.registrarError(new errorS("Errores en el archivo import ", 0, 0, ruta));
            }
        }
        
        public resultado ejecutar(ParseTreeNode r, entorno local)
        {
            switch (r.Term.ToString()) //nombre con que guardo los terminales y no terminales
            {
                case "INSTRUCCION":
                    foreach (ParseTreeNode hijoActual in r.ChildNodes)
                    {
                        ejecutar(hijoActual, local); //ejecuta cada uno de los hijos de las instrucciones
                    }
                    return null;

                case "CLASS":
                    String idClass = r.ChildNodes[0].FindTokenAndGetText();
                    ParseTreeNode instClass = r.ChildNodes[1];//accedo al nodo de instrucciones                    
                    Dictionary<String, Object> l_declaraciones = new Dictionary<String, Object>();
                    List<ParseTreeNode> l_declaraciones2 = new List<ParseTreeNode>();
                    List<ParseTreeNode> l_funciones_metodos = new List<ParseTreeNode>();

                    //insertamos en listas
                    foreach (ParseTreeNode hijoActualIns in instClass.ChildNodes) //ejecutamos las instrucciones del metodo
                    {
                        if (hijoActualIns.Term.ToString().Equals("DECLIST"))
                        {
                            l_declaraciones2.Add(hijoActualIns); //para guardar nodos de declaraciones
                            if (hijoActualIns.ChildNodes[0].Term.ToString().Equals("DECLIST2"))
                            {
                                ParseTreeNode declist2 = hijoActualIns.ChildNodes[0];
                                resultado val = ejecutar(declist2.ChildNodes[1], local);
                                l_declaraciones.Add(declist2.ChildNodes[0].FindTokenAndGetText(), val);
                            }
                            else
                            {
                                l_declaraciones.Add(hijoActualIns.ChildNodes[0].FindTokenAndGetText(), new resultado(null, "null", hijoActualIns.ChildNodes[0].Span.Location.Line, hijoActualIns.ChildNodes[0].Span.Location.Column));
                            }                            
                        }
                        if (hijoActualIns.Term.ToString().Equals("FUNCTION"))
                        {
                            l_funciones_metodos.Add(hijoActualIns);
                        }
                        if (hijoActualIns.Term.ToString().Equals("METODO"))
                        {
                            l_funciones_metodos.Add(hijoActualIns);
                        }
                    }

                    //insertamos simnolo
                    local.insertar(new simbolo(idClass,"class","class",r.Span.Location.Line,r.Span.Location.Column,l_declaraciones,l_funciones_metodos,l_declaraciones2));
                    
                    return null;

                /*FUNCTION*/
                case "FUNCTION":
                    //se guarda como un simbolo mas, solo que tambien se guardan los nodos con los parametros, y nodo de instrucciones
                    String idFunction = r.ChildNodes[0].FindTokenAndGetText();
                    simbolo simFunct = new simbolo(idFunction, "function", "function", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[1], r.ChildNodes[2]);
                    local.insertar(simFunct);
                    return null;

                /*METODO*/
                case "METODO":
                    //se guarda como un simbolo mas, solo que tambien se guardan los nodos con los parametros, y nodo de instrucciones
                    String idMetodo = r.ChildNodes[0].FindTokenAndGetText();
                    simbolo simMet = new simbolo(idMetodo, "function", "metodo", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[1], r.ChildNodes[2]);
                    local.insertar(simMet);
                    return null;
            }

            return null;
        }

    }
}
