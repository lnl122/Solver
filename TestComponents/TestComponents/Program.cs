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
        //Tools.GetEnvInfo();
        //if (!Tools.CheckComponents()) { MessageBox.Show("Не все необхдимые компоненты установлены на ПК.\r\nПроверьте лог-файл."); return; }


        static void Main(string[] args)
        {

            List<string> a1 = new List<string>();
            a1.Add("flame");
            a1.Add("spell");
            a1.Add("djhfkr");
            a1.Add("more");
            a1.Add("glass");
            var a2 = Google.TranslateEnRu(a1);
            a2 = a2;
            /*
            string dir = "C:\\1\\qw";
            string[] dirs = System.IO.Directory.GetFiles(dir);
            List<string> ee = new List<string>();
            int ii = 0;
            int mm = dirs.Length;
            foreach(string dd in dirs)
            {
                List<string> ww = ggg(dd);
                ee.AddRange(ww);
                string[] aarr = ee.ToArray();
                System.IO.File.WriteAllLines("C:\\bad_words_find.txt", aarr);
                ii++;
                Console.WriteLine(ii.ToString() + "/" + mm.ToString());
            }
            */

            //var qq = ggg(@"C:\123.jpg");
            //qq = qq;

            /*
            var a1 = new Associations();
            a1.LoadDictionary(@"C:\assoc.txt");

            var tt = a1.Get2("запах", "кошка");
            tt = tt;
            tt = a1.Get2(a1.Get("запах", 5),a1.Get("кошка", 5));
            a1.SaveDictionary();
            //a1.SaveDictionary();
            a1.Close();
            a1 = null;
            Console.WriteLine(a1);
            */

            /*
            DateTime one = DateTime.Now;
            Associations.Init();
            Associations.LoadDictionary(@"C:\assoc.txt");
            DateTime two = DateTime.Now;
            TimeSpan result = two - one;
            //Console.WriteLine("ch Total - "+result.TotalMilliseconds.ToString()+" ms");

            string[] ff = System.IO.File.ReadAllLines(@"C:\assoc2.txt");
            int i = 0;
            int m = ff.Length;
            foreach(string ss2 in ff)
            {
                string[] ss3 = ss2.Split(' ');
                foreach(string ss in ss3)
                {
                    one = DateTime.Now;
                    var q = Associations.Get(ss, 3);
                    two = DateTime.Now;
                    result = two - one;
                    if (result.TotalMilliseconds > 10)
                    {
                        Console.WriteLine((Math.Floor(1000.0 * i / m) / 10).ToString() + "% - " + i.ToString() + " of " + m.ToString() + "  -  " + result.TotalMilliseconds.ToString() + " ms  -  "+ ss);
                    }
                }
                if (i % 1000 == 0)
                {
                    Associations.SaveDictionary();
                    Console.WriteLine("write...");
                }
                i++;
            }

            Associations.SaveDictionary();
  
            */







            /*
            var a1 = new Associations();
            a1.LoadDictionary(@"C:\assoc.txt");
            var tt = a1.Get(a1.Get("пиво",5),5);
            a1.SaveDictionary();
            tt = tt;
            */

            /*
            while(1 == 0)
            {
                var s1 = new SpellChecker();
                s1.LoadDictionary(@"C:\dict2.txt");
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

                s2.SaveDictionary();
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
                s1.SaveDictionary();
                s1.Close();
                s2.Close();
                s3.Close();
                s4.Close();

            }*/


            Console.ReadKey();
        }
    }
}
