using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using Irony.Ast;
using Irony.Parsing;
using _OLC1_Proyecto2_201404278.sol.analizador;
using System.Diagnostics;
using _OLC1_Proyecto2_201404278.interprete;
using _OLC1_Proyecto2_201404278.utilidades;

namespace _OLC1_Proyecto2_201404278
{
    public partial class Form1 : Form
    {
        Hashtable tablaArchivos;

        public Form1()
        {
            InitializeComponent();
            tablaArchivos = new Hashtable();
            tablaArchivos.Add("main", rbMain);
        }

        private void cREARARHIVOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            String noming = Microsoft.VisualBasic.Interaction.InputBox("Ingrese Nombre:", "Registro de Datos", "Gudiel", 100, 0);
            areaPestanias.TabPages.Insert(0, noming);
            RichTextBox nuevor = new RichTextBox();
            nuevor.SetBounds(0, 0, 1140, 370);
            areaPestanias.TabPages[0].Controls.Add(nuevor);
            tablaArchivos.Add(noming, nuevor);
            //Console.WriteLine(areaPestanias.TabCount);//numero de pestanias
            
        }

        private void gUARDARARCHIVOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            for (int i = 0; i < areaPestanias.TabCount; i++)
            {
                if (areaPestanias.SelectedIndex == i)
                {
                    String ubicacion = Application.StartupPath;
                    String archivo = @"\" + areaPestanias.TabPages[i].Text + ".pyUsac";
                    RichTextBox temp = (RichTextBox)tablaArchivos[areaPestanias.TabPages[i].Text];
                    File.WriteAllText(ubicacion+archivo, temp.Text);
                }
            }
        }

        private void gUARDARCOMOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            
            // filtros
            save.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            if (save.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < areaPestanias.TabCount; i++)
                {
                    if (areaPestanias.SelectedIndex == i)
                    {
                        //save.FileName = "prueba.txt";
                        RichTextBox temp = (RichTextBox)tablaArchivos[areaPestanias.TabPages[i].Text];
                        using (StreamWriter sw = new StreamWriter(save.FileName))
                            sw.WriteLine(temp.Text);
                    }
                }
            }
        }

        private void cERRARARCHIVOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < areaPestanias.TabCount; i++)
            {
                if (areaPestanias.SelectedIndex == i)
                {
                    areaPestanias.TabPages.RemoveAt(i);
                }
            }
        }

        private void mANUALDEUSUARIOToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void mANUALTECNICOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Object a = true;
            Object b = true;
            if ((Boolean)a==true && (Boolean)b==true)
            {
                Console.WriteLine("si");
            }
            else
            {
                Console.WriteLine("no");
            }
        }

        private void aBRIRARBOLIRONYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("ASTIRONY.png"))
            {
                Process p = new Process();
                p.StartInfo.FileName = "ASTIRONY.png";
                p.Start();
            }
            
        }

        private void cOMPILARPROYECTOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*PRUEBAS*/
            //Console.Write(Math.Pow(4, 2));
            //Console.WriteLine(Ackermann(3,6));
            //int [] arr={3,7,2};
            //for(var x=0; x<3 ; x++)
            //{
            //    for(var i=0; i<(3-x-1); i++)
            //    {
            //        if (arr[i]<arr[i+1])
            //        {
            //            var temp = arr[i + 1];
            //            arr[i + 1] = arr[i];
            //        }
            //    }
            //}

            //int [,] aja=new int[3,4];
            /*----*/

            singlentonConsola.listaConsola.Clear();
            singlentonError.listaErrores.Clear();
            rbConsola.Text = "";
            gvErrores.Rows.Clear();
            

            ParseTreeNode resultado = sintactico.analizar(rbMain.Text);

            if (resultado != null)
            {
                rbCompi.Text = "Texto De Entrada Correcta";
                                
                Image nueva = sintactico.getImage(resultado); //genera imagen de arbol irony 

                //recorrido ss = new recorrido(resultado);
                //ss.comenzar(resultado);

                //ejecucion del arbol
                interpretee ejec = new interpretee(resultado);
                ejec.comenzar();
                                
            }
            else
            {
                rbCompi.Text = "Texto De Entrada Incorrecta";

            }

            //consola
            for (int i = 0; i < singlentonConsola.listaConsola.Count; i++)
            {
                rbConsola.Text = rbConsola.Text + singlentonConsola.listaConsola[i] + "\n";
            }

            //errores
            foreach (errorS err in singlentonError.listaErrores)
            {
                String[] intArray2 = new String[4] { err.valor, err.descripcion, err.linea.ToString(), err.columna.ToString() };
                gvErrores.Rows.Add(intArray2);
            }

        }

        int Ackermann(int m, int n)
        {
            if (m == 0)
                return (n + 1);
            else if (n == 0)
                return (Ackermann(m - 1, 1));
            else
                return (Ackermann(m - 1, Ackermann(m, n - 1)));
        }

                
    }
}
