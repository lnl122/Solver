using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;



namespace TestComponents
{
    class Program
    {
        static void Main(string[] args)
        {
            var s1 = new SpellChecker(@"C:\dict2.txt");
            var s2 = new SpellChecker();
            var s3 = new SpellChecker();
            var s4 = new SpellChecker();

            string[] ff = System.IO.File.ReadAllLines(@"C:\tt.txt");
            List<string> ss = new List<string>();
            foreach (string gg in ff)
            {
                ss.Add(gg);
            }

            //DateTime one = DateTime.Now;
            //List<string> sss3 = s3.Check(ss);
            //DateTime two = DateTime.Now;
            //TimeSpan result = two - one;
            //Console.WriteLine("ch Total "+ sss3.Count.ToString()+" - "+result.TotalMilliseconds.ToString()+" ms");
            
            DateTime one1 = DateTime.Now;
            List<string> sss2 = s2.Check(ss);
            DateTime two1 = DateTime.Now;
            TimeSpan result1 = two1 - one1;
            Console.WriteLine("cd1 Total " + sss2.Count.ToString() + " - " + result1.TotalMilliseconds.ToString() + " ms");

            s2.Flush();
            //one = DateTime.Now;
            //sss3 = s1.Check(ss);
            //two = DateTime.Now;
            //result = two - one;
            //Console.WriteLine("ch Total " + sss3.Count.ToString() + " - " + result.TotalMilliseconds.ToString() + " ms");

            one1 = DateTime.Now;
            sss2 = s4.Check(ss);
            two1 = DateTime.Now;
            result1 = two1 - one1;
            Console.WriteLine("cd2 Total " + sss2.Count.ToString() + " - " + result1.TotalMilliseconds.ToString() + " ms");

            //Console.WriteLine(s3.getDict()[3]);
            //Console.WriteLine(s3.getDict()[5]);
            s1.Close();
            s2.Close();
            s3.Close();
            s4.Close();


            Console.ReadKey();
        }
    }
}
