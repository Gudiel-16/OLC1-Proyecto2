using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace _OLC1_Proyecto2_201404278.utilidades
{
    
    class singlentonConsola
    {
        public static ArrayList listaConsola = new ArrayList();

        public static void ingresarImpre(Object valor)
        {
            listaConsola.Add(valor);
        }

    }
}
