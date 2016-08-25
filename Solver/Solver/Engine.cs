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
        public static bool isReady = false;

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
        }

        public static level[] L;

        public static string GetPageLevel(int idx)
        {
            string url = "http://" + gamedomain + "/gameengines/encounter/play/" + gameid + "/?level=" + idx.ToString();
            return get_game_page(url);
        }

        public static void GetLevels()
        {
            L = new level[levels+1];
            level lev = new level();
            lev.number = 0;
            lev.name = "пользовательский уровень *";
            lev.page = "Для пользовательского уровня укажите текст задания, или ссылки на картинки\r\n\r\nДля выбора задания игры необходимо выбрать уровень в списке слева\r\n\r\nhttp://d2.endata.cx/data/games/24889/test_pic_1_16.jpg\r\n";
            lev.text = lev.page;
            lev.isClose = false;
            lev.answers_bad = new List<string>();
            lev.answers_good = new List<string>();
            lev.sectors = 1;
            lev.bonuses = 1;
            lev.sector = new string[1];
            lev.bonus = new string[1];
            lev.ursl = new List<string>();
            lev.ursl.Add("http://d2.endata.cx/data/games/24889/test_pic_1_16.jpg");
            L[0] = lev;
            for(int i=1; i<=levels; i++)
            {
                lev = new level();
                lev.number = i;
                lev.name = "пользовательский уровень *" + i.ToString();
                lev.page = GetPageLevel(i);
                //lev.text = lev.page;
                lev.isClose = false;
                lev.answers_bad = new List<string>();
                lev.answers_good = new List<string>();
                //lev.sectors = 1;
                //lev.bonuses = 1;
                //lev.sector = new string[1];
                //lev.bonus = new string[1];
                //lev.ursl = new List<string>();
                //lev.ursl.Add("http://d2.endata.cx/data/games/24889/test_pic_1_16.jpg");
                L[i] = lev;
            }

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
        //                {  "onmousedown=\"", "value=\"", "data-jiis=\"", "data-ved=\"", "aria-label=\"", "jsl=\"", "id=\"", "data-jibp=\"", "role=\"", "jsaction=\"", "onload=\"", "alt=\"", "title=\"", "width=\"", "height=\"", "data-deferred=\"", "aria-haspopup=\"", "aria-expanded=\"", "<input", "tabindex=\"", "tag=\"", "aria-selected=\"", "name=\"", "type=\"", "action=\"", "method=\"", "autocomplete=\"", "aria-expanded=\"", "aria-grabbed=\"", "data-bucket=\"", "aria-level=\"", "aria-hidden=\"", "aria-dropeffect=\"", "topmargin=\"" , "margin=\"", "data-async-context=\"", "valign=\"", "data-async-context=\"", "unselectable=\"", "<!--", "ID=\"", "style=\"" , "class=\"" , "//<![CDATA[" , "border=\"" , "cellspacing=\"" , "cellpadding=\"" , "target=\"" , "colspan=\"" , "onclick=\"" , "align=\"" , "color=\"" , "nowrap=\"" , "vspace=\"" , "href=\"" , "src=\"", "<cite"  , "{\"", "<g-img"  , "<a data-"   },
        //                {  "\""            , "\""      , "\""          , "\""         , "\""           , "\""    , "\""   , "\""          , "\""     , "\""         , "\""       , "\""    , "\""      ,"\""       , "\""       , "\""              , "\""              , "\""              , ">"     , "\""         , "\""    , "\""              , "\""     , "\""     , "\""       , "\""       , "\""             , "\""              , "\""             , "\""            , "\""           , "\""            , "\""                , "\""           , "\""       , "\""                   , "\""       , "\""                   , "\""             , "-->" , "\""   , "\""       , "\""       , "//]]>"       , "\""        , "\""             , "\""             , "\""        , "\""         , "\""         , "\""       , "\""       , "\""        , "\""        , "\""      , "\""    , "</cite>", "}"  , "</g-img>", "</a>"       }

        public static string ParsingPageListGames(string g)
        {
            if (g.Length < 1)
            {
                return "";
            }
            var tags = tags4list;
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
        public static string get_game_page(string url)
        {
            string ps = "";
            HttpWebRequest getRequest = (HttpWebRequest)WebRequest.Create(url);
            //getRequest.Headers.Add("Accept-Language", "ru-ru");
            //getRequest.Headers.Add("Content-Language", "ru-ru");
            //getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";
            getRequest.CookieContainer = cCont;
            WebResponse getResponse = getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                ps = sr.ReadToEnd();
            }
            return ps;
        }
    }
}
