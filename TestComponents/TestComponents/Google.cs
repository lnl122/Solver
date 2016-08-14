using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestComponents
{
    class Google
    {
        // пути
        private static string googleRU = "https://www.google.ru/searchbyimage?&hl=ru-ru&lr=lang_ru&image_url=";
        private static string IpicUri = "http://ipic.su";

        // вход - урл картинки
        // выход - страница гугля
        private static string GetPageByImageUrl(string imgurl)
        {
            string gurl = googleRU + imgurl;
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
            wc.Headers.Add("Accept-Language", "ru-ru");
            wc.Headers.Add("Content-Language", "ru-ru");
            string page = "";
            try
            {
                page = wc.DownloadString(gurl);
            }
            catch
            {
                page = "";
            }
            wc.Dispose();
            wc = null;

            if (page.Length <= 0)
            {
                return "";
            }
            page = page.ToLower().Replace("\t", " ").Replace("\n", " ");
            int body1 = page.IndexOf("<body");
            int body2 = page.IndexOf("</body>");
            if ((body1 == -1) || (body2 == -1))
            {
                return "";
            }
            page = page.Substring(body1 + 5, body2 - body1 - 5);
            return page;
        }

        // вход - страница
        // выход - текст со страницы после парсинга
        private static string ParsingGooglePage(string g)
        {
            string[,] tags = {
                { "<script"  , "<noscript>" , "<style>" , "onmousedown=\"", "value=\"", "data-jiis=\"", "data-ved=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "unselectable=\"", "<!--", "ID=\"", "style=\"" , "class=\"" , "//<![CDATA[" , "border=\"" , "cellspacing=\"" , "cellpadding=\"" , "target=\"" , "colspan=\"" , "onclick=\"" , "align=\"" , "color=\"" , "nowrap=\"" , "vspace=\"" , "href=\"" , "src=\"", "<cite"  , "{\"", "<g-img"  , "<a data-"   },
                { "</script>", "</noscript>", "</style>", "\""            , "\""      , "\""          , "\""         , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""             , "-->" , "\""   , "\""       , "\""       , "//]]>"       , "\""        , "\""             , "\""             , "\""        , "\""         , "\""         , "\""       , "\""       , "\""        , "\""        , "\""      , "\""    , "</cite>", "}"  , "</g-img>", "</a>"       }
            };
            int ihr1 = g.IndexOf("<hr");
            int ihr2 = g.LastIndexOf("<hr");
            if ((ihr1 < 0) || (ihr2 < 0))
            {
                return "";
            }
            g = g.Substring(ihr1, ihr2 - ihr1);
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
            g = g.Replace("&nbsp;", " ");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace(" >", ">").Replace("data-hve", " ");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace("<em>", " ").Replace("</em>", " ").Replace("data-hve", " ").Replace("<h2>", " ").Replace("<h3>", " ").Replace("</h2>", " ").Replace("</h3>", " ");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace(" >", ">").Replace("data-hve", " ");
            g = g.Replace("<a></a>", "").Replace("<div></div>", "").Replace("<span></span>", "").Replace("<a></a>", "").Replace("<div></div>", "").Replace("<span></span>", "").Replace("<a></a>", "").Replace("<div></div>", "").Replace("<span></span>", "");
            fl = true;
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
            g = g.Replace("<a>&times;</a>", "");
            fl = true;
            while (fl)
            {
                fl = false;
                int idx = g.IndexOf("&times;");
                if (idx > 0)
                {

                    string g1 = g.Substring(0, idx);
                    string g2 = g.Substring(idx);
                    int idx1 = g1.LastIndexOf("<span>");
                    int idx2 = g2.IndexOf("</span>");
                    if ((idx1 > 0) && (idx2 > 0))
                    {
                        g = g.Substring(0, idx1) + g.Substring(idx + idx2 + 7);
                        fl = true;
                    }
                }
            }
            g = g.Replace("страницы с подходящими изображениями", " ");
            g = g.Replace("<a>похожие изображения</a>", " ");
            g = g.Replace("благодарим за замечания.", " ");
            g = g.Replace("пожаловаться на содержание картинки.", " ");
            g = g.Replace("результаты поиска", " ");
            g = g.Replace("<a>сохраненная копия</a>", " ");
            g = g.Replace("<a>похожие</a>", " ");
            g = g.Replace("<ol>", " ").Replace("</ol>", " ").Replace("<li>", " ").Replace("</li>", " ").Replace("data-rt", " ").Replace("&middot;", " ");
            g = g.Replace("<wbr>", " ").Replace("&quot;", " ").Replace("...", " ").Replace("»", " ").Replace("«", " ").Replace("&#39;", " ");
            g = g.Replace("<hr>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace(" >", ">").Replace("> ", ">").Replace(" <", "<").Replace("< ", "<");
            g = g.Replace("<div>", " ").Replace("</div>", " ").Replace("<span>", " ").Replace("</span>", " ").Replace("<a>", " ").Replace("</a>", " ");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("<div e>", " ").Replace("  ", " ").Replace("  ", " ");
            g = g.Replace(";", " ").Replace("+", " ").Replace("\"", " ").Replace("—", " ").Replace("|", " ").Replace(".", " ").Replace("%", " ").Replace("*", " ").Replace("/", " ").Replace(",", " ").Replace("!", " ").Replace("?", " ").Replace(":", " ").Replace("-", " ").Replace("(", " ").Replace(")", " ");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            return g;
        }

        // вход - путь к локальной картинке
        // выход - урл картинки после аплоада
        private static string UploadFileIpic(string filepath)
        {
            string filename = filepath.Substring(filepath.LastIndexOf("\\") + 1);
            string uriaction = IpicUri + "/";
            HttpClient httpClient = new HttpClient();
            //System.Net.ServicePointManager.Expect100Continue = false;
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent("/"), "link");
            form.Add(new StringContent("loadimg"), "action");
            form.Add(new StringContent("ipic.su"), "client");
            var streamContent2 = new StreamContent(File.Open(filepath, FileMode.Open));
            form.Add(streamContent2, "image", filename);
            string sd = "";
            try
            {
                Task<HttpResponseMessage> response = httpClient.PostAsync(uriaction, form);
                HttpResponseMessage res2 = response.Result;
                res2.EnsureSuccessStatusCode();
                HttpContent Cont = res2.Content;
                httpClient.Dispose();
                sd = res2.Content.ReadAsStringAsync().Result;
                sd = sd.Substring(sd.IndexOf("[edit]") + 6);
                sd = sd.Substring(sd.IndexOf("value=\"") + 7);
                sd = sd.Substring(0, sd.IndexOf("\""));
            }
            catch
            {
                sd = "";
            }
            return sd;
        }

        // вход - строка из слов
        // выход - очищенный список слов
        private static List<string> RemoveDirtyWords(string st)
        {
            List<string> res = new List<string>();
            List<string> ru = new List<string>();
            List<string> en = new List<string>();
            string[] badwrds = { "на", "для", "из", "по", "как", "не", "от", "что", "это", "или", "вконтакте", "review", "png", "the",
                "за", "вы", "все", "википедия", "во", "год", "paradise", "том", "эту", "of", "размер", "руб", "бесплатно", "его", "клипарт",
                "описание", "есть", "картинки", "фотографии", "их", "for", "to", "можно", "мы", "назад", "но", "так", "ми", "они", "он",
                "если", "москве", "продажа", "сайт", "то", "только", "цене", "чтобы", "and", "при", "чем", "free", "без", "где", "очень",
                "со", "by", "toys", "two", "вас", "всех", "кто", "многие", "может", "чему", "яндекс", "вот", "нет", "сша", "характеристики",
                "ценам", "же", "ли", "можете", "нас", "обзор", "про", "современные", "того", "уже", "фоне", "&amp", "body", "какой", "под",
                "сайте", "сравнить", "ооо", "себя", "этой", "является", "in", "mb", "бы", "вам", "об", "также", "liveinternet", "заказать",
                "здесь", "какие", "лучшие", "vk", "http", "https", "ru", "com", "net", "org", "youtube", "vkontakte", "facebook", "фото",
                "видео", "смотреть", "купить", "куплю", "продам", "продать", "онлайн", "обои", "цена", "цены", "найти", "самые", "самых",
                "самый", "самая", "фильм", "отзывы", "фильма", "фильм", "фильму", "разрешение", "разрешении", "скидка", "скидки", "выбрать",
                "закачка", "закачки", "новости", "скачать", "форматы", "хорошем", "качестве", "свойства", "смотреть", "страницу", "бесплатные",
                "программы", "перевести", "td", "td", "is", "i", "<", ">", "design", "data", "material", "div", "wikipedia", "with", "был",
                "лет", "g", "on", "that", "быть", "интересные", "new", "stars", "this", "from", "google", "была", "всё", "еще", "i", "jpg",
                "online", "or", "png", "jpeg", "главная", "доставкой", "изготовление", "no", "over", "web", "янв", "фев", "мар", "апр", "май",
                "июн", "июл", "авг", "сен", "окт", "ноя", "дек", "пн", "вт", "ср", "чт", "пт", "сб", "вс",  };
            foreach (string s1 in badwrds)
            {
                st = st.Replace(" "+s1+" ", " ");
            }
            st = st.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            string[] ar = st.Split(' ');
            foreach(string ss in ar)
            {
                if (ss.Length > 1)
                {
                    // дополнительно проверим слова на принадлежность русскому или английскому языку, буквоцыфры и незнакомые языки выбросим
                    bool eng = false;
                    bool rus = false;
                    bool oth = false;
                    for(int i=0; i<ss.Length; i++)
                    {
                        char c = ss[i];
                        if ((c >= 'a') && (c <= 'z'))
                        {
                            eng = true;
                        }
                        else if (((c >= 'а') && (c <= 'я')) || (c == 'ё'))
                        {
                            rus = true;
                        }
                        else
                        {
                            oth = true;
                        }
                    }
                    // переведем английские
                    if (!rus && eng && !oth)
                    {
                        en.Add(ss);
                    }
                    // приведем русские к базовой форме
                    if (rus && !eng && !oth)
                    {
                        ru.Add(ss);
                    }
                }
            }
            // переведем en, найдем базовые к ru
            ru.AddRange(TranslateEnRu(en));
            res = FindBaseWord(ru);
            // удалим дубликаты, ранжируем слова по частоте встречания
            //***

            return res;
        }

        // вход - список слов на русском
        // выход - базовые слова
        private static List<string> FindBaseWord(List<string> lst)
        {
            List<string> res = new List<string>();

            return res;
        }

        // вход - список слов на английском
        // выход - список слов на русском
        private static List<string> TranslateEnRu(List<string> lst)
        {
            char delim = '.';
            List<string> res = new List<string>();
            if(lst.Count < 1) { return res; }
            string s1 = "";
            foreach(string ts1 in lst)
            {
                s1 = s1 + delim + " " + ts1;
            }
            s1 = s1.Substring(2);
            WebClient wc1 = new WebClient();
            wc1.Encoding = System.Text.Encoding.UTF8;
            wc1.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1");
            wc1.Headers.Add("Accept-Language", "ru-ru");
            wc1.Headers.Add("Content-Language", "ru-ru");
            string w2 = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair=en|ru", s1.ToLower());
            string re1 = "";
            try
            {
                re1 = wc1.DownloadString(w2);
            }
            catch
            {
                re1 = "";
                //Log("ERROR: www.google.com/translate_t? вызвал наш таймаут в секунду"); 
            }
            if (re1 == "") { return res; }
            int ii7 = re1.IndexOf("<span title=\"");
            while (ii7 != -1)
            {
                re1 = re1.Substring(ii7 + "<span title=\"".Length);
                re1 = re1.Substring(re1.IndexOf(">") + 1);
                string w12 = re1.Substring(0, re1.IndexOf("</span>"));//words
                string[] ar1 = w12.Split(delim);
                foreach(string w13 in ar1)
                {
                    string w14 = w13.Trim().ToLower();
                    if (w14 == "") { continue; }
                    if (lst.Contains(w14) == false)
                    {
                        res.Add(w14);
                    }
                }

                ii7 = re1.IndexOf("<span title=\"");
            }
            return res;
        }

        // вход - локальный путь к файлу с изображением
        // выход - набор слов со страницы гугля
        public static List<string> GetImageDescription(string path)
        {
            List<string> res = new List<string>();
            string a = UploadFileIpic(path);
            if (a == "") { return res; }
            string b = GetPageByImageUrl(a);
            if (b == "") { return res; }
            string c = ParsingGooglePage(b);
            if (c == "") { return res; }
            res = RemoveDirtyWords(c);
            return res;
        }
    }
}
