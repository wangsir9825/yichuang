using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YiChuang
{
    // Select a row by its text without toggling visibility; checkbox clicks and Space still toggle.
    internal sealed class EngineCheckList : CheckedListBox
    {
        private bool textClick;
        protected override void WndProc(ref Message m)
        {
            if(m.Msg==0x201 || m.Msg==0x203) {
                int x=(short)(m.LParam.ToInt64()&0xffff),y=(short)((m.LParam.ToInt64()>>16)&0xffff);
                int index=IndexFromPoint(x,y);
                textClick=index>=0&&x>SystemInformation.MenuCheckSize.Width+6;
                if(textClick){Focus();SelectedIndex=index;return;}
            }
            if(m.Msg==0x202 && textClick){textClick=false;return;}
            base.WndProc(ref m);
        }
    }

    public sealed class EngineManagerControl : UserControl
    {
        internal Action Changed;
        internal Func<bool> BeforeOpenSetup;
        internal Action<bool> SetupClosed;
        private readonly EngineCheckList list = new EngineCheckList();
        private bool refreshing;
        private readonly Label details = new Label();
        private readonly Button edit, remove, restore, up, down;
        private readonly Action<int> configureBuiltin;
        private readonly bool localOnly;
        public EngineManagerControl(Action<int> configure) : this(configure,false) { }
        internal EngineManagerControl(Action<int> configure,bool localModelsOnly)
        {
            localOnly=localModelsOnly;configureBuiltin=configure;Name=localOnly?"LocalModelManager":"EngineManager";Size=new Size(610,430);
            var title=new Label{Text=localOnly?"我的本地模型":"我的翻译引擎",Font=new Font("Microsoft YaHei UI",12,FontStyle.Bold),AutoSize=true,Location=new Point(0,0)};
            Controls.Add(title);
            var setup=new Button{Name="OpenLocalSetup",Text="本地模型配置向导",Location=new Point(390,0),Size=new Size(210,30),FlatStyle=FlatStyle.Flat,ForeColor=UiTheme.Accent};
            setup.Click+=(s,e)=>{if(BeforeOpenSetup!=null&&!BeforeOpenSetup())return;string saved=Path.Combine(SettingsStore.Folder,"setting.txt");var before=File.GetLastWriteTimeUtc(saved);using(var wizard=new LocalSetupWizard())wizard.ShowDialog(FindForm());RefreshList(ClassMain.isjiekounum);SetupClosed?.Invoke(File.GetLastWriteTimeUtc(saved)!=before);};if(!localOnly)Controls.Add(setup);else setup.Dispose();
            Controls.Add(new Label{Text=localOnly?"自行添加 GGUF 或 Ollama 模型；勾选后在主界面显示。":"勾选后在主界面显示；取消勾选保留配置。选中一项可上移、下移。",Location=new Point(0,34),Size=new Size(600,28)});
            list.Name="ManagedEngines";list.Location=new Point(0,68);list.Size=new Size(600,224);list.IntegralHeight=false;list.HorizontalScrollbar=true;list.BorderStyle=BorderStyle.FixedSingle;
            Controls.Add(list);
            list.CheckOnClick=true;
            list.ItemCheck+=(s,e)=>{
                if(refreshing)return;
                EngineRegistry.SetVisible(((EngineDefinition)list.Items[e.Index]).Id,e.NewValue==CheckState.Checked);
                int selected=(list.SelectedItem as EngineDefinition)?.Id??0;
                if(IsHandleCreated)BeginInvoke(new Action(()=>{if(!IsDisposed)RefreshList(selected);}));
            };
            var actions=new FlowLayoutPanel{Location=new Point(0,305),Size=new Size(610,42),WrapContents=false};
            var add=ActionButton("AddEngine","新增引擎");edit=ActionButton("EditEngine","编辑配置");remove=ActionButton("RemoveEngine","移除引擎");restore=ActionButton("RestoreEngine","恢复内置");up=ActionButton("MoveEngineUp","↑ 上移");down=ActionButton("MoveEngineDown","↓ 下移");
            if(localOnly){add.Text="添加模型";edit.Text="编辑模型";remove.Text="移除模型";restore.Visible=false;}
            actions.Controls.AddRange(new Control[]{add,edit,remove,restore,up,down});Controls.Add(actions);
            details.Name="EngineManagerHint";details.Location=new Point(0,358);details.Size=new Size(600,68);details.ForeColor=UiTheme.Muted;Controls.Add(details);
            list.SelectedIndexChanged+=(s,e)=>RefreshDetails();
            up.Click+=(s,e)=>MoveSelected(-1);down.Click+=(s,e)=>MoveSelected(1);
            add.Click+=(s,e)=>{using(var dialog=new EngineEditorForm(null,localOnly)){if(dialog.ShowDialog(FindForm())==DialogResult.OK)SaveEntry(dialog.Result);}};
            edit.Click+=(s,e)=>EditSelected();
            remove.Click+=(s,e)=>{
                var selected=list.SelectedItem as EngineDefinition;if(selected==null)return;
                string hint=EngineRegistry.IsBuiltin(selected.Id)?"可通过‘恢复内置’重新加入，原有配置会保留。":"该自定义配置将被移除，需要时可重新添加。";
                if(MessageBox.Show(FindForm(),"移除“"+selected.Name+"”？\r\n"+hint+"\r\n本地模型文件不会删除。","移除翻译引擎",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)!=DialogResult.OK)return;
                EngineRegistry.Remove(selected.Id);RefreshList(0);
            };
            restore.Click+=(s,e)=>{
                var menu=new ContextMenuStrip();
                foreach(int id in RestorableIds()) {
                    int selected=id;menu.Items.Add(EngineLabels.BuiltinName(id),null,(a,b)=>{try{EngineRegistry.Restore(selected);RefreshList(selected);}catch(Exception ex){details.Text=ex.Message;}});
                }
                menu.Closed+=(a,b)=>{if(!IsDisposed)BeginInvoke(new Action(()=>menu.Dispose()));};menu.Show(restore,new Point(0,restore.Height));
            };
            RefreshList(ClassMain.isjiekounum);
        }
        private static Button ActionButton(string name,string text)
        {
            return new Button{Name=name,Text=text,TextAlign=ContentAlignment.MiddleCenter,Width=92,Height=36,Margin=new Padding(0,0,8,0),FlatStyle=FlatStyle.Flat,BackColor=UiTheme.Tint};
        }
        public int SelectedEngineId {get{return (list.SelectedItem as EngineDefinition)?.Id??ClassMain.isjiekounum;}}
        public void RefreshList(int selected)
        {
            refreshing=true;list.BeginUpdate();list.Items.Clear();foreach(int id in EngineRegistry.ActiveIds){var entry=EngineRegistry.Get(id);if(!localOnly||IsLocal(entry))list.Items.Add(entry,EngineRegistry.IsVisible(id));}
            list.SelectedIndex=-1;for(int i=0;i<list.Items.Count;i++)if(((EngineDefinition)list.Items[i]).Id==selected)list.SelectedIndex=i;
            if(list.SelectedIndex<0&&list.Items.Count>0)list.SelectedIndex=0;list.EndUpdate();refreshing=false;RefreshDetails();Changed?.Invoke();
        }
        private void RefreshDetails()
        {
            var e=list.SelectedItem as EngineDefinition;edit.Enabled=e!=null&&e.Provider!="transmart"&&e.Provider!="sogou";remove.Enabled=e!=null;
            restore.Enabled=RestorableIds().Any();
            up.Enabled=list.SelectedIndex>0;down.Enabled=list.SelectedIndex>=0&&list.SelectedIndex<list.Items.Count-1;
            details.Text=EngineRegistry.LoadError??(e==null?(localOnly?"还没有本地模型。点击‘添加模型’选择 GGUF 文件，或填写 Ollama 中已安装的模型名称。":"列表为空。点击‘新增引擎’或‘恢复内置’，然后在主界面选择。"):
                ((e.Provider=="ollama"||e.Provider=="llamacpp")?"本地模型："+e.Model:"接口类型："+EngineEditorForm.ProviderLabel(e.Provider))+"\r\n"+(e.Provider=="transmart"||e.Provider=="sogou"?"此网页引擎没有可填写的参数。":"选择‘编辑配置’可调整参数。")+(localOnly?" 点击下方‘保存并关闭’保存更改。":" 点击下方‘保存’或‘应用’保存更改。"));
        }
        private static bool IsLocal(EngineDefinition e){return e!=null&&(e.Provider=="ollama"||e.Provider=="llamacpp");}
        private System.Collections.Generic.IEnumerable<int> RestorableIds(){return EngineRegistry.BuiltinIds.Where(id=>!localOnly&&EngineRegistry.Get(id)==null);}
        private void MoveSelected(int offset){var e=list.SelectedItem as EngineDefinition;if(e==null)return;if(localOnly){int to=list.SelectedIndex+offset;if(to<0||to>=list.Items.Count)return;int other=((EngineDefinition)list.Items[to]).Id;var ids=EngineRegistry.ActiveIds;offset=Array.IndexOf(ids,other)-Array.IndexOf(ids,e.Id);}EngineRegistry.Move(e.Id,offset);RefreshList(e.Id);}
        private void EditSelected()
        {
            var e=list.SelectedItem as EngineDefinition;if(e==null||!edit.Enabled)return;
            if(EngineRegistry.IsBuiltin(e.Id)){configureBuiltin(e.Id);return;}
            using(var dialog=new EngineEditorForm(e,localOnly)){if(dialog.ShowDialog(FindForm())==DialogResult.OK)SaveEntry(dialog.Result);}
        }
        private void SaveEntry(EngineDefinition value){try{RefreshList(EngineRegistry.Save(value));}catch(Exception ex){details.Text=ex.Message;}}
    }

    public sealed class EngineEditorForm : Form
    {
        private readonly TextBox displayName=new TextBox(),endpoint=new TextBox(),apiKey=new TextBox(),source=new TextBox(),target=new TextBox();
        private readonly ComboBox provider=new ComboBox(),model=new ComboBox(),direction=new ComboBox(),prompt=new ComboBox();
        private readonly NumericUpDown timeout=new NumericUpDown();
        private readonly CheckBox automatic=new CheckBox();
        private readonly Label hint=new Label(),status=new Label();
        private readonly Button fetch=new Button();
        private readonly TableLayoutPanel fields=new TableLayoutPanel();
        private readonly int engineId;
        private readonly string[] providers;
        public EngineDefinition Result {get;private set;}
        public static string ProviderLabel(string p) {switch(p){case "llamacpp":return "内置 llama.cpp 本地模型";case "ollama":return "Ollama 本地模型";case "gemini":return "Google Gemini 专用接口";case "qwen":return "Qwen-MT 专用接口";case "deepl":return "DeepL 官方 API";case "transmart":case "sogou":return "网页翻译";default:return "OpenAI 兼容接口";}}
        public EngineEditorForm(EngineDefinition initial) : this(initial,false) { }
        internal EngineEditorForm(EngineDefinition initial,bool localOnly)
        {
            providers=localOnly?new[]{"llamacpp","ollama"}:new[]{"openai","ollama","gemini","qwen","deepl","llamacpp"};
            engineId=initial?.Id??0;Text=localOnly?(initial==null?"添加本地模型":"编辑本地模型"):(initial==null?"新增翻译引擎":"编辑翻译引擎");StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;MinimizeBox=false;MaximizeBox=false;
            Font=new Font("Microsoft YaHei UI",10F);BackColor=Color.White;ForeColor=UiTheme.Ink;AutoScaleMode=AutoScaleMode.None;Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            fields.AutoSize=true;fields.AutoSizeMode=AutoSizeMode.GrowAndShrink;fields.Dock=DockStyle.Top;fields.ColumnCount=2;fields.Padding=new Padding(20,14,20,14);fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,126));fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            displayName.Name="EngineName";endpoint.Name="EngineEndpoint";apiKey.Name="EngineApiKey";apiKey.UseSystemPasswordChar=true;model.Name="EngineModel";provider.Name="EngineProvider";direction.Name="EngineDirection";prompt.Name="EnginePrompt";
            provider.DropDownStyle=direction.DropDownStyle=prompt.DropDownStyle=ComboBoxStyle.DropDownList;
            provider.Items.AddRange(providers.Select(ProviderLabel).Cast<object>().ToArray());
            direction.Items.AddRange(new object[]{"自动判断中英方向","英语 → 简体中文","简体中文 → 英语"});
            prompt.Items.AddRange(new object[]{"通用翻译模型","腾讯 Hy-MT2","Google TranslateGemma"});
            timeout.Name="EngineTimeout";timeout.Minimum=10;timeout.Maximum=600;timeout.Value=180;
            automatic.Name="EngineAutomatic";automatic.Text="自动判断中英方向";automatic.AutoSize=true;automatic.Checked=true;
            fetch.Name="FetchInstalledModels";fetch.Text="读取已安装模型";fetch.Height=32;fetch.FlatStyle=FlatStyle.Flat;fetch.BackColor=UiTheme.Tint;
            AddRow("显示名称",displayName);AddRow("接口类型",provider);AddRow("API 基础地址",endpoint);AddRow("API Key",apiKey);AddRow("模型名称",model);AddRow("本地模型列表",fetch);AddRow("提示模板",prompt);AddRow("超时（秒）",timeout);
            hint.Name="EngineProtocolHint";hint.AutoSize=true;hint.MaximumSize=new Size(425,0);AddRow("填写说明",hint);
            status.Name="EngineEditorStatus";status.AutoSize=true;status.MaximumSize=new Size(425,0);status.ForeColor=Color.Firebrick;AddRow("",status);
            var scroll=new Panel{Dock=DockStyle.Fill,AutoScroll=true};scroll.Controls.Add(fields);
            var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=62,Padding=new Padding(18,12,18,12),FlowDirection=FlowDirection.RightToLeft};
            var save=new Button{Name="SaveEngine",Text="确定",Width=100,Height=36,FlatStyle=FlatStyle.Flat,BackColor=UiTheme.Accent,ForeColor=Color.White};
            var cancel=new Button{Name="CancelEngineEdit",Text="取消",Width=100,Height=36,DialogResult=DialogResult.Cancel};footer.Controls.Add(save);footer.Controls.Add(cancel);Controls.Add(scroll);Controls.Add(footer);AcceptButton=save;CancelButton=cancel;
            save.Click+=(s,e)=>{try{Result=ReadDraft();EngineRegistry.Validate(Result);if(Result.Provider=="llamacpp")GgufModels.ValidateFile(Result.Model);DialogResult=DialogResult.OK;Close();}catch(Exception ex){status.Text=ex.Message;scroll.ScrollControlIntoView(status);}};
            provider.SelectedIndexChanged+=(s,e)=>SetProviderDefaults();
            automatic.CheckedChanged+=(s,e)=>{source.Enabled=target.Enabled=!automatic.Checked;};
            fetch.Click+=async(s,e)=>{
                if(providers[provider.SelectedIndex]=="llamacpp") {using(var dialog=new OpenFileDialog{Filter="GGUF 模型|*.gguf|全部文件|*.*",CheckFileExists=true})if(dialog.ShowDialog(this)==DialogResult.OK)model.Text=dialog.FileName;return;}
                string address=endpoint.Text.Trim();fetch.Enabled=false;status.Text="正在读取 Ollama 实际模型列表…";
                try{var names=await Task.Run(()=>LocalModelClient.ListInstalled(address,()=>!IsDisposed));if(IsDisposed)return;if(endpoint.Text.Trim()!=address){status.Text="地址已改变，请重新读取。";return;}string previous=model.Text;model.Items.Clear();model.Items.AddRange(names);model.Text=previous;status.Text="找到 "+names.Length+" 个本地模型，请在模型名称下拉框选择。";}
                catch(Exception ex){if(!IsDisposed)status.Text=ex is OperationCanceledException?"已取消读取。":ex.Message;}
                finally{if(!IsDisposed)fetch.Enabled=true;}
            };
            provider.SelectedIndex=0;direction.SelectedIndex=0;prompt.SelectedIndex=0;
            if(initial!=null){provider.SelectedIndex=Array.IndexOf(providers,initial.Provider);displayName.Text=initial.Name;endpoint.Text=initial.Endpoint;apiKey.Text=initial.ApiKey;model.Text=initial.Model;direction.SelectedIndex=initial.Direction=="en-zh"?1:initial.Direction=="zh-en"?2:0;prompt.SelectedIndex=initial.Prompt=="hy"?1:initial.Prompt=="gemma"?2:0;timeout.Value=initial.TimeoutSeconds;automatic.Checked=initial.Automatic;source.Text=initial.SourceLanguage;target.Text=initial.TargetLanguage;}
            double scale;using(var g=Graphics.FromHwnd(IntPtr.Zero))scale=Math.Max(1,g.DpiX/96.0);ClientSize=new Size(610,590);MinimumSize=new Size(540,390);Scale(new SizeF((float)scale,(float)scale));
            var work=Screen.PrimaryScreen.WorkingArea;Size=new Size(Math.Min(Width,work.Width-24),Math.Min(Height,work.Height-48));
        }
        private void AddRow(string title,Control c)
        {
            int row=fields.RowCount++;fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var label=new Label{Text=title,AutoSize=true,Margin=new Padding(0,7,12,8)};c.Dock=DockStyle.Top;c.Margin=new Padding(0,5,0,8);fields.Controls.Add(label,0,row);fields.Controls.Add(c,1,row);
        }
        private void ShowField(Control c,bool visible){c.Visible=visible;var position=fields.GetPositionFromControl(c);fields.GetControlFromPosition(0,position.Row).Visible=visible;}
        private void SetProviderDefaults()
        {
            string p=providers[provider.SelectedIndex];bool embedded=p=="llamacpp",local=p=="ollama"||embedded,deepL=p=="deepl";
            apiKey.Clear();model.Items.Clear();model.Text="";status.Text="";
            endpoint.Text=local?"http://127.0.0.1:11434":p=="gemini"?"https://generativelanguage.googleapis.com/v1beta":p=="qwen"?CloudTranslationClient.DefaultQwenEndpoint:deepL?"https://api-free.deepl.com":"";
            endpoint.ReadOnly=p=="gemini";
            if(p=="gemini")model.Text=CloudTranslationClient.DefaultGeminiModel;if(p=="qwen")model.Text="qwen-mt-plus";
            target.Text="ZH";source.Text="";source.Enabled=target.Enabled=!automatic.Checked;
            ShowField(endpoint,!embedded);fetch.Text=embedded?"选择 GGUF 文件":"读取已安装模型";ShowField(apiKey,!local);ShowField(model,!deepL);ShowField(fetch,local);ShowField(prompt,local);
            fields.GetControlFromPosition(0,fields.GetPositionFromControl(model).Row).Text=embedded?"GGUF 文件路径":"模型名称";
            fields.GetControlFromPosition(0,fields.GetPositionFromControl(fetch).Row).Text=embedded?"选择模型文件":"本地模型列表";
            hint.Text=embedded?"选择本机 GGUF 文件；无需服务地址或 API Key。Hy-MT2 / TranslateGemma 选择对应模板；其他模型需有可用聊天模板。请选择与 llama.cpp 兼容的 GGUF 文本模型。新增/移除条目不删除模型文件。":local?"选择已下载的 Ollama 模型。新增或移除配置不会下载、删除模型文件。Hy-MT2 / TranslateGemma 请选择对应提示模板。":
                p=="openai"?"填写服务商提供的 API 基础地址（通常以 /v1 结尾），不要追加 /chat/completions。需要支持 Chat Completions；本机免密钥服务可留空 Key。":
                deepL?"Free 接口：https://api-free.deepl.com；Pro 接口：https://api.deepl.com。翻译方向统一在主界面原文区上方选择。":
                p=="qwen"?"填写与百炼地域匹配的 API Key、兼容模式地址和 Qwen-MT 模型名。普通 Qwen 对话模型请选择 OpenAI 兼容接口。":"填写 Google API Key 和文本模型名。模型名称不要包含 models/ 前缀。";
        }
        public EngineDefinition ReadDraft()
        {
            return new EngineDefinition{Id=engineId,Name=displayName.Text.Trim(),Provider=providers[provider.SelectedIndex],Endpoint=endpoint.Text.Trim().TrimEnd('/'),ApiKey=apiKey.Text.Trim(),Model=model.Text.Trim(),Direction=direction.SelectedIndex==1?"en-zh":direction.SelectedIndex==2?"zh-en":"auto",Prompt=prompt.SelectedIndex==1?"hy":prompt.SelectedIndex==2?"gemma":"generic",TimeoutSeconds=(int)timeout.Value,Automatic=automatic.Checked,SourceLanguage=source.Text.Trim(),TargetLanguage=target.Text.Trim()};
        }
        protected override void OnLoad(EventArgs e){TopMost=Owner!=null&&Owner.TopMost;base.OnLoad(e);}
    }
}
