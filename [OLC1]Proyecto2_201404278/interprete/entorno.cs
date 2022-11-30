using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using _OLC1_Proyecto2_201404278.utilidades;

namespace _OLC1_Proyecto2_201404278.interprete
{
    class entorno
    {
        Hashtable tablaSim;
        public entorno padre;

        public entorno(entorno padre)
        {
            this.padre = padre;
            tablaSim = new Hashtable();
        }

        public void insertar(simbolo s)
        {
            simbolo tmp = (simbolo)tablaSim[s.identificador.ToLower()];        
            if(tmp ==null)
            {
                tablaSim.Add(s.identificador.ToLower(), s);
            }        
            else
            {
                singlentonError.registrarError(new errorS("El identificador ya existe ", s.linea, s.columna, s.identificador));
            }
        }
        
        public simbolo obtener(String id)
        {
            //este sera el entorno actual donde buscara
            entorno temp = this;

            //sino encuenta en el actual, buscara en el padre del actual y asi sucesivamente
            while(temp!=null)
            {
                simbolo s = (simbolo)temp.tablaSim[id.ToLower()];
                if (s!=null)
                {
                    return s;
                }

                temp = temp.padre;
            }

            //sino lo encuentra enctonces retornara null
            return null;  
        }

        public void asignarValor(simbolo s)
        {
            simbolo tmp = (simbolo)tablaSim[s.identificador.ToLower()];
            if (tmp == null)
            {
                singlentonError.registrarError(new errorS("El Identificador no existe ", s.linea, s.columna, s.identificador));
            }
            else
            {
                tablaSim.Remove(s.identificador.ToLower());
                tablaSim.Add(s.identificador.ToLower(), s);                
            }
        }

    }
}
