using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    class Engine
    {

        private static string username = "";    // логин пользователя
        private static string password = "";    // пасс пользвоателя
        private static string userid = "";      // ид пользователя
        private static string gamedomain = "";  // домен игры
        private static string gameid = "";      // ид игры
        private static int levels = 0;          // колво уровней
        public static bool isReady = false;     // структура готова
        public static int last_level;           // последний уровень, к которому было обращение

        private static string cHead;            // куки
        private static CookieContainer cCont;   // куки

        public struct level
        {
            public int number;
            public string name;
            public string page;
            public string text;
            public bool isClose;
            public List<string> answers_good;
            public List<string> answers_bad;
            public int sectors;
            public int bonuses;
            public string[] sector;
            public string[] bonus;
            public List<string> ursl;
            public string formlevelid;
            public string formlevelnumber;
            public DateTime dt;
        }

        public static level[] L;

        // получает страницу по номеру уровня
        public static string GetPageLevel(int idx)
        {
            string url = "http://" + gamedomain + "/gameengines/encounter/play/" + gameid + "/?level=" + idx.ToString();
            last_level = idx;
            return get_game_page(url);
        }
        // возвращает наименование текущего уровня со страницы
        private static string GetLvlName(string g)
        {
            int i1 = g.IndexOf("<ul class=\"section level\">");
            if (i1 == -1) { return "не определен"; }
            g = g.Substring(i1);
            int i2 = g.IndexOf("</ul>");
            g = g.Substring(0, i2);
            i1 = g.IndexOf("<span>");
            if (i1 == -1) { return "не определен"; }
            g = g.Substring(i1+6);
            i2 = g.IndexOf("</span>");
            g = g.Substring(0, i2);
            return g;
        }
        // возвращает признак - уровень закрыт или нет
        private static bool GetLvlClose(string g)
        {
            int i1 = g.IndexOf("<label for=\"answer\">");
            if (i1 == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // возвращает ид уровня
        private static string GetLvlFormlevelid(string g)
        {
            int i1 = g.IndexOf("<form method=\"post\">");
            if (i1 == -1) { return ""; }
            else
            {
                g = g.Substring(i1);
                i1 = g.IndexOf("</form>");
                g = g.Substring(0, i1);
                string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<input");
                foreach (string s1 in ar1)
                {
                    if (s1.Contains("levelid"))
                    {
                        string s2 = s1.Substring(s1.IndexOf("value=\"") + 7);
                        s2 = s2.Substring(0, s2.IndexOf("\""));
                        return s2;
                    }
                }
            }
            return "";
        }        
        // возвращает номер уровня для формы
        private static string GetLvlFormlevelnumber(string g)
        {
            int i1 = g.IndexOf("<form method=\"post\">");
            if (i1 == -1) { return ""; }
            else
            {
                g = g.Substring(i1);
                i1 = g.IndexOf("</form>");
                g = g.Substring(0, i1);
                string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<input");
                foreach (string s1 in ar1)
                {
                    if (s1.Contains("levelnumber"))
                    {
                        string s2 = s1.Substring(s1.IndexOf("value=\"") + 7);
                        s2 = s2.Substring(0, s2.IndexOf("\""));
                        return s2;
                    }
                }
            }
            return "";
        }
        // возвращает список неудачных ответов
        private static List<string> GetLvlAnsBad(string g)
        {
            List<string> res = new List<string>();
            int i1 = g.IndexOf("<ul class=\"history\">");
            if (i1 == -1) { return res; }
            g = g.Substring(i1);
            i1 = g.IndexOf("</ul>");
            g = g.Substring(0, i1);
            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<i>");
            foreach (string s1 in ar1)
            {
                int i2 = s1.IndexOf("</i>");
                if (i2 == -1) { continue; }
                string s2 = s1.Substring(0, i2);
                res.Add(s2);
            }
            return res;
        }        
        // возвращает список удачных ответов
        private static List<string> GetLvlAnsGood(string g)
        {
            List<string> res = new List<string>();
            int i1 = g.IndexOf("<ul class=\"history\">");
            if (i1 == -1) { return res; }
            g = g.Substring(i1+20);
            i1 = g.IndexOf("</ul>");
            g = g.Substring(0, i1);
            g = g.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("<i>", " ").Replace("</i>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "</li>");
            foreach (string s1 in ar1)
            {
                int i2 = s1.IndexOf("<li class=\"correct\">");
                if (i2 == -1) { continue; }
                i2 = s1.IndexOf("<span");
                string s2 = s1.Substring(i2);
                i2 = s2.IndexOf(">");
                s2 = s2.Substring(i2 + 1);
                i2 = s2.IndexOf("<");
                s2 = s2.Substring(0, i2).Trim();
                if ((s2 != "") && (s2 != "пройден по таймауту")) { res.Add(s2); }
            }
            return res;
        }
        // возвращает перечень секторов и ответы на них
        private static string[] GetLvlSectors(string g)
        {
            List<string> res2 = new List<string>();
            string[] res = new string[0];
            g = g.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("<i>", " ").Replace("</i>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            int i1 = g.IndexOf("<div class=\"cols-wrapper\">");
            if (i1 == -1)
            {
                string[] res1 = new string[0];
                return res1;
            }
            g = g.Substring(i1 + ("<div class=\"cols-wrapper\">").Length);
            i1 = g.IndexOf("</div><!--end cols-wrapper -->");
            g = g.Substring(0, i1);
            g = g.Replace("<div class=\"cols\">", "").Replace("</div><!--end cols-->", "").Replace("<div class=\"cols w100per\">", "").Trim();
            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<p>");
            foreach(string s1 in ar1)
            {
                if (s1.Length < 5) { continue; }
                int i2 = s1.IndexOf("class");
                string s2 = s1.Substring(i2);
                if (s2.Contains("color_dis"))
                {
                    res2.Add("");
                }
                if (s2.Contains("color_correct"))
                {
                    i2 = s2.IndexOf(">");
                    s2 = s2.Substring(i2 + 1);
                    i2 = s2.IndexOf("<");
                    s2 = s2.Substring(0, i2);
                    res2.Add(s2);
                }
            }
            res = new string[res2.Count];
            for(int i=0; i<res2.Count; i++)
            {
                res[i] = res2[i];
            }
            return res;
        }
        // возвращает перечень бонусов и ответы на них
        private static string[] GetLvlBonuses(string g)
        {
            List<string> res2 = new List<string>();
            string[] res = new string[0];
            g = g.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("<i>", " ").Replace("</i>", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            int i1 = g.IndexOf("<h3>задание</h3>");
            if (i1 == -1)
            {
                i1 = g.IndexOf("<h3>task</h3>");
                if (i1 == -1)
                {
                    return res;
                }
            }
            g = g.Substring(i1);
            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<div class=\"spacer\"></div>");
            foreach (string s1 in ar1)
            {
                if (s1.Contains("<h3 class=\"color_bonus\">"))
                {
                    res2.Add("");
                }
                if (s1.Contains("<h3 class=\"color_correct\">"))
                {
                    int i2 = s1.IndexOf("<p>");
                    if (i2 == -1)
                    {
                        res2.Add("");
                    }
                    else
                    {
                        string s2 = s1.Substring(i2);
                        i2 = s2.IndexOf("</p>");
                        s2 = s2.Substring(0, i2);
                        s2 = s2.Replace("<p>", "").Replace("</p>", "").Trim();
                        res2.Add(s2);
                    }
                }
            }
            res = new string[res2.Count];
            for (int i = 0; i < res2.Count; i++)
            {
                res[i] = res2[i];
            }
            return res;
        }
        // возвращает текст уровня
        private static string GetLvlText(string g)
        {
            string res = "";
            g = g.Replace("\t", " ").Replace("&nbsp;", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            int i1 = g.IndexOf("<h3>задание</h3>");
            if (i1 == -1)
            {
                i1 = g.IndexOf("<h3>task</h3>");
                if (i1 == -1)
                {
                    return res;
                }
            }
            g = g.Substring(i1).Replace("<h3>задание</h3>", "").Replace("<h3>task</h3>", "");
            i1 = g.IndexOf("</h3>"); if (i1 != -1) { g = g.Substring(0, i1); }
            i1 = g.IndexOf("</div>"); if (i1 != -1) { g = g.Substring(0, i1); }
            g = g.Replace("\n", "\r").Replace("<br/>", "\r").Replace("<p>", " ").Replace("</p>", " ").Replace("\n", "\r").Replace("\n", "\r").Replace("\n", "\r");
            g = g.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("\r ", "\r").Replace(" \r", "\r").Replace("\r\r", "\r").Replace("\r\r", "\r").Replace("\r\r", "\r").Replace("\r\r", "\r");
            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<div class=\"spacer\">");
            res = res + ParseTags(ar1[0], tags4bonus) + "\r\r";

            foreach (string s1 in ar1)
            {
                if (s1.Contains("<h3 class=\"color_bonus\">"))
                {
                    string s2 = ParseTags(s1, tags4bonus);
                    res = res + s1 + "\r\r"; // *** надо изменить, учесть обработку скриптов для картинок, выкинуть лишнее
                }
            }
            res = res.Replace("\r", "\r\n");
            return res;
        }
        // возвращает набор урлов
        private static List<string> GetLvlUrls(string g)
        {
            List<string> res = new List<string>();
            g = g.Replace("\t", " ").Replace("&nbsp;", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            int i1 = g.IndexOf("<h3>задание</h3>");
            if (i1 == -1)
            {
                i1 = g.IndexOf("<h3>task</h3>");
                if (i1 == -1)
                {
                    return res;
                }
            }
            g = g.Substring(i1).Replace("<h3>задание</h3>", "").Replace("<h3>task</h3>", "");

            string[] ar1 = System.Text.RegularExpressions.Regex.Split(g, "<img src=\"");
            foreach (string s1 in ar1)
            {
                if (s1.Substring(0,4) == "http")
                {
                    string s2 = s1.Substring(0, s1.IndexOf("\""));
                    res.Add(s2);
                }
            }
            return res;
        }

        // собирает информацию обо всех уровнях в большой массив
        public static void GetLevels()
        {
            L = new level[levels+1];
            level lev = new level();
            lev.number = 0;
            lev.name = "пользовательский уровень";
            lev.page = "Для пользовательского уровня укажите текст задания, или ссылки на картинки\r\n\r\nДля выбора задания игры необходимо выбрать уровень в списке слева\r\n\r\nhttp://d2.endata.cx/data/games/24889/test_pic_1_16.jpg\r\n";
            lev.text = lev.page;
            lev.isClose = false;
            lev.answers_bad = new List<string>();
            lev.answers_good = new List<string>();
            lev.sectors = 1;
            lev.bonuses = 1;
            lev.sector = new string[1];
            lev.sector[0] = "";
            lev.bonus = new string[1];
            lev.bonus[0] = "";
            lev.ursl = new List<string>();
            lev.ursl.Add("http://d2.endata.cx/data/games/24889/test_pic_1_16.jpg");
            lev.dt = DateTime.Now;
            lev.formlevelid = "";
            lev.formlevelnumber = "";
            L[0] = lev;
            for(int i=1; i<=levels; i++)
            {
                lev = new level();
                lev.number = i;
                lev.page = GetPageLevel(i);
                Log.Store("level_"+i.ToString(), lev.page);
                lev.name = GetLvlName(lev.page);
                lev.isClose = GetLvlClose(lev.page);
                lev.answers_bad = GetLvlAnsBad(lev.page);
                lev.answers_good = GetLvlAnsGood(lev.page);
                lev.sector = GetLvlSectors(lev.page);
                lev.sectors = lev.sector.Length;
                lev.bonus = GetLvlBonuses(lev.page);
                lev.bonuses = lev.sector.Length;
                if (!lev.isClose)
                {
                    lev.formlevelid = GetLvlFormlevelid(lev.page);
                    lev.formlevelnumber = GetLvlFormlevelnumber(lev.page);
                }

                lev.text = GetLvlText(lev.page);
                lev.ursl = GetLvlUrls(lev.page);
                lev.dt = DateTime.Now;
                L[i] = lev;
            }
        }
        // обновляет информацию об уровне
        public static void UpdateLvlInfo(int lvl, string page)
        {
            level lev = L[lvl];
            lev.page = page;
            lev.isClose = GetLvlClose(lev.page);
            if (!lev.isClose)
            {
                lev.formlevelid = GetLvlFormlevelid(lev.page);
                lev.formlevelnumber = GetLvlFormlevelnumber(lev.page);
            }
            List<string> t1 = GetLvlAnsBad(lev.page);
            foreach (string s1 in t1)
            {
                if (!lev.answers_bad.Contains(s1))
                {
                    lev.answers_bad.Add(s1);
                }
            }
            t1 = GetLvlAnsGood(lev.page);
            foreach (string s1 in t1)
            {
                if (!lev.answers_good.Contains(s1))
                {
                    lev.answers_good.Add(s1);
                }
            }
            lev.sector = GetLvlSectors(lev.page);
            lev.sectors = lev.sector.Length;
            lev.bonus = GetLvlBonuses(lev.page);
            lev.bonuses = lev.sector.Length;
            lev.dt = DateTime.Now;
            L[lvl] = lev;
        }

        // выполняем логон в движке
        // вход - урл, логин, пасс
        // выход - страница с ответом
        public static string Logon(string url1, string name, string pass)
        {
            string formParams = string.Format("Login={0}&Password={1}", name, pass);
            string cookieHeader = "";
            var cookies = new CookieContainer();
            cCont = cookies;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url1);
            req.CookieContainer = cookies;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(formParams);
            req.ContentLength = bytes.Length;
            using (Stream os = req.GetRequestStream()) { os.Write(bytes, 0, bytes.Length); }
            string pageSource = "";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                cookieHeader = resp.Headers["Set-cookie"];
                cHead = cookieHeader;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) { pageSource = sr.ReadToEnd(); }
                username = name;
                password = pass;
            }
            catch
            {
                Log.Write("en.cx ERROR: не удалось получить ответ на авторизацию", url1 + " " + name + " " + pass);
            }
            return pageSource;
        }

        private static string[,] tags4list = {
                { "<script"  , "<noscript>" , "<style>" , "<!--", "bgcolor=\"", "align=\"", "nowrap=\"", "style=\"", "class=\"", "class='", "onclick=\"" , "id=\"", "height=\"" },
                { "</script>", "</noscript>", "</style>", "-->" , "\""        , "\""      , "\""       , "\""      , "\""      , "'"      , "\""         , "\""   , "\""        }
            };
        private static string[,] tags4bonus = {
                { "<span class=\"color_sec\">", "бонус ", "bonus ", "<img"   },
                { "</span>"                   , " "     , " "     , ">"      }
            };
        //                {  "onmousedown=\"", "value=\"", "data-jiis=\"", "data-ved=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "unselectable=\"", "<!--", "ID=\"", "style=\"" , "class=\"" , "//<![CDATA[" , "border=\"" , "cellspacing=\"" , "cellpadding=\"" , "target=\"" , "colspan=\"" , "onclick=\"" , "align=\"" , "color=\"" , "nowrap=\"" , "vspace=\"" , "href=\"" , "src=\"", "<cite"  , "{\"", "<g-img"  , "<a data-"   },
        //                {  "\""            , "\""      , "\""          , "\""         , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""             , "-->" , "\""   , "\""       , "\""       , "//]]>"       , "\""        , "\""             , "\""             , "\""        , "\""         , "\""         , "\""       , "\""       , "\""        , "\""        , "\""      , "\""    , "</cite>", "}"  , "</g-img>", "</a>"       }

        // вырезает между указанными тегами
        private static string ParseTags(string g, string[,] tags)
        {
            if (g.Length < 1)
            {
                return "";
            }
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
            return g;
        }

        // парсинг страницы со списком игр
        public static string ParsingPageListGames(string g)
        {
            g = ParseTags(g, tags4list);
            g = g.Replace("&nbsp;", " ").Replace("&quot;", "\"").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            return g;
        }

        // получаем список игр МШ текущего игрока
        // вход - ид
        // выход - список спиков из урл, номера, названия игр
        public static List<List<string>> GetGames(string id)
        {
            List<List<string>> res = new List<List<string>>();
            userid = id;
            string url1 = "http://game.en.cx/UserDetails.aspx?zone=1&tab=1&uid=" + userid + "&page=1";
            string cookieHeader = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url1);
            req.CookieContainer = cCont;
            req.ContentType = "application/x-www-form-urlencoded";
            string pageSource = "";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                cookieHeader = resp.Headers["Set-cookie"];
                cHead = cookieHeader;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) { pageSource = sr.ReadToEnd(); }
            }
            catch
            {
                Log.Write("en.cx ERROR: не удалось получить перечень игр");
                return res;
            }
            int it1 = pageSource.IndexOf("VirtualGamesDescription");
            if (it1 == -1)
            {
                Log.Write("en.cx ERROR: не удалось выполнить парсинг страницы с играми пользвоателя, не нашли текст 'VirtualGamesDescription'");
                return res;
            }
            string ps1 = pageSource.Substring(it1);
            it1 = ps1.IndexOf("QuizDescription");
            if (it1 == -1)
            {
                Log.Write("en.cx ERROR: не удалось выполнить парсинг страницы с играми пользвоателя, не нашли текст 'QuizDescription'");
                return res;
            }
            ps1 = ps1.Substring(0, it1);
            ps1 = ParsingPageListGames(ps1);
            if (ps1.Length < 1) {
                Log.Write("en.cx ERROR: не удалось выполнить парсинг страницы с играми пользвоателя");
                return res;
            }

            string[] ar1 = System.Text.RegularExpressions.Regex.Split(ps1.Replace(" bg>", "").Replace("\r\n", " ").Replace("</tr> ", "").Replace("</td> ", ""), "<tr");
            List<string> l1 = new List<string>();
            
            foreach (string s1 in ar1)
            {
                if (s1.IndexOf("/Teams/TeamDetails.aspx") != -1)
                {
                    l1.Add(s1.Replace(" >", ">").Replace("<span>", " ").Replace("</span>", " ").Replace("<br />", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " "));
                }
            }
            foreach (string s2 in l1)
            {
                string r_url = "";
                string r_name = "";
                string r_num = "";
                bool r_flag = true;
                string[] ar2 = System.Text.RegularExpressions.Regex.Split(s2, "<td>");
                for (int i = 0; i < ar2.Length; i++)
                {
                    if (ar2[i].Length < 5) { continue; }
                    if (ar2[i].Trim().Substring(ar2[i].Trim().Length - 5, 5) == "Место") { r_flag = false; break; }
                    if (ar2[i].Trim()[0] == '#') { r_num = ar2[i]; }
                    if (ar2[i].IndexOf("<a href=\"") != -1)
                    {
                        string q1 = ar2[4].Substring(0, ar2[4].IndexOf("</a>")).Replace("<a href=\"", "");
                        r_url = q1.Substring(0, q1.IndexOf("\">"));
                        r_name = q1.Substring(q1.IndexOf("\">") + 2);
                    }
                }
                if (r_flag) {
                    List<string> l2 = new List<string>();
                    l2.Add(r_url.Trim());
                    l2.Add(r_num.Trim());
                    l2.Add(r_name.Trim());
                    res.Add(l2);
                }
            }
            return res;
        }

        // установить полученные в форме параметры
        public static void SetId(string s1, string s2, string s3, string s4, string s5, int i1)
        {
            userid = s1;
            username = s2;
            password = s3;
            gameid = s4;
            gamedomain = s5;
            levels = i1;
            isReady = true;
        }
        // получает страницу по урлу
        public static string get_game_page(string url)
        {
            string ps = "";
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            //getRequest.Headers.Add("Accept-Language", "ru-ru,ru");
            //getRequest.Headers.Add("Content-Language", "ru-ru,ru");
            ////getRequest.Headers.Set("Accept-Charset", "utf-8");
            ////getRequest.Headers.Set("Accept-Encoding", "utf-8");
            ////getRequest.Headers.
            ////Accept-Charset
            ////Accept-Encoding
            //getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
            getRequest.CookieContainer = cCont;
            WebResponse getResponse = getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                ps = sr.ReadToEnd();
            }
            return ps.ToLower();
        }
    }
}
