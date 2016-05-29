using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;

namespace Solver
{
    class Program
    {
        public static Form Mainform;       // форму объявим глобально
        public static TabControl Tabs;

        public static string mainform_caption = "Solver..";     // имя формы

        public static int mainform_border = 5;      // расстояния между элементами форм, константа

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
            d.temp_path = Environment.GetEnvironmentVariable("TEMP");
            d.self_name = Process.GetCurrentProcess().MainModule.ModuleName;
            d.log_pathfilename = d.local_path + "\\" + d.self_name + ".log";
            d.self_date = File.GetCreationTime(Process.GetCurrentProcess().MainModule.FileName).ToString();
            Program.logfile = new StreamWriter(File.AppendText(d.log_pathfilename).BaseStream);
            Program.logfile.AutoFlush = true;
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
            // проверка орфографии установлена?
            // ???
            // 2do

            // проверка открытия web-ресурсов
            WebClient wc1 = null;
            try { wc1 = new WebClient(); }                                  catch { Log("Не удалось создать объект WebClient");             return false; }
            string re1 = "";
            try { re1 = wc1.DownloadString("http://image.google.com/"); }   catch { Log("ERROR: http://image.google.com/ не открывается");  return false; }
            try { re1 = wc1.DownloadString("http://game.en.cx/"); }         catch { Log("ERROR: http://game.en.cx/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://jpegshare.net/"); }      catch { Log("ERROR: http://jpegshare.net/ не открывается");     return false; }
            try { re1 = wc1.DownloadString("http://goldlit.ru/"); }         catch { Log("ERROR: http://goldlit.ru/ не открывается");        return false; }
            try { re1 = wc1.DownloadString("http://sociation.org/"); }      catch { Log("ERROR: http://sociation.org/ не открывается");     return false; }
            Log("Все необходимые web-ресурсы открываются успешно");

            // все проверки пройдены
            return true;
        }

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
            //List<string> all_games;
            public string game_id;
            public string domain;
        }
        public struct MainTabSt
        {
            public TabPage MainTab;
            public Button BtnUser;
            public Button BtnGame;
            public ListBox LvlList;
            public TextBox LvlText;
        }

        static dEnvInfo Env = new dEnvInfo();
        public static StreamWriter logfile;
        static GameSt dGame = new GameSt();
        static MainTabSt GameTab = new MainTabSt();

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
        }
        public static void Event_BtnUserClick(object sender, EventArgs e)
        {
            // нужная ветка реестра д.б. в HKCU - //[HKEY_CURRENT_USER\Software\lnl122\solver] //"user"="username" //"pass"="userpassword"

            // обратимся к реестру, есть ли там записи о последнем юзере, если есть - прочтем их
            // предложим ввести юзера и пароль, дефолтные значения - то, что было в реестре, или же пусто
            // если отказались вводить имя/пасс - выходим
            // попробуем авторизоваться на гейм.ен.цх с указанной УЗ
            // если не успешно - вернемся в вводу пользователя
            // если авторизовались успешно - записываем данные в реестр, меняем заголовок программы, делаем доступной кнорпку выбора игры
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
            GameTab.MainTab.Controls.Add(GameTab.BtnGame);
            GameTab.LvlList = new ListBox();
            GameTab.LvlList.Items.Add("-: текст уровня пользователя");
            GameTab.MainTab.Controls.Add(GameTab.LvlList);
            GameTab.LvlText = new TextBox();
            GameTab.LvlText.Text = "Для пользовательского уровня укажите текст задания, или ссылки на картинки\r\n\r\nДля выбора задания игры необходимо выбрать уровень в списке слева\r\n";
            GameTab.LvlText.AcceptsReturn = true;
            GameTab.LvlText.AcceptsTab = false;
            GameTab.LvlText.Multiline = true;
            GameTab.LvlText.ScrollBars = ScrollBars.Both;
            GameTab.MainTab.Controls.Add(GameTab.LvlText);
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
