using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;

namespace HotkeyList
{

    public partial class Form1 : Form
    {
        /// Win32API の extern 宣言クラス
        public class WinAPI
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        }


        private BindingSource bindingSource1;
        private DataSet dataset1;

        private const string DefinitionFolder = "Lists";


        public Form1()
        {
            InitializeComponent();

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // 最大化ボタンの非表示
            this.MaximizeBox = !this.MaximizeBox;

            // タスクバーに表示しない
            this.ShowInTaskbar = false;


            this.dataGridView1.RowTemplate.Height = 24;

            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


            this.components = new System.ComponentModel.Container();
            this.bindingSource1 = new BindingSource(this.components);
            //this.dataGridView1 = new DataGridView();
            this.dataset1 = new DataSet();
            this.components.Add(this.dataGridView1);
            this.components.Add(this.dataset1);
            this.dataGridView1.DataSource = this.bindingSource1;
            this.Controls.Add(this.dataGridView1);

            timer1.Enabled = true;

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.TopMost = true;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // トレイリストのアイコンを非表示にする
            notifyIcon1.Visible = false;
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            // 現在の状態が最小化の状態であれば通常の状態に戻す
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            // フォームをアクティブにする
            this.Activate();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            // アクティブなウィンドウハンドルの取得
            IntPtr hWnd = WinAPI.GetForegroundWindow();
            int id;
            // ウィンドウハンドルからプロセスIDを取得
            WinAPI.GetWindowThreadProcessId(hWnd, out id);
            Process process = Process.GetProcessById(id);


            if (process != null)
            {
                if (lblApp.Text != process.ProcessName && process.ProcessName != "HotkeyList")
                {
                    lblApp.Text = process.ProcessName;

                    SetList(process.ProcessName);

                }
            }
            else
            {
                lblApp.Text = "none";
            }

            DateTime dt = DateTime.Now;
            lblDate.Text = dt.ToString("M/dd");
            lblTime.Text = dt.ToString("HH:mm");

        }

        private void SetList(string ProcName)
        {
            string path;
            string path_default;
            path = Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DefinitionFolder + @"\" + ProcName + ".xml";
            path_default = Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DefinitionFolder + @"\Default.xml";

            this.dataset1.Clear();

            if (System.IO.File.Exists(path))
            {
                this.dataset1.ReadXml(path);
                if (System.IO.File.Exists(path_default))
                {
                    this.dataset1.ReadXml(path_default);
                }
                this.bindingSource1.DataSource = dataset1.Tables[0];
                this.dataGridView1.CurrentCell = null;
            }
            else
            {
                if (System.IO.File.Exists(path_default))
                {
                    this.dataset1.ReadXml(path_default);
                    this.bindingSource1.DataSource = dataset1.Tables[0];
                    this.dataGridView1.CurrentCell = null;
                }
                else
                {
                    this.bindingSource1.DataSource = null;
                }


            }


            while (dataset1.Tables[0].Rows.Count < 20)
            {
                dataset1.Tables[0].Rows.Add();
            }



        }



        private void CrateXML(string fileName)
        {
            StreamWriter sw = File.CreateText(fileName);
            sw.WriteLine("<Table>");
            sw.WriteLine("  <Hotkey>");
            sw.WriteLine("    <Action></Action>");
            sw.WriteLine("    <Key></Key>");
            sw.WriteLine("    <Send></Send>");
            sw.WriteLine("  </Hotkey>");
            sw.WriteLine("</Table>");
            sw.WriteLine("");
            sw.WriteLine("<!-- SendはSendKeysへ送る引数を記述 -->");
            sw.WriteLine("<!-- Ctrl : ^ -->");
            sw.WriteLine("<!-- Shift : + -->");
            sw.WriteLine("<!-- Alt : % -->");
            sw.WriteLine("<!-- 例）Ctrl + Shift + N : ^+(n) -->");
            sw.WriteLine("<!-- ※ Winキーは送信不可 -->");
            sw.Close();


        }

        private void lblApp_DoubleClick(object sender, EventArgs e)
        {
            string path;
            path = Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DefinitionFolder + @"\" + lblApp.Text + ".xml";

            if (!System.IO.File.Exists(path))
            {
                CrateXML(path);
            }

            Process ps = new Process();
            ps.StartInfo.FileName = path;
            ps.StartInfo.UseShellExecute = true;
            ps.Start();

        }

        private void lblDate_Click(object sender, EventArgs e)
        {

            string path;
            path = Path.GetDirectoryName(Application.ExecutablePath) + @"\" + DefinitionFolder + @"\Default.xml";

            if (!System.IO.File.Exists(path))
            {
                CrateXML(path);
            }

            Process ps = new Process();
            ps.StartInfo.FileName = path;
            ps.StartInfo.UseShellExecute = true;
            ps.Start();

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string Key_String;

            Key_String = dataGridView1.CurrentRow.Cells[2].Value.ToString();

            if (Key_String != "")
            {
                
                
                //Alt + TAB でターゲットウィンドウをアクティブに
                SendKeys.Send("%{Tab}");

                // アクティブなウィンドウハンドルの取得
                IntPtr hWnd = WinAPI.GetForegroundWindow();
                // ウィンドウハンドルからプロセスIDを取得
                WinAPI.GetWindowThreadProcessId(hWnd, out int id);
                Process process = Process.GetProcessById(id);


                int LoopCount = 0;
                while (lblApp.Text != process.ProcessName && LoopCount < 20){


                    // アクティブなウィンドウハンドルの取得
                    hWnd = WinAPI.GetForegroundWindow();

                    // ウィンドウハンドルからプロセスIDを取得
                    WinAPI.GetWindowThreadProcessId(hWnd, out id);
                    process = Process.GetProcessById(id);

                    LoopCount++;
                    Thread.Sleep(200);

                }

                Debug.Print(LoopCount.ToString());

                if (LoopCount >= 20)
                {
                    MessageBox.Show("画面制御が正しくできませんでした。","エラー");
                }


                //キーの送信
                SendKeys.Send(Key_String);


            }



        }

        private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {

            dataGridView1.Rows[e.RowIndex].Selected = true;


        }

        private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {

            dataGridView1.ClearSelection();

        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {

            // フォームの幅自動設定
            this.Width = this.dataGridView1.Columns[0].Width + this.dataGridView1.Columns[1].Width + this.dataGridView1.Columns[2].Width + 18;


        }

    }
}