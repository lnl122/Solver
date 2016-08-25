// *** добавить в проверки при старте полный цикл: картинка (чебурашка) распознавание + ассоциации = проверяемый результат (гена) - получение гарантии работы цикла
// *** создать словарь слов, на которых не найдено ассоциаций, и, при проверке сразу не проверять их - экономия времени
// *** добавить несколько сервисов аплоада картинок 5-6 штук. при отказе работать к-либо - переходить на следующий
// *** в Word. сделать подборку 1) первых 10 существительных наиболее частых. 2) 3-5 групп слов в порядке приоритетности (топ 10 сущ/все сущ+найденные/ассоциации), готовых для использования
// *** из всех модулей вынести констатны и текстовые константы в статические переменные каждого модуля

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Net;

namespace Solver
{
    class Program
    {

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
            string type = get_task_type_by_name(gChoice.SelectedItem.ToString());
            if (type == "raschl")
            {
                var R1 = new Raschl(LvlList.SelectedIndex, LvlText.Text);
            }
            if (type == "picture")
            {
                var R1 = new Picture(LvlList.SelectedIndex, get_list_of_urls_from_text(LvlText.Text.ToString()));
            }
            //if (type == "picture_association")
            //{
            //    var R1 = new Association(LvlList.SelectedIndex, get_list_of_urls_from_text(LvlText.Text.ToString()));
            //}
            if (type == "logogrif")
            {
                var R1 = new Logogrif(LvlList.SelectedIndex, get_list_of_urls_from_text(LvlText.Text.ToString()));
            }
            if (type == "metagramm")
            {
                var R1 = new Metagramm(LvlList.SelectedIndex, get_list_of_urls_from_text(LvlText.Text.ToString()));
            }
            if (type == "gybrid")
            {
                var R1 = new Gybrid(LvlList.SelectedIndex, get_list_of_urls_from_text(LvlText.Text.ToString()));
            }
            if (type == "gapoifika_books")
            {
                var R1 = new GapoifikaBooks(LvlList.SelectedIndex, LvlText.Text);
            }
            //
        }*/


        // инициализируем наши объекты
        public static void InitComponents()
        {
            string localpath = Environment.CurrentDirectory + @"\Data\";
            SpellChecker.Init();
            SpellChecker.LoadDictionary(localpath + "SpChDict.dat");
            Associations.Init();
            Associations.LoadDictionary(localpath + "AssocDict.dat");
            Associations.LoadDictionaryBad(localpath + "AssocDictBad.dat");
        }

        // завершаем работы наших объектов
        public static void CloseComponents()
        {
            SpellChecker.SaveDictionary();
            Associations.SaveDictionary();
            Associations.SaveDictionaryBad();
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
            if (!CheckComponents())
            {
                System.Windows.Forms.MessageBox.Show("Не все необхдимые компоненты установлены на ПК.\r\nПроверьте лог-файл.");
                return;
            }
            // инициализируем наши собственные компоненты
            InitComponents();

            // создаём форму, передаём её управление
            var MF = new MainForm();
            System.Windows.Forms.Application.Run(MF.MF);

            //var tt = Upload.UploadFile_saveimgru(@"C:\1\34\pics\g24889_l2_p1_n1.jpg");

            // закругляемся
            CloseComponents();
            Log.Write("Выход из программы..");
            Log.Close();
        }
    }
}
