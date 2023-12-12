using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProiectSMU
{
    public class Class1
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Got here");
            Console.ReadLine();

            Form1 form = new Form1();
            form.ShowDialog();
            Application.Run();
        }
    }
}
