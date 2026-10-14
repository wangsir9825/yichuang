using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Xtranslator {
 public partial class Form2 {
  [StructLayout(LayoutKind.Sequential)]
  private struct FrameMargins { public int Left,Right,Top,Bottom; }
  [DllImport("dwmapi.dll")]private static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
  [DllImport("dwmapi.dll")]private static extern int DwmExtendFrameIntoClientArea(IntPtr window,ref FrameMargins margins);
  private bool modernShadowAvailable;
  private Rectangle frameNormalBounds;
  private FormWindowState frameLastState;
  private bool frameRestorePending;
  protected override CreateParams CreateParams {
   get {var cp=base.CreateParams;cp.Style|=0x00040000|0x00C00000|0x00080000|0x00020000|0x00010000;return cp;}
  }
  protected override void OnHandleCreated(EventArgs e){SetStyle(ControlStyles.ResizeRedraw,true);base.OnHandleCreated(e);ConfigureModernFrame();}
  protected override void OnResize(EventArgs e){
   base.OnResize(e);
   if(!modernReady||frameRestorePending)return;
   if(WindowState==FormWindowState.Normal&&frameLastState==FormWindowState.Maximized&&!frameNormalBounds.IsEmpty){
    // WinForms assumes a borderless nonclient size, while DWM retains native styles.
    // Restore the actual last normal bounds after the native state transition finishes.
    var restore=frameNormalBounds;frameRestorePending=true;frameLastState=WindowState;
    BeginInvoke((Action)(()=>{try{if(!IsDisposed&&WindowState==FormWindowState.Normal)Bounds=restore;}finally{frameRestorePending=false;}}));
   }else{
    frameLastState=WindowState;
    if(WindowState==FormWindowState.Normal)frameNormalBounds=Bounds;
   }
  }
  protected override void OnLocationChanged(EventArgs e){
   base.OnLocationChanged(e);
   if(modernReady&&!frameRestorePending&&WindowState==FormWindowState.Normal&&frameLastState==FormWindowState.Normal)frameNormalBounds.Location=Location;
  }
  private void ConfigureModernFrame(){
   if(!IsHandleCreated)return;
   try{
    int enabled=2;var margins=new FrameMargins{Left=1,Right=1,Top=1,Bottom=1};
    int policy=DwmSetWindowAttribute(Handle,2,ref enabled,sizeof(int));
    modernShadowAvailable=DwmExtendFrameIntoClientArea(Handle,ref margins)==0&&policy==0;
   }catch(DllNotFoundException){modernShadowAvailable=false;}catch(EntryPointNotFoundException){modernShadowAvailable=false;}
  }
  protected override void OnPaint(PaintEventArgs e){
   base.OnPaint(e);
   if(ClientSize.Width>4&&ClientSize.Height>4)using(var pen=new Pen(UiTheme.Frame))e.Graphics.DrawRectangle(pen,1,1,ClientSize.Width-3,ClientSize.Height-3);
  }
 }
}
