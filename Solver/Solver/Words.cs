using System;
using System.Collections.Generic;

namespace Solver
{
    //
    // public Words Words(string) - construct
    //
    class Words
    {
        // максимальное количество попыток чтения
        private static int MaxTryToReadPage = 3;
        // на сколько миллисекунд засыпать при неудачном одном чтении
        private static int TimeToSleepMs = 1000;

        public static string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox/40.1";

        private static string[] badwrds = { "на", "для", "из", "по", "как", "не", "от", "что", "это", "или", "вконтакте", "review", "png", "the",
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

        public string src;             // строка со словами, которую нужно разобрать
        //public List<string> srclst;    // список, который нужно разобрать
        public List<string> ru;        // слова только из русских букв
        public List<string> ru_check;  // русские слова после орфографии
        public List<string> en;        // слова только из английских букв
        public List<string> en_trans;  // переведенные английские слова
        public List<string> all_find;  // собранные слова без дубликатов в оригинале (ворд_ру + енг_перевод), ранжированные по частоте
        public List<string> f_b_noun;      // сущ
        public List<string> f_b_adjective; // прил
        public List<string> f_b_verb;      // глагол
        public List<string> f_b_others;    // прочие
        public List<string> all_base;  // все слова из найденных, приведенную в базовую форму, ранжированные по частоте
        public List<string> all_base10;  // топовых 10 слов, из найденных, приведенную в базовую форму, ранжированные по частоте
        public List<string> all_assoc; // ассоциации к найденным словам, все подряд

        public Words(string str)
        {
            // создаем части объектов. пока что пустые.
            src = str;
            ru = new List<string>();
            ru_check = new List<string>();
            en = new List<string>();
            en_trans = new List<string>();
            all_find = new List<string>();
            all_base = new List<string>();
            all_base10 = new List<string>();
            all_assoc = new List<string>();
            f_b_noun = new List<string>();
            f_b_adjective = new List<string>();
            f_b_verb = new List<string>();
            f_b_others = new List<string>();

            // уберем грязные слова
            foreach (string s1 in badwrds)
            {
                str = str.Replace(" " + s1 + " ", " ");
            }
            str = str.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            string[] ar = str.Split(' ');

            // разберем слова по языкам - ру или ен.
            foreach (string ss in ar)
            {
                if (ss.Length > 1)
                {
                    bool eng = false, rus = false, oth = false;
                    for (int i = 0; i < ss.Length; i++)
                    {
                        char c = ss[i];
                        if ((c >= 'a') && (c <= 'z')) {eng = true;  }
                        else if (((c >= 'а') && (c <= 'я')) || (c == 'ё')) {  rus = true;  }
                             else {  oth = true; } // буквоцыфры и незнакомые языки выбросим
                    }
                    if (!rus && eng && !oth) { en.Add(ss); }
                    if (rus && !eng && !oth) { ru.Add(ss); }
                }
            }

            // переведем en, проверим орфографию у ru
            if (en.Count > 0)
            {
                List<string> temp2 = TranslateEnRu(en);
                var spch = new SpellChecker();
                en_trans.AddRange(spch.Check(temp2));
                spch.Close();
            }
            if (ru.Count > 0)
            {
                var spch = new SpellChecker();
                ru_check.AddRange(spch.Check(ru));
                spch.Close();
            }

            // соберем вместе результат
            all_find.AddRange(ru_check);
            all_find.AddRange(en_trans);
            List<string> lt = new List<string>(all_find);

            // убирем дупы, ранжируем. источник - lt
            all_find = KillDupesAndRange(lt);

            // найдем базовые слова, уберем дупы, ранжируем по виду части речи, ранжируем по частоте
            List<string>[] temp1 = FindBaseWord(lt);
            f_b_noun = temp1[0];
            f_b_adjective = temp1[1];
            f_b_verb = temp1[2];
            f_b_others = temp1[3];
            // выберем в базовые только существительные. *** возможно позже будет нужно и прилагательные - надо замерить эффеткивность
            all_base = KillDupesAndRange(f_b_noun);
            all_base10 = KillDupesAndRange(f_b_noun, 10);

            // найдем ассоциации ко всем базовым словам, уберем дупы
            //all_assoc = KillDupesAndRange(Associations.Get(all_base));//вынесено  ниже в отдельный метод

            // объект создан, все счастливо танцую и поют, как в индийских фильмах
        }

        // для объекта находит ассоциации, если они необходимы пользователю. с внешнего сервиса - 10 минут/0% проца, локально - 2 мин/одно ядро
        public void FindAssociations()
        {
            var ss = Associations.Get(all_base);
            all_assoc = KillDupesAndRange(ss);
        }

        // убиваем дупы и ранжирум по частоте
        // вход - список слов
        // выход - базовые слова
        private static List<string> KillDupesAndRange(List<string> lst, int cnt = 99999)
        {
            int mm = 0;
            if(cnt != 99999) { mm = 1; }
            List<string> res = new List<string>();
            if (lst.Count == 0)
            {
                return res;
            }
            List<string> lst2 = new List<string>();
            List<int> idx2 = new List<int>();
            foreach(string str in lst)
            {
                int idx = lst2.IndexOf(str);
                if (idx == -1)
                {
                    lst2.Add(str);
                    idx2.Add(1);
                }
                else
                {
                    idx2[idx]++;
                }
            }
            int m = 0;
            foreach(int ix in idx2)
            {
                if(ix > m)
                {
                    m = ix;
                }
            }
            int l = lst2.Count;
            for(int i=m; i>mm; i--)
            {
                for(int j=0; j<l; j++)
                {
                    if(idx2[j] == i)
                    {
                        res.Add(lst2[j]);
                    }
                }
                if (res.Count >= cnt)
                {
                    break;
                }
            }
            return res;
        }

        // из списка слов находим базовые
        // вход - список слов на русском
        // выход - массив базовых слова
        private static List<string>[] FindBaseWord(List<string> lst)
        {
            List<string> rl1 = new List<string>();
            List<string> rl2 = new List<string>();
            List<string> rl3 = new List<string>();
            List<string> rl4 = new List<string>();
            List<string>[] res = new List<string>[4];
            res[0] = rl1; // сущ
            res[1] = rl2; // прил
            res[2] = rl3; // глаг
            res[3] = rl4; // проч
            if (lst.Count == 0)
            {
                return res;
            }
            string v = "индульгенция";
            foreach(string v3 in lst)
            {
                v = v + " " + v3;
            }
            string v2 = "";
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
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
            System.Net.WebClient cl = new System.Net.WebClient();
            cl.Encoding = System.Text.Encoding.UTF8;
            string re = "";
            bool isNeedReadPage = true;
            int CountTry = 0;
            while (isNeedReadPage)
            {
                try
                {
                    re = cl.DownloadString(v2);
                    isNeedReadPage = false;
                }
                catch
                {
                    System.Threading.Thread.Sleep(TimeToSleepMs);
                    CountTry++;
                    if (CountTry == MaxTryToReadPage)
                    {
                        Log.Write("words ERROR: не удолось получить базовые слова", v2, v.Replace("индульгенция ", ""));
                        Log.Store("words", re);
                        re = "";
                        isNeedReadPage = false;
                    }
                }
            }
            cl.Dispose();
            if (re == "")
            {
                Log.Write("words ERROR: длина страницы нулевая");
                return res;
            }
            Log.Store("words", re);
            int ii1 = re.IndexOf("Начальная форма");
            while (ii1 != -1)
            {
                re = re.Substring(ii1);
                re = re.Substring(re.IndexOf(":") + 1);
                string v5 = re.Substring(0, re.IndexOf("<")).ToLower().TrimEnd().TrimStart();
                re = re.Substring(re.IndexOf("Часть речи") + 1);
                re = re.Substring(re.IndexOf(":") + 1);
                string v5_s = re.Substring(0, re.IndexOf("<")).ToLower().TrimEnd().TrimStart();
                if (v5_s == "существительное")
                {
                    rl1.Add(v5);
                }
                else
                {
                    if (v5_s == "прилагательное")
                    {
                        rl2.Add(v5);
                    }
                    else
                    {
                        if (v5_s.Length >= 6)
                        {
                            if (v5_s.Substring(0, 6) == "глагол")
                            {
                                rl3.Add(v5);
                            }
                            else
                            {
                                rl4.Add(v5);
                            }
                        }
                        else
                        {
                            rl4.Add(v5);
                        }
                    }
                }
                ii1 = re.IndexOf("Начальная форма");
            }
            rl1.Remove("индульгенция");
            res[0] = rl1; // сущ
            res[1] = rl2; // прил
            res[2] = rl3; // глаг
            res[3] = rl4; // проч
            return res;
        }

        // перевод списка слов с англ на рус.
        // вход - список слов на английском
        // выход - список слов на русском
        private static List<string> TranslateEnRu(List<string> lst)
        {
            char delim = '.';
            List<string> res = new List<string>();
            if (lst.Count < 1) { return res; }
            string s1 = "";
            foreach (string ts1 in lst)
            {
                s1 = s1 + delim + " " + ts1;
            }
            s1 = s1.Substring(2);
            System.Net.WebClient wc1 = new System.Net.WebClient();
            wc1.Encoding = System.Text.Encoding.UTF8;
            wc1.Headers.Add("User-Agent", UserAgent);
            wc1.Headers.Add("Accept-Language", "ru-ru");
            wc1.Headers.Add("Content-Language", "ru-ru");
            string w2 = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair=en|ru", s1.ToLower());
            string re1 = "";
            bool isNeedReadPage = true;
            int CountTry = 0;
            while (isNeedReadPage)
            {
                try
                {
                    re1 = wc1.DownloadString(w2);
                    isNeedReadPage = false;
                }
                catch
                {
                    System.Threading.Thread.Sleep(TimeToSleepMs);
                    CountTry++;
                    if (CountTry == MaxTryToReadPage)
                    {
                        Log.Write("g_tra ERROR: не удалось выполнить перевод текста ", w2, s1);
                        Log.Store("g_tra", re1);
                        re1 = "";
                        isNeedReadPage = false;
                    }
                }
            }
            if (re1 == "")
            {
                Log.Write("g_tra ERROR: длина текста нулевая");
                return res;
            }
            Log.Store("g_tra", re1);
            int ii7 = re1.IndexOf("<span title=\"");
            while (ii7 != -1)
            {
                re1 = re1.Substring(ii7 + "<span title=\"".Length);
                re1 = re1.Substring(re1.IndexOf(">") + 1);
                string w12 = re1.Substring(0, re1.IndexOf("</span>"));//words
                string[] ar1 = w12.Split(delim);
                foreach (string w13 in ar1)
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

    }
}
