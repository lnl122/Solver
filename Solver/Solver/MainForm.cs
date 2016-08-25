using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Solver
{
    class MainForm
    {
        public Form MF;         // форму объявим глобально
        public TabControl Tabs; // глобальный Таб - тоже

        // элементы формы
        private static TabPage MainTab;
        private static Button BtnUser;
        private static Button BtnGame;
        private static Button BtnSolve;
        private static ListBox LvlList;
        private static TextBox LvlText;
        private static ComboBox gChoice;
        public static TextBox tbGname;

        private static string mainform_caption = "Solver..";    // имя формы
        public static int border = 5;                           // расстояния между элементами форм, константа

        private static string username;             // логин пользователя
        private static string password;             // пасс пользвоателя
        private static string userid;               // ид пользователя

        private static string[] g_names;
        private static string[] g_urls;

        private static string game_domain;          // домен игры
        private static string game_id;              // ид игры
        private static int game_levels;             // количество уровней

                                                    //private static string game_cHead;           // куки
                                                    //private static CookieContainer game_cCont;  // куки


        public static void Event_SelectGameFromList(object sender, EventArgs e)
        {
            ListBox l4 = (ListBox)sender;
            tbGname.Text = g_urls[l4.SelectedIndex];
        }

        /*public static void Event_LevelSelected(object sender, EventArgs e)
{
    if (LvlList.Items.Count != 1) {
        int newlvl = LvlList.SelectedIndex;
        LvlText.Text = dGame.level_text[newlvl];
    }
}*/


        public static void Event_BtnGameClick(object sender, EventArgs e)
        {
            List<List<string>> l2 = Engine.GetGames(userid);

            // форма для ввода данных
            Form SelectGame = new Form();
            SelectGame.Text = "Выбор игры..";
            SelectGame.StartPosition = FormStartPosition.CenterScreen;
            SelectGame.Width = 35 * border;
            SelectGame.Height = 25 * border;
            SelectGame.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SelectGame.AutoSize = true;
            Label la = new Label();
            la.Text = "Необходимо двойным кликом выбрать игру из списка\r\nили же ввести ссылку на игру в нижнем поле ввода\r\nи нажать 'Открыть игру'";
            la.Top = 2 * border;
            la.Left = border;
            la.Width = 100 * border;
            la.Height = 10 * border;
            SelectGame.Controls.Add(la);
            ListBox lb = new ListBox();
            lb.Top = la.Bottom + border;
            lb.Left = border;
            lb.Width = la.Width;
            lb.Height = 20 * border;

            g_urls = new string[l2.Count];
            g_names = new string[l2.Count];
            for (int i=0; i<l2.Count; i++)
            {
                List<string> l3 = l2[i];
                g_names[i] = l3[1] + " | " + l3[2];
                g_urls[i] = l3[0];
                lb.Items.Add(g_names[i]);
            }
            lb.DoubleClick += new EventHandler(Event_SelectGameFromList);
            SelectGame.Controls.Add(lb);
            tbGname = new TextBox();
            tbGname.Text = "";
            if (Environment.MachineName == "NBIT01") { tbGname.Text="http://demo.en.cx/gameengines/encounter/play/24889"; } // for TEST
            tbGname.Top = lb.Bottom + 2 * border;
            tbGname.Left = border;
            tbGname.Width = lb.Width - 24 * border;
            SelectGame.Controls.Add(tbGname);
            Button blok = new Button();
            blok.Text = "Открыть игру";
            blok.Top = tbGname.Top;
            blok.Left = tbGname.Right + 2 * border;
            blok.Width = 22 * border;
            blok.DialogResult = DialogResult.OK;
            SelectGame.AcceptButton = blok;
            SelectGame.Controls.Add(blok);

            // выберем игру
            string page = "";
            bool fl = true;
            while (fl)
            {
                if (SelectGame.ShowDialog() == DialogResult.OK)
                {
                    string url = tbGname.Text;
                    // попробуем авторизоваться в игре - сначала разберем полученную строку
                    if (url == "") { MessageBox.Show("Не выбрана игра вообще.."); continue; }
                    string url2 = url;
                    if (url2.Substring(0,7) != "http://") { MessageBox.Show("Указана не ссылка.."); continue; }
                    url2 = url.Replace("http://", "");
                    int ii1 = url2.IndexOf("/"); if (ii1 == -1) { MessageBox.Show("указан только хост.."); continue; }
                    game_domain = url2.Substring(0,ii1);
                    url2 = url2.Substring(ii1+1);
                    if(url2.IndexOf("gameengines/encounter/play/") != -1)
                    {
                        ii1 = url2.IndexOf("/?level="); if (ii1 != -1) { url2 = url2.Substring(0, ii1); }
                        game_id = url2.Substring(url2.LastIndexOf("/") + 1);
                    } else
                    {
                        if (url2.IndexOf("GameDetails.aspx?gid=") != -1) { game_id = url2.Substring(url2.LastIndexOf("=") + 1); }
                        else { MessageBox.Show("Ссылку на игру не удалось понять.."); continue; } // ни один из форматов ссылок не подошел
                    }
                    //MessageBox.Show(url + "\r\n" + dGame.game_domain + "\r\n" + dGame.game_id);
                    // если авторизовались успешно - запоминаем игру
                    string ps2 = Engine.Logon("http://" + game_domain + "/Login.aspx", username, password);
                    if (ps2.IndexOf("action=logout") != -1)
                    {
                        // прочесть игру и узнать её параметры
                        string ps3 = Engine.get_game_page("http://" + game_domain + "/GameDetails.aspx?gid=" + game_id);
                        string ps4 = Engine.ParsingPageListGames(ps3).ToLower().Replace("\r\n","");
                        ps4 = ps4.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                        ps4 = ps4.Replace(" >", ">").Replace(" <", "<").Replace("< ", "<").Replace("> ", ">");
                        string ps5 = ps4.Replace("<span>", "").Replace("</span>", "");
                        int fr = ps5.IndexOf("игра:мозговой штурм");
                        int fe = ps5.IndexOf("covering zone:brainstorm");
                        if (fr + fe < 0) { MessageBox.Show("Это не МШ.."); continue; }
                        fr = ps5.IndexOf("<td>последовательность прохождения:штурмовая</td>");
                        fe = ps5.IndexOf("<td>the levels passing sequence:storm</td>");
                        if (fr + fe < 0) { MessageBox.Show("Последовательность не штурмовая.."); continue; }
                        page = Engine.get_game_page("http://" + game_domain + "/gameengines/encounter/play/" + game_id);
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
                        if (Int32.TryParse(q_lvl, out i2)) { game_levels = i2; }
                        if (game_levels == 0) { MessageBox.Show("Не удалось определить количество уровней.."); continue; }
                        // поставим флаг выхода и заблокируем кнопку на будущее.
                        fl = false;
                        BtnGame.Enabled = false;
                        // в лог
                        Log.Write("en.cx Открыта игра " + game_id);
                        string temppath = Environment.CurrentDirectory + "\\pics";
                        if (!Directory.Exists(temppath))
                        {
                            Directory.CreateDirectory(temppath);
                        }
                    }
                    else
                    {
                        // если не успешно - вернемся в вводу пользователя
                        Log.Write("en.cx ERROR: Не удалось подключиться к " + game_domain);
                        MessageBox.Show("Не удалось подключиться к " + game_domain);
                    }
                }
                else
                {
                    // если отказались выбирать игру - выходим
                    fl = false;
                }
            } // выход только если fl = false -- это или отказ польователя в диалоге, или если нажато ОК - проверка пройдена
            // смотрим на page - если не пусто - то подключились
            if (page != "")
            {
                Engine.SetId(userid, username, password, game_id, game_domain, game_levels);
                Engine.GetLevels();
                foreach(Engine.level lev in Engine.L)
                {
                    LvlList.Items.Add(lev.name);
                }
            }
            /*
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
                    LvlList.Items.Add(t2);

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
            }*/
        }



        // создаём основную форму приложения
        public MainForm()
        {
            MF = new Form();
            MF.Size = new Size(System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width / 2, System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height / 2);
            MF.Text = mainform_caption;
            MF.StartPosition = FormStartPosition.CenterScreen;
            MF.AutoSizeMode = AutoSizeMode.GrowOnly;
            MF.SizeChanged += new EventHandler(Event_MainFormChangeSize);
            Tabs = new TabControl();
            MF.Controls.Add(Tabs);
            MainTab = new TabPage();
            MainTab.Text = "Игра";
            Tabs.Controls.Add(MainTab);
            BtnUser = new Button();
            BtnUser.Text = "Логон в EN";
            BtnUser.Click += new EventHandler(Event_BtnUserClick);
            MainTab.Controls.Add(BtnUser);
            BtnGame = new Button();
            BtnGame.Text = "Выбор игры";
            BtnGame.Enabled = false;
            BtnGame.Click += new EventHandler(Event_BtnGameClick);
            MainTab.Controls.Add(BtnGame);
            LvlList = new ListBox();
            LvlList.Items.Add("0: текст уровня пользователя");
            //LvlList.Click += new EventHandler(Event_LevelSelected);
            MainTab.Controls.Add(LvlList);
            LvlText = new TextBox();
            LvlText.Text = "Для пользовательского уровня укажите текст задания, или ссылки на картинки\r\n\r\nДля выбора задания игры необходимо выбрать уровень в списке слева\r\n\r\nhttp://d2.endata.cx/data/games/24889/test_pic_1_16.jpg\r\n";
            LvlText.AcceptsReturn = true;
            LvlText.AcceptsTab = false;
            LvlText.Multiline = true;
            LvlText.ScrollBars = ScrollBars.Both;
            MainTab.Controls.Add(LvlText);

            gChoice = new ComboBox();
            //for (int i = 0; i < (actions.Length / 2); i++) { gChoice.Items.Add(actions[i, 0]); }
            //gChoice.SelectedIndex = 0;
            MainTab.Controls.Add(gChoice);
            BtnSolve = new Button();
            BtnSolve.Text = "Запустить решалку";
            //BtnSolve.Click += new EventHandler(Event_SolveLevel);
            MainTab.Controls.Add(BtnSolve);

            Event_MainFormChangeSize(null, null);
        }


        // ивент на кнопку логона
        // логин и пасс сохраняем в реестре
        // выолняем логон, обновляем авторизацию
        public void Event_BtnUserClick(object sender, EventArgs e)
        {
            // нужная ветка реестра д.б. в HKCU - //[HKEY_CURRENT_USER\Software\lnl122\solver] //"user"="username" //"pass"="userpassword"
            // обратимся к реестру, есть ли там записи о последнем юзере, если есть - прочтем их
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rks = rk.OpenSubKey("Software", true); rk.Close();
            RegistryKey rksl = rks.OpenSubKey("lnl122", true); if (rksl == null) { rksl = rks.CreateSubKey("lnl122"); }
            rks.Close();
            RegistryKey rksls = rksl.OpenSubKey("Solver", true); if (rksls == null) { rksls = rksl.CreateSubKey("Solver"); }
            rksl.Close();
            string user = "";
            string pass = "";
            var r_user = rksls.GetValue("user");
            if (r_user == null) { rksls.SetValue("user", ""); user = ""; } else { user = r_user.ToString(); }
            var r_pass = rksls.GetValue("pass");
            if (r_pass == null) { rksls.SetValue("pass", ""); pass = ""; } else { pass = r_pass.ToString(); }
            rksls.Close();

            // форма для ввода данных
            Form Login = new Form();
            Login.Text = "Введите ник игрока и его пароль..";
            Login.StartPosition = FormStartPosition.CenterScreen;
            Login.Width = 35 * border;
            Login.Height = 25 * border;
            Login.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Login.AutoSize = true;
            Label lu = new Label();
            lu.Text = "ник:";
            lu.Top = 2 * border;
            lu.Left = border;
            lu.Width = 10 * border;
            Login.Controls.Add(lu);
            Label lp = new Label();
            lp.Text = "пароль:";
            lp.Top = lu.Bottom + border;
            lp.Left = border;
            lp.Width = lu.Width;
            Login.Controls.Add(lp);
            TextBox tu = new TextBox();
            tu.Text = user;
            tu.Top = lu.Top;
            tu.Left = lu.Right + border;
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
            blok.Top = lp.Bottom + 2 * border;
            blok.Left = lu.Left;
            blok.Width = tu.Right - 1 * border;
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
                    Log.Write("Пробуем выполнить вход на сайт для пользвоателя " + user);
                    string pageSource = Engine.Logon("http://game.en.cx/Login.aspx", user, pass);
                    // если авторизовались успешно - записываем данные в реестр, меняем заголовок программы, делаем доступной кнорпку выбора игры
                    if (pageSource.IndexOf("action=logout") != -1)
                    {
                        // обновить в реестре 
                        RegistryKey rk2 = Registry.CurrentUser.OpenSubKey("Software\\lnl122\\Solver", true);
                        rk2.SetValue("user", user);
                        rk2.SetValue("pass", pass);
                        rk2.Close();
                        // включим кнопку игры
                        BtnGame.Enabled = true;
                        BtnUser.Enabled = false;
                        // изменим заголовок
                        MF.Text = mainform_caption + " / user: " + user;
                        // запомним параметры игрока
                        username = user;
                        password = pass;
                        pageSource = pageSource.ToLower();
                        pageSource = pageSource.Substring(pageSource.IndexOf(user.ToLower()));
                        pageSource = pageSource.Substring(pageSource.IndexOf("(id"));
                        pageSource = pageSource.Substring(pageSource.IndexOf(">") + 1);
                        userid = pageSource.Substring(0, pageSource.IndexOf("<"));
                        // поставим флаг выхода
                        fl = false;
                        // в лог
                        Log.Write("Имя и пароль пользователя проверены, успешный логон для id=" + userid);
                    }
                    else
                    {
                        // если не успешно - вернемся в вводу пользователя
                        Log.Write("Неверные логин/пароль");
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

        // ивент на изменение размера основной формы приложения
        public void Event_MainFormChangeSize(object sender, EventArgs e)
        {
            Tabs.Top = border;
            Tabs.Left = border;
            Tabs.Width = MF.Width - 5 * border;
            Tabs.Height = MF.Height - 10 * border;
            MainTab.Left = border;
            MainTab.Top = border;
            MainTab.Width = Tabs.Width - 3 * border;
            MainTab.Height = Tabs.Height - 3 * border - 11; // почему 11? хз но работает корректно
            BtnUser.Left = border;
            BtnUser.Top = border;
            BtnUser.Width = 20 * border;
            BtnUser.Height = 5 * border;
            BtnGame.Left = BtnUser.Right + border;
            BtnGame.Top = BtnUser.Top;
            BtnGame.Width = BtnUser.Width;
            BtnGame.Height = BtnUser.Height;
            LvlList.Top = BtnUser.Bottom + border;
            LvlList.Left = border;
            LvlList.Width = MainTab.Width / 4;
            LvlList.Height = MainTab.Height / 2;
            LvlText.Top = LvlList.Top;
            LvlText.Left = LvlList.Right + border;
            LvlText.Width = MainTab.Width - LvlList.Width - 3 * border;
            LvlText.Height = MainTab.Height - BtnUser.Height - 3 * border;
            gChoice.Top = LvlList.Bottom + 2 * border;
            gChoice.Left = border;
            gChoice.Width = LvlList.Width;
            BtnSolve.Top = gChoice.Bottom + 2 * border;
            BtnSolve.Left = border;
            BtnSolve.Width = gChoice.Width;
        }

    }
}
