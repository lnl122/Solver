using System;
using System.Collections.Generic;

namespace Solver
{
    class Words
    {
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
        public List<string> all_base;  // все слова из найденных, приведенную в базовую форму, ранжированные по частоте
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
            all_assoc = new List<string>();

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
                en_trans.AddRange(TranslateEnRu(en));
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
            all_base = KillDupesAndRange(FindBaseWord(lt));

            // найдем ассоциации ко всем базовым словам, уберем дупы
            all_assoc = KillDupesAndRange(Associations.Get(all_base));

            // объект создан, все счастливо танцую и поют, как в индийских фильмах
        }

        // убиваем дупы и ранжирум по сущ/прил/глаг и частоте
        // вход - список слов на русском
        // выход - базовые слова
        private static List<string> KillDupesAndRange(List<string> lst)
        {
            List<string> res = new List<string>();

            return res;
        }

        // из списка слов находим базовые
        // вход - список слов на русском
        // выход - базовые слова
        private static List<string> FindBaseWord(List<string> lst)
        {
            List<string> res = new List<string>();

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
                Log.Write("g_tra ERROR: не удалось выполнить перевод текста '" + s1 + "'");
            }
            if (re1 == "") { return res; }
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
