﻿/*using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;


namespace Solver
{
    class Gybrid
    {
        int image_border_width = 5;
        public Program.words Gybrid_Process_One(string path)
        {
            List<string> wrd = Google.GetImageDescription(path);
            Program.words wrds = new Program.words();
            wrds.g_words = wrd;
            wrds.w_find = wrd;
            return wrds;
        } // вход - локальный путь к одной части, выход - структура о словах
        public List<Program.words> Gybrid_Process(Program.Picture_data d)
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
            foreach (string t2 in L2) { Tasks2.Add(Task<Program.words>.Factory.StartNew(() => Gybrid_Process_One(t2))); }
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
        public List<Program.words> Gybrids_Process(Program.Pictures_data d) // вход структура с урлами всех картинок, без колонок/строк и прочего выход - список структур о словах
        {
            var Tasks2 = new List<Task<List<Program.words>>>();
            foreach (Program.Picture_data p1 in d.pics)
            {
                Tasks2.Add(Task<List<Program.words>>.Factory.StartNew(() => Gybrid_Process(p1)));
            }
            Task.WaitAll(Tasks2.ToArray());
            List<Program.words> r = new List<Program.words>();
            foreach (Task<List<Program.words>> t8 in Tasks2) { foreach (Program.words r8 in t8.Result) { r.Add(r8); } }
            if (d.Auto.Checked)
            {
                Program.words[] w = r.ToArray();
                int wi = w.Length;
                while (Program.input_busy) { System.Threading.Thread.Sleep(1000); }
                Program.input_busy = true;
                for (int i1 = 0; i1 < wi; i1++)
                {
                    if (w[i1].answer == null) { w[i1].answer = ""; }
                    if (w[i1].answer != "") { continue; }
                    for (int i2 = 0; i2 < wi; i2++)
                    {
                        if (w[i2].answer == null) { w[i2].answer = ""; }
                        if (w[i2].answer != "") { continue; }
                        if (i1 == i2) { continue; }
                        // разные  i1 i2, ответов не было
                        List<string> logo1 = is_Gybrid(w[i1].w_find, w[i2].w_find);
                        if (logo1.Count != 0) { foreach (string ss9 in logo1) { if (Program.try_form_send(w[i1].level, ss9)) { w[i1].answer = ss9; w[i2].answer = ss9; break; } } }

                    }
                }
                Program.input_busy = false;
                r = new List<Program.words>(w);

                r = Program.words_find_base(r);
                r = Program.words_base_assoc(r);

                w = r.ToArray();
                wi = w.Length;
                for (int i1 = 0; i1 < wi; i1++)
                {
                    w[i1].w_base_all.AddRange(w[i1].w_assoc);
                    w[i1].w_base_all.AddRange(w[i1].w_find);
                    w[i1].w_find = w[i1].w_base_all;
                }
                while (Program.input_busy) { System.Threading.Thread.Sleep(1000); }
                Program.input_busy = true;
                for (int i1 = 0; i1 < wi; i1++)
                {
                    if (w[i1].answer == null) { w[i1].answer = ""; }
                    if (w[i1].answer != "") { continue; }
                    for (int i2 = 0; i2 < wi; i2++)
                    {
                        if (w[i2].answer == null) { w[i2].answer = ""; }
                        if (w[i2].answer != "") { continue; }
                        if (i1 == i2) { continue; }
                        // разные  i1 i2, ответов не было
                        List<string> logo1 = is_Gybrid(w[i1].w_find, w[i2].w_find);
                        if (logo1.Count != 0) { foreach (string ss9 in logo1) { if (Program.try_form_send(w[i1].level, ss9)) { w[i1].answer = ss9; w[i2].answer = ss9; break; } } }
                    }
                }
                Program.input_busy = false;
                r = new List<Program.words>(w);
            }

            Program.Mainform.BeginInvoke(new Action(() => Gybrid_Buttons_Enable(d)));
            Program.Mainform.BeginInvoke(new Action(() => Gybrid_Show_Anwers(d, r)));

            return r;
        }

        private List<string> is_Gybrid(List<string> w1, List<string> w2)
        {
            List<string> rr = new List<string>();
            foreach (string s1 in w1)
            {
                foreach (string s2 in w2)
                {
                    if (s1.Length < 4) { continue; }
                    if (s2.Length < 4) { continue; }
                    //int s1l = s1.Length;
                    int s2l = s2.Length;
                    if (s1.Substring(0, 3) == s2.Substring(s2l - 3)) { rr.Add(s2 + s1.Substring(3)); }
                    //if (s2.Substring(0, 3) == s1.Substring(s1l - 3)) { rr.Add(s1 + s2.Substring(3)); }
                }
            }
            return new List<string>(rr.Distinct().ToArray());
        }

        private void Gybrid_Show_Anwers(Program.Pictures_data d, List<Program.words> res)
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
            Event_Gybrid_ChangeSize(null, null);
        }
        public Program.Pictures_data Data;
        public Gybrid(int level, List<string> urls)//для только решения картинок
        {
            if (urls.Count == 0) { MessageBox.Show("В задании нет ни одной ссылки на картинки"); return; }
            Data.type = "Gybrid";
            Data.level = level;
            Data.urls = urls;
            Data.Tab = new TabPage();
            Data.Tab.Text = level.ToString() + " : " + "Гибриды";
            Data.pic_cnt = urls.Count;
            Data.olimp_size = 0; // ?? нужно будет для олимпиек
            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_Gybrid_Solve_Click);
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
            Data.BtnClose.Click += new EventHandler(Event_Gybrid_Close_Click);
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
            Data.init_num.ValueChanged += new EventHandler(Event_Gybrid_InitNum_Change);
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
            Data.pics_list.SelectedIndexChanged += new EventHandler(Event_Gybrid_ListPics_Select);
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
            Data.cb_protect.SelectedIndexChanged += new EventHandler(Event_Gybrid_Protect_Change);
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
            Data.cb_str.SelectedIndexChanged += new EventHandler(Event_Gybrid_Str_Change);
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
            Data.cb_col.SelectedIndexChanged += new EventHandler(Event_Gybrid_Col_Change);
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

            Event_Gybrid_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_Gybrid_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }

        private void Event_Gybrid_Protect_Change(object sender, EventArgs e)
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
        private void Event_Gybrid_ListPics_Select(object sender, EventArgs e)
        {
            Data.img.Image = Data.pics[Data.pics_list.SelectedIndex].img;
            Data.init_num.Value = Data.pics[Data.pics_list.SelectedIndex].init_num;
            Data.cb_str.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].str;
            Data.cb_col.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].col;
        }
        private static void Gybrid_Buttons_Enable(Program.Pictures_data d)                  // меняем оптом доступность кнопок
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
        private static void Gybrid_Buttons_Disable(Program.Pictures_data d)                  // меняем оптом доступность кнопок
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
        private void Event_Gybrid_Close_Click(object sender, EventArgs e)
        {
            Data.Tab.Dispose();
        }
        private void Event_Gybrid_ChangeSize(object sender, EventArgs e)
        {
            Data.BtnSolve.Top = MainForm.border;
            Data.BtnSolve.Left = MainForm.border;
            Data.BtnSolve.Width = 20 * MainForm.border;
            Data.BtnSolve.Height = 5 * MainForm.border;
            Data.Auto.Top = MainForm.border;
            Data.Auto.Left = Data.BtnSolve.Right + 2 * MainForm.border;
            Data.BtnClose.Top = MainForm.border;
            Data.BtnClose.Width = 20 * MainForm.border;
            Data.BtnClose.Height = 5 * MainForm.border;
            Data.BtnClose.Left = Program.MainTab.Width - Data.BtnClose.Width - MainForm.border;
            Data.pics_list.Top = Data.BtnSolve.Bottom + 2 * MainForm.border;
            Data.pics_list.Left = MainForm.border;
            Data.pics_list.Width = Program.MainTab.Width / 3 - 2 * MainForm.border;
            Data.pics_list.Height = 30 * MainForm.border;
            Data.lb_prot.Top = Data.pics_list.Bottom + 2 * MainForm.border;
            Data.lb_prot.Left = MainForm.border;
            Data.lb_prot.Width = 25 * MainForm.border;
            Data.lb_prot.Height = 5 * MainForm.border;
            Data.lb_init.Top = Data.lb_prot.Bottom + MainForm.border;
            Data.lb_init.Left = MainForm.border;
            Data.lb_init.Width = Data.lb_prot.Width;
            Data.lb_init.Height = Data.lb_prot.Height;
            Data.lb_str.Top = Data.lb_init.Bottom + MainForm.border;
            Data.lb_str.Left = MainForm.border;
            Data.lb_str.Width = Data.lb_prot.Width;
            Data.lb_str.Height = Data.lb_prot.Height;
            Data.lb_col.Top = Data.lb_str.Bottom + MainForm.border;
            Data.lb_col.Left = MainForm.border;
            Data.lb_col.Width = Data.lb_prot.Width;
            Data.lb_col.Height = Data.lb_prot.Height;
            Data.cb_protect.Top = Data.lb_prot.Top;
            Data.cb_protect.Left = Data.lb_prot.Right + MainForm.border;
            Data.cb_protect.Width = Data.lb_prot.Width;
            Data.cb_protect.Height = Data.lb_prot.Height;
            Data.init_num.Top = Data.lb_init.Top;
            Data.init_num.Left = Data.lb_prot.Right + MainForm.border;
            Data.init_num.Width = Data.lb_prot.Width;
            Data.init_num.Height = Data.lb_prot.Height;
            Data.cb_str.Top = Data.lb_str.Top;
            Data.cb_str.Left = Data.lb_prot.Right + MainForm.border;
            Data.cb_str.Width = Data.lb_prot.Width;
            Data.cb_str.Height = Data.lb_prot.Height;
            Data.cb_col.Top = Data.lb_col.Top;
            Data.cb_col.Left = Data.lb_prot.Right + MainForm.border;
            Data.cb_col.Width = Data.lb_prot.Width;
            Data.cb_col.Height = Data.lb_prot.Height;
            int mm = Data.cb_col.Right;
            if (Data.pics_list.Right > mm) { mm = Data.pics_list.Right; }
            //Data.img.Height = Program.MainTab.Height - Data.pics_list.Top - 1 * MainForm.border;
            Data.img.Top = Data.pics_list.Top;
            Data.img.Left = mm + 2 * MainForm.border;
            Data.img.Width = Program.MainTab.Width - mm - 3 * MainForm.border;
            Data.TextOut.Left = Data.img.Left;
            Data.TextOut.Width = Data.img.Width;
            if (Data.Tab.Text.Substring(Data.Tab.Text.Length - 1, 1) == "#")
            {
                Data.img.Height = (Program.MainTab.Height - Data.pics_list.Top - 1 * MainForm.border) / 2 - MainForm.border;
                Data.TextOut.Height = Data.img.Height;
                Data.TextOut.Top = Data.img.Bottom + MainForm.border;
            }
            else
            {
                Data.img.Height = Program.MainTab.Height - Data.pics_list.Top - 1 * MainForm.border;
                Data.TextOut.Top = Data.img.Top;
                Data.TextOut.Height = Data.img.Height;
            }
        }
        private void Event_Gybrid_InitNum_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].init_num = Convert.ToInt32(Data.init_num.Value);
        }
        private void Event_Gybrid_Str_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].str = Data.cb_str.SelectedIndex;
        }
        private void Event_Gybrid_Col_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].col = Data.cb_col.SelectedIndex;
        }
        private void Event_Gybrid_Solve_Click(object sender, EventArgs e)
        {
            //проверим, все ли готово
            for (int i = 0; i < Data.pic_cnt; i++)
            {
                Data.pics[i].prot = Data.prot;
                if (Data.pics[i].str * Data.pics[i].col * Data.pics[i].init_num == 0)
                { MessageBox.Show("Для " + (i + 1).ToString() + "-й картинки заполнены не все параметры.."); return; }
            }
            //можно стартовать процессы по собранным данным
            Gybrid_Buttons_Disable(Data);
            Program.Log("Начали решать картинки\r\n.\r\n");
            Task<List<Program.words>> t1 = Task<List<Program.words>>.Factory.StartNew(() => Gybrids_Process(Data));
        }


    }
}
*/