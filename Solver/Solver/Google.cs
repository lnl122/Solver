using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    class Google
    {
        // пути
        private static string googleRU = "https://www.google.ru/searchbyimage?&hl=ru-ru&lr=lang_ru&image_url=";
        private static string IpicUri = "http://ipic.su";

        private static string[,] tags = {
                { "<script"  , "<noscript>" , "<style>" , "onmousedown=\"", "value=\"", "data-jiis=\"", "data-ved=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "unselectable=\"", "<!--", "ID=\"", "style=\"" , "class=\"" , "//<![CDATA[" , "border=\"" , "cellspacing=\"" , "cellpadding=\"" , "target=\"" , "colspan=\"" , "onclick=\"" , "align=\"" , "color=\"" , "nowrap=\"" , "vspace=\"" , "href=\"" , "src=\"", "<cite"  , "{\"", "<g-img"  , "<a data-"   },
                { "</script>", "</noscript>", "</style>", "\""            , "\""      , "\""          , "\""         , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""             , "-->" , "\""   , "\""       , "\""       , "//]]>"       , "\""        , "\""             , "\""             , "\""        , "\""         , "\""         , "\""       , "\""       , "\""        , "\""        , "\""      , "\""    , "</cite>", "}"  , "</g-img>", "</a>"       }
            };

        // читаем страницу по урлу, отрезаем шапку
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
                Log.Write("g_img ERROR: не удалось получить страницу гугля для изображение по ссылке " + imgurl);
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

        // парсим текст страницы
        // вход - страница
        // выход - текст со страницы после парсинга
        private static string ParsingGooglePage(string g)
        {
            if (g.Length < 1)
            {
                return "";
            }
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

        // аплоад картинки по пути, получени внешней ссылки
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
                Log.Write("g_img ERROR: не удалось выполнить аплоад картинки " + filepath);
                sd = "";
            }
            return sd;
        }

        // вход - локальный путь к файлу с изображением
        // выход - набор слов со страницы гугля
        public static Words GetImageDescription(string path)
        {
            string a = UploadFileIpic(path);
            if (a == "") { return null; }
            string b = GetPageByImageUrl(a);
            if (b == "") { return null; }
            string c = ParsingGooglePage(b);
            if (c == "") { return null; }
            Words res = new Words(c);
            return res;
        }
    }
}
