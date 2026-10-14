using System.Drawing;
using System.Windows.Forms;
// A dedicated document for manual cross-window shortcut checks; no UI automation.
internal static class SelectionFixture {
 [System.STAThread] static void Main(){
  Application.EnableVisualStyles();
  var form=new Form{Text="译窗快捷键验收专用文本",Width=900,Height=280};
  var editor=new TextBox{Multiline=true,Dock=DockStyle.Fill,Font=new Font("Microsoft YaHei UI",14),Text="Simply hover your mouse cursor over any sentence to view its corresponding translation. Both versions should be highlighted together."};
  form.Controls.Add(editor);form.Shown+=(s,e)=>{editor.Focus();editor.SelectAll();};Application.Run(form);
 }
}
