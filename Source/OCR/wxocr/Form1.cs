using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace wxocr;

public class Form1 : Form
{
	public delegate void SetResultDelegate(IntPtr result);

	public class StringResult
	{
		public string result_ = "";

		public void SetResult(IntPtr dt)
		{
			int i;
			for (i = 0; Marshal.ReadByte(dt, i) != 0; i++)
			{
			}
			byte[] array = new byte[i];
			Marshal.Copy(dt, array, 0, i);
			result_ = Encoding.UTF8.GetString(array);
		}

		public string GetResult()
		{
			return result_;
		}
	}

	public class OcrResponse
	{
		public int errcode { get; set; }

		public string imgpath { get; set; }

		public int width { get; set; }

		public int height { get; set; }

		public OcrItem[] ocr_response { get; set; }
	}

	public class OcrItem
	{
		public double left { get; set; }

		public double top { get; set; }

		public double right { get; set; }

		public double bottom { get; set; }

		public double rate { get; set; }

		public string text { get; set; }
	}

	private const string MMF_NAME = "Global\\MySharedMemory";

	private const string EVENT_NAME = "Global\\MySharedMemoryEvent";

	private const int CAPACITY = 1024;

	private IContainer components = null;

	private Button btnRunOCR;

	[DllImport("wcocr.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool wechat_ocr([MarshalAs(UnmanagedType.LPWStr)] string ocr_exe, [MarshalAs(UnmanagedType.LPWStr)] string wechat_dir, [MarshalAs(UnmanagedType.LPStr)] string imgfn, SetResultDelegate set_res);

	[DllImport("wcocr.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void stop_ocr();

	public Form1()
	{
		InitializeComponent();
	}

    public static string RecognizeImage(string image, bool forceIndependent = false)
    {
        if (!forceIndependent)
        {
            var (binary, runtime, version) = WeChatPathResolver.GetWeChatPaths();
            if (!string.IsNullOrEmpty(binary) && !string.IsNullOrEmpty(runtime))
            {
                try
                {
                    var native = new StringResult();
                    if (wechat_ocr(binary, runtime, image, native.SetResult))
                    {
                        var response = JsonConvert.DeserializeObject<OcrResponse>(native.GetResult());
                        if (response?.ocr_response != null)
                        {
                            var lines = response.ocr_response.Select(item => item.text ?? "").ToList();
                            for (int i=0;i<lines.Count-1;i++) if(lines[i].EndsWith("-")){lines[i]=lines[i].TrimEnd('-')+lines[i+1].Trim();lines.RemoveAt(i+1);i--;}
                            string text=string.Join(" ",lines);
                            if(!string.IsNullOrWhiteSpace(text))return text;
                        }
                    }
                }
                catch (Exception) { /* Existing WeChat component failed; continue with the bundled engine. */ }
                finally { try { stop_ocr(); } catch { } }
            }
        }
        return IndependentOcr.Recognize(image);
    }

    public static void RunOcrDirectly()
    {
        using ScreenshotForm screenshotForm = new ScreenshotForm();
        if(screenshotForm.ShowDialog()!=DialogResult.OK||screenshotForm.CapturedImage==null)return;
        string image=Path.Combine(Path.GetTempPath(),"YiChuangOcr_"+Guid.NewGuid().ToString("N")+".png");
        try
        {
            screenshotForm.CapturedImage.Save(image,ImageFormat.Png);
            string text=RecognizeImage(image);
            if(string.IsNullOrWhiteSpace(text))MessageBox.Show("未识别到文字，请选择更清晰的文字区域后重试。","截图识别");
            else sendData(text).GetAwaiter().GetResult();
        }
        catch(Exception ex){MessageBox.Show("截图识别失败："+ex.Message,"截图识别");}
        finally{try{if(File.Exists(image))File.Delete(image);}catch{}}
    }

	private void btnRunOCR_Click(object sender, EventArgs e)
	{
	}

	private void Form1_Load(object sender, EventArgs e)
	{
	}

	private static async System.Threading.Tasks.Task sendData(string message)
	{
		try
		{
			using NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "YiChuangOcr2918", PipeDirection.InOut);
			await pipeClient.ConnectAsync(5000).ConfigureAwait(false);
			byte[] data = Encoding.UTF8.GetBytes(message);
			await pipeClient.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
			Console.WriteLine("数据已发送");
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			Console.WriteLine("错误: " + ex2.Message);
			throw;
		}
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.btnRunOCR = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.btnRunOCR.Location = new System.Drawing.Point(47, 17);
		this.btnRunOCR.Name = "btnRunOCR";
		this.btnRunOCR.Size = new System.Drawing.Size(122, 23);
		this.btnRunOCR.TabIndex = 0;
		this.btnRunOCR.Text = "OCR2剪切板";
		this.btnRunOCR.UseVisualStyleBackColor = true;
		this.btnRunOCR.Visible = false;
		this.btnRunOCR.Click += new System.EventHandler(btnRunOCR_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(217, 74);
		base.Controls.Add(this.btnRunOCR);
		base.Name = "Form1";
		this.Text = "WXOCR";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
		base.Load += new System.EventHandler(Form1_Load);
		base.ResumeLayout(false);
	}
}
