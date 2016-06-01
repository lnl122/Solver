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
            string basename = Program.Env.temp_path + "\\" + d.level + "\\" + d.cnt + "_#.jpg";
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
                    nb.Save(fn);
                    L2.Add(fn);
                }
            }
            var Tasks2 = new List<Task<Program.words>>();
            foreach (string t2 in L2)
            {
                // создать дополнительные дочерние потоки, передать им управление
                Task<Program.words> ta = Task<Program.words>.Factory.StartNew(() => Picture_Process_One(t2));
                Tasks2.Add(ta);
            }
            // дождаться выполнения потоков, собрать результаты вместе
            Task.WaitAll(Tasks2.ToArray());
            List<Program.words> r2 = new List<Program.words>();
            foreach (Task<Program.words> t8 in Tasks2) { r2.Add(t8.Result); }
            return r2;
        } // вход - структура одной картинки, выход - список структур о словах

        //??
        public List<Program.words> Pictures_Process(Pictures_data d) // вход структура с урлами всех картинок, без колонок/строк и прочего выход - список структур о словах
        {
            List<Program.words> r = new List<Program.words>();


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
            public TextBox init_num;//нач номер
            public Label lb_str;
            public Label lb_col;
            public Label lb_prot;
            public Label lb_init;
            public PictureBox img;
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
            Data.init_num = new TextBox();
            Data.init_num.Text = "0";
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
            Event_Picture_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_Picture_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }
        private void Event_Picture_Solve_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }//??
        private void Event_Picture_Close_Click(object sender, EventArgs e)
        {
            Data.Tab.Dispose();
        }
        private void Event_Picture_Protect_Change(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }//??
        private void Event_Picture_Str_Change(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }//??
        private void Event_Picture_Col_Change(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }//??
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

            /*
                  public ComboBox cb_str;//строк
                  public ComboBox cb_col;//колонок
                  public ComboBox cb_protect;//защита
                  public TextBox init_num;//нач номер
                  */
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
        //============================================================================

        //private static string[] bad_words = { "рабочего стола", "высокого качества", "&gt", "png", "dvd", "the", "buy", "avito", "авг", "апр", "без", "вас", "дек", "для", "его", "жми", "или", "июл", "июн", "как", "кто", "лет", "мар", "мем", "ноя", "окт", "они", "при", "про", "сен", "смс", "так", "тег", "фев", "что", "эту", "янв", "file", "free", "англ", "есть", "обои", "фото", "цена", "цены", "ютуб", "[pdf]", "stock", "видео", "куплю", "можно", "найти", "одной", "песен", "самые", "самых", "сразу", "тегам", "фильм", "images", "купить", "онлайн", "отзывы", "почему", "продам", "скидки", "услуги", "фильма", "фильму", "шаблон", "яндекс", "youtube", "выбрать", "закачка", "закачки", "маркете", "новости", "продажа", "продать", "рабочий", "родился", "скачать", "сколько", "способы", "форматы", "хорошем", "download", "выгодная", "выгодные", "выгодный", "картинки", "качестве", "магазине", "описание", "подборка", "свойства", "смотреть", "страницу", "kinopoisk", "photoshop", "wallpaper", "бесплатно", "перевести", "программы", "бесплатные", "применение", "разрешение", "широкоформатные", "ответить" };

        //Picture_data d;




        /*public void Event_Picture_Msg(object sender, EventArgs e)
        {
            Tabs.ButtonsEnable(Data.Tab, false);
            if ((Data.ver.SelectedIndex * Data.hor.SelectedIndex) != 0)
            {
                Data.bmp = new Bitmap(Data.pic.Image);
                Data.str = Data.hor.SelectedIndex;
                Data.col = Data.ver.SelectedIndex;
                Task t1 = Task.Factory.StartNew(() => Picture_Process(Data));
            }
            else
            {
                Data.Text.Text = "Сначала нужно выбрать количество колонок и строк..";
                Tabs.ButtonsEnable(Data.Tab, true);
            }

        }
        */
        //var R1 = new Picture(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString())), "");
        /*public Picture__old(int level, List<string> url, string act) 
        {
            // act = "association" or ""
            Data.type = "Picture";
            Data.Tab = new TabPage();
            Data.Tab.Text = "Картинка";
            Data.level = level;
            Data.cnt = cnt;
            Data.act = act;

            var BtnSolve = new Button();
            BtnSolve.Text = "Решить";
            BtnSolve.Location = new Point(Program.mainform_border, Program.mainform_border);
            BtnSolve.Click += new EventHandler(Event_Picture_Msg);
            Data.Tab.Controls.Add(BtnSolve);

            var BtnClose = new Button();
            BtnClose.Text = "Закрыть";
            BtnClose.Location = new Point(Program.Tabs.Width - 18 * Program.mainform_border, Program.mainform_border);
            BtnClose.Click += new EventHandler(Tabs.Event_Close_Tab);
            Data.Tab.Controls.Add(BtnClose);

            Data.hor = new ComboBox();
            Data.hor.Top = Program.mainform_border;
            Data.hor.Left = BtnSolve.Right + Program.mainform_border;
            Data.hor.Width = BtnSolve.Width;
            Data.hor.Items.Add("Строк:"); Data.hor.Items.Add("1"); Data.hor.Items.Add("2"); Data.hor.Items.Add("3"); Data.hor.Items.Add("4"); Data.hor.Items.Add("5"); Data.hor.Items.Add("6"); Data.hor.Items.Add("7"); Data.hor.Items.Add("8"); Data.hor.Items.Add("9");
            Data.hor.SelectedIndex = 0;
            Data.Tab.Controls.Add(Data.hor);

            Data.ver = new ComboBox();
            Data.ver.Top = Program.mainform_border;
            Data.ver.Left = Data.hor.Right + Program.mainform_border;
            Data.ver.Width = BtnSolve.Width;
            Data.ver.Items.Add("Колонок:"); Data.ver.Items.Add("1"); Data.ver.Items.Add("2"); Data.ver.Items.Add("3"); Data.ver.Items.Add("4"); Data.ver.Items.Add("5"); Data.ver.Items.Add("6"); Data.ver.Items.Add("7"); Data.ver.Items.Add("8"); Data.ver.Items.Add("9");
            Data.ver.SelectedIndex = 0;
            Data.Tab.Controls.Add(Data.ver);

            Data.pic = new PictureBox();
            Data.pic.Top = BtnSolve.Bottom + Program.mainform_border;
            Data.pic.Left = Program.mainform_border;
            Data.pic.Width = Program.Tabs.Width * 2 / 3;
            Data.pic.Height = Program.Tabs.Height - BtnSolve.Height - 9 * Program.mainform_border;
            Data.pic.SizeMode = PictureBoxSizeMode.StretchImage;
            Data.pic.ImageLocation = url;
            Data.pic.Load();
            Data.Tab.Controls.Add(Data.pic);

            Data.Text = new TextBox();
            Data.Text.Top = BtnSolve.Bottom + Program.mainform_border;
            Data.Text.Left = Data.pic.Right + Program.mainform_border;
            Data.Text.Width = Program.Tabs.Width * 2 / 3 - 7 * Program.mainform_border;
            Data.Text.Height = Data.pic.Height;
            Data.Text.AcceptsReturn = true;
            Data.Text.AcceptsTab = false;
            Data.Text.Multiline = true;
            Data.Text.ScrollBars = ScrollBars.Both;
            Data.Text.Text = "";
            Data.Tab.Controls.Add(Data.Text);

            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }*/


    }
}
