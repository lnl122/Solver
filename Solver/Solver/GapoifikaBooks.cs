/*using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Solver
{
    class GapoifikaBooks
    {
        Program.GapoifikaBooks_data Data;

        public List<string> GapoifikaBooks_Process(Program.GapoifikaBooks_data d) 
        {
            // решаем гапоифику по книгам в одном процессе
            string[] ar1 = Regex.Split(d.def, "\r\n");
            MessageBox.Show(ar1.Length.ToString());
            // вбиваем ответы

            Program.Mainform.BeginInvoke(new Action(() => GapoifikaBooks_Buttons_Enable(d)));
            Program.Mainform.BeginInvoke(new Action(() => GapoifikaBooks_Show_Anwers(d.answers)));

            return d.answers;
        }

        private void GapoifikaBooks_Show_Anwers(List<string> dd)
        {
            Data.BtnSolve.Enabled = false;
            Data.TextOut.Text = "";
            foreach (string wrd in dd)
            {
                Data.TextOut.Text += wrd;
                Data.TextOut.Text += "\r\n";
            }
            Data.Tab.Text = Data.Tab.Text + " #";
            Event_GapoifikaBooks_ChangeSize(null, null);
        }

        public GapoifikaBooks(int level, string def)//для только решения картинок
        {
            def = def.Trim().TrimEnd().TrimStart();
            if ((def == "") || (def == null)) { MessageBox.Show("Текст пуст. решать нечего!"); return; }
            Data.type = "GapoifikaBooks";
            Data.level = level;
            Data.def = def;
            Data.Tab = new TabPage();
            Data.Tab.Text = level.ToString() + " : " + "ГапоификаКниги";

            Data.BtnSolve = new Button();
            Data.BtnSolve.Text = "Решить";
            Data.BtnSolve.Click += new EventHandler(Event_GapoifikaBooks_Solve_Click);
            Data.Tab.Controls.Add(Data.BtnSolve);
            Data.Auto = new CheckBox();
            Data.Auto.Text = "авто-вбивать";
            Data.Auto.Checked = true;
            if (Data.level < 1)
            {
                Data.Auto.Checked = false;
                Data.Auto.Enabled = false;
            }
            Data.Tab.Controls.Add(Data.Auto);
            Data.BtnClose = new Button();
            Data.BtnClose.Text = "Закрыть";
            Data.BtnClose.Click += new EventHandler(Event_GapoifikaBooks_Close_Click);
            Data.Tab.Controls.Add(Data.BtnClose);
            Data.TextIn = new TextBox();
            Data.TextIn.Visible = true;
            Data.TextIn.AcceptsReturn = true;
            Data.TextIn.AcceptsTab = false;
            Data.TextIn.Multiline = true;
            Data.TextIn.ScrollBars = ScrollBars.Both;
            Data.TextIn.Text = def;
            Data.Tab.Controls.Add(Data.TextIn);
            Data.TextOut = new TextBox();
            Data.TextOut.Visible = true;
            Data.TextOut.AcceptsReturn = true;
            Data.TextOut.AcceptsTab = false;
            Data.TextOut.Multiline = true;
            Data.TextOut.ScrollBars = ScrollBars.Both;
            Data.Tab.Controls.Add(Data.TextOut);
            Event_GapoifikaBooks_ChangeSize(null, null);
            Program.Mainform.SizeChanged += new EventHandler(Event_GapoifikaBooks_ChangeSize);
            Program.Tabs.Controls.Add(Data.Tab);
            Program.Tabs.SelectTab(Program.Tabs.TabCount - 1);
        }
        private void Event_GapoifikaBooks_Close_Click(object sender, EventArgs e)
        {
            Data.Tab.Dispose();
        }
        private static void GapoifikaBooks_Buttons_Enable(Program.GapoifikaBooks_data d)                  // меняем оптом доступность кнопок
        {
            d.BtnSolve.Enabled = true;
            d.BtnClose.Enabled = true;
            if (d.level != -1) { d.Auto.Enabled = true; }
            d.TextIn.Enabled = true;
            d.TextIn.Enabled = true;
        }
        private static void GapoifikaBooks_Buttons_Disable(Program.GapoifikaBooks_data d)                  // меняем оптом доступность кнопок
        {
            d.BtnSolve.Enabled = false;
            d.BtnClose.Enabled = false;
            d.Auto.Enabled = false;
            d.TextIn.Enabled = false;
            d.TextIn.Enabled = false;
        }
        private void Event_GapoifikaBooks_ChangeSize(object sender, EventArgs e)
        {
            Data.BtnSolve.Top = MainForm.border;
            Data.BtnSolve.Left = MainForm.border;
            Data.BtnSolve.Width = 20 * MainForm.border;
            Data.BtnSolve.Height = 5 * MainForm.border;
            Data.Auto.Top = MainForm.border;
            Data.Auto.Left = Data.BtnSolve.Right + 2 * MainForm.border;
            Data.BtnClose.Top = MainForm.border;
            Data.BtnClose.Width = 20 * MainForm.border;
            Data.BtnClose.Height = 5 * MainForm.border;
            Data.BtnClose.Left = Program.MainTab.Width - Data.BtnClose.Width - MainForm.border;
            Data.TextIn.Left = Data.BtnSolve.Left;
            Data.TextIn.Width = (Program.MainTab.Width - 3 * MainForm.border)/2;
            Data.TextIn.Top = Data.BtnSolve.Bottom + 2 * MainForm.border;
            Data.TextIn.Height = Program.MainTab.Height - Data.TextIn.Top - MainForm.border;
            Data.TextOut.Left = Data.TextIn.Right + MainForm.border;
            Data.TextOut.Width = Data.TextIn.Width;
            Data.TextOut.Top = Data.TextIn.Top;
            Data.TextOut.Height = Data.TextIn.Height;
        }

        private void Event_GapoifikaBooks_Solve_Click(object sender, EventArgs e)
        {
            //проверим, все ли готово
            
            //можно стартовать процессы по собранным данным
            GapoifikaBooks_Buttons_Disable(Data);
            Program.Log("Начали решать гапоифику по книгам\r\n.\r\n");
            Task<List<string>> t1 = Task<List<string>>.Factory.StartNew(() => GapoifikaBooks_Process(Data));
        }

    }
}
*/