using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace _OLC1_Proyecto2_201404278.utilidades
{
    class singlentonError
    {
        public static List<errorS> listaErrores = new List<errorS>();

        public static void registrarError(errorS e)
        {
            listaErrores.Add(e);
        }
    }
}
