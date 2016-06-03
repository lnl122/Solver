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
        public Picture.Pictures_data Data;
        public Association(int level, List<string> urls)
        {
            Data.type = "Association";
            Data.Tab = new TabPage();
            Data.Tab.Text = "Ассоциации";
            Data.level = level;
        }
    }
    class Picture
    {
        int image_border_width = 5;
        public Program.words Picture_Process_One(string path)
        {
            return Program.parse_google_page_words(Program.get_google_url_page(Program.upload_file(path)));
        } // вход - локальный путь к одной части, выход - структура о словах
        public List<Program.words> Picture_Process(Picture_data d)
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
            foreach (string t2 in L2)
            {
                //System.Threading.Thread.Sleep(1000);
                // создать дополнительные дочерние потоки, передать им управление
                Task<Program.words> ta = Task<Program.words>.Factory.StartNew(() => Picture_Process_One(t2));
                Tasks2.Add(ta);
            }
            // дождаться выполнения потоков, собрать результаты вместе
            Task.WaitAll(Tasks2.ToArray());
            List<Program.words> r2 = new List<Program.words>();
            int iii = d.init_num;
            foreach (Task<Program.words> t8 in Tasks2)
            {
                Program.words r8 = t8.Result;
                r8.level = d.level;
                r8.number = iii;
                iii++;
                r2.Add(r8);
            }
            return r2;
        } // вход - структура одной картинки, выход - список структур о словах
        public List<Program.words> Pictures_Process(Pictures_data d) // вход структура с урлами всех картинок, без колонок/строк и прочего выход - список структур о словах
        {
            var Tasks2 = new List<Task<List<Program.words>>>();
            foreach (Picture_data p1 in d.pics)
            {
                Task<List<Program.words>> ta = Task<List<Program.words>>.Factory.StartNew(() => Picture_Process(p1));
                //System.Threading.Thread.Sleep(3000);
                Tasks2.Add(ta);
            }
            Task.WaitAll(Tasks2.ToArray());

            List<Program.words> r = new List<Program.words>();

            foreach (Task<List<Program.words>> t8 in Tasks2)
            {
                foreach (Program.words r8 in t8.Result)
                {
                    r.Add(r8);
                }
            }

            //if (d.Auto.Checked) { while (Program.input_busy) { Thread.Sleep(1000); } Program.input_busy = true; }
            Program.Mainform.BeginInvoke(new Action(() => Picture_Buttons_Enable(d)));
            return r;
        }

        public enum prot { none, begin1, begin2, begin3, end1, end2, end3 };
        public struct Pictures_data // все картинки одного уровня 1/2/4 штуки для олимпиек
        {
            public string type;
            public int level;//уровень
            public List<string> urls;//урлы
            public string[] ar_urls;//урлы
            public Picture_data[] pics;//структура каждой пикчи, массив
            public int pic_cnt;//сколько картинок в улах
            public TabPage Tab;//таб формы
            public int olimp_size;//размер олимпийки
            public prot prot; // какая защита
            public Button BtnSolve;
            public Button BtnClose;
            public CheckBox Auto;//автовбивать
            public ComboBox cb_str;//строк
            public ComboBox cb_col;//колонок
            public ComboBox cb_protect;//защита
            public ListBox pics_list;//перечень картинок
            public NumericUpDown init_num;//нач номер
            public Label lb_str;
            public Label lb_col;
            public Label lb_prot;
            public Label lb_init;
            public PictureBox img;
            public TextBox TextOut;
        }
        public struct Picture_data // для одной картинки, под распознавание 16/20/25 мелких
        {
            public Image img;//пикча
            public Bitmap bmp;//пикча
            public int level;//уровень
            public int str;//колво строк
            public int col;//колво колонок
            public int cnt;//номер части (для нескольких картинок одного задания)
            public int init_num;//нач номер картинок
        }
        public Pictures_data Data;
        public Picture(int level, List<string> urls)//для только решения картинок
        {
            if (urls.Count == 0) { MessageBox.Show("В задании нет ни одной ссылки на картинки");  return; }
            Data.type = "Picture";
            Data.level = level;
            Data.urls = urls;
            Data.Tab = new TabPage();
            Data.Tab.Text = "Картинки";
            Data.pic_cnt = urls.Count;
            Data.olimp_size = 0; // ?? нужно будет для олимпиек
            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_Picture_Solve_Click);
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
            Data.BtnClose = new Button();
            Data.BtnClose.Text = "Закрыть";
            Data.BtnClose.Click += new EventHandler(Event_Picture_Close_Click);
            Data.Tab.Controls.Add(Data.BtnClose);
            Data.pics = new Picture_data[Data.pic_cnt];
            for (int i = 0; i < Data.pic_cnt; i++)
            {
                Data.pics[i].level = Data.level;
                Data.pics[i].str = 0;
                Data.pics[i].col = 0;
                Data.pics[i].cnt = i+1;
                Data.pics[i].init_num = 0;
            }
            Data.prot = prot.none;
            Data.init_num = new NumericUpDown();
            Data.init_num.Minimum = 0;
            Data.init_num.Maximum = 257;
            Data.init_num.Increment = 1;
            Data.init_num.Value = 1;
            Data.pics[0].init_num = Convert.ToInt32(Data.init_num.Value);
            Data.init_num.ValueChanged += new EventHandler(Event_Picture_InitNum_Change);
            Data.Tab.Controls.Add(Data.init_num);
            Data.pics_list = new ListBox();
            Data.ar_urls = new string[Data.pic_cnt];
            int ii9 = 0;
            foreach (string u in Data.urls) {
                Data.pics_list.Items.Add(u);
                Data.ar_urls[ii9] = u;
                ii9++;
            }
            Data.pics_list.SelectedIndex = Data.pic_cnt-1;
            Data.pics_list.SelectedIndexChanged += new EventHandler(Event_Picture_ListPics_Select);
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
            Data.cb_protect.SelectedIndexChanged += new EventHandler(Event_Picture_Protect_Change);
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
            Data.cb_str.SelectedIndexChanged += new EventHandler(Event_Picture_Str_Change);
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
            Data.cb_col.SelectedIndexChanged += new EventHandler(Event_Picture_Col_Change);
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

            Event_Picture_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_Picture_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }

        private void Event_Picture_Protect_Change(object sender, EventArgs e)
        {
            switch (Data.cb_protect.SelectedIndex)
            {
                case 0: Data.prot = prot.none;   break;
                case 1: Data.prot = prot.begin1; break;
                case 2: Data.prot = prot.begin2; break;
                case 3: Data.prot = prot.begin3; break;
                case 4: Data.prot = prot.end1;   break;
                case 5: Data.prot = prot.end2;   break;
                case 6: Data.prot = prot.end3;   break;
                default: Data.prot = prot.none;  break;
            }
        }
        private void Event_Picture_ListPics_Select(object sender, EventArgs e)
        {
            Data.img.Image = Data.pics[Data.pics_list.SelectedIndex].img;
            Data.init_num.Value = Data.pics[Data.pics_list.SelectedIndex].init_num;
            Data.cb_str.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].str;
            Data.cb_col.SelectedIndex = Data.pics[Data.pics_list.SelectedIndex].col;
        }
        private static void Picture_Buttons_Enable(Pictures_data d)                  // меняем оптом доступность кнопок
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
        private static void Picture_Buttons_Disable(Pictures_data d)                  // меняем оптом доступность кнопок
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
        private void Event_Picture_Close_Click(object sender, EventArgs e)
        {
            Data.Tab.Dispose();
        }
        private void Event_Picture_ChangeSize(object sender, EventArgs e)
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
            Data.img.Top = Data.pics_list.Top;
            Data.img.Left = mm + 2 * Program.mainform_border;
            Data.img.Width = Program.GameTab.MainTab.Width - mm - 3 * Program.mainform_border;
            Data.img.Height = Program.GameTab.MainTab.Height - Data.pics_list.Top - 1 * Program.mainform_border;
            Data.TextOut.Top = Data.img.Top;
            Data.TextOut.Left = Data.img.Left;
            Data.TextOut.Width = Data.img.Width;
            Data.TextOut.Height = Data.img.Height;
        }
        private void Event_Picture_InitNum_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].init_num = Convert.ToInt32(Data.init_num.Value);
        }
        private void Event_Picture_Str_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].str = Data.cb_str.SelectedIndex;
        }
        private void Event_Picture_Col_Change(object sender, EventArgs e)
        {
            Data.pics[Data.pics_list.SelectedIndex].col = Data.cb_col.SelectedIndex;
        }
        private void Event_Picture_Solve_Click(object sender, EventArgs e)
        {
            //проверим, все ли готово
            for (int i=0; i<Data.pic_cnt; i++) { if (Data.pics[i].str * Data.pics[i].col * Data.pics[i].init_num == 0) { MessageBox.Show("Для " + (i+1).ToString() + "-й картинки заполнены не все параметры.."); return; } }

            //можно стартовать процессы по собранным данным
            Picture_Buttons_Disable(Data);
            Program.Log("Начали решать картинки\r\n.\r\n");
            Task<List<Program.words>> t1 = Task<List<Program.words>>.Factory.StartNew(() => Pictures_Process(Data));
            List<Program.words> res = t1.Result;
            Data.img.Height = Data.img.Height / 2 - Program.mainform_border;
            Data.TextOut.Height = Data.img.Height;
            Data.TextOut.Top = Data.img.Bottom + Program.mainform_border;
            Data.TextOut.Visible = true;
            foreach (Program.words wrd in res)
            {
                Data.TextOut.Text += (wrd.number + " : ");
                Data.TextOut.Text += (wrd.g_variant + " | ");
                foreach (string str in wrd.g_words)
                {
                    Data.TextOut.Text += (str + " ");
                }
                Data.TextOut.Text += "\r\n";
            }
        }




    }
}
