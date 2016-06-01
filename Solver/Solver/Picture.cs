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

        public List<Program.words> Pictures_Process(Pictures_data d) // вход структура с урлами всех картинок, без колонок/строк и прочего выход - список структур о словах
        {
            List<Program.words> r = new List<Program.words>();


            return r;
        }

        public struct Pictures_data // все картинки одного уровня 1/2/4 штуки для олимпиек
        {
            public string type;
            public int level;//уровень
            public List<string> urls;//урлы
            public Picture_data[] pics;//структура каждой пикчи, массив
            public int pic_cnt;//сколько картинок в улах
            public TabPage Tab;//таб формы
            public int olimp_size;//размер олимпийки
            public string protect_beg;//какая защита
            public string protect_end;//какая защита
        }
        public struct Picture_data // для одной картинки, под распознавание 16/20/25 мелких
        {
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
            Data.olimp_size = 0; // ??
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
