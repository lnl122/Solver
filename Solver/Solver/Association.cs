using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Solver
{
    class Association
    {
        int image_border_width = 5;
        public Program.words Association_Process_One(string path)
        {
            return Program.parse_google_page_words(Program.get_google_url_page(Program.upload_file(path)));
        } // вход - локальный путь к одной части, выход - структура о словах
        public List<Program.words> Association_Process(Program.Picture_data d)
        {
            var L2 = new List<string>();
            string dlevel2 = d.level.ToString();
            if (dlevel2 == "-1") { dlevel2 = "0"; }
            string dgameid = Program.dGame.game_id;
            if ((dgameid == null) || (dgameid == "")) { dgameid = "0"; }
            string basename = Program.Env.temp_path + "\\g" + dgameid + "_l" + dlevel2 + "_p" + d.cnt.ToString() + "_n" + "#" + ".jpg";
            //string basename = Program.Env.temp_path + "\\" + dlevel2 + "\\pic_#_(" + d.cnt.ToString() + ").jpg";
            //string basename = Program.Env.temp_path + "\\" + dlevel2 + "\\pic_" + dlevel2 + "#_(" + d.cnt.ToString() + ").jpg";
            int total_parts = d.str * d.col;
            int w = d.bmp.Width;
            int h = d.bmp.Height;
            int dw = w / d.col - 2 * image_border_width; // ширина одного блока
            int dh = h / d.str - 2 * image_border_width;
            int cnt = 0;
            for (int r = 0; r < d.str; r++)
            {
                for (int c = 0; c < d.col; c++)
                {
                    cnt++;
                    int sw = image_border_width + (w * c / d.col);
                    int sh = image_border_width + (h * r / d.str);
                    Bitmap nb = new Bitmap(dw, dh);
                    Rectangle re = new Rectangle(sw, sh, dw, dh);
                    nb = d.bmp.Clone(re, System.Drawing.Imaging.PixelFormat.Undefined);
                    string fn = basename.Replace("#", cnt.ToString());
                    nb.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
                    L2.Add(fn);
                }
            }
            var Tasks2 = new List<Task<Program.words>>();
            foreach (string t2 in L2) { Tasks2.Add(Task<Program.words>.Factory.StartNew(() => Association_Process_One(t2))); }
            // дождаться выполнения потоков, собрать результаты вместе
            Task.WaitAll(Tasks2.ToArray());
            List<Program.words> r2 = new List<Program.words>();
            int iii = d.init_num;
            foreach (Task<Program.words> t8 in Tasks2)
            {
                Program.words r8 = t8.Result;
                r8.level = d.level;
                r8.number = iii;
                r8.prot = d.prot;
                iii++;
                r2.Add(r8);
            }
            return r2;
        } // вход - структура одной картинки, выход - список структур о словах

        public List<Program.words> Associations_Process(Program.Pictures_data d) // вход структура с урлами всех картинок, без колонок/строк и прочего выход - список структур о словах
        {
            var Tasks2 = new List<Task<List<Program.words>>>();
            foreach (Program.Picture_data p1 in d.pics)
            {
                Tasks2.Add(Task<List<Program.words>>.Factory.StartNew(() => Association_Process(p1)));
            }
            Task.WaitAll(Tasks2.ToArray());
            List<Program.words> r = new List<Program.words>();
            foreach (Task<List<Program.words>> t8 in Tasks2) { foreach (Program.words r8 in t8.Result) { r.Add(r8); } }
            r = Program.words_google_to_find(r); // eng/rus/bad sorting
            if (d.Auto.Checked)
            {
                r = Program.words_to_engine(r, "find");
                r = Program.words_find_base(r);
                r = Program.words_to_engine(r, "base");
                r = Program.words_base_assoc(r);
                r = Program.words_to_engine(r, "assoc");
                //теперь надо решать сами ассоциации
                // r = тексты с ответами на все картинки, + их номера.
                int cnt_1_wrd = r.Count();
                int last_wrd = cnt_1_wrd * 2 - 1;
                int i1 = 1;
                int i2 = i1 + 1;
                int i3 = cnt_1_wrd + 1;
                while(i3 <= last_wrd)
                {
                    r = try_assoc(r, get_wi(r, i1), get_wi(r, i2), i3);
                    i1++; i1++;
                    i2++; i2++;
                    i3++;
                }
            }

            Program.Mainform.BeginInvoke(new Action(() => Association_Buttons_Enable(d)));
            Program.Mainform.BeginInvoke(new Action(() => Association_Show_Anwers(d, r)));

            return r;
        }

        private void Association_Show_Anwers(Program.Pictures_data d, List<Program.words> res)
        {
            Data.TextOut.Visible = true;
            Data.BtnSolve.Enabled = false;
            foreach (Program.words wrd in res)
            {
                if ((wrd.answer != "") && (wrd.answer != null))
                {
                    Data.TextOut.Text += (wrd.number + " = " + wrd.answer + " !");
                }
                else
                {
                    Data.TextOut.Text += (wrd.number + " : ");
                    Data.TextOut.Text += (wrd.g_variant + " | ");
                    foreach (string str in wrd.w_find) { Data.TextOut.Text += (str + " "); }
                    Data.TextOut.Text += " | ";
                    foreach (string str in wrd.w_base) { Data.TextOut.Text += (str + " "); }
                    Data.TextOut.Text += " | ";
                    foreach (string str in wrd.w_assoc) { Data.TextOut.Text += (str + " "); }
                }
                Data.TextOut.Text += "\r\n";
            }
            Data.Tab.Text = Data.Tab.Text + " #";
            Event_Association_ChangeSize(null, null);
        }

        public Program.Pictures_data Data;
        public Association(int level, List<string> urls)//
        {
            if (urls.Count == 0) { MessageBox.Show("В задании нет ни одной ссылки на картинки"); return; }
            Data.type = "Association";
            Data.level = level;
            Data.urls = urls;
            Data.Tab = new TabPage();
            Data.Tab.Text = level.ToString() + " : " + "Олимпийка";
            Data.pic_cnt = urls.Count;
            Data.olimp_size = 0; // ?? нужно будет для олимпиек
            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_Association_Solve_Click);
            Data.Tab.Controls.Add(Data.BtnSolve);
            Data.Auto = new CheckBox();
            Data.Auto.Text = "авто-вбивать";
            Data.Auto.Checked = true;
            if (Data.level < 1)
            {
                Data.Auto.Checked = false;
                Data.Auto.Enabled = false;
            }
            Data.Tab.Controls.Add(Data.Auto);
            Data.BtnClose = new Button();
            Data.BtnClose.Text = "Закрыть";
            Data.BtnClose.Click += new EventHandler(Event_Association_Close_Click);
            Data.Tab.Controls.Add(Data.BtnClose);
            Data.pics = new Program.Picture_data[Data.pic_cnt];
            for (int i = 0; i < Data.pic_cnt; i++)
            {
                Data.pics[i].level = Data.level;
                Data.pics[i].str = 0;
                Data.pics[i].col = 0;
                Data.pics[i].cnt = i + 1;
                Data.pics[i].init_num = 0;
            }
            Data.prot = Program.prot.none;
            Data.init_num = new NumericUpDown();
            Data.init_num.Minimum = 0;
            Data.init_num.Maximum = 257;
            Data.init_num.Increment = 1;
            Data.init_num.Value = 1;
            Data.pics[0].init_num = Convert.ToInt32(Data.init_num.Value);
            Data.init_num.ValueChanged += new EventHandler(Event_Association_InitNum_Change);
            Data.Tab.Controls.Add(Data.init_num);
            Data.pics_list = new ListBox();
            Data.ar_urls = new string[Data.pic_cnt];
            int ii9 = 0;
            foreach (string u in Data.urls)
            {
                Data.pics_list.Items.Add(u);
                Data.ar_urls[ii9] = u;
                ii9++;
            }
            Data.pics_list.SelectedIndex = Data.pic_cnt - 1;
            Data.pics_list.SelectedIndexChanged += new EventHandler(Event_Association_ListPics_Select);
            Data.Tab.Controls.Add(Data.pics_list);
            Data.cb_protect = new ComboBox();
            Data.cb_protect.Items.Add("Без защиты");
            Data.cb_protect.Items.Add("5слово");
            Data.cb_protect.Items.Add("05слово");
            Data.cb_protect.Items.Add("005слово");
            Data.cb_protect.Items.Add("слово5");
            Data.cb_protect.Items.Add("слово05");
            Data.cb_protect.Items.Add("слово005");
            Data.cb_protect.SelectedIndex = 0;
            Data.cb_protect.SelectedIndexChanged += new EventHandler(Event_Association_Protect_Change);
            Data.Tab.Controls.Add(Data.cb_protect);
            Data.cb_str = new ComboBox();
            Data.cb_str.Items.Add("Строк");
            Data.cb_str.Items.Add("1");
            Data.cb_str.Items.Add("2");
            Data.cb_str.Items.Add("3");
            Data.cb_str.Items.Add("4");
            Data.cb_str.Items.Add("5");
            Data.cb_str.Items.Add("6");
            Data.cb_str.Items.Add("7");
            Data.cb_str.Items.Add("8");
            Data.cb_str.Items.Add("9");
            Data.cb_str.SelectedIndex = 0;
            Data.cb_str.SelectedIndexChanged += new EventHandler(Event_Association_Str_Change);
            Data.Tab.Controls.Add(Data.cb_str);
            Data.cb_col = new ComboBox();
            Data.cb_col.Items.Add("Колонок");
            Data.cb_col.Items.Add("1");
            Data.cb_col.Items.Add("2");
            Data.cb_col.Items.Add("3");
            Data.cb_col.Items.Add("4");
            Data.cb_col.Items.Add("5");
            Data.cb_col.Items.Add("6");
            Data.cb_col.Items.Add("7");
            Data.cb_col.Items.Add("8");
            Data.cb_col.Items.Add("9");
            Data.cb_col.SelectedIndex = 0;
            Data.cb_col.SelectedIndexChanged += new EventHandler(Event_Association_Col_Change);
            Data.Tab.Controls.Add(Data.cb_col);
            Data.lb_init = new Label();
            Data.lb_init.Text = "Начальный номер:";
            Data.Tab.Controls.Add(Data.lb_init);
            Data.lb_col = new Label();
            Data.lb_col.Text = "Колонок:";
            Data.Tab.Controls.Add(Data.lb_col);
            Data.lb_str = new Label();
            Data.lb_str.Text = "Строк:";
            Data.Tab.Controls.Add(Data.lb_str);
            Data.lb_prot = new Label();
            Data.lb_prot.Text = "Защита:";
            Data.Tab.Controls.Add(Data.lb_prot);
            Data.img = new PictureBox();
            for (int i = 0; i < Data.pic_cnt; i++)
            {
                Data.img.Load(Data.ar_urls[i]);
                Data.pics[i].img = Data.img.Image;
                Data.pics[i].bmp = new Bitmap(Data.pics[i].img);
            }
            Data.img.SizeMode = PictureBoxSizeMode.StretchImage;
            Data.Tab.Controls.Add(Data.img);
            Data.pics_list.SelectedIndex = 0;
            Data.img.Image = Data.pics[0].img;
            Data.TextOut = new TextBox();
            Data.TextOut.Visible = false;
            Data.TextOut.AcceptsReturn = true;
            Data.TextOut.AcceptsTab = false;
            Data.TextOut.Multiline = true;
            Data.TextOut.ScrollBars = ScrollBars.Both;
            Data.Tab.Controls.Add(Data.TextOut);

            Event_Association_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_Association_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }

        private void Event_Association_Protect_Change(object sender, EventArgs e)
        {
            switch (Data.cb_protect.SelectedIndex)
            {
                case 0: Data.prot = Program.prot.none; break;
                case 1: Data.prot = Program.prot.begin1; break;
                case 2: Data.prot = Program.prot.begin2; break;
                case 3: Data.prot = Program.prot.begin3; break;
                case 4: Data.prot = Program.prot.end1; break;
                case 5: Data.prot = Program.prot.end2; break;
                case 6: Data.prot = Program.prot.end3; break;
                default: Data.prot = Program.prot.none; break;
            }
        }
        private void Event_Association_ListPics_Select(object sender, EventArgs e)
        {
            Data.img.Image = Data.pics[Data.pics_list.SelectedIndex].img;
            Data.init_num.Value = Data.pics[Data.pics_list.SelectedIndex].init_num;
            Data.cb_str.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].str;
            Data.cb_col.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].col;
        }
        private static void Association_Buttons_Enable(Program.Pictures_data d)                  // меняем оптом доступность кнопок
        {
            d.BtnSolve.Enabled = true;
            d.BtnClose.Enabled = true;
            if (d.level != -1) { d.Auto.Enabled = true; }
            d.cb_str.Enabled = true;
            d.cb_col.Enabled = true;
            d.cb_protect.Enabled = true;
            d.pics_list.Enabled = true;
            d.init_num.Enabled = true;
        }
        private static void Association_Buttons_Disable(Program.Pictures_data d)                  // меняем оптом доступность кнопок
        {
            d.BtnSolve.Enabled = false;
            d.BtnClose.Enabled = false;
            d.Auto.Enabled = false;
            d.cb_str.Enabled = false;
            d.cb_col.Enabled = false;
            d.cb_protect.Enabled = false;
            d.pics_list.Enabled = false;
            d.init_num.Enabled = false;
        }
        private void Event_Association_Close_Click(object sender, EventArgs e)
        {
            Data.Tab.Dispose();
        }
        private void Event_Association_ChangeSize(object sender, EventArgs e)
        {
            Data.BtnSolve.Top = Program.mainform_border;
            Data.BtnSolve.Left = Program.mainform_border;
            Data.BtnSolve.Width = 20 * Program.mainform_border;
            Data.BtnSolve.Height = 5 * Program.mainform_border;
            Data.Auto.Top = Program.mainform_border;
            Data.Auto.Left = Data.BtnSolve.Right + 2 * Program.mainform_border;
            Data.BtnClose.Top = Program.mainform_border;
            Data.BtnClose.Width = 20 * Program.mainform_border;
            Data.BtnClose.Height = 5 * Program.mainform_border;
            Data.BtnClose.Left = Program.GameTab.MainTab.Width - Data.BtnClose.Width - Program.mainform_border;
            Data.pics_list.Top = Data.BtnSolve.Bottom + 2 * Program.mainform_border;
            Data.pics_list.Left = Program.mainform_border;
            Data.pics_list.Width = Program.GameTab.MainTab.Width / 3 - 2 * Program.mainform_border;
            Data.pics_list.Height = 30 * Program.mainform_border;
            Data.lb_prot.Top = Data.pics_list.Bottom + 2 * Program.mainform_border;
            Data.lb_prot.Left = Program.mainform_border;
            Data.lb_prot.Width = 25 * Program.mainform_border;
            Data.lb_prot.Height = 5 * Program.mainform_border;
            Data.lb_init.Top = Data.lb_prot.Bottom + Program.mainform_border;
            Data.lb_init.Left = Program.mainform_border;
            Data.lb_init.Width = Data.lb_prot.Width;
            Data.lb_init.Height = Data.lb_prot.Height;
            Data.lb_str.Top = Data.lb_init.Bottom + Program.mainform_border;
            Data.lb_str.Left = Program.mainform_border;
            Data.lb_str.Width = Data.lb_prot.Width;
            Data.lb_str.Height = Data.lb_prot.Height;
            Data.lb_col.Top = Data.lb_str.Bottom + Program.mainform_border;
            Data.lb_col.Left = Program.mainform_border;
            Data.lb_col.Width = Data.lb_prot.Width;
            Data.lb_col.Height = Data.lb_prot.Height;
            Data.cb_protect.Top = Data.lb_prot.Top;
            Data.cb_protect.Left = Data.lb_prot.Right + Program.mainform_border;
            Data.cb_protect.Width = Data.lb_prot.Width;
            Data.cb_protect.Height = Data.lb_prot.Height;
            Data.init_num.Top = Data.lb_init.Top;
            Data.init_num.Left = Data.lb_prot.Right + Program.mainform_border;
            Data.init_num.Width = Data.lb_prot.Width;
            Data.init_num.Height = Data.lb_prot.Height;
            Data.cb_str.Top = Data.lb_str.Top;
            Data.cb_str.Left = Data.lb_prot.Right + Program.mainform_border;
            Data.cb_str.Width = Data.lb_prot.Width;
            Data.cb_str.Height = Data.lb_prot.Height;
            Data.cb_col.Top = Data.lb_col.Top;
            Data.cb_col.Left = Data.lb_prot.Right + Program.mainform_border;
            Data.cb_col.Width = Data.lb_prot.Width;
            Data.cb_col.Height = Data.lb_prot.Height;
            int mm = Data.cb_col.Right;
            if (Data.pics_list.Right > mm) { mm = Data.pics_list.Right; }
            //Data.img.Height = Program.GameTab.MainTab.Height - Data.pics_list.Top - 1 * Program.mainform_border;
            Data.img.Top = Data.pics_list.Top;
            Data.img.Left = mm + 2 * Program.mainform_border;
            Data.img.Width = Program.GameTab.MainTab.Width - mm - 3 * Program.mainform_border;
            Data.TextOut.Left = Data.img.Left;
            Data.TextOut.Width = Data.img.Width;
            if (Data.Tab.Text.Substring(Data.Tab.Text.Length-1,1) == "#")
            {
                Data.img.Height = (Program.GameTab.MainTab.Height - Data.pics_list.Top - 1 * Program.mainform_border) / 2 - Program.mainform_border;
                Data.TextOut.Top = Data.img.Bottom + Program.mainform_border;
                Data.TextOut.Height = Data.img.Height;
            }
            else
            {
                Data.img.Height = Program.GameTab.MainTab.Height - Data.pics_list.Top - 1 * Program.mainform_border;
                Data.TextOut.Top = Data.img.Top;
                Data.TextOut.Height = Data.img.Height;
            }
        }
        private void Event_Association_InitNum_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].init_num = Convert.ToInt32(Data.init_num.Value);
        }
        private void Event_Association_Str_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].str = Data.cb_str.SelectedIndex;
        }
        private void Event_Association_Col_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].col = Data.cb_col.SelectedIndex;
        }
        private void Event_Association_Solve_Click(object sender, EventArgs e)
        {
            //проверим, все ли готово
            for (int i = 0; i < Data.pic_cnt; i++)
            {
                Data.pics[i].prot = Data.prot;
                if (Data.pics[i].str * Data.pics[i].col * Data.pics[i].init_num == 0)
                { MessageBox.Show("Для " + (i + 1).ToString() + "-й картинки заполнены не все параметры.."); return; }
            }
            //можно стартовать процессы по собранным данным
            Association_Buttons_Disable(Data);
            Program.Log("Начали решать ассоциации\r\n.\r\n");
            Task<List<Program.words>> t1 = Task<List<Program.words>>.Factory.StartNew(() => Associations_Process(Data));
        }
        private string get_wi(List<Program.words> q, int i)
        {
            foreach (Program.words q1 in q) { if (q1.number == i) { return q1.answer; } }
            return "";
        }
        private List<Program.words> try_assoc(List<Program.words> q, string s1, string s2, int idx)
        {
            if (s1 == null) { s1 = ""; }
            if (s2 == null) { s2 = ""; }
            List<Program.words> w = new List<Program.words>();
            List<Program.words> w9 = new List<Program.words>();
            Program.words w1 = new Program.words();
            foreach (Program.words q1 in q) { w1.level = q1.level; w1.number = idx; w1.prot = q1.prot; break; }
            if ((s1 == "") || (s2 == "")) { w.AddRange(q); w1.w_find = new List<string>(); w1.w_base = new List<string>(); w1.w_assoc = new List<string>(); w.Add(w1); return w; }
            w1.w_find = Program.get_assoc_word(s1);
            w1.w_base = Program.get_assoc_word(s2);
            w1.w_assoc = new List<string>();
            foreach (string str1 in w1.w_find) { if (w1.w_base.Contains(str1)) { w1.w_assoc.Add(str1); } }
            w9.Add(w1);
            w9 = Program.words_to_engine(w9, "assoc");
            w.AddRange(q);
            w.AddRange(w9);
            return w;
        }
    }
}
