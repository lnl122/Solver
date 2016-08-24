// *** добавить в проверки при старте полный цикл: картинка (чебурашка) распознавание + ассоциации = проверяемый результат (гена) - получение гарантии работы цикла
// *** создать словарь слов, на которых не найдено ассоциаций, и, при проверке сразу не проверять их - экономия времени
// *** добавить несколько сервисов аплоада картинок 5-6 штук. при отказе работать к-либо - переходить на следующий
// *** в Word. сделать подборку 1) первых 10 существительных наиболее частых. 2) 3-5 групп слов в порядке приоритетности (топ 10 сущ/все сущ+найденные/ассоциации), готовых для использования


using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;

namespace Solver
{
    class Program
    {
        //public static Form Mainform;       // форму объявим глобально
        //public static TabControl Tabs;

        //public static string mainform_caption = "Solver..";     // имя формы

        //public static int mainform_border = 5;      // расстояния между элементами форм, константа
        //public static int rnd_min = 800;//1300;
        //public static int rnd_max = 1500;//3300;
        //public static bool input_busy = false;
        
        // получаем строку с версиями установленных .net
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
            return res.Trim();
        }
        // получаем строку с версией MS Word
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
        // проверяем наличие, настройки и также работу всех необходимых компонент, ведем лог
        private static bool CheckComponents()
        {
            // .NET
            string DotNetVersions = GetVersionDotNetFromRegistry();
            Log.Write("check Найденные версии .NET: " + DotNetVersions);
            if (DotNetVersions.IndexOf("v2.0") == -1) { Log.Write("check ERROR: Отсутствует .NET v2.0"); return false; }
            if (DotNetVersions.IndexOf("v3.0") == -1) { Log.Write("check ERROR: Отсутствует .NET v3.0"); return false; }
            if (DotNetVersions.IndexOf("v4.0") == -1) { Log.Write("check ERROR: Отсутствует .NET v4.0"); return false; }
            if ((DotNetVersions.IndexOf("v4.5") == -1) && (DotNetVersions.IndexOf("v4.6") == -1)) { Log.Write("check ERROR: Отсутствует .NET v4.5 или v4.6"); return false; }
            
            // MS Word
            string WordVersion = GetVersionMicrosoftWord();
            if (WordVersion == "") { Log.Write("check ERROR: Отсутствует установленный Microsoft Word"); return false; }
            int ii1 = 0;
            if (Int32.TryParse(WordVersion.Substring(0, WordVersion.IndexOf(".")), out ii1))
            {
                if (ii1 <= 11) { Log.Write("check ERROR: Версия Microsoft Word ниже 11.0, необходим Microsoft Word 2007 или более новый"); return false; }
            } else
            {
                Log.Write("check ERROR: Не удалось определить версию Microsoft Word"); return false;
            }
            Log.Write("check Найден Microsoft Word версии " + WordVersion);
            try {
                var testSC = new SpellChecker();
                if(testSC.CheckOne("мама") && testSC.CheckOne("мыла") && testSC.CheckOne("раму"))
                {
                    Log.Write("check Проверка орфографии установлена");
                }
            } catch
            {
                Log.Write("ERROR: Не удалось запустить проверку орфографии, или же проверка русского языка не установлена.."); return false;
            }

            // проверка открытия web-ресурсов
            WebClient wc1 = null;
            try { wc1 = new WebClient(); }                                  catch { Log.Write("check ERROR: Не удалось создать объект WebClient");      return false; }
            string re1 = "";
            try { re1 = wc1.DownloadString("http://image.google.com/"); }   catch { Log.Write("check ERROR: http://image.google.com/ не открывается");  return false; }
            try { re1 = wc1.DownloadString("http://game.en.cx/"); }         catch { Log.Write("check ERROR: http://game.en.cx/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://jpegshare.net/"); }      catch { Log.Write("check ERROR: http://jpegshare.net/ не открывается");     return false; }
            //try { re1 = wc1.DownloadString("http://ipic.su/"); }            catch { Log.Write("check ERROR: http://ipic.su/ не открывается");           return false; }
            try { re1 = wc1.DownloadString("http://goldlit.ru/"); }         catch { Log.Write("check ERROR: http://goldlit.ru/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://sociation.org/"); }      catch { Log.Write("check ERROR: http://sociation.org/ не открывается");     return false; }
            try { re1 = wc1.DownloadString("https://ru.wiktionary.org/"); } catch { Log.Write("check ERROR: https://ru.wiktionary.org/ не открывается"); return false; }
            Log.Write("check Все необходимые web-ресурсы открываются успешно");

            // все проверки пройдены
            return true;
        }

        /*public static string[,] actions = {
            //{ "Решать самостоятельно",      "manual" },
            { "Расчленёнки",                    "raschl" },
            { "Картинки - только решить",       "picture"},
            { "* Картинки + ассоциации *",      "picture_association"},
            { "Картинки + логогрифы СОН-СЛОН",  "logogrif"},
            { "Картинки + метаграммы КОТ-КИТ",  "metagramm"},
            { "Картинки + гибриды карТОНус",    "gybrid"},
            { "* Кубрая *",                     "kubray"},
            { "Гапоифика - названия книг",      "gapoifika_books"}

            };
*/

        /*public struct GameSt
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
        }*/
        /*public struct MainTabSt
        {
            public TabPage MainTab;
            public Button BtnUser;
            public Button BtnGame;
            public ListBox LvlList;
            public TextBox LvlText;
            public ComboBox gChoice;
            public Button BtnSolve;
        }*/

        //public static GameSt dGame = new GameSt();
        //public static MainTabSt GameTab = new MainTabSt();

        //public enum prot { none, begin1, begin2, begin3, end1, end2, end3 };


        /*public static List<Program.words> words_to_engine(List<Program.words> q, string s)
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
                if (s == "base_all") { w2 = w1.w_base_all; }
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
        }*/

        /*public static string Game_Logon(string url1, string name, string pass)
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
        }*/

        /*public static string get_game_page(string url)
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
        }*/
        /*public static string parse_level_text(string t1)
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
        }*/

        /*public static string set_word_protect(string v, int num, Program.prot p)
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
        }*/
        /*public static bool try_form_send(int lvl, string val)
        {
            if (lvl < 1) { return false; }
            if (val.Length <= 3) { return false; }
            if ( ( (val[0] >= 'a') && (val[0] <= 'z') ) || ((val[val.Length - 1] >= 'a') && (val[val.Length - 1] <= 'z')) ) { return false; }
            if ( ( (val[0] >= '0') && (val[0] <= '9')) || ((val[val.Length - 1] >= '0') && (val[val.Length - 1] <= '9'))) { return false; }
            val = val.Replace('ё','е');

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
            */
        //req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        /*req.CookieContainer = dGame.game_cCont;
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
        Log("bad_answer="+ val);
        return false;
    }*/

        /*public static void Event_SolveLevel(object sender, EventArgs e)
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
            //if (type == "picture_association")
            //{
            //    var R1 = new Association(GameTab.LvlList.SelectedIndex, get_list_of_urls_from_text(GameTab.LvlText.Text.ToString()));
            //}
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
            if (type == "gapoifika_books")
            {
                var R1 = new GapoifikaBooks(GameTab.LvlList.SelectedIndex, GameTab.LvlText.Text);
            }
            //
        }*/
        /*public static void Event_MainFormChangeSize(object sender, EventArgs e)
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
        }*/
        /*public static void Event_SelectGameFromList(object sender, EventArgs e)
        {
            ListBox l4 = (ListBox)sender;
            dGame.tb.Text = dGame.g_urls[l4.SelectedIndex];
            //Form f1 = l4.Parent;
            //f1.Close();
        }*/
        /*public static void Event_BtnUserClick(object sender, EventArgs e)
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
        }*/
        /*public static void Event_BtnGameClick(object sender, EventArgs e)
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
                        if (page.IndexOf("Ошибка. Состав вашей команды превышает") != -1) { MessageBox.Show("Состав вашей команды превышает установленный максимум.."); continue; }
                        
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
                        Log("Открыта игра " + dGame.game_id);
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
        }*/
        /*public static void Event_LevelSelected(object sender, EventArgs e)
        {
            if (GameTab.LvlList.Items.Count != 1) {
                int newlvl = GameTab.LvlList.SelectedIndex;
                GameTab.LvlText.Text = dGame.level_text[newlvl];
            }
        }*/
        /*
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
        */

        // инициализируем наши объекты
        public static void InitComponents()
        {
            string localpath = Environment.CurrentDirectory + @"\Data\";
            SpellChecker.Init();
            SpellChecker.LoadDictionary(localpath + "SpChDict.dat");
            Associations.Init();
            Associations.LoadDictionary(localpath + "AssocDict.dat");

        }

        // завершаем работы наших объектов
        public static void CloseComponents()
        {
            SpellChecker.SaveDictionary();
            Associations.SaveDictionary();
        }

        // код основной программы
        static void Main(string[] args)
        {
            // инитим лог
            Log.Init();
            Log.Write("________________________________________________________________________________");
            Log.Write("      Старт программы..");
            Log.Write("      Сборка от " + File.GetCreationTime(Process.GetCurrentProcess().MainModule.FileName).ToString());
            Log.Write("      ПК: " + Environment.MachineName);
            Log.Write("      " + System.Environment.OSVersion.VersionString + ", " + Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") + ", ver:" + Environment.Version.ToString() + ", CPU: " + Environment.ProcessorCount.ToString() + ", 64bit:" + Environment.Is64BitOperatingSystem.ToString());

            // выполняем проверки окружения
            if (!CheckComponents()) {
                MessageBox.Show("Не все необхдимые компоненты установлены на ПК.\r\nПроверьте лог-файл.");
                return;
            }
            // инициализируем наши собственные компоненты
            InitComponents();



            // ***
            //var tt = Google.UploadFile_pixicru(@"C:\1\34\pics\g24889_l2_p1_n4.jpg");
            var tt = Google.GetImageDescription(@"C:\1\34\pics\g24889_l2_p1_n4.jpg");
            tt = tt;
            // ***


            // создаём форму, передаём её управление
            //CreateMainForm();
            //System.Windows.Forms.Application.Run(Mainform);

            // закругляемся
            CloseComponents();
            Log.Write("Выход из программы..");
            Log.Close();
        }
    }
}
