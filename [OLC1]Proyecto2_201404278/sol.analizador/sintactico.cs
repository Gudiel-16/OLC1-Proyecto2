using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2_201404278.sol.arbol;
using System.Drawing;
using System.IO;
using _OLC1_Proyecto2_201404278.utilidades;


namespace _OLC1_Proyecto2_201404278.sol.analizador
{
    class sintactico : Grammar
    {
        static ParseTree arbol = null;
        public static ParseTreeNode analizar(String cadena)
        {
            gramatica gramatica = new gramatica();
            LanguageData lenguaje = new LanguageData(gramatica);
            Parser parser = new Parser(lenguaje);
            arbol = parser.Parse(cadena);

            //para errores lexicos y sintacticos
            if (arbol.HasErrors())
            {
                int elementos = arbol.ParserMessages.Count;
                for (int i = 0; i < elementos; i++)
                {
                    singlentonError.registrarError(new errorS(arbol.ParserMessages[i].Message,arbol.ParserMessages[i].Location.Line,arbol.ParserMessages[i].Location.Column,"--"));
                }
            }

            return arbol.Root;
        }

        public static Image getImage(ParseTreeNode raiz)
        {
            String grafoDot = generarArbol.getDot(raiz);
            WINGRAPHVIZLib.DOT dot = new WINGRAPHVIZLib.DOT();
            WINGRAPHVIZLib.BinaryImage img = dot.ToPNG(grafoDot);
            byte[] imageBytes = Convert.FromBase64String(img.ToBase64String());
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            Image imagen = Image.FromStream(ms, true);
            img.Save("ASTIRONY.png");
            return imagen;
        }

    }
}
