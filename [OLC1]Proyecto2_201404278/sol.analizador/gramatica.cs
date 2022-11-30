using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace _OLC1_Proyecto2_201404278.sol.analizador
{
    class gramatica : Grammar
    {
         public gramatica() : base(caseSensitive:false) // lo de base es para caseSensitive
        {
            #region ER TIPOS DE DATOS
            RegexBasedTerminal tCarac = new RegexBasedTerminal("tCarac", "['][a-zA-Z][']");
            
            //RegexBasedTerminal tNull = new RegexBasedTerminal("tNull", "[null]");
            StringLiteral tCad = new StringLiteral("tCad", "\"");
            RegexBasedTerminal numero = new RegexBasedTerminal("tInt", "[0-9]+|[-][0-9]+");
            RegexBasedTerminal tDouble = new RegexBasedTerminal("tDouble", "[0-9]+[.][0-9]+|[-][0-9]+[.][0-9]+");            
            IdentifierTerminal tId = new IdentifierTerminal("tId");
            //RegexBasedTerminal tBool = new RegexBasedTerminal("tBool", "[true]|[false]");
            CommentTerminal comentarioLinea = new CommentTerminal("comentLinea","//","\n","\r\n");
            CommentTerminal comentarioBloque = new CommentTerminal("comentLinea", "/*", "*/");
            #endregion

            #region comentarios
            NonGrammarTerminals.Add(comentarioLinea);
            NonGrammarTerminals.Add(comentarioBloque);
            #endregion

            #region Terminales EXPRESIONES ARITMETICAS
            var mas = ToTerm("+");
            var menos = ToTerm("-");
            var por = ToTerm("*");
            var div = ToTerm("/");
            var pot = ToTerm("pow");
            #endregion

            #region Terminales EXPRESIONES BOOLEANAS
            var mayor = ToTerm(">");
            var menor = ToTerm("<");
            var igualigual = ToTerm("==");
            var difigual = ToTerm("<>");
            var menorigugal = ToTerm("<=");
            var mayorigual = ToTerm(">=");
            #endregion

            #region Terminales EXPRESIONES LOGICAS
            var or = ToTerm("||");
            var and = ToTerm("&&");
            var xor = ToTerm("^");
            var not = ToTerm("!");
            #endregion

            #region Terminales SIGNOS  
            var parenabre = ToTerm("(");
            var parencierra = ToTerm(")");
            var puntoycoma = ToTerm(";");
            var coma = ToTerm(",");
            var igual = ToTerm("=");
            var llaveabre = ToTerm("{");
            var llavecierra = ToTerm("}");
            var dospuntos = ToTerm(":");
            var corchabre = ToTerm("[");
            var corchcierra = ToTerm("]");
            var punto = ToTerm(".");
            #endregion

            #region Terminales PALABRAS RESERVADAS
            var tNull = ToTerm("null");
            var tBoolT = ToTerm("true");
            var tBoolF = ToTerm("false");
            var log = ToTerm("log");
            var varr = ToTerm("var");
            var alert = ToTerm("alert");
            var graph = ToTerm("graph");
            var iff = ToTerm("if");
            var elsee = ToTerm("else");
            var switchh = ToTerm("switch");
            var casee = ToTerm("case");
            var defaultt = ToTerm("default");
            var whilee = ToTerm("while");
            var doo = ToTerm("do");
            var breakk = ToTerm("break");
            var continuee = ToTerm("continue");
            var aumento = ToTerm("++");
            var decremento = ToTerm("--");
            var forrr = ToTerm("for");
            var importar = ToTerm("importar");
            var function = ToTerm("function");
            var voidd = ToTerm("void");
            var returnn = ToTerm("return");
            var array = ToTerm("array");
            var classs = ToTerm("class");
            var neww = ToTerm("new");
            var mainn = ToTerm("main");

            #endregion
             
            #region precedencia
            RegisterOperators(11, Associativity.Left, "+", "-");
            RegisterOperators(10, Associativity.Left, "*", "/");
            RegisterOperators(9, Associativity.Right, "pow");
            RegisterOperators(8, "==");
            RegisterOperators(7, "<>");
            RegisterOperators(6, ">", "<","<=", ">=");
            RegisterOperators(5, Associativity.Left, "||");
            RegisterOperators(4, Associativity.Left, "^");
            RegisterOperators(3, Associativity.Left, "&&");
            RegisterOperators(2, Associativity.Right, "!");
            RegisterOperators(1, Associativity.Left, "(", ")");
            
            #endregion

            #region No Terminales
            NonTerminal S = new NonTerminal("S");
            NonTerminal EA = new NonTerminal("EA");
            NonTerminal INST = new NonTerminal("INST");
            NonTerminal INSTRUCCION = new NonTerminal("INSTRUCCION");
            NonTerminal AUMENTO = new NonTerminal("AUMENTO");
            NonTerminal DECREMENT=new NonTerminal("DECREMENT");
            NonTerminal FUNLOG = new NonTerminal("FUNLOG");
            NonTerminal FUNALERT = new NonTerminal("FUNALERT");
            NonTerminal FUNGRAPH = new NonTerminal("FUNGRAPH");
            NonTerminal DEC = new NonTerminal("DEC");
            NonTerminal DECLIST = new NonTerminal("DECLIST");
            NonTerminal DECLIST2 = new NonTerminal("DECLIST2");
            NonTerminal ASIG = new NonTerminal("ASIG");
            NonTerminal SENTIF = new NonTerminal("SENTIF");
            NonTerminal SENTIF2 = new NonTerminal("SENTIF2");
            NonTerminal SENTIF3 = new NonTerminal("SENTIF3");
            NonTerminal SENTIF4 = new NonTerminal("SENTIF4");
            NonTerminal SENTWIT = new NonTerminal("SENTWIT");
            NonTerminal SENTWIT2 = new NonTerminal("SENTWIT2");
            NonTerminal SENTWIT3 = new NonTerminal("SENTWIT3");
            NonTerminal SENTWHILE = new NonTerminal("SENTWHILE");
            NonTerminal SENTDOWHILE = new NonTerminal("SENTDOWHILE");
            NonTerminal MANIPFLUJO = new NonTerminal("MANIPFLUJO");
            NonTerminal SENTFOR = new NonTerminal("SENTFOR");
            NonTerminal DECASIG = new NonTerminal("DECASIG");
            NonTerminal IMPORT = new NonTerminal("IMPORT");
            NonTerminal METODO = new NonTerminal("METODO");
            NonTerminal METODO2 = new NonTerminal("METODO2");
            NonTerminal METODO3 = new NonTerminal("METODO3");
            NonTerminal FUNCTION = new NonTerminal("FUNCTION");
            NonTerminal FUNCTION2 = new NonTerminal("FUNCTION2");
            NonTerminal FUNCTION3 = new NonTerminal("FUNCTION3");
            NonTerminal RETURN = new NonTerminal("RETURN");
            NonTerminal LLAMADA = new NonTerminal("LLAMADA");
            NonTerminal LLAMADA2 = new NonTerminal("LLAMADA2");
            NonTerminal DECARRAY = new NonTerminal("DECARRAY");
            NonTerminal DECARRAY2 = new NonTerminal("DECARRAY2");
            NonTerminal DECARRAY3 = new NonTerminal("DECARRAY3");
            NonTerminal DECARRAY33 = new NonTerminal("DECARRAY33");
            NonTerminal DECARRAY4 = new NonTerminal("DECARRAY4");
            NonTerminal DECARRAY5 = new NonTerminal("DECARRAY5");
            NonTerminal DECARRAY6 = new NonTerminal("DECARRAY6");
            NonTerminal DECARRAY66 = new NonTerminal("DECARRAY66");
            NonTerminal DECARRAY7 = new NonTerminal("DECARRAY7");
            NonTerminal DECARRAYLIST = new NonTerminal("DECARRAYLIST");
            NonTerminal ASIGARRAY = new NonTerminal("ASIGARRAY");
            NonTerminal ASIGARRAY2 = new NonTerminal("ASIGARRAY2");
            NonTerminal ASIGARRAY3 = new NonTerminal("ASIGARRAY3");
            NonTerminal OBTARRAY = new NonTerminal("OBTARRAY");
            NonTerminal OBTARRAY2 = new NonTerminal("OBTARRAY2");
            NonTerminal OBTARRAY3 = new NonTerminal("OBTARRAY3");
            NonTerminal CLASS = new NonTerminal("CLASS");
            NonTerminal DECCLASS = new NonTerminal("DECCLASS");
            NonTerminal REASIGCLASS = new NonTerminal("REASIGCLASS");
            NonTerminal REASIGCLASS2 = new NonTerminal("REASIGCLASS2");
            NonTerminal MAIN = new NonTerminal("MAIN");
            NonTerminal LLAMCLASS_METFUN = new NonTerminal("LLAMCLASS_METFUN");
            NonTerminal LLAMCLASS_METFUN2 = new NonTerminal("LLAMCLASS_METFUN2");

            #endregion


            #region Gramatica
            S.Rule = INSTRUCCION;

            INSTRUCCION.Rule = MakePlusRule(INSTRUCCION,INST); // ES COMO TENER -> INSTRUCCION + INST | INST

            INST.Rule = FUNLOG
                        | FUNALERT
                        | FUNGRAPH
                        | DEC
                        | ASIG                        
                        | SENTIF
                        | SENTWIT
                        | SENTWHILE
                        | SENTDOWHILE
                        | MANIPFLUJO
                        | SENTFOR
                        | IMPORT
                        | FUNCTION
                        | METODO
                        | LLAMADA + puntoycoma
                        | LLAMCLASS_METFUN + puntoycoma
                        | RETURN
                        | DECARRAY
                        | CLASS
                        | DECCLASS
                        | MAIN
                        | AUMENTO + puntoycoma
                        | DECREMENT + puntoycoma
                        | REASIGCLASS + puntoycoma
                        | ASIGARRAY + puntoycoma;
            
            // aumento-decremento
            AUMENTO.Rule=EA + aumento;

            DECREMENT.Rule =EA + decremento;

            //Funcion log
            FUNLOG.Rule = log + parenabre + EA + parencierra + puntoycoma;

            //Funcion alert
            FUNALERT.Rule = alert + parenabre + EA + parencierra + puntoycoma;

            //Funcion graph
            FUNGRAPH.Rule = graph + parenabre + EA + coma + EA + parencierra + puntoycoma;

            //Para declarar variables
            DEC.Rule = varr + DECLIST + puntoycoma;

            DECLIST.Rule = MakePlusRule(DECLIST,coma,tId)
                | MakePlusRule(DECLIST,coma,DECLIST2);

            DECLIST2.Rule = tId + igual + EA;
            
             //Para asignar valor a variables
            ASIG.Rule = tId + igual + EA + puntoycoma;

             //sentencia IF
            SENTIF.Rule = iff + parenabre + EA + parencierra + llaveabre + INSTRUCCION + llavecierra + SENTIF2;

            SENTIF2.Rule = MakeStarRule(SENTIF2,SENTIF3);

            SENTIF3.Rule = elsee + iff + parenabre + EA + parencierra + llaveabre + INSTRUCCION + llavecierra
                        | elsee + llaveabre + INSTRUCCION + llavecierra; 
             
             //sentencia switch
            SENTWIT.Rule = switchh + parenabre + EA + parencierra + llaveabre + SENTWIT2 + defaultt + dospuntos + INSTRUCCION + llavecierra;

            SENTWIT2.Rule = MakeStarRule(SENTWIT2,SENTWIT3);

            SENTWIT3.Rule = casee + EA + dospuntos + INSTRUCCION;
             
             //sentencia while
            SENTWHILE.Rule = whilee + parenabre + EA + parencierra + llaveabre + INSTRUCCION + llavecierra;

             //sentencia do-while
            SENTDOWHILE.Rule= doo + llaveabre + INSTRUCCION + llavecierra + whilee + parenabre + EA + parencierra + puntoycoma;
                          
             //sentencia for
            SENTFOR.Rule = forrr + parenabre + DECASIG + EA + puntoycoma + EA + parencierra + llaveabre + INSTRUCCION + llavecierra;

            DECASIG.Rule = DEC
                          | EA + puntoycoma;

            //sentencia de manipulacion de flujo
            MANIPFLUJO.Rule = breakk + puntoycoma
                           | continuee + puntoycoma;
        
            //sentencia importar
            IMPORT.Rule = importar + parenabre + EA + parencierra + puntoycoma;

             //funciones
            FUNCTION.Rule = function + tId + parenabre + FUNCTION2 + parencierra + llaveabre + INSTRUCCION + llavecierra;

            FUNCTION2.Rule = MakeStarRule(FUNCTION2, coma, FUNCTION3);

            FUNCTION3.Rule = varr + tId;

            //metodos 
            METODO.Rule = function + voidd + tId + parenabre + METODO2 + parencierra + llaveabre + INSTRUCCION + llavecierra;

            METODO2.Rule = MakeStarRule(METODO2, coma, METODO3);

            METODO3.Rule = varr + tId;

            //llamada
            LLAMADA.Rule = tId + parenabre + LLAMADA2 + parencierra;

            LLAMADA2.Rule = MakeStarRule(LLAMADA2,coma,EA);

            //llamada clase, haciendo referencia a una funcion o metodo
            LLAMCLASS_METFUN.Rule = tId + punto + tId + parenabre + LLAMCLASS_METFUN2 + parencierra;

            LLAMCLASS_METFUN2.Rule = MakeStarRule(LLAMCLASS_METFUN2, coma, EA);

             //return
            RETURN.Rule = returnn + EA + puntoycoma;

            //clase
            CLASS.Rule = classs + tId + llaveabre + INSTRUCCION + llavecierra;

            //declaracion de objectos de clase
            DECCLASS.Rule = varr + tId + igual + neww + tId + parenabre + parencierra + puntoycoma;

            //reasignacion de variables globales de clase
            REASIGCLASS.Rule = tId + punto + tId + igual + EA ;

            //reasignacion que ira en las expresiones
            REASIGCLASS2.Rule = tId + punto + tId;

            //main
            MAIN.Rule = mainn + parenabre + parencierra + llaveabre + INSTRUCCION + llavecierra;

            //declarar arreglos
            DECARRAY.Rule = varr + DECARRAY2 + puntoycoma;

            DECARRAY2.Rule = DECARRAY2 + coma + tId + DECARRAY3
                           | tId + DECARRAY3;

            DECARRAY3.Rule = MakePlusRule(DECARRAY3,DECARRAY33);

            DECARRAY33.Rule = corchabre + EA + corchcierra + DECARRAYLIST;

            DECARRAYLIST.Rule = igual + DECARRAY4
                            | Empty;

            DECARRAY4.Rule = llaveabre + DECARRAY5 + llavecierra;

            DECARRAY5.Rule = MakePlusRule(DECARRAY5, coma, DECARRAY6);

            DECARRAY6.Rule = llaveabre + DECARRAY7 + llavecierra;
             
            DECARRAY7.Rule = MakePlusRule(DECARRAY7, coma, EA)
                            | MakePlusRule(DECARRAY7, coma, DECARRAY6);

            //asignar array y obtener arrary
            ASIGARRAY.Rule = tId + OBTARRAY2 + igual + EA;
             
            OBTARRAY.Rule = tId + OBTARRAY2;

            OBTARRAY2.Rule = MakePlusRule(OBTARRAY2,OBTARRAY3);

            OBTARRAY3.Rule = corchabre + EA + corchcierra;

            //expresiones
            EA.Rule = EA + mas + EA
                | EA + menos + EA
                | EA + por + EA
                | EA + div + EA
                | EA + pot + EA
                | EA + mayor + EA
                | EA + menor + EA
                | EA + igualigual + EA
                | EA + difigual + EA
                | EA + mayorigual + EA
                | EA + menorigugal + EA
                | EA + or + EA
                | EA + and + EA
                | EA + xor + EA
                | EA + not + EA
                | not + EA
                | parenabre + EA + parencierra
                | numero
                | tDouble
                | tBoolT
                | tBoolF
                | tCad
                | tCarac
                | tNull
                | tId
                | AUMENTO
                | DECREMENT
                | LLAMADA
                | LLAMCLASS_METFUN
                | REASIGCLASS2
                | OBTARRAY;
                     
            #endregion

            #region Preferencias
            this.Root = S; //donde empezara la gramatica
            this.MarkTransient(S,INST,DEC,DECASIG,FUNCTION3,METODO3,DECARRAY,DECARRAY4,DECARRAY6);
            this.MarkPunctuation(",",";","=","(",")","{","}",":",".","[","]","var","log","alert","graph","while","do","for","++","--","if","else","switch","case","default","function","void","return","main","class","new","importar"); //quita estos nodos del arbol
            #endregion

        }
    
    }
}
