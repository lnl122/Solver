using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;

namespace Solver
{
    class Program
    {
        public static Form Mainform;       // форму объявим глобально
        public static TabControl Tabs;

        public static string mainform_caption = "Solver..";     // имя формы

        public static int mainform_border = 5;      // расстояния между элементами форм, константа
        public static int rnd_min = 800;//1300;
        public static int rnd_max = 1500;//3300;
        public static bool input_busy = false;

        public static void Log(string t)
        {
            Program.logfile.WriteLine("{0} {1} {2}", DateTime.Today.ToShortDateString(), DateTime.Now.ToLongTimeString(), t);
        }
        private static dEnvInfo GetEnvInfo(string[] args)
        {
            dEnvInfo d = new dEnvInfo();
            //заполняем переменные окружения, с которыми потом будем работать
            d.windows_name = System.Environment.OSVersion.VersionString;
            d.system_architecture = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            d.system_processors = Environment.ProcessorCount;
            d.local_path = Environment.CurrentDirectory;
            d.system_is64bit = Environment.Is64BitOperatingSystem;
            d.system_name = Environment.MachineName;
            d.system_version = Environment.Version.ToString();
            d.self_name = Process.GetCurrentProcess().MainModule.ModuleName;
            d.log_pathfilename = d.local_path + "\\" + d.self_name + ".log";
            d.self_date = File.GetCreationTime(Process.GetCurrentProcess().MainModule.FileName).ToString();
            Program.logfile = new StreamWriter(File.AppendText(d.log_pathfilename).BaseStream);
            Program.logfile.AutoFlush = true;
            d.temp_path = d.local_path + "\\pics"; if (!Directory.Exists(d.temp_path)) { Directory.CreateDirectory(d.temp_path); }
            //if (!Directory.Exists(d.temp_path + "\\0")) { Directory.CreateDirectory(d.temp_path + "\\0"); }
            Log("________________________________________________________________________________");
            Log("Старт программы..");
            Log("Сборка от "+d.self_date);
            Log("ПК: "+ d.system_name);
            Log(d.windows_name+", "+ d.system_architecture+", ver:"+d.system_version+", CPU: "+ d.system_processors.ToString() + ", 64bit:" + d.system_is64bit.ToString());
            return d;
        }
        private static string GetVersionDotNetFromRegistry()
        {
            string res = "";
            using (RegistryKey ndpKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    if (versionKeyName.StartsWith("v"))
                    {
                        res = res + versionKeyName + " ";
                    }
                }
            }
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    int releaseKey = (int)ndpKey.GetValue("Release");
                    if (releaseKey >= 393295) { res = res + " v4.6"; }
                    else
                    {
                        if ((releaseKey >= 379893)) { res = res + " v4.5.2"; }
                        else
                        {
                            if ((releaseKey >= 378675)) { res = res + " v4.5.1"; }
                            else
                            {
                                if ((releaseKey >= 378389)) { res = res + " v4.5"; }
                            }
                        }
                    }
                }
            }
            return res;
        }
        private static string GetVersionMicrosoftWord()
        {
            try
            {
                var WordApp = new Microsoft.Office.Interop.Word.Application();
                string s1 = WordApp.Version;
                WordApp.Quit();
                return s1;
            }
            catch
            {
                return "";
            }
        }
        private static bool CheckComponents()
        {
            // .NET
            string DotNetVersions = GetVersionDotNetFromRegistry().Trim();
            Log("Найденные версии .NET: " + DotNetVersions);
            if (DotNetVersions.IndexOf("v2.0") == -1) { Log("ERROR: Отсутствует .NET v2.0"); return false; }
            if (DotNetVersions.IndexOf("v3.0") == -1) { Log("ERROR: Отсутствует .NET v3.0"); return false; }
            if (DotNetVersions.IndexOf("v4.0") == -1) { Log("ERROR: Отсутствует .NET v4.0"); return false; }
            if ((DotNetVersions.IndexOf("v4.5") == -1) && (DotNetVersions.IndexOf("v4.6") == -1)) { Log("ERROR: Отсутствует .NET v4.5 или v4.6"); return false; }
            // MS Word
            string WordVersion = GetVersionMicrosoftWord();
            if (WordVersion == "") { Log("ERROR: Отсутствует установленный Microsoft Word"); return false; }
            int ii1 = 0;
            if (Int32.TryParse(WordVersion.Substring(0, WordVersion.IndexOf(".")), out ii1))
            {
                if (ii1 <= 11) { Log("ERROR: Версия Microsoft Word ниже 11.0, необходим Microsoft Word 2007 или более новый"); return false; }
            } else
            {
                Log("ERROR: Не удалось определить версию Microsoft Word"); return false;
            }
            Log("Найден Microsoft Word версии " + WordVersion);
            try { 
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = false;
                wordApp.CheckSpelling("мама мыла раму");
                wordApp.Quit();
                Log("Проверка орфографии установлена");
            } catch
            {
                Log("ERROR: Не удалось запустить проверку орфографии, или же проверка русского языка не установлена.."); return false;
            }
            // проверка орфографии установлена?
            // ???
            // 2do

            // проверка открытия web-ресурсов
            WebClient wc1 = null;
            try { wc1 = new WebClient(); }                                  catch { Log("ERROR: Не удалось создать объект WebClient");      return false; }
            string re1 = "";
            try { re1 = wc1.DownloadString("http://image.google.com/"); }   catch { Log("ERROR: http://image.google.com/ не открывается");  return false; }
            try { re1 = wc1.DownloadString("http://game.en.cx/"); }         catch { Log("ERROR: http://game.en.cx/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://jpegshare.net/"); }      catch { Log("ERROR: http://jpegshare.net/ не открывается");     return false; }
            try { re1 = wc1.DownloadString("http://ipic.su/"); }            catch { Log("ERROR: http://ipic.su/ не открывается");           return false; }
            try { re1 = wc1.DownloadString("http://goldlit.ru/"); }         catch { Log("ERROR: http://goldlit.ru/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://sociation.org/"); }      catch { Log("ERROR: http://sociation.org/ не открывается");     return false; }
            Log("Все необходимые web-ресурсы открываются успешно");

            // все проверки пройдены
            return true;
        }

        public static string[,] actions = {
            //{ "Решать самостоятельно",      "manual" },
            { "Расчленёнки",                    "raschl" },
            { "Картинки - только решить",       "picture"},
            { "Картинки + ассоциации",          "picture_association"},
            { "Картинки + логогрифы СОН-СЛОН",  "logogrif"},
            { "Картинки + метаграммы КОТ-КИТ",  "metagramm"},
            { "Картинки + гибриды оСПАржа",     "gybrid"}

            };

        public struct dEnvInfo
        {
            public string system_name;
            public string windows_name;
            public bool system_is64bit;
            public string system_architecture;
            public string system_version;
            public int system_processors;
            public string local_path;
            public string log_pathfilename;
            public string self_name;
            public string self_date;
            public string temp_path;
            //public string registry_path;
        }
        public struct GameSt
        {
            public string username;
            public string password;
            public string userid;
            public string game_id;
            public string game_domain;
            //public CookieCollection game_cColl;
            public CookieContainer game_cCont;
            public string game_cHead;
            public string[] g_names;
            public string[] g_urls;
            public int game_levels;
            public TextBox tb;
            public string[] level_name;
            public string[] level_text;
            public string[] level_full;
            //public string[] level_pics;
        }
        public struct MainTabSt
        {
            public TabPage MainTab;
            public Button BtnUser;
            public Button BtnGame;
            public ListBox LvlList;
            public TextBox LvlText;
            public ComboBox gChoice;
            public Button BtnSolve;
        }

        public static dEnvInfo Env = new dEnvInfo();
        public static StreamWriter logfile;
        public static GameSt dGame = new GameSt();
        public static MainTabSt GameTab = new MainTabSt();

        public static string[] bad_words = {
                "рабочего стола", "высокого качества", "&gt", "png", "dvd", "the", "buy", "avito", "авг", "апр", "без", "вас", "дек", "для", "его", "жми", "или", "июл", "июн", "как", "кто", "лет", "мар", "мем", "ноя", "окт", "они", "при", "про", "сен", "смс", "так", "тег", "фев", "что", "эту", "янв", "file", "free", "англ", "есть", "обои", "фото", "цена", "цены", "ютуб", "[pdf]", "stock", "видео", "куплю", "можно", "найти", "одной", "песен", "самые", "самых", "сразу", "тегам", "фильм", "images", "купить", "онлайн", "отзывы", "почему", "продам", "скидки", "услуги", "фильма", "фильму", "шаблон", "яндекс", "youtube", "выбрать", "закачка", "закачки", "маркете", "новости", "продажа", "продать", "рабочий", "родился", "скачать", "сколько", "способы", "форматы", "хорошем", "download", "выгодная", "выгодные", "выгодный", "картинки", "качестве", "магазине", "описание", "подборка", "свойства", "смотреть", "страницу", "kinopoisk", "photoshop", "wallpaper", "бесплатно", "перевести", "программы", "бесплатные", "применение", "разрешение"
                , "широкоформатные", "ответить"
            };

        public struct words
        {
            public int level;
            public prot prot;
            public int number;
            public string answer;
            public string g_variant;
            public List<string> g_words;
            public List<string> g_words_ru;
            public List<string> g_words_en;
            public List<string> g_words_en_trans;
            public List<string> w_find;
            public List<string> w_base;
            public List<string> w_base_all;
            public List<string> w_assoc;
            //public List<string> w_all;
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
            public System.Windows.Forms.CheckBox Auto;//автовбивать
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
            public prot prot; // какая защита
            public int str;//колво строк
            public int col;//колво колонок
            public int cnt;//номер части (для нескольких картинок одного задания)
            public int init_num;//нач номер картинок
        }
        public static words parse_google_page_words(string gtext2)
        {
            words w = new words();
            w.g_words = new List<string>();

            string g = gtext2.Substring(gtext2.IndexOf("<body"));

            string[,] tags = {
                { "<script>" , "<noscript>" , "<!--z-->", "<style>" , "href=\"", "style=\"", "class=\"", "<form"  , "onmousedown=\"", "value=\"", "<cite" , "data-jiis=\"", "data-ved=\"", "target=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "src=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "\"http://", "\"https://", "unselectable=\"", "{\""     , ",\"rh\":", "<p>"   },
                { "</script>", "</noscript>", "</body>" , "</style>", "\""     , "\""      , "\""      , "</form>", "\""            , "\""      , "/cite>", "\""          , "\""         , "\""       , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""    , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""       , "\""        , "\""             , ",\"pt\":", "}"       , "</p>"  }
            };
            int tags_len = tags.Length / 2;
            bool fl = true;
            for (int i = 0; i < tags_len; i++)
            {
                fl = true;
                while (fl)
                {
                    fl = false;
                    int i1 = g.IndexOf(tags[0, i]);
                    if (i1 != -1)
                    {
                        string g2 = g.Substring(i1 + tags[0, i].Length);
                        int i2 = g2.IndexOf(tags[1, i]);
                        g = g.Substring(0, i1) + g2.Substring(i2 + tags[1, i].Length);
                        fl = true;
                    }
                }
            }
            int svi = g.IndexOf(">Скорее всего, на картинке");
            string sv = "";
            if (svi != -1)
            {
                sv = g.Substring(g.IndexOf(">Скорее всего, на картинке") + 26).Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace(" >", ">");
                sv = sv.Substring(sv.IndexOf("<a>") + 3);
                sv = sv.Substring(0, sv.IndexOf("</a>"));
            }
            w.g_variant = sv;

            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace(" >", ">");
            g = g.Replace("<div>", " ").Replace("</div>", " ").Replace("<span>", " ").Replace("</span>", " ").Replace("<button>", " ").Replace("</button>", " ");
            g = g.Replace("<ol>", " ").Replace("</ol>", " ").Replace("<li>", " ").Replace("</li>", " ").Replace("<a data-p>", " ").Replace("</ul>", " ").Replace("<ul>", " ").Replace("<em>", " ").Replace("</em>", " ");
            g = g.Replace("<table>", " ").Replace("</table>", " ").Replace("<td>", " ").Replace("</td>", " ").Replace("<tr>", " ").Replace("</tr>", " ").Replace("<div data-async->", " ").Replace("<div e>", " ");
            g = g.Replace("<textarea>", " ").Replace("</textarea>", " ").Replace("<a data-p>", " ").Replace(" data-rt", "").Replace("<g-img><img></g-img>", " ").Replace("<div data-hve>", " ").Replace("</body></html>", " ").Replace("<img>", " ").Replace("<a></a>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("<a>Сохраненная&nbsp;копия</a>", " ").Replace("<g-review-stars></g-review-stars>", " ").Replace("<a>Похожие</a>", " ").Replace("<h3>", " ").Replace("</h3>", " ").Replace("<h2>", " ").Replace("</h2>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("<a>Пожаловаться</a>", " ").Replace("<a>Отмена</a>", " ").Replace("Пожаловаться на содержание картинки.", " ").Replace("<a>Пожаловаться на другую картинку.</a>", " ").Replace("<a>Пожаловаться на картинки</a>", " ").Replace("<a>Похожие изображения</a>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("<body margin>", " ").Replace("<br>", " ").Replace("&nbsp;", " ").Replace("</a>", " ").Replace("<a>", " ").Replace("<!--m-->", " ").Replace("<!--n-->", " ").Replace("<hr>", " ").Replace("</html>", " ").Replace("<div>", " ").Replace("\"", " ").Replace("Результаты поиска", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace(",", " ").Replace(".", " ").Replace("(", " ").Replace(")", " ").Replace("-", " ").Replace("!", " ").Replace("?", " ").Replace(":", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("< ", "<").Replace(" >", ">").Replace("<a>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Substring(g.LastIndexOf("</p>") + 4);
            g = g.Replace("Размер изображения", " ").Replace("Страницы с подходящими изображениями", " ").Replace("Изображения других размеров не найдены", " ").Replace("Скорее всего на картинке", " ").Replace("Благодарим за замечания", " ").Replace("\\u0026quot;", " ").Replace("  ", " ").Replace("  ", " ").Replace("ВКонтакте", "").Replace("ВКонтакте", "").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("Есть изображения других размеров", " ").Replace("Все размеры", " ").Replace("Маленькие", " ").Replace("Средние", " ").Replace("Большие", " ").Replace("&middot;", " ").Replace("&quot;", " ").Replace("YouTube", " ").Replace("#", " ").Replace("×", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("<wbr>", " ").Replace("—", " ").Replace("<", " ").Replace(">", " ").Replace("\"", " ").Replace("&times;", " ").Replace("\\", " ").Replace("|", " ").Replace("«", " ").Replace("»", " ").Replace("/", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("u2014", " ").Replace(";", " ").Replace("+", " ").Replace(" ", " ").Replace(" ", " ").Replace(" ", " ");

            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");

            g = (" " + sv + " " + g + " ").Replace("  ", " ");
            //foreach (string bw in bad_words) { g = g.Replace(" " + bw.ToLower() + " ", " ").Replace(" " + bw.ToUpper() + " ", " ").Replace(" " + bw.Substring(0, 1).ToUpper() + bw.Substring(1).ToLower() + " ", " ").Replace("  ", " ").Replace("  ", " "); }
            g = g.Trim().TrimEnd().TrimStart();
            string[] parts = g.Split(' ');
            string[] part_str = new string[parts.Length];
            int[] part_len = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) { part_len[i] = 0; part_str[i] = ""; }
            int max_idx = 0;
            foreach (string part in parts)
            {
                if (part.Length < 3) { continue; } // too short
                int idx = Array.IndexOf(part_str, part);
                if (idx == -1)
                { // new
                    part_str[max_idx] = part;
                    part_len[max_idx]++;
                    max_idx++;
                }
                else
                { // exiting
                    part_len[idx]++;
                }
            }
            for (int cur_idx = 0; cur_idx < max_idx; cur_idx++)
            {
                string vv = part_str[cur_idx];
                if (part_len[cur_idx] < 3) // 1 or 2 times in text
                {
                    part_str[cur_idx] = "";
                }
                if (vv.Length > 0) // for non-empty string
                {
                    char ch = vv[0];
                    char chl = vv.ToLower()[0];
                    //if ((chl >= 'a') && (chl <= 'z') && (chl == ch)) // english words, starts with small letter
                    //{
                    //    part_str[cur_idx] = "";
                    //}
                    int tempint;
                    if (Int32.TryParse(vv, out tempint)) // string as number
                    {
                        part_str[cur_idx] = "";
                    }
                    //if (wordApp.CheckSpelling(vv.Substring(0, 1).ToUpper() + vv.Substring(1, vv.Length - 1)) == false)
                    //{
                    //    part_str[cur_idx] = "";
                    //}
                }

                part_str[cur_idx] = part_str[cur_idx].ToLower(); // others - to lower case
            }
            // убрать дупы
            string[] part_end = part_str.Distinct().ToArray();

            //foreach (string sa in part_end) { w.g_words.Add(sa); } // позже, после сортировки

            int[] part_end_cnt = new int[part_end.Length];
            for (int i = 0; i < part_end.Length; i++) { part_end_cnt[i] = 0; }
            for (int i = 0; i < part_end.Length; i++)
            {
                for (int cur_idx = 0; cur_idx < max_idx; cur_idx++)
                {
                    if (part_end[i] == part_str[cur_idx])
                    {
                        part_end_cnt[i] += part_len[cur_idx];
                    }
                }
            }

            // отсортировать по part_end_cnt http://goldlit.ru/component/slog?words= %D0 %BC %D0 %B0 %D1 %87 %D0 %B5 %D1 %85 %D0 %B0 +  %D1 %85 %D0 %B0 + %D1 %85 %D0 %B0
            int m = part_end.Length;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i == j) { continue; }
                    if (part_end_cnt[i] > part_end_cnt[j])
                    {
                        int a = part_end_cnt[j];
                        part_end_cnt[j] = part_end_cnt[i];
                        part_end_cnt[i] = a;
                        string b = part_end[j];
                        part_end[j] = part_end[i];
                        part_end[i] = b;
                    }
                }
            }
            foreach (string sa in part_end) { if (sa != "") { w.g_words.Add(sa); } }
            return w;
        }
        public static string upload_file_jpegshare(string filepath)
        {
            string filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            string uri = "http://jpegshare.net";
            string uriaction = uri + "/upload.php";
            string parse_b = "[img]" + uri + "/images";
            string parse_e = "[/img]";
            HttpClient httpClient = new HttpClient();
            System.Net.ServicePointManager.Expect100Continue = false;
            MultipartFormDataContent form = new MultipartFormDataContent();
            byte[] img_bytes = System.IO.File.ReadAllBytes(filepath);
            form.Add(new ByteArrayContent(img_bytes, 0, img_bytes.Count()), "imgfile", filename);
            Task<HttpResponseMessage> response = httpClient.PostAsync(uriaction, form);
            HttpResponseMessage res2 = response.Result;
            res2.EnsureSuccessStatusCode();
            HttpContent Cont = res2.Content;
            httpClient.Dispose();
            string sd = res2.Content.ReadAsStringAsync().Result;
            sd = sd.Substring(sd.IndexOf(parse_b) + 5); // 5 = [img]
            sd = sd.Substring(0, sd.IndexOf(parse_e));
            return sd;
        }
        public static string upload_file_ipic(string filepath)
        {
            string filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            string uri = "http://ipic.su";
            string uriaction = uri + "/";
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.Expect100Continue = false;
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent("/"), "link");
            form.Add(new StringContent("loadimg"), "action");
            form.Add(new StringContent("ipic.su"), "client");
            //form.Add(new StringContent(filename), "name");
            var streamContent2 = new StreamContent(File.Open(filepath, FileMode.Open));
            form.Add(streamContent2, "image", filename);
            //form.Add(new StringContent("client"), "ipic.su");
            //form.Add(new StringContent("client"), "ipic.su");
            //form.Add(new StringContent("client"), "ipic.su");

            //byte[] img_bytes = System.IO.File.ReadAllBytes(filepath);
            //form.Add(new ByteArrayContent(img_bytes, 0, img_bytes.Count()), "image", filename);

            Task<HttpResponseMessage> response = httpClient.PostAsync(uriaction, form);
            HttpResponseMessage res2 = response.Result;
            res2.EnsureSuccessStatusCode();
            HttpContent Cont = res2.Content;
            httpClient.Dispose();
            string sd = res2.Content.ReadAsStringAsync().Result;
            sd = sd.Substring(sd.IndexOf("[edit]") + 6);
            sd = sd.Substring(sd.IndexOf("value=\"") + 7);
            sd = sd.Substring(0, sd.IndexOf("\""));
            return sd;
            //">[edit]</a>:< br />< input type = "text" value="htt
        }
        public static string upload_file(string filepath)
        {
            return upload_file_ipic(filepath);
            //return upload_file_jpegshare(filepath);
        }
        public static List<Program.words> words_google_to_find(List<Program.words> q)
        {
            var wordApp = new Microsoft.Office.Interop.Word.Application();
            wordApp.Visible = false;
            List<Program.words> w = new List<Program.words>();
            foreach (Program.words q1 in q)
            {
                // дял всех найденных списков
                Program.words w1 = q1;
                w1.g_words_ru = new List<string>();
                w1.g_words_en = new List<string>();
                w1.g_words_en_trans = new List<string>();
                w1.w_find = new List<string>();
                w1.w_base = new List<string>();
                w1.w_assoc = new List<string>();
                foreach (string w2 in w1.g_words)
                {
                    //рассмотрим один набор слов для одной картинки
                    bool bad = false;
                    //проверим на плохое слово
                    for (int i = 0; i < Program.bad_words.Length; i++) { if (w2 == Program.bad_words[i]) { bad = true; } }
                    if (bad) { continue; }
                    // lang? ru/en
                    string w3 = w2.ToLower().Replace("ё", "е");
                    char c1 = w3[0];
                    char c2 = w3[w3.Length - 1];
                    if (((c1 >= 'a') && (c1 <= 'z')) || ((c2 >= 'a') && (c2 <= 'z'))) { w1.g_words_en.Add(w3.ToLower()); }
                    if (((c1 >= 'а') && (c1 <= 'я')) || ((c2 >= 'а') && (c2 <= 'я')))
                    {
                        if (wordApp.CheckSpelling(w3)) { w1.g_words_ru.Add(w3); }
                        else { if (wordApp.CheckSpelling(w3.Substring(0, 1).ToUpper() + w3.Substring(1, w3.Length - 1))) { w1.g_words_ru.Add(w3); } }
                    }
                }
                w1.w_find = w1.g_words_ru;
                if (w1.g_words_en.Count != 0)
                {
                    //переведем их
                    string tren = "";
                    foreach (string t3 in w1.g_words_en) { tren = tren + t3 + ". "; }
                    tren = tren.TrimEnd();
                    tren = Program.get_trans_word(tren).ToLower();
                    string[] ar1 = tren.Split('.');
                    foreach (string ar2 in ar1) { if (ar2 != "") { w1.g_words_en_trans.Add(ar2.TrimStart().TrimEnd()); } }
                    w1.w_find.AddRange(w1.g_words_en_trans);
                }
                //убрать дупы
                if (w1.w_find != null) { w1.w_find = w1.w_find.Distinct().ToArray().ToList(); }
                w1.w_find.Remove("");
                //добавить в решение
                w.Add(w1);
            }
            wordApp.Quit();
            return w;
        }
        public static List<Program.words> words_to_engine(List<Program.words> q, string s)
        {
            List<Program.words> w = new List<Program.words>();
            while (Program.input_busy) { System.Threading.Thread.Sleep(1000); }
            Program.input_busy = true;
            foreach (Program.words q1 in q)
            {
                Program.words w1 = q1;
                if ((w1.answer != "") && (w1.answer != null)) { w.Add(w1); continue; }
                List<string> w2 = new List<string>();
                if (s == "find") { w2 = w1.w_find; }
                if (s == "base") { w2 = w1.w_base; }
                if (s == "assoc") { w2 = w1.w_assoc; }
                foreach (string w3 in w2)
                {
                    bool fl2 = Program.try_form_send(w1.level, set_word_protect(w3, w1.number, w1.prot));
                    if (fl2)
                    {
                        w1.answer = w3;
                        break;
                    }
                }
                w.Add(w1);
            }
            Program.input_busy = false;
            return w;
        }
        public static List<Program.words> words_find_base(List<Program.words> q)
        {
            List<Program.words> w = new List<Program.words>();
            foreach (Program.words q1 in q)
            {
                Program.words w1 = q1;
                w1.w_base = new List<string>();
                w1.w_base_all = new List<string>();
                if ((w1.answer != "") && (w1.answer != null)) { w.Add(w1); continue; }
                string[] ss = Program.get_start_word(String.Join(" ", w1.w_find.Distinct().ToArray())).Split(' ');
                foreach (string s2 in ss)
                {
                    w1.w_base_all.Add(s2);
                    if (!w1.w_find.Contains(s2)) { w1.w_base.Add(s2); }
                }
                w1.w_base_all = new List<string>(w1.w_base_all.Distinct().ToArray());
                w.Add(w1);
            }
            return w;
        }
        public static List<Program.words> words_base_assoc(List<Program.words> q)
        {
            //List<string> get_assoc_word(string v, int max_res_cnt=10000)
            List<Program.words> w = new List<Program.words>();
            foreach (Program.words q1 in q)
            {
                Program.words w1 = q1;
                w1.w_assoc = new List<string>();
                if ((w1.answer != "") && (w1.answer != null)) { w.Add(w1); continue; }
                if (w1.w_base_all.Count >= 5) { w.Add(w1); continue; }
                foreach (string s2 in w1.w_base) { w1.w_assoc.AddRange(Program.get_assoc_word(s2, 10)); }
                w1.w_assoc = new List<string>(w1.w_assoc.Distinct().ToArray());
                w.Add(w1);
            }
            return w;
        }
        public static string get_start_word(string v)
        {
            Encoding utf8 = Encoding.UTF8;

            string v2 = "";
            v = "индульгенция " + v;
            if (v == "") { return ""; }
            byte[] b4 = utf8.GetBytes(v.ToLower());
            for (int j = 0; j < b4.Length; j++)
            {
                if (b4[j] != 32)
                {
                    v2 += "%" + b4[j].ToString("X");
                }
                else
                {
                    v2 += "+";
                }
            }
            v2 = "http://goldlit.ru/component/slog?words=" + v2;
            WebClient cl = new WebClient();
            cl.Encoding = System.Text.Encoding.UTF8;
            bool ffl = true;
            string re = "";
            while (ffl)
            {
                try
                {
                    re = cl.DownloadString(v2);
                    ffl = false;
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
            cl.Dispose();
            List<string> v3 = new List<string>();
            int ii1 = re.IndexOf("Начальная форма");
            while (ii1 != -1)
            {
                re = re.Substring(ii1);
                re = re.Substring(re.IndexOf(":") + 1);
                string v5 = re.Substring(0, re.IndexOf("<")).ToLower().TrimEnd().TrimStart();
                v3.Add(v5);
                ii1 = re.IndexOf("Начальная форма");
            }
            v3.Remove("индульгенция");
            return String.Join(" ", v3.Distinct().ToArray());
        }
        public static List<string> get_assoc_word(string v, int max_res_cnt=10000)
        {
            List<string> res7 = new List<string>();
            if (v == "") { return res7; }
            string[] arr1 = v.Split(' ');
            string re = "";
            foreach (string w1 in arr1)
            {
                if (w1 == "") { continue; }
                WebClient cl = new WebClient();
                cl.Encoding = System.Text.Encoding.UTF8;
                string w2 = "http://sociation.org/word/" + w1;
                bool ffl = true;
                int iil = 0;
                while (ffl)
                {
                    try
                    {
                        re = cl.DownloadString(w2);
                        ffl = false;
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        iil++;
                        if(iil == 15)
                        {
                            Log("ERROR: sociation.org вызвал наш таймаут в секунду");
                            re = "";
                            ffl = false;
                        }
                    }
                }
                cl.Dispose();
                int ii1 = re.IndexOf("<ol ");
                if (ii1 == -1) { continue; } else { re = re.Substring(ii1); }
                int ii2 = re.IndexOf("<li>");
                if (ii2 == -1) { continue; } else { re = re.Substring(ii2); }
                int ii3 = re.IndexOf("</ol>");
                if (ii3 == -1) { continue; } else { re = re.Substring(0, ii3); }
                string[] ar2 = Regex.Split(re, "</a>");
                int cnt = 0;
                foreach (string ww2 in ar2)
                {
                    //string ww3 = ww2.Replace("</a>", "");
                    int ii4 = ww2.LastIndexOf(">");
                    if (ii4 == -1) { continue; }
                    string ww4 = ww2.Substring(ii4 + 1).Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
                    if (ww4 == "") { continue; }
                    res7.Add(ww4.ToLower());
                    cnt++;
                    if (cnt >= max_res_cnt) { break; }
                }
            }
            return res7;
        }
        public static string get_trans_word(string s1)
        {
            if (s1 == "") { return ""; }
            WebClient wc1 = new WebClient();
            wc1.Encoding = System.Text.Encoding.UTF8;
            wc1.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
            wc1.Headers.Add("Accept-Language", "ru-ru");
            wc1.Headers.Add("Content-Language", "ru-ru");
            string w2 = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair=en|ru", s1.ToLower());
            string re1 = "";
            try { re1 = wc1.DownloadString(w2); } catch { re1 = ""; Log("ERROR: www.google.com/translate_t? вызвал наш таймаут в секунду"); }
            if (re1 == "") { return ""; }
            string ans = "";
            int ii7 = re1.IndexOf("<span title=\"");
            while (ii7 != -1) {
                re1 = re1.Substring(ii7 + "<span title=\"".Length);
                re1 = re1.Substring(re1.IndexOf(">") + 1);
                string w12 = re1.Substring(0, re1.IndexOf("</span>"));//word
                //if (s1.IndexOf(w12) == -1) { ans = ans + w12.ToLower().Replace(".", "").TrimStart().TrimEnd() + " "; }
                if (s1.IndexOf(w12.Replace(".", "").Replace(" ", "")) == -1) { ans = ans + w12.ToLower() + " "; }
                ii7 = re1.IndexOf("<span title=\"");
            }
            return ans.TrimEnd();
        }
        public static string get_google_url_page(string url)
        {
            string googleRU = "https://www.google.ru/searchbyimage?&hl=ru-ru&lr=lang_ru&image_url=";
            string gurl = googleRU + url;

            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
            wc.Headers.Add("Accept-Language", "ru-ru");
            wc.Headers.Add("Content-Language", "ru-ru");
            return wc.DownloadString(gurl);
        }
        public static string Game_Logon(string url1, string name, string pass)
        {
            string formParams = string.Format("Login={0}&Password={1}", name, pass);
            string cookieHeader = "";
            var cookies = new CookieContainer();
            dGame.game_cCont = cookies;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url1);
            req.CookieContainer = cookies;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = Encoding.UTF8.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream()) { os.Write(bytes, 0, bytes.Length); }
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookieHeader = resp.Headers["Set-cookie"];
            dGame.game_cHead = cookieHeader;
            string pageSource = "";
            using (StreamReader sr = new StreamReader(resp.GetResponseStream())) { pageSource = sr.ReadToEnd(); }
            return pageSource;
        }
        public static string parse_html_body(string g)
        {
            g = g.Substring(g.IndexOf("<body>")+6).Replace("</body>", "").Replace("</html>", "");
            string[,] tags = {
                { "<script"  , "<noscript>" , "<style>" , "onmousedown=\"", "value=\"", "data-jiis=\"", "data-ved=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "unselectable=\"", "<!--", "ID=\"", "style=\"" , "class=\"" , "//<![CDATA[" , "border=\"" , "cellspacing=\"" , "cellpadding=\"" , "target=\"" , "colspan=\"" , "onclick=\"" , "align=\"" , "color=\"" , "nowrap=\"" , "vspace=\"" },
                { "</script>", "</noscript>", "</style>", "\""            , "\""      , "\""          , "\""         , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""             , "-->" , "\""   , "\""       , "\""       , "//]]>"       , "\""        , "\""             , "\""             , "\""        , "\""         , "\""         , "\""       , "\""       , "\""        , "\""        }
            };
            int tags_len = tags.Length / 2;
            bool fl = true;
            for (int i = 0; i < tags_len; i++)
            {
                fl = true;
                while (fl)
                {
                    fl = false;
                    int i1 = g.IndexOf(tags[0, i]);
                    if (i1 != -1)
                    {
                        string g2 = g.Substring(i1 + tags[0, i].Length);
                        int i2 = g2.IndexOf(tags[1, i]);
                        g = g.Substring(0, i1) + g2.Substring(i2 + tags[1, i].Length);
                        fl = true;
                    }
                }
            }
            g = g.Trim().Replace("\t"," ").Replace("&nbsp;", " ").Replace("<br/>", "\r\n").Replace("<b>", " ").Replace("</b>", " ").Replace("<u>", " ").Replace("</u>", " ").Replace("<i>", " ").Replace("</i>", " ").Trim();
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace(" \r\n", "\r\n").Replace("\r\n ", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace(" >", ">").Replace("<br/>", "\r\n").Replace("<br />", "").Replace("\r\n\r\n", "\r\n");
            g = g.Replace("<div>", "").Replace("</div>", "").Replace("<span>", "").Replace("</span>", "");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace(" \r\n", "\r\n").Replace("\r\n ", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace("\r\n\r\n", "\r\n").Replace(" >", ">").Replace("<br/>", "\r\n").Replace("<br />", "").Replace("\r\n\r\n", "\r\n");
            return g;
        }
        public static string get_game_page(string url)
        {
            string ps = "";
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            //getRequest.Headers.Add("Accept-Language", "ru-ru");
            //getRequest.Headers.Add("Content-Language", "ru-ru");
            //getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
            getRequest.CookieContainer = dGame.game_cCont;
            WebResponse getResponse = getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                ps = sr.ReadToEnd();
            }
            return ps;
        }
        public static string parse_level_text(string t1)
        {
            t1 = t1.Substring(t1.IndexOf("<ul class=\"section level\">"));
            t1 = t1.Substring(t1.IndexOf("</ul>"));
            t1 = t1.Replace("<br/>", "\r\n").Replace("<div class=\"spacer\"></div>", "").Replace("<h3 class=\"color_bonus\">", "").Replace("<!-- container -->", "").Replace("</body>", "").Replace("</html>", "").Replace("</ul><!--end level-->", "").Replace("<p>", "").Replace("</p>", "").Replace("<h3 class=\"color_correct\">", "").Replace("<h3>", "").Replace("</h3>", "");
            string t2 = "";
            int ii1 = 0;
            int ii2 = 0;
            bool fl = true;
            while (fl)
            {
                fl = false;
                ii1 = t1.IndexOf("<p"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
                ii1 = t1.IndexOf("<span"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
                ii1 = t1.IndexOf("<strong"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
                ii1 = t1.IndexOf("<script"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf("</script>"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 9); }
                ii1 = t1.IndexOf("<!--"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf("-->"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 3); }
                ii1 = t1.IndexOf("//<![CDATA["); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf("//]]>"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 5); }
                ii1 = t1.IndexOf("<h3"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
                ii1 = t1.IndexOf("<div"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
                ii1 = t1.IndexOf("<a"); if (ii1 != -1) { fl = true; t2 = t1.Substring(ii1); ii2 = t2.IndexOf(">"); t1 = t1.Substring(0, ii1) + "\r\n" + t2.Substring(ii2 + 1); }
            }
            //<span class="color_sec">(completed, award 1 minute)</span>
            t1 = t1.Replace("</a>", "\r\n").Replace("<br />", "\r\n").Replace("<u>", "").Replace("</u>", "").Replace("<i>", "").Replace("</i>", "").Replace("<b>", "").Replace("</b>", "").Replace("</strong>", "\r\n").Replace("</span>", "\r\n").Replace("</p>", "\r\n").Replace("&nbsp;", " ").Replace("<br>", "\r\n").Replace("</div>", "\r\n");
            t1 = t1.Replace("\t", " ").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n");
            t1 = t1.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            t1 = t1.Replace("\r\n ", "\r\n").Replace(" \r\n", "\r\n").Replace("\r ", "\r").Replace(" \r", "\r").Replace("\n ", "\n").Replace(" \n", "\n");
            t1 = t1.Replace("\r\r", "\r").Replace("\r\r", "\r").Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\r\r", "\r").Replace("\r\r", "\r").Replace("\n\n", "\n").Replace("\n\n", "\n");
            t1 = t1.Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n").Replace("\r\n\r\n\r\n", "\r\n\r\n");
            t1 = t1.Replace("\r\n)\r\n", ")\r\n");
            return t1;
        }
        public static string get_task_type_by_name(string abc)
        {
            for (int i = 0; i < (actions.Length / 2); i++)
            {
                if (abc == actions[i, 0])
                {
                    return actions[i, 1];
                }
            }
            return "";
        }
        public static System.Collections.Generic.List<string> get_list_of_urls_from_text(string abc)
        {
            var L1 = new System.Collections.Generic.List<string>();
            string[] lines = Regex.Split(abc, "\r\n");
            foreach (string str in lines)
            {
                if (str.Length < 5) { continue; }
                if (str.Substring(0, 4) == "http")
                {
                    L1.Add(str);
                }
            }
            return L1;
        }
        public static string set_word_protect(string v, int num, Program.prot p)
        {
            string vv = "000";
            switch (p)
            {
                case Program.prot.none      : return v;
                case Program.prot.begin1    : return num.ToString() + v;
                case Program.prot.begin2    : vv += num.ToString(); return vv.Substring(vv.Length - 2, 2) + v;
                case Program.prot.begin3    : vv += num.ToString(); return vv.Substring(vv.Length - 3, 3) + v;
                case Program.prot.end1      : return v + num.ToString();
                case Program.prot.end2      : vv += num.ToString(); return v + vv.Substring(vv.Length - 2, 2);
                case Program.prot.end3      : vv += num.ToString(); return v + vv.Substring(vv.Length - 3, 3);
                default                     : return v;
            }
        }
        public static bool try_form_send(int lvl, string val)
        {
            if (lvl < 1) { return false; }
            string url = "http://" + dGame.game_domain + "/gameengines/encounter/play/" + dGame.game_id + "/?level=" + lvl.ToString();
            Random rnd1 = new Random();
            string t1 = get_game_page(url);
            System.Threading.Thread.Sleep(rnd1.Next(Program.rnd_min, Program.rnd_max));
            string t2 = t1;
            string tt1 = "name=\"LevelId\" value=\"";
            t1 = t1.Substring(t1.IndexOf(tt1) + tt1.Length);
            string LevelId = t1.Substring(0, t1.IndexOf("\""));
            string tt2 = "name=\"LevelNumber\" value=\"";
            t2 = t2.Substring(t2.IndexOf(tt2) + tt2.Length);
            string LevelNumber = t2.Substring(0, t2.IndexOf("\""));

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ServicePoint.Expect100Continue = false;
            req.Referer = url;
            req.KeepAlive = true;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.CookieContainer = dGame.game_cCont;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            string formParams = string.Format("LevelId={0}&LevelNumber={1}&LevelAction.Answer={2}", LevelId, LevelNumber, val);
            byte[] bytes = Encoding.UTF8.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }
            string ps = "";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                ps = sr.ReadToEnd();
            }

            ps = ps.Replace("\t", "").Replace("\n", "").Replace("\r", "");
            ps = ps.Substring(ps.IndexOf("<ul class=\"history\">"));
            ps = ps.Substring(0, ps.IndexOf("</ul>")).Replace("<ul class=\"history\">", "").Replace("</li>", "");
            string[] hist = Regex.Split(ps, "<li");
            foreach (string str in hist)
            {
                int i1 = str.IndexOf(">" + dGame.username + "<");
                int i2 = str.IndexOf(">" + val + "<");
                if ((i1 != -1) && (i2 != -1))
                {
                    int i3 = str.IndexOf("class=\"correct\"");
                    if (i3 != -1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void Event_SolveLevel(object sender, EventArgs e)
        {
            string type = get_task_type_by_name(GameTab.gChoice.SelectedItem.ToString());
            if (type == "raschl")
            {
                var R1 = new Raschl(GameTab.LvlList.SelectedIndex, GameTab.LvlText.Text);
            }
            if (type == "picture")
            {
                var R1 = new Picture(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            }
            if (type == "picture_association")
            {
                var R1 = new Association(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            }
            if (type == "logogrif")
            {
                var R1 = new Logogrif(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            }
            if (type == "metagramm")
            {
                var R1 = new Metagramm(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            }
            if (type == "gybrid")
            {
                var R1 = new Gybrid(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            }
            //
        }
        public static void Event_MainFormChangeSize(object sender, EventArgs e)
        {
            Tabs.Top = mainform_border;
            Tabs.Left = mainform_border;
            Tabs.Width = Mainform.Width - 5 * mainform_border;
            Tabs.Height = Mainform.Height - 10 * mainform_border;
            GameTab.MainTab.Left = mainform_border;
            GameTab.MainTab.Top = mainform_border;
            GameTab.MainTab.Width = Tabs.Width - 3 * mainform_border;
            GameTab.MainTab.Height = Tabs.Height - 3 * mainform_border - 11; // почему 11? хз но работает корректно
            GameTab.BtnUser.Left = mainform_border;
            GameTab.BtnUser.Top = mainform_border;
            GameTab.BtnUser.Width = 20 * mainform_border;
            GameTab.BtnUser.Height = 5 * mainform_border;
            GameTab.BtnGame.Left = GameTab.BtnUser.Right + mainform_border;
            GameTab.BtnGame.Top = GameTab.BtnUser.Top;
            GameTab.BtnGame.Width = GameTab.BtnUser.Width;
            GameTab.BtnGame.Height = GameTab.BtnUser.Height;
            GameTab.LvlList.Top = GameTab.BtnUser.Bottom + mainform_border;
            GameTab.LvlList.Left = mainform_border;
            GameTab.LvlList.Width = GameTab.MainTab.Width / 4;
            GameTab.LvlList.Height = GameTab.MainTab.Height / 2;
            GameTab.LvlText.Top = GameTab.LvlList.Top;
            GameTab.LvlText.Left = GameTab.LvlList.Right + mainform_border;
            GameTab.LvlText.Width = GameTab.MainTab.Width - GameTab.LvlList.Width - 3 * mainform_border;
            GameTab.LvlText.Height = GameTab.MainTab.Height - GameTab.BtnUser.Height - 3 * mainform_border;
            GameTab.gChoice.Top = GameTab.LvlList.Bottom + 2 * Program.mainform_border;
            GameTab.gChoice.Left = Program.mainform_border;
            GameTab.gChoice.Width = GameTab.LvlList.Width;
            GameTab.BtnSolve.Top = GameTab.gChoice.Bottom + 2 * Program.mainform_border;
            GameTab.BtnSolve.Left = Program.mainform_border;
            GameTab.BtnSolve.Width = GameTab.gChoice.Width;
        }
        public static void Event_SelectGameFromList(object sender, EventArgs e)
        {
            ListBox l4 = (ListBox)sender;
            dGame.tb.Text = dGame.g_urls[l4.SelectedIndex];
            //Form f1 = l4.Parent;
            //f1.Close();
        }
        public static void Event_BtnUserClick(object sender, EventArgs e)
        {
            // нужная ветка реестра д.б. в HKCU - //[HKEY_CURRENT_USER\Software\lnl122\solver] //"user"="username" //"pass"="userpassword"
            // обратимся к реестру, есть ли там записи о последнем юзере, если есть - прочтем их
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rks = rk.OpenSubKey("Software", true); rk.Close();
            RegistryKey rksl = rks.OpenSubKey("lnl122", true); if (rksl == null) { rksl = rks.CreateSubKey("lnl122"); } rks.Close();
            RegistryKey rksls = rksl.OpenSubKey("Solver", true); if (rksls == null) { rksls = rksl.CreateSubKey("Solver"); } rksl.Close();
            string user = "";
            string pass = "";
            var r_user = rksls.GetValue("user");
            if (r_user == null) { rksls.SetValue("user",""); user = ""; } else { user = r_user.ToString(); }
            var r_pass = rksls.GetValue("pass");
            if (r_pass == null) { rksls.SetValue("pass", ""); pass = ""; } else { pass = r_pass.ToString(); }
            rksls.Close();

            // форма для ввода данных
            Form Login = new Form();
            Login.Text = "Введите ник игрока и его пароль..";
            Login.StartPosition = FormStartPosition.CenterScreen;
            Login.Width = 35 * mainform_border;
            Login.Height = 25 * mainform_border;
            Login.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Login.AutoSize = true;
            Label lu = new Label();
            lu.Text = "ник:";
            lu.Top = 2 * mainform_border;
            lu.Left = mainform_border;
            lu.Width = 10 * mainform_border;
            Login.Controls.Add(lu);
            Label lp = new Label();
            lp.Text = "пароль:";
            lp.Top = lu.Bottom + mainform_border;
            lp.Left = mainform_border;
            lp.Width = lu.Width;
            Login.Controls.Add(lp);
            TextBox tu = new TextBox();
            tu.Text = user;
            tu.Top = lu.Top;
            tu.Left = lu.Right + mainform_border;
            tu.Width = 3 * lu.Width;
            Login.Controls.Add(tu);
            TextBox tp = new TextBox();
            tp.Text = pass;
            tp.Top = lp.Top;
            tp.Left = tu.Left;
            tp.Width = tu.Width;
            Login.Controls.Add(tp);
            Button blok = new Button();
            blok.Text = "выполнить вход";
            blok.Top = lp.Bottom + 2 * mainform_border;
            blok.Left = lu.Left;
            blok.Width = tu.Right - 1 * mainform_border;
            blok.DialogResult = DialogResult.OK;
            Login.AcceptButton = blok;
            Login.Controls.Add(blok);

            // предложим ввести юзера и пароль, дефолтные значения - то, что было в реестре, или же пусто
            bool fl = true;
            while (fl)
            {
                if (Login.ShowDialog() == DialogResult.OK)
                {
                    // попробуем авторизоваться на гейм.ен.цх с указанной УЗ
                    user = tu.Text;
                    pass = tp.Text;
                    Log("Пробуем выполнить вход на сайт для пользвоателя " + user);
                    string pageSource = Game_Logon("http://game.en.cx/Login.aspx", user, pass);
                    // если авторизовались успешно - записываем данные в реестр, меняем заголовок программы, делаем доступной кнорпку выбора игры
                    if (pageSource.IndexOf("action=logout") != -1)
                    {
                        // обновить в реестре 
                        RegistryKey rk2 = Registry.CurrentUser.OpenSubKey("Software\\lnl122\\Solver", true);
                        rk2.SetValue("user", user);
                        rk2.SetValue("pass", pass);
                        rk2.Close();
                        // включим кнопку игры
                        GameTab.BtnGame.Enabled = true;
                        GameTab.BtnUser.Enabled = false;
                        // изменим заголовок
                        Mainform.Text = mainform_caption + " / user: " + user;
                        // запомним параметры игрока
                        dGame.username = user;
                        dGame.password = pass;
                        pageSource = pageSource.ToLower();
                        pageSource = pageSource.Substring(pageSource.IndexOf(user.ToLower()));
                        pageSource = pageSource.Substring(pageSource.IndexOf("(id"));
                        pageSource = pageSource.Substring(pageSource.IndexOf(">")+1);
                        dGame.userid = pageSource.Substring(0, pageSource.IndexOf("<"));
                        // поставим флаг выхода
                        fl = false;
                        // в лог
                        Log("Имя и пароль пользователя проверены, успешный логон для id=" + dGame.userid);
                    }
                    else
                    {
                        // если не успешно - вернемся в вводу пользователя
                        Log("Неверные логин/пароль");
                        MessageBox.Show("Неверные логин/пароль");
                    }
                }
                else
                {
                    // если отказались вводить имя/пасс - выходим
                    fl = false;
                }
            } // выход только если fl = false -- это или отказ польователя в диалоге, или если нажато ОК - корректная УЗ
        }
        public static void Event_BtnGameClick(object sender, EventArgs e)
        {
            string url1 = "http://game.en.cx/UserDetails.aspx?zone=1&tab=1&uid=" + dGame.userid + "&page=1";
            string cookieHeader = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url1);
            req.CookieContainer = dGame.game_cCont;
            req.ContentType = "application/x-www-form-urlencoded";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            cookieHeader = resp.Headers["Set-cookie"];
            dGame.game_cHead = cookieHeader;
            string pageSource = "";
            using (StreamReader sr = new StreamReader(resp.GetResponseStream())) { pageSource = sr.ReadToEnd(); }
            string ps1 = parse_html_body(pageSource);
            ps1 = ps1.Substring(ps1.IndexOf("Послужной список"));
            ps1 = ps1.Substring(ps1.IndexOf("Игры"));
            ps1 = ps1.Substring(ps1.IndexOf("Мозговой штурм"));
            string[] ar1 = Regex.Split(ps1.Replace(" bg>", "").Replace("\r\n", " ").Replace("</tr> ", "").Replace("</td> ", ""), "<tr");
            System.Collections.Generic.List<string> l1 = new System.Collections.Generic.List<string>();
            System.Collections.Generic.List<string> l2 = new System.Collections.Generic.List<string>();
            foreach (string s1 in ar1) { if (s1.IndexOf("/Teams/TeamDetails.aspx") != -1) { l1.Add(s1.Replace("> ", ">").Replace(" <", "<")); } }
            foreach (string s2 in l1)
            {
                string r_url = "";
                string r_name = "";
                string r_num = "";
                bool r_flag = true;
                string[] ar2 = Regex.Split(s2,"<td>");
                for (int i = 0; i < ar2.Length; i++)
                {
                    if (ar2[i].Length < 5) { continue; }
                    if (ar2[i].Substring(ar2[i].Length - 5, 5) == "Место") { r_flag = false; break; }
                    if (ar2[i][0] == '#') { r_num = ar2[i]; }
                    if (ar2[i].IndexOf("<a href=\"") != -1)
                    {
                        string q1 = ar2[4].Substring(0, ar2[4].IndexOf("</a>")).Replace("<a href=\"", "");
                        r_url = q1.Substring(0, q1.IndexOf("\">"));
                        r_name = q1.Substring(q1.IndexOf("\">") + 2);
                    }
                }
                if (r_flag) { l2.Add(r_url+"|"+r_num+" | "+r_name); }
            }
            // l2 - list of games
            dGame.g_names = new string[l2.Count];
            dGame.g_urls = new string[l2.Count];
            for(int i=0; i< l2.Count; i++)
            {
                int ii2 = l2[i].IndexOf("|");
                dGame.g_urls[i] = l2[i].Substring(0,ii2);
                dGame.g_names[i] = l2[i].Substring(ii2+1);
            }

            // форма для ввода данных
            Form SelectGame = new Form();
            SelectGame.Text = "Выбор игры..";
            SelectGame.StartPosition = FormStartPosition.CenterScreen;
            SelectGame.Width = 35 * mainform_border;
            SelectGame.Height = 25 * mainform_border;
            SelectGame.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SelectGame.AutoSize = true;
            Label la = new Label();
            la.Text = "Необходимо двойным кликом выбрать игру из списка\r\nили же ввести ссылку на игру в нижнем поле ввода\r\nи нажать 'Открыть игру'";
            la.Top = 2 * mainform_border;
            la.Left = mainform_border;
            la.Width = 100 * mainform_border;
            la.Height = 10 * mainform_border;
            SelectGame.Controls.Add(la);
            ListBox lb = new ListBox();
            lb.Top = la.Bottom + mainform_border;
            lb.Left = mainform_border;
            lb.Width = la.Width;
            lb.Height = 20 * mainform_border;
            for (int i = 0; i < dGame.g_names.Length; i++) { lb.Items.Add(dGame.g_names[i]); }
            lb.DoubleClick += new EventHandler(Event_SelectGameFromList);
            SelectGame.Controls.Add(lb);
            dGame.tb = new TextBox();
            dGame.tb.Text = "";
            if (Env.system_name == "NBIT01") { dGame.tb.Text="http://demo.en.cx/gameengines/encounter/play/24889"; } // for TEST
            dGame.tb.Top = lb.Bottom + 2 * mainform_border;
            dGame.tb.Left = mainform_border;
            dGame.tb.Width = lb.Width - 24 * mainform_border;
            SelectGame.Controls.Add(dGame.tb);
            Button blok = new Button();
            blok.Text = "Открыть игру";
            blok.Top = dGame.tb.Top;
            blok.Left = dGame.tb.Right + 2 * mainform_border;
            blok.Width = 22 * mainform_border;
            blok.DialogResult = DialogResult.OK;
            SelectGame.AcceptButton = blok;
            SelectGame.Controls.Add(blok);

            // предложим ввести юзера и пароль, дефолтные значения - то, что было в реестре, или же пусто
            string page = "";
            bool fl = true;
            while (fl)
            {
                if (SelectGame.ShowDialog() == DialogResult.OK)
                {
                    string url = dGame.tb.Text;
                    // попробуем авторизоваться в игре - сначала разберем полученную строку
                    if (url == "") { MessageBox.Show("Не выбрана игра вообще.."); continue; }
                    string url2 = url;
                    if (url2.Substring(0,7) != "http://") { MessageBox.Show("Указана не ссылка.."); continue; }
                    url2 = url.Replace("http://", "");
                    int ii1 = url2.IndexOf("/"); if (ii1 == -1) { MessageBox.Show("указан только хост.."); continue; }
                    dGame.game_domain = url2.Substring(0,ii1);
                    url2 = url2.Substring(ii1+1);
                    if(url2.IndexOf("gameengines/encounter/play/") != -1)
                    {
                        ii1 = url2.IndexOf("/?level="); if (ii1 != -1) { url2 = url2.Substring(0, ii1); }
                        dGame.game_id = url2.Substring(url2.LastIndexOf("/") + 1);
                    } else
                    {
                        if (url2.IndexOf("GameDetails.aspx?gid=") != -1) { dGame.game_id = url2.Substring(url2.LastIndexOf("=") + 1); }
                        else { MessageBox.Show("Ссылку на игру не удалось понять.."); continue; } // ни один из форматов ссылок не подошел
                    }
                    //MessageBox.Show(url + "\r\n" + dGame.game_domain + "\r\n" + dGame.game_id);
                    // если авторизовались успешно - запоминаем игру
                    string ps2 = Game_Logon("http://" + dGame.game_domain + "/Login.aspx", dGame.username, dGame.password);
                    if (ps2.IndexOf("action=logout") != -1)
                    {
                        // прочесть игру и узнать её параметры
                        string ps3 = get_game_page("http://" + dGame.game_domain + "/GameDetails.aspx?gid=" + dGame.game_id);
                        string ps4 = parse_html_body(ps3).ToLower().Replace("\r\n","");
                        int fr = ps4.IndexOf("<td>игра:мозговой штурм</td>");
                        int fe = ps4.IndexOf("<td>covering zone:brainstorm");
                        if (fr + fe < 0) { MessageBox.Show("Это не МШ.."); continue; }
                        fr = ps4.IndexOf("<td>последовательность прохождения:штурмовая</td>");
                        fe = ps4.IndexOf("<td>the levels passing sequence:storm</td>");
                        if (fr + fe < 0) { MessageBox.Show("Последовательность не штурмовая.."); continue; }
                        page = get_game_page("http://" + dGame.game_domain + "/gameengines/encounter/play/" + dGame.game_id);
                        if (page.IndexOf("class=\"gameCongratulation\"") != -1) { MessageBox.Show("Эта игра уже закончилась.."); continue; }
                        if (page.IndexOf("<span id=\"animate\">Поздравляем!!!</span>") != -1) { MessageBox.Show("Эта игра уже закончилась.."); continue; }
                        if (page.IndexOf("Капитан команды не включил вас в состав для участия в этой игре.") != -1) { MessageBox.Show("Капитан команды не включил вас в состав для участия в этой игре.."); continue; }
                        if (page.IndexOf("<span id=\"Panel_lblGameError\">") != -1) { MessageBox.Show("Эта игра ещё не началась.."); continue; }
                        if (page.IndexOf("Вход в игру произойдет автоматически") != -1) { MessageBox.Show("Эта игра ещё не началась.."); continue; }
                        //определим количтсво уровней
                        string q_lvl = page.Substring(page.IndexOf("<body")).Replace("\r", "").Replace("\n", "").Replace("\t", "");
                        string t1 = "<ul class=\"section level\">";
                        string t2 = "</ul>";
                        int i2 = q_lvl.IndexOf(t1);
                        q_lvl = q_lvl.Substring(i2 + t1.Length);
                        q_lvl = q_lvl.Substring(0, q_lvl.IndexOf(t2));
                        i2 = q_lvl.LastIndexOf("<i>");
                        q_lvl = q_lvl.Substring(i2 + 3);
                        q_lvl = q_lvl.Substring(0, q_lvl.IndexOf("</i>"));
                        if (Int32.TryParse(q_lvl, out i2)) { dGame.game_levels = i2; }
                        if (dGame.game_levels == 0) { MessageBox.Show("Не удалось определить количество уровней.."); continue; }
                        // поставим флаг выхода и заблокируем кнопку на будущее.
                        fl = false;
                        GameTab.BtnGame.Enabled = false;
                        // в лог
                        //MessageBox.Show("Открыта игра " + dGame.userid);
                        Log("Открыта игра " + dGame.userid);
                        string temppath = Env.local_path + "\\pics"; if (!Directory.Exists(temppath)) { Directory.CreateDirectory(temppath); }
                        //temppath = temppath + "\\" + dGame.game_id;  if (!Directory.Exists(temppath)) { Directory.CreateDirectory(temppath); }
                        //for (int i8 = 0; i8 <= dGame.game_levels; i8++) { if (!Directory.Exists(temppath+"\\"+i8.ToString())) { Directory.CreateDirectory(temppath + "\\" + i8.ToString()); } }
                        Env.temp_path = temppath;
                    }
                    else
                    {
                        // если не успешно - вернемся в вводу пользователя
                        Log("ERROR Не удалось подключиться к "+ dGame.game_domain);
                        MessageBox.Show("Не удалось подключиться к " + dGame.game_domain);
                    }
                }
                else
                {
                    // если отказались выбирать игру - выходим
                    fl = false;
                }
            } // выход только если fl = false -- это или отказ польователя в диалоге, или если нажато ОК - проверка пройдена
            // смотрим на page - если не пусто - то подключились
            if(page != "")
            {
                dGame.level_name = new string[dGame.game_levels+1];
                dGame.level_text = new string[dGame.game_levels+1];
                dGame.level_full = new string[dGame.game_levels+1];
                //dGame.level_pics = new string[dGame.game_levels+1];
                string url_base = "http://" + dGame.game_domain + "/gameengines/encounter/play/" + dGame.game_id + "/?level=";
                for (int i = 1; i <= dGame.game_levels; i++)
                {
                    string t1 = get_game_page(url_base + i.ToString());
                    dGame.level_full[i] = t1;
                    string t2 = t1.Substring(t1.IndexOf("<li class=\"level-active\">"));
                    t2 = t2.Substring(t2.IndexOf("<span>") + 6);
                    t2 = t2.Substring(0, t2.IndexOf("</span>"));
                    t2 = i.ToString() + " : " + t2;
                    dGame.level_name[i] = t2;
                    GameTab.LvlList.Items.Add(t2);

                    t1 = parse_level_text(t1);
                    string pics = "";
                    fl = true;
                    while (fl)
                    {
                        fl = false;
                        int ii1 = t1.IndexOf("<img");
                        if (ii1 != -1)
                        {
                            fl = true;
                            string t5 = t1.Substring(ii1);
                            int ii2 = t5.IndexOf(">");
                            string p1 = t5.Substring(0, ii2 + 1);
                            int jj1 = p1.IndexOf("src=\"");
                            p1 = p1.Substring(jj1 + 5);
                            jj1 = p1.IndexOf("\"");
                            p1 = p1.Substring(0, jj1);
                            pics = pics + p1 + "\r\n";
                            t1 = t1.Substring(0, ii1) + "\r\n\r\nImage:\r\n" + p1 + "\r\n" + t5.Substring(ii2 + 1);
                        }
                    }
                    dGame.level_text[i] = t1;
                }
            }
        }
        public static void Event_LevelSelected(object sender, EventArgs e)
        {
            if (GameTab.LvlList.Items.Count != 1) {
                int newlvl = GameTab.LvlList.SelectedIndex;
                GameTab.LvlText.Text = dGame.level_text[newlvl];
            }
        }

        private static void CreateMainForm()
        {
            Mainform = new Form();
            Mainform.Size = new Size(System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width / 2, System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height / 2);
            Mainform.Text = mainform_caption;
            Mainform.StartPosition = FormStartPosition.CenterScreen;
            Mainform.AutoSizeMode = AutoSizeMode.GrowOnly;
            Mainform.SizeChanged += new EventHandler(Event_MainFormChangeSize);
            Tabs = new TabControl();
            Mainform.Controls.Add(Tabs);
            GameTab.MainTab = new TabPage();
            GameTab.MainTab.Text = "Игра";
            Tabs.Controls.Add(GameTab.MainTab);
            GameTab.BtnUser = new Button();
            GameTab.BtnUser.Text = "Логон в EN";
            GameTab.BtnUser.Click += new EventHandler(Event_BtnUserClick);
            GameTab.MainTab.Controls.Add(GameTab.BtnUser);
            GameTab.BtnGame = new Button();
            GameTab.BtnGame.Text = "Выбор игры";
            GameTab.BtnGame.Enabled = false;
            GameTab.BtnGame.Click += new EventHandler(Event_BtnGameClick);
            GameTab.MainTab.Controls.Add(GameTab.BtnGame);
            GameTab.LvlList = new ListBox();
            GameTab.LvlList.Items.Add("-: текст уровня пользователя");
            GameTab.LvlList.Click += new EventHandler(Event_LevelSelected);
            GameTab.MainTab.Controls.Add(GameTab.LvlList);
            GameTab.LvlText = new TextBox();
            GameTab.LvlText.Text = "Для пользовательского уровня укажите текст задания, или ссылки на картинки\r\n\r\nДля выбора задания игры необходимо выбрать уровень в списке слева\r\n\r\nhttp://d2.endata.cx/data/games/24889/test_pic_1_16.jpg\r\n";
            GameTab.LvlText.AcceptsReturn = true;
            GameTab.LvlText.AcceptsTab = false;
            GameTab.LvlText.Multiline = true;
            GameTab.LvlText.ScrollBars = ScrollBars.Both;
            GameTab.MainTab.Controls.Add(GameTab.LvlText);

            GameTab.gChoice = new ComboBox();
            for (int i = 0; i < (actions.Length / 2); i++) { GameTab.gChoice.Items.Add(actions[i, 0]); }
            GameTab.gChoice.SelectedIndex = 0;
            GameTab.MainTab.Controls.Add(GameTab.gChoice);
            GameTab.BtnSolve = new Button();
            GameTab.BtnSolve.Text = "Запустить решалку";
            GameTab.BtnSolve.Click += new EventHandler(Event_SolveLevel);
            GameTab.MainTab.Controls.Add(GameTab.BtnSolve);


            Event_MainFormChangeSize(null, null);
        }
        static void Main(string[] args)
        {
            Program.Env = GetEnvInfo(args);
            if (!CheckComponents()) { MessageBox.Show("Не все необхдимые компоненты установлены на ПК.\r\nПроверьте лог-файл."); return; }
            //создаём форму, передаём её управление
            CreateMainForm();
            System.Windows.Forms.Application.Run(Mainform);
            Log("Выход из программы..");
        }
    }
}
