using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq.Expressions;



namespace regresyon
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }
        Excel.Application xlApp;
        Excel.Workbook xlWorkbook;
        Excel._Worksheet xlWorksheet;
        Excel.Range xlRange;
        private void Form1_Load(object sender, EventArgs e)
        {
            string fullPathToExcel = Application.StartupPath+"/stage2 raw.xlsx"; //ie C:\Temp\YourExcel.xls
            xlApp = new Excel.Application();
            xlWorkbook = xlApp.Workbooks.Open(fullPathToExcel);
            xlWorksheet = xlWorkbook.Sheets[1];
            xlRange = xlWorksheet.UsedRange;
        }

        public static Func<double, double> RegresyonModeliOlustur(double[] dizi1, double[] dizi2)
        {
            var uzunluk = dizi1.Length;
            if (uzunluk != dizi2.Length)
            {
                throw new Exception("Uzunluklar Aynı Olmalı");
            }
            var xOrtalama = dizi1.Average();
            var yOrtalama = dizi2.Average();

            var xKareUzakliklarToplami = 0D;
            var yKareUzakliklarToplami = 0D;
            var xyUzakliklarCarpimiToplami = 0D;

            for (int i = 0; i < uzunluk; i++)
            {
                var x = dizi1[i];
                var y = dizi2[i];

                var xUzaklik = xOrtalama - x;
                var yUzaklik = yOrtalama - y;

                xKareUzakliklarToplami += Math.Pow(xUzaklik, 2);
                yKareUzakliklarToplami += Math.Pow(yUzaklik, 2);
                xyUzakliklarCarpimiToplami += xUzaklik * yUzaklik;
            }

            var xStandartSapma = Math.Sqrt(xKareUzakliklarToplami / (uzunluk - 1D));
            var yStandartSapma = Math.Sqrt(yKareUzakliklarToplami / (uzunluk - 1D));
            var kovaryans = xyUzakliklarCarpimiToplami / (uzunluk - 1D);

            var r = (kovaryans / (xStandartSapma * yStandartSapma));
            var b = r * (yStandartSapma / xStandartSapma);
            var a = yOrtalama - b * xOrtalama;

            // f(x) = a + bx;
            var parametre = Expression.Parameter(typeof(double), "x"); // x'i tanımla
            var ifade = Expression.Lambda(Expression.Add(Expression.Constant(a), // a +
                                          Expression.Multiply(Expression.Constant(b), // b *
                                                              parametre) // x
                                                              ),
                                         parametre);// (x) =>

            return ifade.Compile() as Func<double, double>;
        }
        public static double R2(double[] dizi1, double[] dizi2, Func<double, double> model)
        {
            var yOrtalama = dizi2.Average();
            var SS_toplam = dizi2.Sum(y => Math.Pow(y - yOrtalama, 2));
            var SS_artik = dizi1.Select((x, i) => Math.Pow(model(x) - dizi2[i], 2)).Sum();
            return (1.0 - SS_artik / SS_toplam);
        }

        private void btnTahminEt_Click(object sender, EventArgs e)
        {

            try
            {
                double asa;
                double.TryParse(txtD1.Text, out asa);
                if (txtD1.Text != "" && txtD1.Text != "" && (Convert.ToInt32(txtD1.Text) * 30) > Convert.ToInt32(txtboyut.Text))
                {
                    int aralik = Convert.ToInt32(txtboyut.Text);
                    int boyut = Convert.ToInt32(txtD1.Text) * 30;
                    int i = boyut - aralik;
                    double[] xs = new double[boyut];
                    double[] ys = new double[boyut];
                    for (; i <= boyut - 5; i++)
                    {
                        xs[i] = Convert.ToDouble(xlRange.Cells[i + 2, 1].Value2.ToString());
                        ys[i] = Convert.ToDouble(xlRange.Cells[i + 2, 2].Value2.ToString());
                    }

                    var model = RegresyonModeliOlustur(xs, ys);
                    txtSonuc.Text = model(asa).ToString();
                    txthata.Text = R2(xs, ys, model).ToString();
                }
            }
            catch
            {

            }
             
            
        }
    }
}
