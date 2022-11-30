using _OLC1_Proyecto2_201404278.sol.arbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2_201404278.utilidades;
using System.Windows.Forms;

namespace _OLC1_Proyecto2_201404278.interprete
{
    class interpretee
    {
        public ParseTreeNode raiz;

        public interpretee(ParseTreeNode r)
        {
            raiz = r;
        }
    
        public void comenzar()
        {
            ejecutar(raiz, new entorno(null));                           
        }

        

        public resultado ejecutar(ParseTreeNode r, entorno local)
        {
            switch (r.Term.ToString()) //nombre con que guardo los terminales y no terminales
            {
                case "INSTRUCCION":
                    //primera pasada, guardara metodos, funciones y clases en el archivo de entrada
                    foreach (ParseTreeNode hijoActual in r.ChildNodes)
                    {
                        if (hijoActual.Term.ToString().Equals("FUNCTION") || hijoActual.Term.ToString().Equals("METODO") || hijoActual.Term.ToString().Equals("CLASS"))
                        {
                            ejecutar(hijoActual, local); //ejecuta cada uno de los hijos de las instrucciones
                        }                        
                    }
                    //segunda pasada, ira ejecutando tipo top-down(arriba-abajo) lo que contenga
                    foreach (ParseTreeNode hijoActual in r.ChildNodes)
                    {
                        if (!hijoActual.Term.ToString().Equals("FUNCTION") && !hijoActual.Term.ToString().Equals("METODO") && !hijoActual.Term.ToString().Equals("CLASS"))
                        {
                            ejecutar(hijoActual, local); //ejecuta cada uno de los hijos de las instrucciones
                        }  
                    }

                    return null;

/*--FUNLOG--*/  case "FUNLOG":                                       
                    try
                    {
                        resultado valorRet = ejecutar(r.ChildNodes[0], local); //mando el hijo que sera EA 
                        if (!valorRet.valor.Equals("error"))
                        {
                            singlentonConsola.listaConsola.Add(valorRet.valor);
                        }                        
                    }
                    catch (Exception)
                    {                      
                    }
                    return null;

/*--FUNALERT--*/case "FUNALERT":
                    resultado valorFunAlert = ejecutar(r.ChildNodes[0],local);
                    MessageBox.Show(valorFunAlert.valor.ToString(),"ALERT",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                    return null;

/*--FUNGRAPH--*/case "FUNGRAPH":
                    resultado valorFunGraph = ejecutar(r.ChildNodes[0], local);
                    resultado valorFunGraph2 = ejecutar(r.ChildNodes[1], local);

                    if ((valorFunGraph.tipo.Equals("tCad")) && (valorFunGraph2.tipo.Equals("tCad")))
                    {
                        try
                        {
                            String grafo = valorFunGraph2.valor.ToString();

                            WINGRAPHVIZLib.DOT dot = new WINGRAPHVIZLib.DOT();
                            WINGRAPHVIZLib.BinaryImage img = dot.ToPNG(grafo);
                            img.Save(valorFunGraph.valor.ToString() + ".png");
                        }
                        catch (Exception)
                        {
                            singlentonError.registrarError(new errorS("Error en la funcion Graph ", r.Span.Location.Line, r.Span.Location.Column, "-"));
                        }
                        
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("Ambos valores de la funcion Graph deben ser String ", r.Span.Location.Line, r.Span.Location.Column, "-"));
                    }

                    return null;

/*--DECLIST--*/ case "DECLIST":
                    if (r.ChildNodes.Count==1) //si solo tiene un hijo
                    {
                        if (r.ChildNodes[0].Term.ToString().Equals("DECLIST2")) //viene de la forma: var a=3;
                        {
                           return ejecutar(r.ChildNodes[0], local);
                        }
                        else //viene de la forma: var nuev;
                        {
                            String iddeclist = r.ChildNodes[0].FindTokenAndGetText();
                            simbolo simboloDecList = new simbolo(iddeclist, null, "null", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                            local.insertar(simboloDecList);

                        }
                    }
                    else //tiene dos o mas hijos
                    {
                        //se valida que no tenga dos o mas declaracion de esta forma: var a=2,b=3,c=4; la forma correcta seria: var a,b,c=4;
                        int contdl2 = 0;
                        foreach (ParseTreeNode hijoActual in r.ChildNodes)
                        {
                            if (hijoActual.Term.ToString().Equals("DECLIST2"))
                            {
                                contdl2++;
                            }
                        }

                        if (contdl2==0) //viene de la forma: var a,b,c;
                        {
                            foreach (ParseTreeNode hijoActual in r.ChildNodes)
                            {
                                String iddeclist = hijoActual.FindTokenAndGetText();
                                simbolo simboloDecList = new simbolo(iddeclist, null, "null", hijoActual.Span.Location.Line, hijoActual.Span.Location.Column);
                                local.insertar(simboloDecList);
                            }
                        }
                        else if (contdl2 == 1) //viene de la forma: var a,b=2,c,d; hay que validar que venga de ultimo asi: var a,b,c=4;
                        {
                            if (r.ChildNodes[r.ChildNodes.Count-1].Term.ToString().Equals("DECLIST2"))//viene de la forma correcta
                            {
                                String identt = r.ChildNodes[r.ChildNodes.Count - 1].FindTokenAndGetText(); //extraigo el id de la ultima declaracion
                                resultado valorAsignar = ejecutar(r.ChildNodes[r.ChildNodes.Count-1],local); //extraigo el valor de la ultima declaracion
                                                                
                                if (valorAsignar.tipo.Equals("array"))
                                {
                                    ParseTreeNode declist2 = r.ChildNodes[r.ChildNodes.Count - 1];
                                    ParseTreeNode ea = declist2.ChildNodes[1];
                                    simbolo arr = local.obtener(ea.ChildNodes[0].FindTokenAndGetText());

                                    foreach (ParseTreeNode hijoActual in r.ChildNodes) //recorro todos los nodos para asignar valor
                                    {
                                        if (!hijoActual.Term.ToString().Equals("DECLIST2")) //estara en el ultimo nodo por eso se ejectuan todos porque este se ejecuto arriba
                                        {
                                            simbolo simboloDecListt = new simbolo(hijoActual.FindTokenAndGetText(), valorAsignar.valor, valorAsignar.tipo, hijoActual.Span.Location.Line, hijoActual.Span.Location.Column,arr.o_array,arr.dimension);
                                            local.insertar(simboloDecListt);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (ParseTreeNode hijoActual in r.ChildNodes) //recorro todos los nodos para asignar valor
                                    {
                                        if (!hijoActual.Term.ToString().Equals("DECLIST2")) //estara en el ultimo nodo por eso se ejectuan todos porque este se ejecuto arriba
                                        {
                                            simbolo simboloDecListt = new simbolo(hijoActual.FindTokenAndGetText(), valorAsignar.valor, valorAsignar.tipo, hijoActual.Span.Location.Line, hijoActual.Span.Location.Column);
                                            local.insertar(simboloDecListt);
                                        }
                                    }
                                }                                                                
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("Error En la declaracion de variables ", r.Span.Location.Line, r.Span.Location.Column, "-"));
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("Error En la declaracion de variables ", r.Span.Location.Line, r.Span.Location.Column, "-"));
                        }
                    }
                    return null;

                case "DECLIST2":
                    String ident = r.ChildNodes[0].FindTokenAndGetText();//nombre de la variable que es el hijo 0
                    resultado valDecList2 = ejecutar(r.ChildNodes[1],local);//retornara el valor de la expresion y mandamos hijo 1
                    if (valDecList2!=null)
                    {
                        if (valDecList2.tipo.ToString().Equals("array"))
                        {
                            ParseTreeNode ea=r.ChildNodes[1];
                            simbolo arr = local.obtener(ea.ChildNodes[0].FindTokenAndGetText());
                            if (arr != null)
                            {
                                if (arr.tipo.Equals("array"))
                                {
                                    local.insertar(new simbolo(ident, arr.valor, arr.tipo, arr.linea, arr.columna, arr.o_array, arr.dimension));
                                    return valDecList2;
                                }
                            }
                        }
                        else
                        {
                            simbolo simboloDecList2 = new simbolo(ident, valDecList2.valor, valDecList2.tipo, valDecList2.linea, valDecList2.columna);
                            local.insertar(simboloDecList2);
                            return valDecList2;
                        }                        
                    }
                    return null;

/*-- ASIG --*/  case "ASIG":
                    String identAsig = r.ChildNodes[0].FindTokenAndGetText();//nombre de la variable que es el hijo 0
                    resultado valAsig = ejecutar(r.ChildNodes[1], local);//retornara el valor de la expresion y mandamos hijo 1
                    if (valAsig!=null)
                    {
                        if (valAsig.tipo.Equals("array"))
                        {
                            ParseTreeNode ea = r.ChildNodes[1];
                            simbolo arr = local.obtener(ea.ChildNodes[0].FindTokenAndGetText());
                            if (arr!=null)
                            {
                                simbolo simboloAsig = new simbolo(identAsig, arr.valor, arr.tipo, arr.linea, arr.columna, arr.o_array, arr.dimension);
                                local.asignarValor(simboloAsig);
                            }
                        }
                        else if (valAsig.tipo.Equals("class"))
                        {
                            ParseTreeNode ea = r.ChildNodes[1];
                            simbolo arr = local.obtener(ea.ChildNodes[0].FindTokenAndGetText());
                            if (arr != null)
                            {
                                simbolo simboloAsig = new simbolo(identAsig, arr.valor, arr.tipo, arr.linea, arr.columna, arr.dec, arr.l_funciones_metodos,arr.l_declaraciones);
                                local.asignarValor(simboloAsig);
                            }
                        }
                        else
                        {
                            simbolo simboloAsig = new simbolo(identAsig, valAsig.valor, valAsig.tipo, valAsig.linea, valAsig.columna);
                            local.asignarValor(simboloAsig);
                        }
                    }
                    
                    return null;

/*ARRAY*/       case "DECARRAY2":
                    #region decarray2
                    String nomArr = r.ChildNodes[0].FindTokenAndGetText(); //Nombre del arreglo
                    ParseTreeNode decarray3 = r.ChildNodes[1];//accedo a DECARRAY3, contiene como hijos las dimensiones

                    if (decarray3 != null)
                    {
                        if (decarray3.ChildNodes.Count==1) //1 DEMENSION
                        {
                            ParseTreeNode decarray33 = decarray3.ChildNodes[0];
                            resultado dimension = ejecutar(decarray33.ChildNodes[0],local);
                            ParseTreeNode decarrayList = decarray33.ChildNodes[1];
                            if (decarrayList.ChildNodes.Count>0)
                            {
                                ParseTreeNode decarray5 = decarrayList.ChildNodes[0];
                                ParseTreeNode decarray7 = decarray5.ChildNodes[0];
                                if (decarray7.ChildNodes.Count== Int32.Parse(dimension.valor.ToString()))
                                {
                                    int [] newarray=new int[Int32.Parse(dimension.valor.ToString())];
                                    for (int i = 0; i < decarray7.ChildNodes.Count; i++)
                                    {
                                        resultado valor_indice = ejecutar(decarray7.ChildNodes[i],local);
                                        newarray[i] = Int32.Parse(valor_indice.valor.ToString());
                                    }
                                    local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column,newarray,"1dimension"));
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("Numero de elementos no concuerdan con tamaño ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }
                            }
                            else
                            {   //se agrega array sin definir valores
                                int[] newarray = new int[Int32.Parse(dimension.valor.ToString())];
                                local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, newarray,"1dimension"));
                            }
                        }
                        else if (decarray3.ChildNodes.Count == 2) //2 DIMENSIONES
                        {
                            ParseTreeNode decarray33_0 = decarray3.ChildNodes[0];
                            ParseTreeNode decarray33_1 = decarray3.ChildNodes[1];
                            resultado dimension_0 = ejecutar(decarray33_0.ChildNodes[0], local);
                            resultado dimension_1 = ejecutar(decarray33_1.ChildNodes[0], local);
                            ParseTreeNode decarrayList = decarray33_1.ChildNodes[1];

                            if (decarrayList.ChildNodes.Count > 0)
                            {
                                ParseTreeNode decarray5 = decarrayList.ChildNodes[0];
                                ParseTreeNode decarray7_0 = decarray5.ChildNodes[0];
                                ParseTreeNode decarray7_1 = decarray5.ChildNodes[1];

                                if ((decarray7_0.ChildNodes.Count == Int32.Parse(dimension_0.valor.ToString())) && (decarray7_1.ChildNodes.Count == Int32.Parse(dimension_1.valor.ToString())))
                                {
                                    int[,] newarray = new int[Int32.Parse(dimension_0.valor.ToString()), Int32.Parse(dimension_1.valor.ToString())];
                                    for (int i = 0; i < decarray5.ChildNodes.Count; i++)
                                    {
                                        ParseTreeNode array7 = decarray5.ChildNodes[i];
                                        for (int j = 0; j < array7.ChildNodes.Count; j++)
                                        {
                                            resultado valor_indice = ejecutar(array7.ChildNodes[j], local);
                                            newarray[i,j] = Int32.Parse(valor_indice.valor.ToString());
                                        }                                        
                                    }
                                    local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, newarray, "2dimension"));
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("Numero de elementos no concuerdan con tamaño ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }
                            }
                            else
                            {   //se agrega array sin definir valores
                                int[,] newarray = new int[Int32.Parse(dimension_0.valor.ToString()), Int32.Parse(dimension_1.valor.ToString())];
                                local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, newarray, "2dimension"));
                            }
                        }
                        else if (decarray3.ChildNodes.Count == 3) //3 DIMENSIONES
                        {
                            ParseTreeNode decarray33_0 = decarray3.ChildNodes[0];
                            ParseTreeNode decarray33_1 = decarray3.ChildNodes[1];
                            ParseTreeNode decarray33_2 = decarray3.ChildNodes[2];
                            resultado dimension_0 = ejecutar(decarray33_0.ChildNodes[0], local);
                            resultado dimension_1 = ejecutar(decarray33_1.ChildNodes[0], local);
                            resultado dimension_2 = ejecutar(decarray33_2.ChildNodes[0], local);
                            ParseTreeNode decarrayList = decarray33_2.ChildNodes[1];

                            if (decarrayList.ChildNodes.Count > 0)
                            {
                                ParseTreeNode decarray5 = decarrayList.ChildNodes[0];

                                //validar que esten los indices cabales
                                int numIndices = Int32.Parse(dimension_0.valor.ToString()) * Int32.Parse(dimension_1.valor.ToString()) * Int32.Parse(dimension_2.valor.ToString());
                                int numIndices2 = 0;

                                for (int i = 0; i < decarray5.ChildNodes.Count; i++)
                                {
                                    ParseTreeNode decarray7 = decarray5.ChildNodes[i];
                                    for (int j = 0; j < decarray7.ChildNodes.Count; j++)
                                    {
                                        ParseTreeNode decarray77 = decarray7.ChildNodes[j];
                                        for (int k = 0; k < decarray77.ChildNodes.Count; k++)
                                        {
                                            numIndices2++;
                                        }                                        
                                    }
                                }

                                //ingresando datos en array
                                if (numIndices==numIndices2)
                                {
                                    int[,,] newarray = new int[Int32.Parse(dimension_0.valor.ToString()), Int32.Parse(dimension_1.valor.ToString()), Int32.Parse(dimension_2.valor.ToString())];
                                    for (int i = 0; i < decarray5.ChildNodes.Count; i++)
                                    {
                                        ParseTreeNode array7 = decarray5.ChildNodes[i];
                                        for (int j = 0; j < array7.ChildNodes.Count; j++)
                                        {
                                            ParseTreeNode decarray77 = array7.ChildNodes[j];
                                            for (int k = 0; k < decarray77.ChildNodes.Count; k++)
                                            {
                                                resultado valor_indice = ejecutar(decarray77.ChildNodes[k], local);
                                                newarray[i,j,k] = Int32.Parse(valor_indice.valor.ToString());
                                            }                                              
                                        }
                                    }
                                    local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, newarray, "3dimension"));
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("Numero de elementos no concuerdan con tamaño ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }
                            }
                            else
                            {   //se agrega array sin definir valores
                                int[, ,] newarray = new int[Int32.Parse(dimension_0.valor.ToString()), Int32.Parse(dimension_1.valor.ToString()), Int32.Parse(dimension_2.valor.ToString())];
                                local.insertar(new simbolo(nomArr, "array", "array", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, newarray, "3dimension"));
                            }
                        }
                    }                  

                    #endregion 
                    return null;

                case "ASIGARRAY":
                    String nombreArr = r.ChildNodes[0].FindTokenAndGetText(); //nombre del arreglo a reasignar valor
                    ParseTreeNode iobtarray2 = r.ChildNodes[1]; //accedo a ASIGARRAY2

                    if (iobtarray2 != null)
                    {
                        if (iobtarray2.ChildNodes.Count == 1) //UNA DIMENSION
                        {
                            ParseTreeNode obtarray3 = iobtarray2.ChildNodes[0]; 
                            resultado expIndice = ejecutar(obtarray3.ChildNodes[0], local); //indice a cambiar
                            resultado nuevoValorIndice = ejecutar(r.ChildNodes[2],local); //nuevo valor de indice

                            //busco el array
                            simbolo miArr = local.obtener(nombreArr);

                            if (miArr!=null)
                            {
                                Object obtenerArray = miArr.o_array; //obtengo el mero array

                                if (miArr.dimension.Equals("1dimension"))
                                {
                                    int[] arr = (int[])miArr.o_array;
                                    try
                                    {
                                        arr[Int32.Parse(expIndice.valor.ToString())]=Int32.Parse(nuevoValorIndice.valor.ToString());
                                    }
                                    catch (System.IndexOutOfRangeException)
                                    {
                                        singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                    }
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }                             }
                            else
                            {
                                singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            }
                            
                        }
                        else if (iobtarray2.ChildNodes.Count == 2)
                        {
                            ParseTreeNode obtarray3_0 = iobtarray2.ChildNodes[0];
                            ParseTreeNode obtarray3_1 = iobtarray2.ChildNodes[1];
                            resultado expIndice_0 = ejecutar(obtarray3_0.ChildNodes[0], local); //indice 0
                            resultado expIndice_1 = ejecutar(obtarray3_1.ChildNodes[0], local); //indice 1
                            resultado nuevoValorIndice = ejecutar(r.ChildNodes[2], local); //nuevo valor de indice

                            //busco el array
                            simbolo miArr = local.obtener(nombreArr);

                            if (miArr != null)
                            {
                                Object obtenerArray = miArr.o_array; //obtengo el mero array

                                if (miArr.dimension.Equals("2dimension"))
                                {
                                    int[,] arr = (int[,])miArr.o_array;
                                    try
                                    {
                                        arr[Int32.Parse(expIndice_0.valor.ToString()), Int32.Parse(expIndice_1.valor.ToString())]=Int32.Parse(nuevoValorIndice.valor.ToString());
                                    }
                                    catch (System.IndexOutOfRangeException)
                                    {
                                        singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                    }
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                } 
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            }
                        }
                        else if (iobtarray2.ChildNodes.Count == 3)
                        {
                            ParseTreeNode obtarray3_0 = iobtarray2.ChildNodes[0];
                            ParseTreeNode obtarray3_1 = iobtarray2.ChildNodes[1];
                            ParseTreeNode obtarray3_2 = iobtarray2.ChildNodes[2];
                            resultado expIndice_0 = ejecutar(obtarray3_0.ChildNodes[0], local); //indice 0
                            resultado expIndice_1 = ejecutar(obtarray3_1.ChildNodes[0], local); //indice 1
                            resultado expIndice_2 = ejecutar(obtarray3_2.ChildNodes[0], local); //indice 2
                            resultado nuevoValorIndice = ejecutar(r.ChildNodes[2], local); //nuevo valor de indice

                            //busco el array
                            simbolo miArr = local.obtener(nombreArr);

                            if (miArr != null)
                            {
                                Object obtenerArray = miArr.o_array; //obtengo el mero array

                                if (miArr.dimension.Equals("3dimension"))
                                {
                                    int[, ,] arr = (int[, ,])miArr.o_array;
                                    try
                                    {
                                        arr[Int32.Parse(expIndice_0.valor.ToString()), Int32.Parse(expIndice_1.valor.ToString()), Int32.Parse(expIndice_2.valor.ToString())]=Int32.Parse(nuevoValorIndice.valor.ToString());
                                    }
                                    catch (System.IndexOutOfRangeException)
                                    {
                                        singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                    }
                                }
                                else
                                {
                                    singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                } 
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            }
                        }
                    }

                    return null;

                case "OBTARRAY":
                    #region obtarray
                    String nomArra = r.ChildNodes[0].FindTokenAndGetText(); //nombre del array a obtener indice
                    ParseTreeNode obtarray2 = r.ChildNodes[1];

                    if (obtarray2.ChildNodes.Count==1) //1 DIMENSION
                    {
                        ParseTreeNode obtarray3 = obtarray2.ChildNodes[0];
                        resultado expIndice = ejecutar(obtarray3.ChildNodes[0], local); //obtengo el valor del indice

                        //obtengo el array
                        simbolo miArr = local.obtener(nomArra);

                        if (miArr!=null)
                        {
                            Object obtenerArray = miArr.o_array; //obtengo el mero array

                            if (miArr.dimension.Equals("1dimension"))
                            {
                                int[] arr = (int[]) miArr.o_array;
                                try
                                {
                                    int val = arr[Int32.Parse(expIndice.valor.ToString())];
                                    return new resultado(val, "tInt", miArr.linea, miArr.columna);
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                    singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }                                
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            }                            
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        }
                    }
                    else if (obtarray2.ChildNodes.Count == 2) // 2 DIMENSIONES
                    {
                        ParseTreeNode obtarray3_0 = obtarray2.ChildNodes[0];
                        ParseTreeNode obtarray3_1 = obtarray2.ChildNodes[1];
                        resultado expIndice_0 = ejecutar(obtarray3_0.ChildNodes[0], local); //obtengo el valor del indice 0
                        resultado expIndice_1 = ejecutar(obtarray3_1.ChildNodes[0], local); //obtengo el valor del indice 1

                        //obtengo el array
                        simbolo miArr = local.obtener(nomArra);

                        if (miArr != null)
                        {
                            Object obtenerArray = miArr.o_array; //obtengo el mero array

                            if (miArr.dimension.Equals("2dimension"))
                            {
                                int[,] arr = (int[,])miArr.o_array;
                                try
                                {
                                    int val = arr[Int32.Parse(expIndice_0.valor.ToString()), Int32.Parse(expIndice_1.valor.ToString())];
                                    return new resultado(val, "tInt", miArr.linea, miArr.columna);
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                    singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            }                             
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        }

                    }
                    else if (obtarray2.ChildNodes.Count == 3) // 3 DIMENSIONES
                    {
                        ParseTreeNode obtarray3_0 = obtarray2.ChildNodes[0];
                        ParseTreeNode obtarray3_1 = obtarray2.ChildNodes[1];
                        ParseTreeNode obtarray3_2 = obtarray2.ChildNodes[2];
                        resultado expIndice_0 = ejecutar(obtarray3_0.ChildNodes[0], local); //obtengo el valor del indice 0
                        resultado expIndice_1 = ejecutar(obtarray3_1.ChildNodes[0], local); //obtengo el valor del indice 1
                        resultado expIndice_2 = ejecutar(obtarray3_2.ChildNodes[0], local); //obtengo el valor del indice 2

                        //obtengo el array
                        simbolo miArr = local.obtener(nomArra);

                        if (miArr != null)
                        {
                            Object obtenerArray = miArr.o_array; //obtengo el mero array

                            if (miArr.dimension.Equals("3dimension"))
                            {
                                int[,,] arr = (int[,,])miArr.o_array;
                                try
                                {
                                    int val = arr[Int32.Parse(expIndice_0.valor.ToString()), Int32.Parse(expIndice_1.valor.ToString()), Int32.Parse(expIndice_2.valor.ToString())];
                                    return new resultado(val, "tInt", miArr.linea, miArr.columna);
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                    singlentonError.registrarError(new errorS("Indice fuera de rango ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                }
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("No coinciden indices ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            } 
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        }
                    }

                    #endregion
                    return null;

/*--AUMENTO--*/case "AUMENTO":
                    resultado aumen = ejecutar(r.ChildNodes[0],local);//obtengo el valor de la expresion
                    ParseTreeNode identaum = r.ChildNodes[0];
                    String identaum2 = identaum.ChildNodes[0].FindTokenAndGetText(); //obtengo el nombre del identificador para actualizar
                    if (aumen != null)
                    {
                        switch (aumen.tipo.ToString())
                        {
                            case "tCarac":
                                int suma = Encoding.ASCII.GetBytes(aumen.valor.ToString())[0] + 1;
                                simbolo simboloaum = new simbolo(identaum2, suma, "tInt", aumen.linea, aumen.columna);
                                local.asignarValor(simboloaum);
                                return null;
                            case "tCad":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "tInt":
                                int suma2 = Int32.Parse(aumen.valor.ToString()) + 1;
                                simbolo simboloaum2 = new simbolo(identaum2, suma2, "tInt", aumen.linea, aumen.columna);
                                local.asignarValor(simboloaum2);
                                return null;
                            case "tDouble":
                                int suma3 = Int32.Parse(aumen.valor.ToString()) + 1;
                                simbolo simboloaum3 = new simbolo(identaum2, suma3, "tDouble", aumen.linea, aumen.columna);
                                local.asignarValor(simboloaum3);
                                return null;
                            case "true":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "false":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "null":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.Span.Location.Line, r.Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        return null;
                    }
                    return null;

/*--DECREMENT--*/case "DECREMENT":
                    resultado decrem = ejecutar(r.ChildNodes[0], local);//obtengo el valor de la expresion
                    ParseTreeNode identdec = r.ChildNodes[0];
                    String identdec2 = identdec.ChildNodes[0].FindTokenAndGetText(); //obtengo el nombre del identificador para actualizar
                    if (decrem != null)
                    {
                        switch (decrem.tipo.ToString())
                        {
                            case "tCarac":
                                int suma = Encoding.ASCII.GetBytes(decrem.valor.ToString())[0] - 1;
                                simbolo simbolodec = new simbolo(identdec2, suma, "tInt", decrem.linea, decrem.columna);
                                local.asignarValor(simbolodec);
                                return null;
                            case "tCad":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "tInt":
                                int suma2 = Int32.Parse(decrem.valor.ToString()) - 1;
                                simbolo simbolodec2 = new simbolo(identdec2, suma2, "tInt", decrem.linea, decrem.columna);
                                local.asignarValor(simbolodec2);
                                return null;
                            case "tDouble":
                                int suma3 = Int32.Parse(decrem.valor.ToString()) - 1;
                                simbolo simbolodec3 = new simbolo(identdec2, suma3, "tDouble", decrem.linea, decrem.columna);
                                local.asignarValor(simbolodec3);
                                return null;
                            case "true":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "false":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            case "null":
                                singlentonError.registrarError(new errorS("El identificador para aumentar debe ser numerico ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;

                            default:
                                break;
                        }
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.Span.Location.Line, r.Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        return null;
                    }
                    return null;

/*--SENTFOR--*/ case "SENTFOR":
                    entorno nuevoentornofor = new entorno(local);
                    resultado entfor = ejecutar(r.ChildNodes[0], nuevoentornofor); //entrada del for
                    resultado condfor = ejecutar(r.ChildNodes[1],nuevoentornofor); //condicion del for
                    ParseTreeNode instrucFor=r.ChildNodes[3]; //instrucciones
                    Boolean banderaForBrake = false;
                    Boolean banderaForContinue = false;

                    if (condfor!=null)
                    {
                        if (condfor.tipo.ToString().Equals("true") || condfor.tipo.ToString().Equals("false"))
                        {
                            while ((Boolean)condfor.valor==true)
                            {
                                foreach (ParseTreeNode hijoActual in instrucFor.ChildNodes) //ejecutamos las Instrucciones
                                {
                                    resultado resultFor = ejecutar(hijoActual, nuevoentornofor);//ejecuta un if, log, alert o lo que venga
                                                                       
                                    if (hijoActual.Term.ToString().Equals("MANIPFLUJO")) //por si viene un brake o continue, que no este dentro de un if
                                    {
                                        ParseTreeNode nue = hijoActual.ChildNodes[0];
                                        if (nue.FindTokenAndGetText().Equals("break"))
                                        {
                                            banderaForBrake = true;
                                            break;
                                        }
                                        else if (nue.FindTokenAndGetText().Equals("continue")) //se sale del foreach
                                        {
                                            banderaForContinue = true;
                                            break;
                                        }                                        
                                    }
                                    else if (hijoActual.Term.ToString().Equals("RETURN"))
                                    {
                                        return ejecutar(hijoActual.ChildNodes[0], local);
                                    }

                                    if (resultFor != null)
                                    {
                                        if (resultFor.valor.ToString().Equals("break")) //se sale del foreach
                                        {
                                            banderaForBrake = true;
                                            break;
                                        }
                                        else if (resultFor.valor.ToString().Equals("continue") && resultFor.tipo.ToString().Equals("continue")) //se sale del foreach
                                        {
                                            banderaForContinue = true;
                                            break;
                                        }
                                        //este es para el return
                                        else if (resultFor.tipo.ToString().Equals("tInt") || resultFor.tipo.ToString().Equals("tCarac") || resultFor.tipo.ToString().Equals("tCad") ||
                                            resultFor.tipo.ToString().Equals("tDouble") /*|| otps.tipo.ToString().Equals("true") || otps.tipo.ToString().Equals("false")*/)
                                        {
                                            return resultFor;
                                        }
                                    }
                                }

                                if (banderaForBrake == true) //se sale del while
                                {
                                    break;
                                }
                                if (banderaForContinue == true) //se sale del while pero continua (ya no ejecuta las siguientes instrucciones)
                                {
                                    ejecutar(r.ChildNodes[2], nuevoentornofor); // actualiza valor
                                    condfor = ejecutar(r.ChildNodes[1], nuevoentornofor); //actualiza la condicion
                                    continue;
                                }
                                //ejecutar(r.ChildNodes[3], nuevoentornofor); // ejecuta lista de sentencias
                                ejecutar(r.ChildNodes[2], nuevoentornofor); // actualiza valor
                                condfor = ejecutar(r.ChildNodes[1], nuevoentornofor); //verificar condicion
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("La condicion debe ser de tipo Boolean ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].ChildNodes[0].FindTokenAndGetText()));
                            return null;
                        }
                    }

                    return null;

/*-SENTWHILE-*/ case "SENTWHILE":
                    resultado valw = ejecutar(r.ChildNodes[0],local); //valor de la condicion
                    ParseTreeNode instrucciones = r.ChildNodes[1];// accedo al nodo que contiene como hijos todas las instrucciones
                    Boolean banderaWhileBrake = false;
                    Boolean banderaWhileContinue = false;

                    if (valw!=null)
                    {
                        if (valw.tipo.ToString().Equals("true") || valw.tipo.ToString().Equals("false"))
                        {
                            while ((Boolean)valw.valor == true)
                            {
                                
                                foreach (ParseTreeNode hijoActual in instrucciones.ChildNodes) //ejecutamos las Instrucciones
                                {                                    

                                    if (hijoActual.Term.ToString().Equals("MANIPFLUJO")) //por si viene un brake o continue, que no este dentro de un if
                                    {
                                        ParseTreeNode nue = hijoActual.ChildNodes[0];
                                        if (nue.FindTokenAndGetText().Equals("break"))
                                        {
                                            banderaWhileBrake = true;
                                            break;
                                        }
                                        else if (nue.FindTokenAndGetText().Equals("continue")) //se sale del foreach
                                        {
                                            banderaWhileContinue = true;
                                            break;
                                        }

                                    }
                                    else if (hijoActual.Term.ToString().Equals("RETURN"))
                                    {
                                        return ejecutar(hijoActual.ChildNodes[0], local);
                                    }

                                    resultado result = ejecutar(hijoActual, local);//ejecuta un if, log, alert o lo que venga

                                    if (result!=null)
                                    {
                                        if (result.valor.ToString().Equals("break")) //se sale del foreach
                                        {
                                            banderaWhileBrake = true;
                                            break;
                                        }
                                        else if (result.valor.ToString().Equals("continue")) //se sale del foreach
                                        {
                                            banderaWhileContinue = true;
                                            break;
                                        }
                                        //este es para el return
                                        else if (result.tipo.ToString().Equals("tInt") || result.tipo.ToString().Equals("tCarac") || result.tipo.ToString().Equals("tCad") ||
                                            result.tipo.ToString().Equals("tDouble") /*|| otps.tipo.ToString().Equals("true") || otps.tipo.ToString().Equals("false")*/)
                                        {
                                            return result;
                                        }
                                    }
                                    
                                }
                                if (banderaWhileBrake==true) //se sale del while
                                {
                                    break;
                                }
                                if (banderaWhileContinue==true) //se sale del while pero continua (ya no ejecuta las siguientes instrucciones)
                                {
                                    valw = ejecutar(r.ChildNodes[0], local); //actualiza la condicion
                                    continue;
                                }
                                valw = ejecutar(r.ChildNodes[0], local); //actualiza la condicion
                                
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("La condicion debe ser de tipo Boolean ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            return null;
                        }
                        
                    }                    
                    return null;

/*SENTDOWHILE*/ case "SENTDOWHILE":
                    resultado valdw = ejecutar(r.ChildNodes[1], local); //valor de la condicion

                    ParseTreeNode instruccionesdoW = r.ChildNodes[0];// accedo al nodo que contiene como hijos todas las instrucciones
                    Boolean banderaDoWhileBrake = false;
                    Boolean banderaDoWhileContinue = false;

                    //se ejecuta al menos una vez en el doWhile
                    foreach (ParseTreeNode hijoActual in instruccionesdoW.ChildNodes) //ejecutamos las Instrucciones
                    {
                        resultado result = ejecutar(hijoActual, local);//ejecuta un if, log, alert o lo que venga

                        if (hijoActual.Term.ToString().Equals("MANIPFLUJO")) //por si viene un brake o continue, que no este dentro de un if
                        {
                            ParseTreeNode nue = hijoActual.ChildNodes[0];
                            if (nue.FindTokenAndGetText().Equals("break"))
                            {
                                banderaDoWhileBrake = true;
                                break;
                            }
                            else if (nue.FindTokenAndGetText().Equals("continue")) //se sale del foreach
                            {
                                banderaDoWhileContinue = true;
                                break;
                            }
                        }
                        else if (hijoActual.Term.ToString().Equals("RETURN"))
                        {
                            return ejecutar(hijoActual.ChildNodes[0], local);
                        }

                        if (result != null)
                        {
                            if (result.valor.ToString().Equals("break")) //se sale del foreach
                            {
                                banderaDoWhileBrake = true;
                                break;
                            }
                            else if (result.valor.ToString().Equals("continue")) //se sale del foreach
                            {
                                banderaDoWhileContinue = true;
                                break;
                            }
                            //este es para el return
                            else if (result.tipo.ToString().Equals("tInt") || result.tipo.ToString().Equals("tCarac") || result.tipo.ToString().Equals("tCad") ||
                                result.tipo.ToString().Equals("tDouble") /*|| otps.tipo.ToString().Equals("true") || otps.tipo.ToString().Equals("false")*/)
                            {
                                return result;
                            }
                        }
                        valdw = ejecutar(r.ChildNodes[1], local); //actualiza condicion (pueden a ver asignaciones)
                    }
                    if (banderaDoWhileBrake == true) //se sale del while
                    {
                        return null;
                    }
                    if (banderaDoWhileContinue == true) //se sale del while pero continua (ya no ejecuta las siguientes instrucciones)
                    {
                        //valdw = ejecutar(r.ChildNodes[1], local); //actualiza la condicion
                        return null;
                    }
                    
                    //aqui es donde ya entrara en el ciclo si se cumple
                    if (valdw != null)
                    {
                        if (valdw.tipo.ToString().Equals("true") || valdw.tipo.ToString().Equals("false"))
                        {
                            while ((Boolean)valdw.valor == true)
                            {
                                foreach (ParseTreeNode hijoActual in instruccionesdoW.ChildNodes) //ejecutamos las Instrucciones
                                {
                                    resultado result = ejecutar(hijoActual, local);//ejecuta un if, log, alert o lo que venga

                                    if (hijoActual.Term.ToString().Equals("MANIPFLUJO")) //por si viene un brake o continue, que no este dentro de un if
                                    {
                                        ParseTreeNode nue = hijoActual.ChildNodes[0];
                                        if (nue.FindTokenAndGetText().Equals("break"))
                                        {
                                            banderaDoWhileBrake = true;
                                            break;
                                        }
                                        else if (nue.FindTokenAndGetText().Equals("continue")) //se sale del foreach
                                        {
                                            banderaWhileContinue = true;
                                            break;
                                        }
                                    }
                                    else if (hijoActual.Term.ToString().Equals("RETURN"))
                                    {
                                        return ejecutar(hijoActual.ChildNodes[0], local);
                                    }

                                    if (result != null)
                                    {
                                        if (result.valor.ToString().Equals("break")) //se sale del foreach
                                        {
                                            banderaDoWhileBrake = true;
                                            break;
                                        }
                                        else if (result.valor.ToString().Equals("continue")) //se sale del foreach
                                        {
                                            banderaDoWhileContinue = true;
                                            break;
                                        }
                                        //este es para el return
                                        else if (result.tipo.ToString().Equals("tInt") || result.tipo.ToString().Equals("tCarac") || result.tipo.ToString().Equals("tCad") ||
                                            result.tipo.ToString().Equals("tDouble") /*|| otps.tipo.ToString().Equals("true") || otps.tipo.ToString().Equals("false")*/)
                                        {
                                            return result;
                                        }
                                    }
                                }
                                if (banderaDoWhileBrake == true) //se sale del while
                                {
                                    break;
                                }
                                if (banderaDoWhileContinue == true) //se sale del while pero continua (ya no ejecuta las siguientes instrucciones)
                                {
                                    valdw = ejecutar(r.ChildNodes[1], local); //actualiza la condicion
                                    continue;
                                }
                                valdw = ejecutar(r.ChildNodes[1], local); //actualiza la condicion
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("La condicion debe ser de tipo Boolean ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            return null;
                        }

                    }

                    return null;

/*SENTIF*/      case "SENTIF":
                    //este contiene 3 hijos, condicion, lista de instrucciones y nodo que contiene lista de if else
                    resultado conif = ejecutar(r.ChildNodes[0],local); //ejecutamos la condicion
                    if (conif!=null)
                    {
                        if (conif.tipo.ToString().Equals("true") || conif.tipo.ToString().Equals("false"))
                        {
                            if ((Boolean)conif.valor==true)
                            {
                                ParseTreeNode intif = r.ChildNodes[1]; //accedo al nodo instruccion
                                foreach (ParseTreeNode hijoActual in intif.ChildNodes) //ejecutamos las Instrucciones
                                {
                                    if (hijoActual.Term.ToString().Equals("MANIPFLUJO"))
                                    {
                                        if (hijoActual.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                        {
                                            return new resultado("break", "break", 0, 0);
                                        }else//sera continue
                                        {
                                            return new resultado("continue", "continue", 0, 0);
                                        }
                                    }
                                    else if (hijoActual.Term.ToString().Equals("RETURN"))
                                    {
                                        return ejecutar(hijoActual.ChildNodes[0],local);
                                    }
                                    else
                                    {
                                        if (hijoActual.Term.ToString().Equals("DECLIST"))//para que no nos retorne cuando se declare una variable dentro de la funcion
                                        {
                                            ejecutar(hijoActual, local);
                                        }
                                        else
                                        {                                        
                                            //esto es por si viene un if if break, cada vez que el valor sea break retornara break o continue
                                            resultado otps=ejecutar(hijoActual,local);
                                            if (otps!=null)
                                            {
                                                if (otps.valor.ToString().Equals("break") && otps.tipo.ToString().Equals("break"))
                                                {
                                                    return otps;
                                                }
                                                else if (otps.valor.ToString().Equals("continue") && otps.tipo.ToString().Equals("continue"))
                                                {
                                                    return otps;
                                                }
                                                //este es para el return
                                                else if (otps.tipo.ToString().Equals("tInt") || otps.tipo.ToString().Equals("tCarac") || otps.tipo.ToString().Equals("tCad") ||
                                                    otps.tipo.ToString().Equals("tDouble") /*|| otps.tipo.ToString().Equals("true") || otps.tipo.ToString().Equals("false")*/)
                                                {
                                                    return otps;
                                                }
                                            } 
                                        }
                                    }

                                }
                                //ejecutar(r.ChildNodes[1],local); //como la condicion es true se ejecuta la lista de instrucciones
                                                                 // y los else if y el else quedan obsoletos
                            }
                            else
                            {
                                resultado if2=ejecutar(r.ChildNodes[2],local); // se ejecuta el nodo SENTIF2 que contiene una lista de nodos con los else if o solo el else
                                if (if2!=null)
                                {
                                    if (if2.valor.ToString().Equals("break") && if2.tipo.ToString().Equals("break"))
                                    {
                                        return new resultado("break", "break", 0, 0);
                                    }
                                    else if (if2.valor.ToString().Equals("continue") && if2.tipo.ToString().Equals("continue"))
                                    {
                                        return new resultado("continue", "continue", 0, 0);
                                    }
                                    //este es para el return
                                    else if (if2.tipo.ToString().Equals("tInt") || if2.tipo.ToString().Equals("tCarac") || if2.tipo.ToString().Equals("tCad") ||
                                        if2.tipo.ToString().Equals("tDouble") /*|| if2.tipo.ToString().Equals("true") || if2.tipo.ToString().Equals("false")*/)
                                    {
                                        return if2;
                                    }
                                }
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("La condicion debe ser de tipo Boolean ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            return null;
                        }
                        
                    }

                    return null;

/*SENTIF2*/     case "SENTIF2":
                    if (r.ChildNodes.Count>0) //si es mayor que cero, contiene uno o varios else if o solo un else
                    {
                        foreach (ParseTreeNode hijoActual in r.ChildNodes)
                        {
                            resultado senif=ejecutar(hijoActual, local); //este ejecuta SENTIF3, que nos devolvera un valor                            
                            if (senif!=null)
                            {                                
                                if (senif.valor.ToString().Equals("break") && senif.tipo.ToString().Equals("break"))
                                {
                                    return new resultado("break", "break", 0, 0);
                                }
                                else if (senif.valor.ToString().Equals("continue") && senif.tipo.ToString().Equals("continue"))
                                {
                                    return new resultado("continue", "continue", 0, 0);
                                }
                                //este es para el return
                                else if (senif.tipo.ToString().Equals("tInt") || senif.tipo.ToString().Equals("tCarac") || senif.tipo.ToString().Equals("tCad") ||
                                    senif.tipo.ToString().Equals("tDouble") /*|| senif.tipo.ToString().Equals("true") || senif.tipo.ToString().Equals("false")*/)
                                {
                                    return senif;
                                }
                                else if ((Boolean)senif.valor==true) // quiere decir que en el else if donde entro retorno true, y los else if restantes quedan obsoletos
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    
                    return null;

/*SENTIF3*/     case "SENTIF3":

                    if (r.ChildNodes.Count==1) //quiere decir tiene un solo hijo, entonces es solamente el ELSE
                    {
                        ParseTreeNode instruccionelse = r.ChildNodes[0];
                        foreach (ParseTreeNode hijoActual in instruccionelse.ChildNodes)
                        {
                            
                            if (hijoActual.Term.ToString().Equals("MANIPFLUJO"))
                            {
                                if (hijoActual.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                {
                                    return new resultado("break", "break", 0, 0);
                                }
                                else//sera continue
                                {
                                    return new resultado("continue", "continue", 0, 0);
                                }
                            }
                            else if (hijoActual.Term.ToString().Equals("RETURN"))
                            {
                                return ejecutar(hijoActual.ChildNodes[0], local);
                            }
                            else
                            {
                                resultado resultelse = ejecutar(hijoActual, local);
                                
                                if (resultelse != null)
                                {
                                    Console.WriteLine(resultelse.tipo.ToString());
                                    if (resultelse.valor.ToString().Equals("break") && resultelse.tipo.ToString().Equals("break"))
                                    {
                                        return new resultado("break", "break", 0, 0);
                                    }
                                    else if (resultelse.valor.ToString().Equals("continue") && resultelse.tipo.ToString().Equals("continue"))
                                    {
                                        return new resultado("continue", "continue", 0, 0);
                                    }
                                    //este es para el return
                                    else if (resultelse.tipo.ToString().Equals("tInt") || resultelse.tipo.ToString().Equals("tCarac") || resultelse.tipo.ToString().Equals("tCad") ||
                                        resultelse.tipo.ToString().Equals("tDouble") /*|| resultelse.tipo.ToString().Equals("true") || resultelse.tipo.ToString().Equals("false")*/)
                                    {
                                        return resultelse;
                                    }
                                }
                            }
                        }
                            //ejecutar(r.ChildNodes[0], local); //se ejectua las instrucciones
                    }
                    else //quiere decir que es un ELSE IF
                    {
                        resultado condif = ejecutar(r.ChildNodes[0], local); //ejecutamos la condicion
                        if (condif!=null)
                        {
                            if (condif.tipo.ToString().Equals("true") || condif.tipo.ToString().Equals("false"))
                            {
                                if ((Boolean)condif.valor==true)
                                {
                                    ParseTreeNode instelseif = r.ChildNodes[1]; //accedo al nodo instrucciones
                                    foreach (ParseTreeNode hijoActual in instelseif.ChildNodes) //recorro instrucciones
                                    {
                                        if (hijoActual.Term.ToString().Equals("MANIPFLUJO"))
                                        {
                                            if (hijoActual.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                            {
                                                return new resultado("break", "break", 0, 0);
                                            }
                                            else//sera continue
                                            {
                                                return new resultado("continue", "continue", 0, 0);
                                            }
                                        }
                                        else if (hijoActual.Term.ToString().Equals("RETURN"))
                                        {
                                            return ejecutar(hijoActual.ChildNodes[0], local);
                                        }
                                        else
                                        {
                                            resultado resultelseif = ejecutar(hijoActual, local);
                                            if (resultelseif != null)
                                            {
                                                if (resultelseif.valor.ToString().Equals("break") && resultelseif.tipo.ToString().Equals("break"))
                                                {
                                                    return new resultado("break", "break", 0, 0);
                                                }
                                                else if (resultelseif.valor.ToString().Equals("continue") && resultelseif.tipo.ToString().Equals("continue"))
                                                {
                                                    return new resultado("continue", "continue", 0, 0);
                                                }
                                                //este es para el return
                                                else if (resultelseif.tipo.ToString().Equals("tInt") || resultelseif.tipo.ToString().Equals("tCarac") || resultelseif.tipo.ToString().Equals("tCad") ||
                                                    resultelseif.tipo.ToString().Equals("tDouble") /*|| resultelseif.tipo.ToString().Equals("true") || resultelseif.tipo.ToString().Equals("false")*/)
                                                {
                                                    return resultelseif;
                                                }
                                            }
                                        }
                                    }
                                    //ejecutar(r.ChildNodes[1], local); //como la condicion es true se ejecuta la lista de instrucciones
                                }
                                //retornamos la condicion para verificar en SENTIF2 si es verdadera o falsa, si retorna esto no vino ningun break ni continue
                                //if (condif.tipo.ToString().Equals("true"))
                                //{
                                //    return new resultado("btrue", "btrue", 0, 0);//bandera para poder retornar booleanos, sino nos da problemas
                                //}
                                //else if (condif.tipo.ToString().Equals("false"))
                                //{
                                //    return new resultado("bfalse", "bfalse", 0, 0);//bandera para poder retornar booleanos, sino nos da problemas
                                //}
                                return condif;
                            }
                            else
                            {
                                singlentonError.registrarError(new errorS("La condicion debe ser de tipo Boolean ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                return null;
                            }
                        }
                    }

                    return null;

/*SWITCH*/      case "SENTWIT":
                    //este contiene 3 hijos, condicion, lista de instrucciones y nodo que contiene lista de if else
                    resultado consw = ejecutar(r.ChildNodes[0], local); //la expresion
                    ParseTreeNode sentwit2 = r.ChildNodes[1]; //accedo al nodo SENTWIT2, este contiene como hijos los SENTWIT3 que son los case
                    Boolean bandera = false;

                    if (consw!=null)
                    {
                        if (sentwit2.ChildNodes.Count > 0) //verifica si tiene case
                        {
                            foreach (ParseTreeNode hijoActual in sentwit2.ChildNodes) //ejecutamos los case
                            {
                                resultado expresc = ejecutar(hijoActual.ChildNodes[0], local); //se ejecuta la expresion del case
                                if (expresc!=null)
                                {
                                    if (consw.tipo.ToString().Equals(expresc.tipo.ToString())) //si los tipos son iguales
                                    {
                                        if (consw.valor.ToString().Equals(expresc.valor.ToString())) //si los valores son iguales ejecuta instrucciones
                                        {
                                            ParseTreeNode instsw = hijoActual.ChildNodes[1]; //accedo al nodo de instrucciones del case actual

                                            foreach (ParseTreeNode hijoActualIns in instsw.ChildNodes) //ejecutamos las instrucciones de case actual
                                            {
                                                if (hijoActualIns.Term.ToString().Equals("MANIPFLUJO"))
                                                {
                                                    if (hijoActualIns.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                                    {
                                                        return new resultado("break", "break", 0, 0);
                                                    }
                                                    else//sera continue
                                                    {
                                                        return new resultado("continue", "continue", 0, 0);
                                                    }
                                                }
                                                else if (hijoActualIns.Term.ToString().Equals("RETURN"))
                                                {
                                                    return ejecutar(hijoActualIns.ChildNodes[0], local);
                                                }
                                                else
                                                {
                                                    resultado resultsw = ejecutar(hijoActualIns, local);
                                                    if (resultsw != null)
                                                    {
                                                        if (resultsw.valor.ToString().Equals("break") && resultsw.tipo.ToString().Equals("break"))
                                                        {
                                                            return new resultado("break", "break", 0, 0);
                                                        }
                                                        else if (resultsw.valor.ToString().Equals("continue") && resultsw.tipo.ToString().Equals("continue"))
                                                        {
                                                            return new resultado("continue", "continue", 0, 0);
                                                        }
                                                        //este es para el return
                                                        else if (resultsw.tipo.ToString().Equals("tInt") || resultsw.tipo.ToString().Equals("tCarac") || resultsw.tipo.ToString().Equals("tCad") ||
                                                            resultsw.tipo.ToString().Equals("tDouble") /*|| resultelseif.tipo.ToString().Equals("true") || resultelseif.tipo.ToString().Equals("false")*/)
                                                        {
                                                            return resultsw;
                                                        }
                                                    }
                                                }
                                            }
                                            //ejecutar(hijoActual.ChildNodes[1],local);//se ejecutan las instrucciones del case actual
                                            bandera = true; //se activa bandera para no ejecutar default
                                            //break; //me salgo del foreach
                                        }
                                    }
                                    else
                                    {
                                        singlentonError.registrarError(new errorS("Tipo de dato del case no coincide ", expresc.linea, expresc.columna, expresc.valor.ToString()));
                                        return null;
                                    }
                                }
                            }

                            if (bandera==false)
                            {
                                ParseTreeNode instrucciondef = r.ChildNodes[2];
                                foreach (ParseTreeNode hijoActualIns in instrucciondef.ChildNodes) //ejecutamos las instrucciones de case actual
                                {
                                    if (hijoActualIns.Term.ToString().Equals("MANIPFLUJO"))
                                    {
                                        if (hijoActualIns.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                        {
                                            return new resultado("break", "break", 0, 0);
                                        }
                                        else//sera continue
                                        {
                                            return new resultado("continue", "continue", 0, 0);
                                        }
                                    }
                                    else if (hijoActualIns.Term.ToString().Equals("RETURN"))
                                    {
                                        return ejecutar(hijoActualIns.ChildNodes[0], local);
                                    }
                                    else
                                    {
                                        resultado resultsw = ejecutar(hijoActualIns, local); // se ejecuta el default
                                        if (resultsw != null)
                                        {
                                            if (resultsw.valor.ToString().Equals("break") && resultsw.tipo.ToString().Equals("break"))
                                            {
                                                return new resultado("break", "break", 0, 0);
                                            }
                                            else if (resultsw.valor.ToString().Equals("continue") && resultsw.tipo.ToString().Equals("continue"))
                                            {
                                                return new resultado("continue", "continue", 0, 0);
                                            }
                                            //este es para el return
                                            else if (resultsw.tipo.ToString().Equals("tInt") || resultsw.tipo.ToString().Equals("tCarac") || resultsw.tipo.ToString().Equals("tCad") ||
                                                resultsw.tipo.ToString().Equals("tDouble") /*|| resultelseif.tipo.ToString().Equals("true") || resultelseif.tipo.ToString().Equals("false")*/)
                                            {
                                                return resultsw;
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }
                        else//sino tiene case, ejecuta solo el default
                        {
                            ParseTreeNode instrucciondef = r.ChildNodes[2];
                            foreach (ParseTreeNode hijoActualIns in instrucciondef.ChildNodes) //ejecutamos las instrucciones de case actual
                            {
                                if (hijoActualIns.Term.ToString().Equals("MANIPFLUJO"))
                                {
                                    if (hijoActualIns.ChildNodes[0].FindTokenAndGetText().Equals("break"))//si es break;
                                    {
                                        return new resultado("break", "break", 0, 0);
                                    }
                                    else//sera continue
                                    {
                                        return new resultado("continue", "continue", 0, 0);
                                    }
                                }
                                else if (hijoActualIns.Term.ToString().Equals("RETURN"))
                                {
                                    return ejecutar(hijoActualIns.ChildNodes[0], local);
                                }
                                else
                                {
                                    resultado resultsw = ejecutar(hijoActualIns, local); // se ejecuta el default
                                    if (resultsw != null)
                                    {
                                        if (resultsw.valor.ToString().Equals("break") && resultsw.tipo.ToString().Equals("break"))
                                        {
                                            return new resultado("break", "break", 0, 0);
                                        }
                                        else if (resultsw.valor.ToString().Equals("continue") && resultsw.tipo.ToString().Equals("continue"))
                                        {
                                            return new resultado("continue", "continue", 0, 0);
                                        }
                                        //este es para el return
                                        else if (resultsw.tipo.ToString().Equals("tInt") || resultsw.tipo.ToString().Equals("tCarac") || resultsw.tipo.ToString().Equals("tCad") ||
                                            resultsw.tipo.ToString().Equals("tDouble") /*|| resultelseif.tipo.ToString().Equals("true") || resultelseif.tipo.ToString().Equals("false")*/)
                                        {
                                            return resultsw;
                                        }
                                    }
                                }
                            }
                        }
                    }     

                    return null;

/*FUNCTION*/   case "FUNCTION":
                    //se guarda como un simbolo mas, solo que tambien se guardan los nodos con los parametros, y nodo de instrucciones
                    String idFunction = r.ChildNodes[0].FindTokenAndGetText();
                    simbolo simFunct = new simbolo(idFunction, "function", "function", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[1], r.ChildNodes[2]);
                    local.insertar(simFunct);
                    return null;

/*METODO*/      case "METODO":
                    //se guarda como un simbolo mas, solo que tambien se guardan los nodos con los parametros, y nodo de instrucciones
                    String idMetodo = r.ChildNodes[0].FindTokenAndGetText();
                    simbolo simMet = new simbolo(idMetodo, "metodo", "metodo", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[1], r.ChildNodes[2]);
                    local.insertar(simMet);
                    return null;

/*LLAMADA*/    case "LLAMADA":
                    String idLlamada = r.ChildNodes[0].FindTokenAndGetText();
                    ParseTreeNode parametrosActuales = r.ChildNodes[1];//nodo que tiene como hijos los paremetros actuales
                    simbolo funcionOMetodo = local.obtener(idLlamada); //obtengo la funcion

                    if (funcionOMetodo!=null)
                    {
                        ParseTreeNode parametrosFormales = funcionOMetodo.parametrosF; //obtengo nodo de parametros formales de la funcion
                        if (parametrosActuales.ChildNodes.Count == parametrosFormales.ChildNodes.Count)
                        {
                            if (funcionOMetodo.tipo.ToString().Equals("function"))
                            {
                                //creamos nuevo entorno
                                entorno entorLlamada = new entorno(local);
                                //guardamos variables formales con el valor de las variables locales
                                for (int i = 0; i < parametrosActuales.ChildNodes.Count; i++)//recorremos hasta el tamanio de lo hijos
                                {
                                    Boolean bander = false;
                                    String idParamF = parametrosFormales.ChildNodes[i].FindTokenAndGetText();//obtengo el id del parametro formal

                                    //verificamos si es array
                                    simbolo arr = local.obtener(parametrosActuales.ChildNodes[i].FindTokenAndGetText());
                                    if (arr != null)
                                    {
                                        if (arr.tipo.Equals("array"))
                                        {
                                            entorLlamada.insertar(new simbolo(idParamF, arr.valor, arr.tipo, arr.linea, arr.columna, arr.o_array,arr.dimension));
                                            bander = true;
                                        }
                                        else if (arr.tipo.Equals("class"))
                                        {
                                            entorLlamada.insertar(new simbolo(idParamF, arr.valor, arr.tipo, arr.linea, arr.columna, arr.dec, arr.l_funciones_metodos, arr.l_declaraciones));
                                            bander = true;
                                        }
                                    }
                                    //sino es array ni class es expresion y se ejecuta
                                    if (bander == false)
                                    {
                                        resultado valParamA = ejecutar(parametrosActuales.ChildNodes[i], local);//ejecutamos para que nos devuelva el valor del parametro actual
                                        entorLlamada.insertar(new simbolo(idParamF, valParamA.valor, valParamA.tipo, valParamA.linea, valParamA.columna)); //insertamos en el entorno
                                    }  
                                }
                                //ejecutamos las instrucciones
                                ParseTreeNode instruccionesLlamada = funcionOMetodo.instrucciones;
                                foreach (ParseTreeNode hijoActualIns in instruccionesLlamada.ChildNodes) //ejecutamos las instrucciones de la funcion
                                {
                                    if (hijoActualIns.Term.ToString().Equals("RETURN"))
                                    {
                                        resultado resulRet = ejecutar(hijoActualIns.ChildNodes[0], entorLlamada);//ejecutamos la expresion que tiene return
                                        //Console.WriteLine(resulRet.valor);
                                        return resulRet; //retornamos el valor
                                    }
                                    else
                                    {
                                        if (hijoActualIns.Term.ToString().Equals("DECLIST"))//para que no nos retorne cuando se declare una variable dentro de la funcion
                                        {
                                            ejecutar(hijoActualIns, entorLlamada);
                                        }
                                        else
                                        {
                                            resultado resultsw = ejecutar(hijoActualIns, entorLlamada);
                                            if (resultsw != null)
                                            {
                                                //si el resultado es una expresion la retornamos, sera un valor de RETURN
                                                if (resultsw.tipo.ToString().Equals("tInt") || resultsw.tipo.ToString().Equals("tCarac") || resultsw.tipo.ToString().Equals("tCad") ||
                                                resultsw.tipo.ToString().Equals("tDouble") /*|| resultsw.tipo.ToString().Equals("true") || resultsw.tipo.ToString().Equals("false")*/)
                                                {
                                                    return resultsw;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            else if (funcionOMetodo.tipo.ToString().Equals("metodo"))
                            {
                                //creamos nuevo entorno
                                entorno entorLlamada = new entorno(local);
                                //guardamos variables formales con el valor de las variables locales
                                for (int i = 0; i < parametrosActuales.ChildNodes.Count; i++)//recorremos hasta el tamanio de lo hijos
                                {
                                    Boolean bander = false;
                                    String idParamF = parametrosFormales.ChildNodes[i].FindTokenAndGetText();//obtengo el id del parametro formal
                                    
                                    //verificamos si es array
                                    simbolo arr = local.obtener(parametrosActuales.ChildNodes[i].FindTokenAndGetText());
                                    if (arr!=null)
                                    {
                                        if (arr.tipo.Equals("array"))
                                        {
                                            entorLlamada.insertar(new simbolo(idParamF,arr.valor,arr.tipo,arr.linea,arr.columna,arr.o_array,arr.dimension));
                                            bander = true;
                                        }
                                        else if (arr.tipo.Equals("class"))
                                        {
                                            entorLlamada.insertar(new simbolo(idParamF,arr.valor,arr.tipo,arr.linea,arr.columna,arr.dec,arr.l_funciones_metodos,arr.l_declaraciones));
                                            bander = true;
                                        }
                                    }
                                    //sino es array ni class es expresion y se ejecuta
                                    if (bander==false)
                                    {
                                        resultado valParamA = ejecutar(parametrosActuales.ChildNodes[i], local);//ejecutamos para que nos devuelva el valor del parametro actual
                                        entorLlamada.insertar(new simbolo(idParamF, valParamA.valor, valParamA.tipo, valParamA.linea, valParamA.columna)); //insertamos en el entorno
                                    }                                        
                                }

                                //ejecutamos las instrucciones
                                ParseTreeNode instruccionesLlamada = funcionOMetodo.instrucciones;

                                foreach (ParseTreeNode hijoActualIns in instruccionesLlamada.ChildNodes) //ejecutamos las instrucciones del metodo
                                {
                                    resultado resultsw = ejecutar(hijoActualIns, entorLlamada);
                                }
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("El numero de parametros no coincide ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        }
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        return null;
                    }                    
                    
                    return null;

/*CLASS*/       case "CLASS":
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

/*DECCLASS*/    case "DECCLASS":
                    String idObj = r.ChildNodes[0].FindTokenAndGetText();
                    String idClase = r.ChildNodes[1].FindTokenAndGetText();

                    simbolo miClass = local.obtener(idClase);

                    //se crea una clonacion de la clase con el nuevo id
                    if (miClass!=null)
                    {
                        if (miClass.tipo.Equals("class"))
                        {
                            local.insertar(new simbolo(idObj, "class", "class", r.Span.Location.Line, r.Span.Location.Column, miClass.dec, miClass.l_funciones_metodos,miClass.l_declaraciones));
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No hace referencia a ninguna clase ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                            return null;
                        }                        
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                        return null;
                    }

                    return null;

                case "REASIGCLASS":
                    String idClasee = r.ChildNodes[0].FindTokenAndGetText(); //id nombre de la clase
                    String nomVar = r.ChildNodes[1].FindTokenAndGetText(); //nombre de la variable a cambiar valor
                    resultado nuevaAsig = ejecutar(r.ChildNodes[2],local); //nueva expresion para asignar

                    simbolo miclass = local.obtener(idClasee); //obtengo la clase

                    if (miclass!=null)
                    {
                        Dictionary<String, Object> listDec = miclass.dec;
                        if (listDec.ContainsKey(nomVar))
                        {
                            listDec.Remove(nomVar);
                            listDec.Add(nomVar, nuevaAsig);
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                        }
                        
                        //foreach (ParseTreeNode item in listDec)
                        //{
                        //    if (item.ChildNodes[0].Term.ToString().Equals("DECLIST2")) //de la forma, vara=12;
                        //    {
                        //        ParseTreeNode dec2 = item.ChildNodes[0];
                        //        if (dec2 != null)
                        //        {
                        //            if (dec2.ChildNodes[0].FindTokenAndGetText().Equals(nomVar))//si encuentra variable
                        //            {
                        //                dec2.ChildNodes[1] = nuevaAsig;
                        //            }
                        //        }
                        //    }
                        //    else //de la forma, var b;
                        //    {

                        //    }                            
                        //}
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                    }

                    return null;

                case "REASIGCLASS2":
                    String idClaseee = r.ChildNodes[0].FindTokenAndGetText(); //id nombre de la clase
                    String nomVarr = r.ChildNodes[1].FindTokenAndGetText(); //nombre de la variable a cambiar valor

                    simbolo miclasss = local.obtener(idClaseee); //obtengo la clase

                    if (miclasss!=null)
                    {
                        Dictionary<String,Object> listDec = miclasss.dec;
                        if (listDec.ContainsKey(nomVarr))
                        {
                            resultado aja = (resultado)listDec[nomVarr];
                            return aja;
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                        }
                        
                        //foreach (ParseTreeNode item in listDec)
                        //{
                        //    if (item.ChildNodes[0].Term.ToString().Equals("DECLIST2")) //de la forma, vara=12;
                        //    {
                        //        ParseTreeNode dec2 = item.ChildNodes[0];
                        //        if (dec2 != null)
                        //        {
                        //            return ejecutar(dec2.ChildNodes[1], local); //retornamos la expresion
                        //        }
                        //    }
                        //    else //de la forma, var b;
                        //    {

                        //    }                            
                        //}
                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                    }

                    return null;

/*LLAMCLASS_METFUN*/case "LLAMCLASS_METFUN":
                    String nomClase = r.ChildNodes[0].FindTokenAndGetText();
                    String nomMetFun = r.ChildNodes[1].FindTokenAndGetText();
                    ParseTreeNode decLlam = r.ChildNodes[2]; //nodo con lista de declaraciones
                    Boolean bandellamclass = false;

                    simbolo classOb = local.obtener(nomClase);

                    if (classOb != null)
                    {
                        if (classOb.tipo.Equals("class"))
                        {
                            List<ParseTreeNode> metfunc = classOb.l_funciones_metodos;
                            foreach (ParseTreeNode hijoActual in metfunc) //recorremos la lista de metodos y funciones
                            {
                                if (hijoActual.ChildNodes[0].FindTokenAndGetText().ToLower().Equals(nomMetFun.ToLower()))//si encontramos el metodo o funcion
                                {
                                    bandellamclass = true;
                                    if (hijoActual.Term.ToString().Equals("FUNCTION"))//verificamos si es funcion
                                    {                                        
                                        ParseTreeNode listDecF=hijoActual.ChildNodes[1];
                                        if (decLlam.ChildNodes.Count == listDecF.ChildNodes.Count) //verificasmos si tiene el mismo numero de parametros
                                        {
                                            //creamos nuevo entorno
                                            entorno entorLlamada = new entorno(local);

                                            //guardamos las variables globales que tiene la clase en el nuevo entorno
                                            foreach (ParseTreeNode hadec in classOb.l_declaraciones)
                                            {
                                                ejecutar(hadec,entorLlamada);
                                            }

                                            //guardamos variables formales con el valor de las variables locales
                                            for (int i = 0; i < listDecF.ChildNodes.Count; i++)//recorremos hasta el tamanio de lo hijos
                                            {
                                                String idParamF = listDecF.ChildNodes[i].FindTokenAndGetText();//obtengo el id del parametro formal
                                                resultado valParamA = ejecutar(decLlam.ChildNodes[i], local);//ejecutamos para que nos devuelva el valor del parametro actual
                                                entorLlamada.insertar(new simbolo(idParamF, valParamA.valor, valParamA.tipo, valParamA.linea, valParamA.columna)); //insertamos en el entorno
                                            }
                                            //ejecutamos las instrucciones
                                            ParseTreeNode instFun = hijoActual.ChildNodes[2];
                                            foreach (ParseTreeNode hijoActualIns in instFun.ChildNodes) //ejecutamos las instrucciones de la funcion
                                            {
                                                if (hijoActualIns.Term.ToString().Equals("RETURN"))
                                                {
                                                    resultado resulRet = ejecutar(hijoActualIns.ChildNodes[0], entorLlamada);//ejecutamos la expresion que tiene return
                                                    //Console.WriteLine(resulRet.valor);
                                                    return resulRet; //retornamos el valor
                                                }
                                                else
                                                {
                                                    if (hijoActualIns.Term.ToString().Equals("DECLIST"))//para que no nos retorne cuando se declare una variable dentro de la funcion
                                                    {
                                                        ejecutar(hijoActualIns, entorLlamada);
                                                    }
                                                    else
                                                    {
                                                        resultado resultsw = ejecutar(hijoActualIns, entorLlamada);
                                                        if (resultsw != null)
                                                        {
                                                            //si el resultado es una expresion la retornamos, sera un valor de RETURN
                                                            if (resultsw.tipo.ToString().Equals("tInt") || resultsw.tipo.ToString().Equals("tCarac") || resultsw.tipo.ToString().Equals("tCad") ||
                                                            resultsw.tipo.ToString().Equals("tDouble") /*|| resultsw.tipo.ToString().Equals("true") || resultsw.tipo.ToString().Equals("false")*/)
                                                            {
                                                                return resultsw;
                                                            }
                                                        }
                                                    }
                                                }

                                            }

                                        }
                                        else
                                        {
                                            singlentonError.registrarError(new errorS("El numero de parametros no coincide ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                                        }
                                    }
                                    else if (hijoActual.Term.ToString().Equals("METODO"))
                                    {
                                        ParseTreeNode listDecF = hijoActual.ChildNodes[1];
                                        if (decLlam.ChildNodes.Count == listDecF.ChildNodes.Count) //verificasmos si tiene el mismo numero de parametros
                                        {
                                            //creamos nuevo entorno
                                            entorno entorLlamada = new entorno(local);

                                            //guardamos las variables globales que tiene la clase en el nuevo entorno
                                            foreach (ParseTreeNode hadec in classOb.l_declaraciones)
                                            {
                                                ejecutar(hadec, entorLlamada);
                                            }

                                            //guardamos variables formales con el valor de las variables locales
                                            for (int i = 0; i < listDecF.ChildNodes.Count; i++)//recorremos hasta el tamanio de lo hijos
                                            {
                                                String idParamF = listDecF.ChildNodes[i].FindTokenAndGetText();//obtengo el id del parametro formal
                                                resultado valParamA = ejecutar(decLlam.ChildNodes[i], local);//ejecutamos para que nos devuelva el valor del parametro actual
                                                entorLlamada.insertar(new simbolo(idParamF, valParamA.valor, valParamA.tipo, valParamA.linea, valParamA.columna)); //insertamos en el entorno
                                            }
                                            //ejecutamos las instrucciones
                                            ParseTreeNode instMet = hijoActual.ChildNodes[2];
                                            foreach (ParseTreeNode hijoActualIns in instMet.ChildNodes) //ejecutamos las instrucciones de la funcion
                                            {
                                                resultado instm = ejecutar(hijoActualIns,entorLlamada);
                                            }
                                        }
                                        else
                                        {
                                            singlentonError.registrarError(new errorS("El numero de parametros no coincide ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                                        }
                                    }                                    
                                }
                            }
                            if (bandellamclass == false)
                            {
                                singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[1].Span.Location.Line, r.ChildNodes[1].Span.Location.Column, r.ChildNodes[1].FindTokenAndGetText()));
                            }
                        }
                        else
                        {
                            singlentonError.registrarError(new errorS("No hace referencia a ninguna clase ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                            return null;
                        }

                    }
                    else
                    {
                        singlentonError.registrarError(new errorS("No existe el identificador ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        return null;
                    }
                    return null;

/*IMPORT*/      case "IMPORT":

                    resultado ruta = ejecutar(r.ChildNodes[0],local);
                    try
                    {
                        string text = System.IO.File.ReadAllText(@ruta.valor.ToString());
                        //Console.WriteLine(text);
                        interprete_import newImport = new interprete_import(local); //instanciamos clase y mandamos entorno que seria global(local)
                        newImport.leerImport(text); //ejecutamos para que guarde
                    }
                    catch (Exception)
                    {
                        singlentonError.registrarError(new errorS("No existe ruta en el import ", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                        return null;
                    }                    
                    return null;

/*MAIN*/        case "MAIN":
                    ParseTreeNode instrucMain=r.ChildNodes[0];
                    foreach (ParseTreeNode hijoactual in instrucMain.ChildNodes)
                    {
                        ejecutar(hijoactual,local);
                    }
                    return null;

/*-- EA --*/    case "EA":
                    if (r.ChildNodes.Count==1) //(si solo tiene un hijo) ej: EA->tInt
                    {
                        if (r.ChildNodes[0].Term.ToString().Equals("EA")) //para cuando vienen parentesis (EN ARBOL ESTA EA->EA)
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("AUMENTO"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("DECREMENT"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("LLAMADA"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("LLAMCLASS_METFUN"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("REASIGCLASS2"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else if (r.ChildNodes[0].Term.ToString().Equals("OBTARRAY"))
                        {
                            return ejecutar(r.ChildNodes[0], local);
                        }
                        else
                        {                        
                            switch (r.ChildNodes[0].Term.ToString())//tipo de dato a retornar
                            {
                                case "tCarac" :
                                    Object val0 = r.ChildNodes[0].FindTokenAndGetText().Substring(1,r.ChildNodes[0].FindTokenAndGetText().Length-2).ToString(); //retornara el valor de la expresion (quito ' ')
                                    return new resultado(val0, "tCarac", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "tCad":
                                    Object val1 = r.ChildNodes[0].FindTokenAndGetText().Substring(1,r.ChildNodes[0].FindTokenAndGetText().Length-2).ToString(); //retornara el valor de la expresion (quito comillas)
                                    return new resultado(val1, "tCad", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "tInt" :
                                    Object val2 = r.ChildNodes[0].FindTokenAndGetText().ToString(); //retornara el valor de la expresion
                                    return new resultado(val2, "tInt", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "tDouble":
                                    Object val3 = r.ChildNodes[0].FindTokenAndGetText().ToString(); //retornara el valor de la expresion
                                    return new resultado(val3, "tDouble", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "true":
                                    return new resultado(true, "true", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "false":
                                    return new resultado(false, "false", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "null":
                                    return new resultado(null, "null", r.ChildNodes[0].Span.Location.Line, r.ChildNodes[0].Span.Location.Column);
                                case "tId":
                                    simbolo valId = local.obtener(r.ChildNodes[0].FindTokenAndGetText());
                                    
                                    if (valId != null)
                                    {
                                        return new resultado(valId.valor, valId.tipo, valId.linea, valId.columna);
                                    }
                                    else
                                    {
                                        singlentonError.registrarError(new errorS("No existe el identificador ", r.Span.Location.Line, r.Span.Location.Column, r.ChildNodes[0].FindTokenAndGetText()));
                                        return null;
                                    }
                                default:
                                    break;
                            }
                        }


                    }
                    else if (r.ChildNodes.Count == 2) //cuando dos hijos es tipo: !true
                    {
                        resultado val21 = ejecutar(r.ChildNodes[1], local);//ejecutamos hijo derecho para saber valor
                        if (val21!=null)
                        {
                            if (val21.tipo.Equals("true"))
                            {
                                return new resultado(false, "false", val21.linea, val21.columna);//como viene true se cambia a false
                            }else
                            {
                                return new resultado(true, "true", val21.linea, val21.columna);//como viene false se cambia a true
                            }
                        }
                    }
                    else if(r.ChildNodes.Count==3) //cuando tiene tres hijos
                    {
                        resultado val1= ejecutar(r.ChildNodes[0],local);
                        resultado val2= ejecutar(r.ChildNodes[2], local);

                        if (val1 != null)
                        {
                            if (val2 != null)
                            {
                                switch (val1.tipo) //tipo de dato del primer valor que nos retorna
                                {
                                    case "tInt":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //int op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + char
                                                        int sumc0 = Int32.Parse(val1.valor.ToString()) + Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc0, "tInt", val2.linea, val2.columna);
                                                    case "-": //int - char
                                                        int sumc1 = Int32.Parse(val1.valor.ToString()) - Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc1, "tInt", val2.linea, val2.columna);
                                                    case "*": //int * char
                                                        int sumc2 = Int32.Parse(val1.valor.ToString()) * Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc2, "tInt", val2.linea, val2.columna);
                                                    case "/": //int / char
                                                        int sumc3 = Int32.Parse(val1.valor.ToString()) / Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc3, "tInt", val2.linea, val2.columna);
                                                    case "pow": //int pow char
                                                        Object pow = Math.Pow(Int32.Parse(val1.valor.ToString()), Int32.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tInt", val2.linea, val2.columna);
                                                    case ">"://int > char
                                                        if ((Int32.Parse(val1.valor.ToString())) > (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://int < char
                                                        if ((Int32.Parse(val1.valor.ToString())) < (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://int == char
                                                        if ((Int32.Parse(val1.valor.ToString())) == (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://int <> char
                                                        if ((Int32.Parse(val1.valor.ToString())) != (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://int >= char
                                                        if ((Int32.Parse(val1.valor.ToString())) >= (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://int <= char
                                                        if ((Int32.Parse(val1.valor.ToString())) <= (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://int or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //int op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + int
                                                        int sumi1 = Int32.Parse(val1.valor.ToString()) + Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi1, "tInt", val2.linea, val2.columna);
                                                    case "-": //int - int
                                                        int sumi2 = Int32.Parse(val1.valor.ToString()) - Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi2, "tInt", val2.linea, val2.columna);
                                                    case "*": //int * int
                                                        int sumi4 = Int32.Parse(val1.valor.ToString()) * Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi4, "tInt", val2.linea, val2.columna);
                                                    case "/": //int / int
                                                        int sumi5 = Int32.Parse(val1.valor.ToString()) / Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi5, "tInt", val2.linea, val2.columna);
                                                    case "pow": //int pow int
                                                        Object pow = Math.Pow(Int32.Parse(val1.valor.ToString()), Int32.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tInt", val2.linea, val2.columna);
                                                    case ">"://int > int
                                                        if ((Int32.Parse(val1.valor.ToString())) > (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://int < int
                                                        if ((Int32.Parse(val1.valor.ToString())) < (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://int == int
                                                        if ((Int32.Parse(val1.valor.ToString())) == (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://int <> int
                                                        if ((Int32.Parse(val1.valor.ToString())) != (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://int >= int
                                                        if ((Int32.Parse(val1.valor.ToString())) >= (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://int <= int
                                                        if ((Int32.Parse(val1.valor.ToString())) <= (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://int or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return null;
                                                    case "&&"://int and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //int op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + string
                                                        String sum2 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sum2, "tCad", val2.linea, val2.columna);
                                                    case "-": //int - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return null;
                                                    case "*": //int * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //int / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //int pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://int > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://int < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://int == string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://int <> string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://int >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://int <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://int or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //int op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + double
                                                        Double sumd0 = Double.Parse(val1.valor.ToString()) + Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd0, "tDouble", val2.linea, val2.columna);
                                                    case "-": //int - double
                                                        Double sumd1 = Double.Parse(val1.valor.ToString()) - Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd1, "tDouble", val2.linea, val2.columna);
                                                    case "*": //int * double
                                                        Double sumd2 = Double.Parse(val1.valor.ToString()) * Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd2, "tDouble", val2.linea, val2.columna);
                                                    case "/": //int / double
                                                        Double sumd3 = Double.Parse(val1.valor.ToString()) / Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd3, "tDouble", val2.linea, val2.columna);
                                                    case "pow": //int pow double
                                                        Object pow = Math.Pow(Double.Parse(val1.valor.ToString()), Double.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tDouble", val2.linea, val2.columna);
                                                    case ">"://int > double
                                                        if (Double.Parse(val1.valor.ToString()) > (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://int < double
                                                        if (Double.Parse(val1.valor.ToString()) < (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://int == double
                                                        if (Double.Parse(val1.valor.ToString()) == (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://int <> double
                                                        if (Double.Parse(val1.valor.ToString()) != (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://int >= double
                                                        if (Double.Parse(val1.valor.ToString()) >= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://int <= double
                                                        if (Double.Parse(val1.valor.ToString()) <= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://int or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // int op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //int - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //int * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //int / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //int pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://int > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://int < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://int == true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://int <> true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://int >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://int <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://int or true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // int op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //int - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //int * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //int / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //int pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://int > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://int < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://int == false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://int <> false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://int >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://int <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://int or false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // int op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //int + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //int - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //int * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //int / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //int pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://int > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://int < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://int == null
                                                        //singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        }                                                       
                                                    case "<>"://int <> null
                                                        //singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        }
                                                        
                                                    case ">="://int >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://int <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://int or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://int and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://int xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://int not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Int con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "tCad":

                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //string op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + char
                                                        String sumc0 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumc0, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - char
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * char
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / char
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow char
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == char
                                                        if (val1.ToString().Equals(val2.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://string <> char
                                                        if (!val1.ToString().Equals(val2.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://string >= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //string op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + int
                                                        String sumi1 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumi1, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - int
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * int
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / int
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow int
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://string <> int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://string >= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //string op string

                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + string
                                                        String sum2 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sum2, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - string
                                                        singlentonError.registrarError(new errorS("Error al Restar String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == string                                                
                                                        if (val1.valor.ToString().Equals(val2.valor.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://string <> string
                                                        if (!val1.valor.ToString().Equals(val2.valor.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://string >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //string op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + double
                                                        String sumd0 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumd0, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - double
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * double
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / double
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow double
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://string <> double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://string >= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // string op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + true
                                                        String sumd0 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumd0, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - true
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://string <> true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://string >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // string op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + false
                                                        String sumd0 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumd0, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - false
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://string <> false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://string >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // string op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //string + null
                                                        String sumd0 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sumd0, "tCad", val2.linea, val2.columna);
                                                    case "-": //string - null
                                                        singlentonError.registrarError(new errorS("Error al Restar String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //string * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //string / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //string pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://string > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://string < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://string == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://string <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://string >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://string <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://string or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://string and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://string xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://string not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, String con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "tCarac":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //char op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + char
                                                        int sumc0 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] + Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc0, "tInt", val2.linea, val2.columna);
                                                    case "-": //char - char
                                                        int sumc1 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] - Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc1, "tInt", val2.linea, val2.columna);
                                                    case "*": //char * char
                                                        int sumc2 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] * Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc2, "tInt", val2.linea, val2.columna);
                                                    case "/": //char / char
                                                        int sumc3 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] / Encoding.ASCII.GetBytes(val2.valor.ToString())[0];
                                                        return new resultado(sumc3, "tInt", val2.linea, val2.columna);
                                                    case "pow": //char pow char
                                                        Object pow = Math.Pow(Encoding.ASCII.GetBytes(val1.valor.ToString())[0], Encoding.ASCII.GetBytes(val2.valor.ToString())[0]);
                                                        return new resultado(pow, "tInt", val2.linea, val2.columna);
                                                    case ">"://char > char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) > (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://char < char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) < (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://char == char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) == (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://char <> char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) != (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://char >= char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) >= (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://char <= char
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) <= (Encoding.ASCII.GetBytes(val2.valor.ToString())[0]))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://char or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //char op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + int
                                                        int sumi1 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] + Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi1, "tInt", val2.linea, val2.columna);
                                                    case "-": //char - int
                                                        int sumi2 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] - Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi2, "tInt", val2.linea, val2.columna);
                                                    case "*": //char * int
                                                        int sumi4 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] * Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi4, "tInt", val2.linea, val2.columna);
                                                    case "/": //char / int
                                                        int sumi5 = Encoding.ASCII.GetBytes(val1.valor.ToString())[0] / Int32.Parse(val2.valor.ToString());
                                                        return new resultado(sumi5, "tInt", val2.linea, val2.columna);
                                                    case "pow": //char pow int
                                                        Object pow = Math.Pow(Encoding.ASCII.GetBytes(val1.valor.ToString())[0], Int32.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tInt", val2.linea, val2.columna);
                                                    case ">"://char > int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) > (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://char < int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) < (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://char == int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) == (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://char <> int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) != (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://char >= int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) >= (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://char <= int
                                                        if ((Encoding.ASCII.GetBytes(val1.valor.ToString())[0]) <= (Int32.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://char or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //char op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + string
                                                        String sum2 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sum2, "tCad", val2.linea, val2.columna);
                                                    case "-": //char - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //char * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //char / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Int Char String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //char pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Int Char String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://char > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://char < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://char == string
                                                        if (val1.valor.ToString().Equals(val2.valor.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://char <> string
                                                        if (!val1.valor.ToString().Equals(val2.valor.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://char >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://char <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://char or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //char op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + double
                                                        Double sumd0 = Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString()) + Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd0, "tDouble", val2.linea, val2.columna);
                                                    case "-": //char - double
                                                        Double sumd1 = Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString()) - Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd1, "tDouble", val2.linea, val2.columna);
                                                    case "*": //char * double
                                                        Double sumd2 = Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString()) * Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd2, "tDouble", val2.linea, val2.columna);
                                                    case "/": //char / double
                                                        Double sumd3 = Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString()) / Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd3, "tDouble", val2.linea, val2.columna);
                                                    case "pow": //char pow double
                                                        Object pow = Math.Pow(Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString()), Double.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tDouble", val2.linea, val2.columna);
                                                    case ">"://char > double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) > (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://char < double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) < (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://char == double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) == (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://char <> double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) != (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://char >= double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) >= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://char <= double
                                                        if ((Double.Parse(Encoding.ASCII.GetBytes(val1.valor.ToString())[0].ToString())) <= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://char or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // char op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //char - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //char * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //char / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //char pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://char > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://char < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://char == true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://char <> true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://char >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://char <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://char or true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // char op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //char - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //char * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //char / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //char pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://char > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://char < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://char == false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://char <> false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://char >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://char <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://char or false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // char op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //char + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //char - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //char * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //char / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //char pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://char > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://char < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://char == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://char <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://char >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://char <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://char or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://char and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://char xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://char not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Char con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "tDouble":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //double op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + char
                                                        Double sumc0 = Double.Parse(val1.valor.ToString()) + Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString());
                                                        return new resultado(sumc0, "tDouble", val2.linea, val2.columna);
                                                    case "-": //double - char
                                                        Double sumc1 = Double.Parse(val1.valor.ToString()) + Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString());
                                                        return new resultado(sumc1, "tDouble", val2.linea, val2.columna);
                                                    case "*": //double * char
                                                        Double sumc2 = Double.Parse(val1.valor.ToString()) + Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString());
                                                        return new resultado(sumc2, "tDouble", val2.linea, val2.columna);
                                                    case "/": //double / char
                                                        Double sumc3 = Double.Parse(val1.valor.ToString()) + Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString());
                                                        return new resultado(sumc3, "tDouble", val2.linea, val2.columna);
                                                    case "pow": //double pow char
                                                        Object pow = Math.Pow(Double.Parse(val1.valor.ToString()), Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString()));
                                                        return new resultado(pow, "tDouble", val2.linea, val2.columna);
                                                    case ">"://double > char
                                                        if ((Double.Parse(val1.valor.ToString())) > (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://double < char
                                                        if ((Double.Parse(val1.valor.ToString())) < (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://double == char
                                                        if ((Double.Parse(val1.valor.ToString())) == (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://double <> char
                                                        if ((Double.Parse(val1.valor.ToString())) != (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://double >= char
                                                        if ((Double.Parse(val1.valor.ToString())) >= (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://double <= char
                                                        if ((Double.Parse(val1.valor.ToString())) <= (Double.Parse(Encoding.ASCII.GetBytes(val2.valor.ToString())[0].ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://double or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //double op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + int
                                                        Double sumi1 = Double.Parse(val1.valor.ToString()) + Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumi1, "tInt", val2.linea, val2.columna);
                                                    case "-": //double - int
                                                        Double sumi2 = Double.Parse(val1.valor.ToString()) - Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumi2, "tInt", val2.linea, val2.columna);
                                                    case "*": //double * int
                                                        Double sumi4 = Double.Parse(val1.valor.ToString()) * Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumi4, "tInt", val2.linea, val2.columna);
                                                    case "/": //double / int
                                                        Double sumi5 = Double.Parse(val1.valor.ToString()) / Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumi5, "tInt", val2.linea, val2.columna);
                                                    case "pow": //double pow int
                                                        Object pow = Math.Pow(Double.Parse(val1.valor.ToString()), Double.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tInt", val2.linea, val2.columna);
                                                    case ">"://double > int
                                                        if ((Double.Parse(val1.valor.ToString())) > (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://double < int
                                                        if ((Double.Parse(val1.valor.ToString())) < (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://double == int
                                                        if ((Double.Parse(val1.valor.ToString())) == (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://double <> int
                                                        if ((Double.Parse(val1.valor.ToString())) != (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://double >= int
                                                        if ((Double.Parse(val1.valor.ToString())) >= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://double <= int
                                                        if ((Double.Parse(val1.valor.ToString())) <= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://double or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //double op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + string
                                                        String sum2 = val1.valor.ToString() + val2.valor.ToString();
                                                        return new resultado(sum2, "tCad", val2.linea, val2.columna);
                                                    case "-": //double - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //double * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //double / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //double pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://double > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://double < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://double == string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://double <> string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://double >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://double <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://double or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //double op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + double
                                                        Double sumd0 = Double.Parse(val1.valor.ToString()) + Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd0, "tDouble", val2.linea, val2.columna);
                                                    case "-": //double - double
                                                        Double sumd1 = Double.Parse(val1.valor.ToString()) - Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd1, "tDouble", val2.linea, val2.columna);
                                                    case "*": //double * double
                                                        Double sumd2 = Double.Parse(val1.valor.ToString()) * Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd2, "tDouble", val2.linea, val2.columna);
                                                    case "/": //double / double
                                                        Double sumd3 = Double.Parse(val1.valor.ToString()) / Double.Parse(val2.valor.ToString());
                                                        return new resultado(sumd3, "tDouble", val2.linea, val2.columna);
                                                    case "pow": //double pow double
                                                        Object pow = Math.Pow(Double.Parse(val1.valor.ToString()), Double.Parse(val2.valor.ToString()));
                                                        return new resultado(pow, "tDouble", val2.linea, val2.columna);
                                                    case ">"://double > double
                                                        if ((Double.Parse(val1.valor.ToString())) > (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<"://double < double
                                                        if ((Double.Parse(val1.valor.ToString())) < (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "=="://double == double
                                                        if ((Double.Parse(val1.valor.ToString())) == (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<>"://double <> double
                                                        if ((Double.Parse(val1.valor.ToString())) != (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case ">="://double >= double
                                                        if ((Double.Parse(val1.valor.ToString())) >= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "<="://double <= double
                                                        if ((Double.Parse(val1.valor.ToString())) <= (Double.Parse(val2.valor.ToString())))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else { return new resultado(false, "false", val2.linea, val2.columna); }
                                                    case "||"://double or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // double op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //double - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //double * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //double / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //double pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://double > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://double < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://double == true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://double <> true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://double >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://double <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://double or true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // double op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //double - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //double * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //double / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //double pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://double > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://double < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://double == false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://double <> false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://double >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://double <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://double or false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // double op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //double + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //double - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //double * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //double / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //double pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://double > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://double < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://double == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://double <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://double >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://double <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://double or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://double and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://double xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://double not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Double con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "true":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //true op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + char
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - char
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * char
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / char
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow char
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://true and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://true xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://true not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //true op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + int
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - int
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * int
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / int
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow int
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://true and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://true xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://true not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //true op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + string
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://true and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://true xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://true not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //true op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + double
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - double
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * double
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / double
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow double
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://true and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://true xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://true not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // true op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or true
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "&&"://true and true
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "^"://true xor true
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "!"://true not true
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // true op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://true <> false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://true >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or false
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "&&"://true and false
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "^"://true xor false
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "!"://true not false
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // true op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //true + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //true - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //true * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //true / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //true pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://true > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://true < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://true == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://true <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://true >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://true <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://true or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://true and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://true xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://true not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "false":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //false op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + char
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - char
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * char
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / char
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow char
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://false <> char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://false >= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://false and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://false xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://false not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //false op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + int
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - int
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * int
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / int
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow int
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://false <> int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://false >= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://false and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://false xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://false not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //false op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + string
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://false <> string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://false >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://false and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://false xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://false not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //false op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + double
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - double
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * double
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / double
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow double
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<>"://false <> double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">="://false >= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://false and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://false xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://false not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // false op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == true
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://false <> true
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://false >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or true
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "&&"://false and true
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "^"://false xor true
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    case "!"://false not true
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // false op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == false
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://false <> false
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://false >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or false
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "&&"://false and false
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "^"://false xor false
                                                        return new resultado(false, "false", val2.linea, val2.columna);
                                                    case "!"://false not false
                                                        return new resultado(true, "true", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // false op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //false + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //false - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //false * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //false / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //false pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://false > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://false < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://false == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://false <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://false >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://false <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://false or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://false and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://false xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://false not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Boolean con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    case "null":
                                        switch (val2.tipo) //tipo de dato del segundo valor que nos retorna
                                        {
                                            case "tCarac": //null op char
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + char
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - char
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * char
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / char
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow char
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == char
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> char
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not char
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Char ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tInt": //null op int
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + int
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - int
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * int
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / int
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow int
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == int
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> int
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not int
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Int ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tCad": //null op string
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + string
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - string
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * string
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / string
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow string
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == string
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> string
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not string
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con String ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tDouble": //null op double
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + double
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - double
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * double
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / double
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow double
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == double
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> double
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not double
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Double ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "true": // null op true                                        
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + true
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - true
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * true
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / true
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow true
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == true
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> true
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not true
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "false": // null op false
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //null + false
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //null - false
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //null * false
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //null / false
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //null pow false
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == false
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> false
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not false
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Boolean ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "null": // Null op null
                                                switch (r.ChildNodes[1].FindTokenAndGetText())
                                                {
                                                    case "+": //Null + null
                                                        singlentonError.registrarError(new errorS("Error al Sumar Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "-": //Null - null
                                                        singlentonError.registrarError(new errorS("Error al Restar Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "*": //Null * null
                                                        singlentonError.registrarError(new errorS("Error al Multiplicar Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "/": //Null / null
                                                        singlentonError.registrarError(new errorS("Error al Dividir Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "pow": //Null pow null
                                                        singlentonError.registrarError(new errorS("Error en Potencia Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case ">"://null > null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<"://null < null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "=="://null == null
                                                        if (val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case "<>"://null <> null
                                                        if (!val1.tipo.ToString().Equals(val2.tipo.ToString()))
                                                        {
                                                            return new resultado(true, "true", val2.linea, val2.columna);
                                                        }
                                                        else
                                                        {
                                                            return new resultado(false, "false", val2.linea, val2.columna);
                                                        } 
                                                    case ">="://null >= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "<="://null <= null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Relacional, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "||"://null or null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "&&"://null and null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "^"://null xor null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    case "!"://null not null
                                                        singlentonError.registrarError(new errorS("Error con la Expresion Logica, Null con Null ", val2.linea, val2.columna, val2.valor.ToString()));
                                                        return new resultado("error", "error", val2.linea, val2.columna);
                                                    default:
                                                        break;
                                                }
                                                return null;
                                            case "tId":
                                                break;
                                            default:
                                                break;
                                        }
                                        return null;

                                    default:
                                        break;
                                }
                            }
                        }                        
                    }
                    break; //FIN EA

                default:
                    Console.WriteLine("DEFAULT");
                    break;
            }

            return null;
        }
    }
}
