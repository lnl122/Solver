using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Solver
{
    class Raschl
    {
        struct Raschl_one_string
        {
            public int numstr;
            public string[] str;
            public int[] num;
            public int wrd_cnt;
        }                                             // струкрура данных для одной строчки расчлененок
        struct Raschl_WrdSplChk
        {
            public string[] wrd;
            public int wrd_cnt;
        }
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
            public ComboBox WrdLen;
            public CheckBox Auto;
            public int wrd_cnt;
        }// основная структура задания
        static Raschl_data Data = new Raschl_data();

        public Raschl(int level, string def)                                                         // Создаем новый Таб + структуру публичных данных
        {
            Data.type = "Rashcl";
            Data.Tab = new TabPage();
            Data.level = level;
            Data.wrd_cnt = 1;
            Data.Tab.Text = level.ToString() + " : " + "Расчлененки";
            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_Raschl_Solve_Click);
            Data.Tab.Controls.Add(Data.BtnSolve);
            Data.Auto = new CheckBox();
            Data.Auto.Text = "авто-вбивать";
            Data.Auto.Checked = true;
            if (Data.level == -1)
            {
                Data.Auto.Checked = false;
                Data.Auto.Enabled = false;
            }
            Data.Tab.Controls.Add(Data.Auto);
            Data.WrdLen = new ComboBox();
            Data.WrdLen.Items.Add("из 1 слова");
            Data.WrdLen.Items.Add("из 2 слов");
            Data.WrdLen.SelectedIndexChanged += new EventHandler(Event_Raschl_ChangeWrdCnt);
            Data.WrdLen.SelectedIndex = 0;
            Data.Tab.Controls.Add(Data.WrdLen);
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
        
        public static void Event_Raschl_ChangeWrdCnt(object sender, EventArgs e)
        {
            Data.wrd_cnt = 1;
            if (Data.WrdLen.SelectedIndex == 1) { Data.wrd_cnt = 2; }
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
            Data.Auto.Top = Program.mainform_border;
            Data.Auto.Left = Data.BtnSolve.Right + 2 * Program.mainform_border;
            Data.WrdLen.Top = Program.mainform_border;
            Data.WrdLen.Left = Data.Auto.Right + 2 * Program.mainform_border;
            Data.WrdLen.Height = Data.BtnSolve.Height;
            Data.WrdLen.Width = 20 * Program.mainform_border;
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
        public void Event_Raschl_Solve_Click(object sender, EventArgs e)                // по нажанию "Решить", ивент
        {            
            Raschl_Buttons_Disable();
            Data.normal = Rashcl_NormalizeData(Data.TextIn.Text.ToString());
            if (Data.normal != "")
            {
                Program.Log("Начали решать расчленёнки. Нормализованный текст задания:\r\n\r\n"+ Data.normal + "\r\n.\r\n");
                Task t1 = Task.Factory.StartNew(() => Raschl_Process(Data));
            }
            else
            {
                Data.TextOut.Text = "Входные данные вероятно не приведены в формат ресчлененок.\r\n\r\nДопустимые форматы:\r\nСлово(3),слово(2)\r\nслово ( 4) , слово  (2 ) , слово(1)\r\nслово (2)  Слово( 3) слово(2)\r\n\r\nИли же текст:\r\n\r\nслово(2)\r\nСлово ( 3 ) ,\r\nслово (2)\r\n\r\n где каждая расчлененка отделена от предыдущей минимум одной пустой строкой\r\n";
                Raschl_Buttons_Enable();
            }
        }
        public static void Raschl_Buttons_Enable()                  // меняем оптом доступность кнопок
        {
            Data.BtnSolve.Enabled = true;
            Data.BtnClose.Enabled = true;
            Data.TextIn.Enabled = true;
            Data.TextOut.Enabled = true;
            Data.WrdLen.Enabled = true;
            if (Data.level != -1) { Data.Auto.Enabled = true; }
        }
        public static void Raschl_Buttons_Disable()                  // меняем оптом доступность кнопок
        {
            Data.BtnSolve.Enabled = false;
            Data.BtnClose.Enabled = false;
            Data.TextIn.Enabled = false;
            Data.TextOut.Enabled = false;
            Data.WrdLen.Enabled = false;
            Data.Auto.Enabled = false;
        }
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
        public void Raschl_Complete(TabPage mTab, TextBox mTextOut, string str) // делегат, принимающий возвращенные потоком параметры. взаимодействует с ГУИ
        {
            mTab.Text = mTab.Text + " *";
            mTextOut.Text = str;
            // возвращаем доступность кнопок
            Raschl_Buttons_Enable();
        }

        private List<string> Raschl_Process_Word_SpellCheck(Raschl_WrdSplChk d)
        {
            //Microsoft.Office.Interop.Word.Application 
            // 2do если надо по два/три слова проверять?
            var wordApp = new Microsoft.Office.Interop.Word.Application();
            wordApp.Visible = false;
            List<string> res = new List<string>();
            if (d.wrd_cnt == 1)
            {
                foreach (string s1 in d.wrd)
                {
                    if(wordApp.CheckSpelling(s1))
                    {
                        res.Add(s1);
                    } else
                    {
                        if (wordApp.CheckSpelling(s1.Substring(0, 1).ToUpper() + s1.Substring(1, s1.Length - 1)))
                        {
                            res.Add(s1);
                        }
                    }
                }
            }
            if (d.wrd_cnt == 2)
            {
                foreach (string s1 in d.wrd)
                {
                    string[] ar1 = s1.Split(' ');
                    if (ar1.Length < 2) { continue; }
                    if (wordApp.CheckSpelling(ar1[0]))
                    {
                        //пробуем второе слово
                        if (wordApp.CheckSpelling(ar1[1]))
                        {
                            //ура
                            res.Add(s1);
                        }
                        else
                        {
                            //пробуем капитализировать второе слово
                            if (wordApp.CheckSpelling(ar1[1].Substring(0, 1).ToUpper() + ar1[1].Substring(1, ar1[1].Length - 1)))
                            {
                                //второе слово подошло капитализированное
                                //ура
                                res.Add(s1);
                            }
                            //первое слово никак не подошло
                        }
                    }
                    else
                    {
                        //пробуем капитализировать первое слово
                        if (wordApp.CheckSpelling(ar1[0].Substring(0, 1).ToUpper() + ar1[0].Substring(1, ar1[0].Length - 1)))
                        {
                            //первое слово подошло капитализированное
                            if (wordApp.CheckSpelling(ar1[1]))
                            {
                                //ура
                                res.Add(s1);
                            }
                            else
                            {
                                //пробуем капитализировать второе слово
                                if (wordApp.CheckSpelling(ar1[1].Substring(0, 1).ToUpper() + ar1[1].Substring(1, ar1[1].Length - 1)))
                                {
                                    //второе слово подошло капитализированное
                                    //ура
                                    res.Add(s1);
                                }
                                //второе слово никак не подошло
                            }
                        }
                        //первое слово никак не подошло
                    }
                }
            }

            wordApp.Quit();
            return res;
        }                       // проверка орфографии Ворд лимитированного колва слов (обычно до 1000)
        private List<string> Raschl_Process_One(Raschl_one_string d)
        {
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
            if (d.wrd_cnt == 2)
            {
                // если нужно для 2-х слов
                List<string> l3 = new List<string>();
                foreach (string s7 in allwrds) { for(int i=1; i<s7.Length; i++) { l3.Add(s7.Substring(0,i)+" "+s7.Substring(i)); } }
                total = l3.Count;
                allwrds = l3.ToArray();
            }

            int lim = 1000;
            curwrd = 0;
            int partcnt = ((total + lim - 1) / lim) - 1;
            var Tasks3 = new List<Task<List<string>>>();
            for (int i = 0; i <= partcnt; i++)
            {
                int beg = (i * lim);
                int end = (System.Math.Min(((i + 1) * lim) - 1, total - 1));
                string[] w6 = new string[end - beg + 1];
                for (int j = 0; j < (end - beg + 1); j++)
                {
                    w6[j] = allwrds[beg + j];
                }
                Raschl_WrdSplChk w7 = new Raschl_WrdSplChk();
                w7.wrd = w6;
                w7.wrd_cnt = d.wrd_cnt;
                Task<List<string>> tb = Task<List<string>>.Factory.StartNew(() => Raschl_Process_Word_SpellCheck(w7));
                Tasks3.Add(tb);
            }
            Task.WaitAll(Tasks3.ToArray());
            List<string> result = new List<string>();
            foreach (Task<List<string>> t9 in Tasks3)
            {
                List<string> tt = new List<string>();
                tt = t9.Result;
                foreach(string tt1 in tt) { result.Add(tt1); }
            }
            return result;
        }               // решение одной расчлененки, если много переборов - то форимруем новые процессы
        private void Raschl_Process(Raschl_data d)
        {
            string t0 = (d.normal + "#").Replace("##", "#").Replace("##", "#");
            // разобрать на отдельные строки заданий
            string[] t1 = Regex.Split(t0, "\\#");
            //int cur_tasks = 0;
            var Tasks2 = new List<Task<List<string>>>();
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
                s1.wrd_cnt = d.wrd_cnt;
                // создать дополнительные дочерние потоки, передать им управление
                Task<List<string>> ta = Task<List<string>>.Factory.StartNew(() => Raschl_Process_One(s1));
                Tasks2.Add(ta);
            }
            // дождаться выполнения потоков, собрать результаты вместе
            Task.WaitAll(Tasks2.ToArray());
            string result = "";
            if (d.Auto.Checked) { while (Program.input_busy) { Thread.Sleep(1000); } Program.input_busy = true; }
            foreach (Task<List<string>> t8 in Tasks2)
            {
                // 2do - если есть уровень - надо вбивать, остаток показать юзеру
                // вбиваем
                if (d.Auto.Checked)
                {
                    bool fl = false;
                    foreach (string ss in t8.Result)
                    {
                        bool fl2 = Program.try_form_send(d.level, ss);
                        if (fl2) { fl = true;  break; }
                    }
                    if (fl) { result = result + "ok"; }
                    else  { foreach (string ss in t8.Result) { result = result + ss + " "; } }
                }
                else { foreach (string ss in t8.Result) { result = result + ss + " "; } }
                result = result + "\r\n";
            }
            if (d.Auto.Checked) { Program.input_busy = false; }
            // call delegate
            Program.Mainform.BeginInvoke(new Action(() => Raschl_Complete(d.Tab, d.TextOut, result)));
        }                           // решение расчлененки - формирование прочих процессов
        
    }
}
