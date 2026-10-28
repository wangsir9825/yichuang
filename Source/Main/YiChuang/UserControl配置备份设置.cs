using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using YiChuang.Properties;

namespace YiChuang
{
	public class UserControl配置备份设置 : UserControl
	{
        internal Action<bool> ConfigurationCommitted;
		private IContainer components = null;

		private GroupBox groupBox2;

		private Label label2;

		private Button button3;

		private Button button2;

		private Button button1;

		private TextBox txtPath;

		private Label label1;

		private Button button4;

		public UserControl配置备份设置()
		{
			((global::_61BD9B3E)_63AF42A1._71AC5D16[383])(this);
			InitializeComponent();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (!((global::_6108BC1E)_63AF42A1._71AC5D16[384])(ClassMain.Backup_Path))
			{
				((global::D8883001)_63AF42A1._71AC5D16[651])("当前文件夹路径不存在");
				return;
			}
			string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(ClassMain.Backup_Path, "\\YiChuang备份");
			if (!((global::_6108BC1E)_63AF42A1._71AC5D16[384])(text))
			{
				((global::B2980F37)_63AF42A1._71AC5D16[548])(text);
			}
			ClassMain.OpenFolder(text);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (((global::_329CCB02)_63AF42A1._71AC5D16[296])("您确定要导出备份数据吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
                string saved=Path.Combine(SettingsStore.Folder,"setting.txt");var before=File.GetLastWriteTimeUtc(saved);
				ClassMain.Export_Backup();
                if(File.GetLastWriteTimeUtc(saved)!=before){txtPath.Text=ClassMain.Backup_Path;ConfigurationCommitted?.Invoke(false);}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog folderBrowserDialog = ((global::DBAD9B83)_63AF42A1._71AC5D16[472])())
			{
				if (((global::DBA4DA3D)_63AF42A1._71AC5D16[737])(folderBrowserDialog) == DialogResult.OK)
				{
					((global::B23A488F)_63AF42A1._71AC5D16[183])(txtPath, ((global::_2A10511D)_63AF42A1._71AC5D16[489])(folderBrowserDialog));
				}
			}
		}

		private void txtPath_TextChanged(object sender, EventArgs e)
		{
			ClassMain.Backup_Path = ((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(txtPath));
		}

		private void UserControl配置备份设置_Load(object sender, EventArgs e)
		{
			((global::B23A488F)_63AF42A1._71AC5D16[183])(txtPath, ((global::E83366AE)_63AF42A1._71AC5D16[201])(ClassMain.Backup_Path));
		}

		private void button4_Click(object sender, EventArgs e)
		{
			try
			{
                using(var dialog=new OpenFileDialog{Filter="Text files (*.json)|*.json",FilterIndex=1}) {
                    if(dialog.ShowDialog(FindForm())!=DialogResult.OK)return;
                    if(new FileInfo(dialog.FileName).Length>4*1024*1024)throw new InvalidDataException("备份文件过大。");
                    string text=RSA.RSADecrypt(File.ReadAllText(dialog.FileName));
                    // Validate and persist before changing live configuration.
                    SettingsStore.ImportBackup(text);
                    ClassMain.read_setSetting(text);
                    txtPath.Text=ClassMain.Backup_Path;ConfigurationCommitted?.Invoke(true);
                    MessageBox.Show(FindForm(),"导入成功，配置已保存。","配置备份",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
			}
			catch (Exception)
			{
				MessageBox.Show(FindForm(),"导入失败，请使用完整有效的配置备份文件。","配置备份",MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			_9DA58D0D._5D33D13B(this, disposing);
		}

		private void InitializeComponent()
        {
            AutoScaleMode=AutoScaleMode.None;Size=new System.Drawing.Size(550,310);
            var flow=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false};
            flow.Controls.Add(new Label{Text="配置备份",AutoSize=true,Font=new System.Drawing.Font("Microsoft YaHei UI",15,System.Drawing.FontStyle.Bold),Margin=new Padding(0,0,0,24)});
            flow.Controls.Add(new Label{Text="备份文件夹",AutoSize=true,Margin=new Padding(0,0,0,8)});
            txtPath=new TextBox{Name="BackupPath",Width=530,Text=ClassMain.Backup_Path};txtPath.TextChanged+=txtPath_TextChanged;flow.Controls.Add(txtPath);
            var actions=new FlowLayoutPanel{AutoSize=true,Margin=new Padding(0,16,0,18)};
            button1=new Button{Text="选择文件夹",AutoSize=true};button1.Click+=button1_Click;actions.Controls.Add(button1);
            button2=new Button{Text="导出备份",AutoSize=true};button2.Click+=button2_Click;actions.Controls.Add(button2);
            button4=new Button{Text="导入备份",AutoSize=true};button4.Click+=button4_Click;actions.Controls.Add(button4);
            button3=new Button{Text="打开备份目录",AutoSize=true};button3.Click+=button3_Click;actions.Controls.Add(button3);flow.Controls.Add(actions);
            flow.Controls.Add(new Label{Text="备份包含已填写的 API 密钥，请妥善保管。",AutoSize=true,MaximumSize=new System.Drawing.Size(530,0)});Controls.Add(flow);
        }
    }
}
