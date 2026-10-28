using System;
using System.Drawing;
using System.Windows.Forms;

namespace YiChuang
{
    internal sealed class UserControlNetworkProxy:UserControl
    {
        private readonly ComboBox mode=new ComboBox();
        private readonly TextBox address=new TextBox();
        private readonly Label explanation=new Label();
        private static readonly string[] modes={"system","direct","manual"};
        internal UserControlNetworkProxy()
        {
            Name="NetworkProxyPage";AutoScaleMode=AutoScaleMode.None;Size=new Size(620,405);
            LabelAt("所有在线引擎，共用一个网络设置",0,0,610,40);
            LabelAt("连接方式",0,58,610,27);
            mode.Name="NetworkProxyMode";mode.DropDownStyle=ComboBoxStyle.DropDownList;mode.SetBounds(0,91,610,32);
            mode.Items.AddRange(new object[]{"跟随系统代理（默认）","直连（不使用代理）","指定代理"});Controls.Add(mode);
            explanation.SetBounds(0,139,610,48);Controls.Add(explanation);
            LabelAt("HTTP 代理地址",0,203,610,27);
            address.Name="NetworkProxyAddress";address.SetBounds(0,237,610,30);Controls.Add(address);
            LabelAt("本机代理示例：http://127.0.0.1:10808\r\n使用另一台电脑的代理时，将 127.0.0.1 换成那台电脑的局域网 IP。",0,282,610,55);
            LabelAt("保存或应用后，对后续请求生效；正在翻译时可停止后重新翻译。\r\n本机模型始终在本机通信，不需要联网，也不经过代理。",0,351,610,54);
            mode.SelectedIndexChanged+=(s,e)=>UpdateMode();
            var options=NetworkProxy.Current;RestoreValues(new[]{options.Mode,options.Address});
        }
        private void LabelAt(string text,int x,int y,int width,int height)
        { Controls.Add(new Label{Text=text,Location=new Point(x,y),Size=new Size(width,height)}); }
        private void UpdateMode()
        {
            address.Enabled=mode.SelectedIndex==2;
            explanation.Text=mode.SelectedIndex==0?"使用当前 Windows 用户的系统代理设置。浏览器扩展中的代理不会自动共享。":mode.SelectedIndex==1?"在线翻译和译窗内的下载直接连接目标网站。":"填写 HTTP 或混合代理端口；支持通过此端口访问 HTTPS 网站。\r\n暂不支持 SOCKS 专用端口、订阅链接或需要账号密码的代理。";
        }
        internal string[] Values { get { return new[]{mode.SelectedIndex<0?"":modes[mode.SelectedIndex],address.Text}; } }
        internal void RestoreValues(string[] values) { mode.SelectedIndex=Array.IndexOf(modes,values[0]);address.Text=values[1];UpdateMode(); }
        internal NetworkProxyOptions Candidate() { var values=Values;var result=new NetworkProxyOptions(values[0],values[1]);result.Validate();return result; }
    }
}
