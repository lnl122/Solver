using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Solver
{
    class Raschl
    {
        struct Raschl_data
        {
            public string type;// { get; set; }// = "Raschl";
            public TabPage Tab;
            public Button BtnSolve;
            public Button BtnClose;
            public TextBox TextIn;
            public string normal;
            public TextBox TextOut;
            public int level;
        }// основная структура задания
        static Raschl_data Data = new Raschl_data();

        public Raschl(int level, string def)                                                         // Создаем новый Таб + структуру публичных данных
        {
            Data.type = "Rashcl";
            Data.Tab = new TabPage();
            Data.level = level;
            Data.Tab.Text = "Расчлененки";
            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_Raschl_Solve_Click);
            Data.Tab.Controls.Add(Data.BtnSolve);
            Data.BtnClose = new Button();
            Data.BtnClose.Text = "Закрыть";
            Data.BtnClose.Click += new EventHandler(Event_Raschl_Close_Click);
            Data.Tab.Controls.Add(Data.BtnClose);
            Data.TextIn = new TextBox();
            Data.TextIn.AcceptsReturn = true;
            Data.TextIn.AcceptsTab = false;
            Data.TextIn.Multiline = true;
            Data.TextIn.ScrollBars = ScrollBars.Both;
            Data.TextIn.Text = def;
            Data.Tab.Controls.Add(Data.TextIn);
            Data.TextIn.TextChanged += new EventHandler(Event_Raschl_DataIn_Changed);
            Data.TextOut = new TextBox();
            Data.TextOut.AcceptsReturn = true;
            Data.TextOut.AcceptsTab = false;
            Data.TextOut.Multiline = true;
            Data.TextOut.ScrollBars = ScrollBars.Both;
            Data.TextOut.Text = "";
            Data.Tab.Controls.Add(Data.TextOut);
            Event_Raschl_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_Raschl_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }

        public static void Event_Raschl_Close_Click(object sender, EventArgs e)      // Закрываем Таб
        {
            Data.Tab.Dispose();
        }
        public static void Event_Raschl_DataIn_Changed(object sender, EventArgs e)            // при изменении текста
        {
            Data.Tab.Text = Data.Tab.Text.Replace(" *", "");
        }
        public static void Event_Raschl_ChangeSize(object sender, EventArgs e)
        {
            Data.BtnSolve.Top = Program.mainform_border;
            Data.BtnSolve.Left = Program.mainform_border;
            Data.BtnSolve.Width = 20 * Program.mainform_border;
            Data.BtnSolve.Height = 5 * Program.mainform_border;
            Data.BtnClose.Top = Program.mainform_border;
            Data.BtnClose.Width = 20 * Program.mainform_border;
            Data.BtnClose.Height = 5 * Program.mainform_border;
            Data.BtnClose.Left = Program.GameTab.MainTab.Width - Data.BtnClose.Width - Program.mainform_border;
            Data.TextIn.Top = 7 * Program.mainform_border;
            Data.TextIn.Left = Program.mainform_border;
            Data.TextIn.Height = (Program.Tabs.Height - 15 * Program.mainform_border) / 2;
            Data.TextIn.Width = Program.Tabs.Width - 4 * Program.mainform_border;
            Data.TextOut.Top = Data.TextIn.Bottom + Program.mainform_border;
            Data.TextOut.Left = Program.mainform_border;
            Data.TextOut.Height = Data.TextIn.Height;
            Data.TextOut.Width = Data.TextIn.Width;
        }


        public static void ButtonsEnable(TabPage mTab, bool flag)                  // меняем оптом доступность кнопок
        {
            foreach (var el in mTab.Controls)
            {
                var btn = new Button();
                string els = el.GetType().ToString();
                if (els == "System.Windows.Forms.Button")
                {
                    btn = (System.Windows.Forms.Button)el;
                    btn.Enabled = flag;
                }
            }
        }
        struct Raschl_one_string
        {
            public int numstr;
            public string[] str;
            public int[] num;
            public TextBox tb;
        }                                             // струкрура данных для одной строчки расчлененок
                                                      
        

        public void Raschl_Complete(TabPage mTab, TextBox mTextOut, string str) // делегат, принимающий возвращенные потоком параметры. взаимодействует с ГУИ
        {
            mTab.Text = mTab.Text + " *";
            mTextOut.Text = str;

            // возвращаем доступность кнопок
            ButtonsEnable(mTab, true);
        }
        public void Raschl_One_Complete(TextBox tb, int numstr, string res)
        {
            string t1 = tb.Text;
            for (int i = 0; i <= numstr; i++) { t1 = t1 + "\r\n"; }
            string[] t2 = Regex.Split(t1, "\r\n");
            t2[numstr - 1] = res;
            string res2 = "";
            foreach (string t3 in t2)
            {
                res2 = res2 + t3 + "\r\n";
            }
            tb.Text = res2;
            //2do - надо убирать завершающие символы \r\n , дублирующиеся в конце много раз
        }  // делегат, отображение одной решенной строки в живую
        private string Rashcl_NormalizeData(string d)
        {
            string t0 = d.ToLower().Replace(" ", "").Replace(",", "").Replace("\r\n", "#").Replace("###", "##").Replace("###", "##").Replace("###", "##").Replace("###", "##");
            t0 = (t0 + "##").Replace("###", "##").Replace("###", "##");
            //t1 = строитель(3)блеф(2)картон(2)#жироприказ(4)слюда(2)чемодан(2)гарнир(2)лезвие(1)#житель(3)тепло(2)рогожа(3)мрак(2)мозг(1)карман(2)##
            //t1 = строитель(3)#блеф(2)#картон(2)##жироприказ(4)#слюда(2)#чемодан(2)#гарнир(2)#лезвие(1)##
            // определим тип входного данного
            int s1 = t0.Length - t0.Replace(")", "").Length;        // правые скобки
            int s12 = t0.Length - t0.Replace("(", "").Length;       // левые скобки
            int s2 = (t0.Length - t0.Replace(")#", "").Length) / 2; // после каждой правой скобки - новая строка - сколько раз
            if ((s1 == 0) || (s1 != s12)) { return ""; }            // если нет правых скобок вообще или их количество не равно числу левых скобок
            string[] t2 = Regex.Split(t0, "\\(");
            int res;
            bool fl = true;
            for (int i = 1; i < t2.Length; i++)
            {
                string[] t4 = Regex.Split(t2[i], "\\)");
                fl = fl && Int32.TryParse(t4[0], out res);
            }
            if (!fl) { return ""; }                                 // если внутри скобок есть не число
            if (s1 == s2)
            {
                // type строитель(3)#блеф(2)#картон(2)##жироприказ(4)#слюда(2)#чемодан(2)#гарнир(2)#лезвие(1)##
                t0 = t0.Replace("#", "$").Replace("$$", "#").Replace("$", "");
            }
            else
            {
                // type строитель(3)блеф(2)картон(2)#жироприказ(4)слюда(2)чемодан(2)гарнир(2)лезвие(1)#житель(3)тепло(2)рогожа(3)мрак(2)мозг(1)карман(2)##
            }
            t0 = t0.Replace("##", "#").Replace("##", "#");
            return t0; // or "" above by text
        }                   // нормализация вида задачи
        private string Raschl_Process_Word_SpellCheck(string[] d)
        {
            //Microsoft.Office.Interop.Word.Application 
            var wordApp = new Microsoft.Office.Interop.Word.Application();
            wordApp.Visible = false;
            string res = " ";
            foreach (string s1 in d)
            {
                string s2 = s1.Substring(0, 1).ToUpper() + s1.Substring(1, s1.Length - 1);
                if (wordApp.CheckSpelling(s2) || wordApp.CheckSpelling(s1))
                {
                    res = res + s1 + " ";
                }
            }
            wordApp.Quit();
            return res.TrimStart().TrimEnd().Trim();
        }                       // проверка орфографии Ворд лимитированного колва слов (обычно до 1000)
        private string Raschl_Process_One(Raschl_one_string d)
        {
            string res = "";
            int words = d.str.Length;
            // d. = .str[], .num[]
            // перебирать все варианты, проверять орфографию каждого
            int[] cur = new int[words]; // текущие координаты
            int[] sta = new int[words]; // длинна слов
            int total = 1;
            for (int i = 0; i < words; i++)
            {
                cur[i] = 0;
                sta[i] = d.str[i].Length - d.num[i]; // максимальное начало строки, с нуля 0..ххх
                total = total * (sta[i] + 1);
            }
            string[] allwrds = new string[total];
            int curwrd = 0;
            while (cur[words - 1] <= sta[words - 1])
            {
                string r2 = "";
                for (int i = 0; i < words; i++)
                {
                    r2 = r2 + d.str[i].Substring(cur[i], d.num[i]);
                }
                allwrds[curwrd] = r2;
                curwrd++;
                cur[0]++;
                for (int i = 0; i < words - 1; i++)
                {
                    if (cur[i] > sta[i])
                    {
                        cur[i] = 0;
                        cur[i + 1]++;
                    }
                }
            }//while
            // int total, string allwrds[]
            int lim = 1000;
            curwrd = 0;
            int partcnt = ((total + lim - 1) / lim) - 1;
            var Tasks3 = new List<Task<string>>();
            for (int i = 0; i <= partcnt; i++)
            {
                int beg = (i * lim);
                int end = (System.Math.Min(((i + 1) * lim) - 1, total - 1));
                string[] w6 = new string[end - beg + 1];
                for (int j = 0; j < (end - beg + 1); j++)
                {
                    w6[j] = allwrds[beg + j];
                }
                Task<string> tb = Task<string>.Factory.StartNew(() => Raschl_Process_Word_SpellCheck(w6));
                Tasks3.Add(tb);
            }
            Task.WaitAll(Tasks3.ToArray());
            string result = "";
            foreach (Task<string> t9 in Tasks3)
            {
                result = result + " " + t9.Result;
            }
            res = result.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            if ((res == " ") || (res == ""))
            {
                return "";
            }
            else
            {
                res = res.Substring(1, res.Length - 1);
                Program.Mainform.BeginInvoke(new Action(() => Raschl_One_Complete(d.tb, d.numstr, res)));
                return res;
            }
        }               // решение одной расчлененки, если много переборов - то форимруем новые процессы
        private void Raschl_Process(Raschl_data d)
        {
            string t0 = (d.normal + "#").Replace("##", "#").Replace("##", "#");
            // разобрать на отдельные строки заданий
            string[] t1 = Regex.Split(t0, "\\#");
            //int cur_tasks = 0;
            var Tasks2 = new List<Task<string>>();
            //Task[] Tasks1 = new Task[Program.max_subtasks];
            int res = 0;
            int cur = 0;
            foreach (string t2 in t1)
            {
                cur++;
                if (t2 == "") { continue; }
                Raschl_one_string s1 = new Raschl_one_string();
                string[] t3 = Regex.Split(t2, "\\)");
                string[] wrds = new string[t3.Length - 1];
                int[] nums = new int[t3.Length - 1];
                for (int i = 0; i < t3.Length - 1; i++)
                {
                    string t4 = t3[i];
                    string[] t5 = Regex.Split(t4, "\\(");
                    wrds[i] = t5[0];
                    Int32.TryParse(t5[1], out res);
                    nums[i] = res;
                }
                // собрать данные в структуры по одной на строку
                s1.num = nums;
                s1.str = wrds;
                s1.numstr = cur;
                s1.tb = d.TextOut;
                // создать дополнительные дочерние потоки, передать им управление
                Task<string> ta = Task<string>.Factory.StartNew(() => Raschl_Process_One(s1));
                Tasks2.Add(ta);
            }
            // дождаться выполнения потоков, собрать результаты вместе
            Task.WaitAll(Tasks2.ToArray());
            string result = "";
            foreach (Task<string> t8 in Tasks2)
            {
                result = result + "\r\n" + t8.Result;
            }
            result = result.Substring(2, result.Length - 2);
            // call delegate
            Program.Mainform.BeginInvoke(new Action(() => Raschl_Complete(d.Tab, d.TextOut, result)));
        }                           // решение расчлененки - формирование прочих процессов
        public void Event_Raschl_Solve_Click(object sender, EventArgs e)                // по нажанию "Решить", ивент
        {
            // по-идее надо бы проверить типизацию входных данных, и, отказывать в обработке, если формат не верный
            ButtonsEnable(Data.Tab, false);
            Data.normal = Rashcl_NormalizeData(Data.TextIn.Text.ToString());
            if (Data.normal != "")
            {
                Task t1 = Task.Factory.StartNew(() => Raschl_Process(Data));
            }
            else
            {
                Data.TextOut.Text = "Входные данные вероятно не приведены в формат ресчлененок.\r\n\r\nДопустимые форматы:\r\nСлово(3),слово(2)\r\nслово ( 4) , слово  (2 ) , слово(1)\r\nслово (2)  Слово( 3) слово(2)\r\n\r\nИли же текст:\r\n\r\nслово(2)\r\nСлово ( 3 ) ,\r\nслово (2)\r\n\r\n где каждая расчлененка отделена от предыдущей минимум одной пустой строкой\r\n";
                ButtonsEnable(Data.Tab, true);
            }
        }
        
    }
}
