using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace YiChuang
{
    // Read-only TOM ranges use RichEdit's own glyph metrics, including DPI and line spacing.
    // Method slots are from Microsoft's TOM.h: ITextDocument::Range and ITextRange::GetPoint.
    internal sealed class NativeTextGeometry : IDisposable
    {
        private IntPtr document;
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window,int message,IntPtr wParam,out IntPtr lParam);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int RangeCall(IntPtr self,int first,int last,out IntPtr range);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int PointCall(IntPtr self,int flags,out int x,out int y);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int ParaCall(IntPtr self,out IntPtr para);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] private delegate int SpacingCall(IntPtr self,int rule,float points);
        private static T Method<T>(IntPtr self,int slot) where T:class {return Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(self),slot*IntPtr.Size),typeof(T)) as T;}
        internal NativeTextGeometry(IntPtr window) {
            IntPtr ole;SendMessage(window,0x43c,IntPtr.Zero,out ole);
            if(ole==IntPtr.Zero)return;
            try {var iid=new Guid("8CC497C0-A1DF-11ce-8098-00AA0047BE5D");Marshal.QueryInterface(ole,ref iid,out document);}
            finally {Marshal.Release(ole);}
        }
        internal bool Bounds(int start,int end,out Rectangle result) {
            result=Rectangle.Empty;if(document==IntPtr.Zero||end<=start)return false;
            IntPtr range;
            if(Method<RangeCall>(document,24)(document,start,end-1,out range)!=0||range==IntPtr.Zero)return false;
            try {
                var point=Method<PointCall>(range,54);int left,top,right,bottom,x,y;
                const int client=256|512,startFlag=32;
                if(point(range,client|startFlag,out left,out top)!=0||point(range,client|startFlag|8,out x,out bottom)!=0||point(range,client|2,out right,out y)!=0)return false;
                result=Rectangle.FromLTRB(left,top,Math.Max(left+1,right),Math.Max(top+1,bottom));return true;
            } finally {Marshal.Release(range);}
        }
        public void Dispose(){if(document!=IntPtr.Zero){Marshal.Release(document);document=IntPtr.Zero;}}
        internal bool SetMinimumLineHeight(int length,float points) {
            if(document==IntPtr.Zero)return false;IntPtr range,para=IntPtr.Zero;
            if(Method<RangeCall>(document,24)(document,0,length,out range)!=0||range==IntPtr.Zero)return false;
            try {if(Method<ParaCall>(range,20)(range,out para)!=0||para==IntPtr.Zero)return false;return Method<SpacingCall>(para,43)(para,3,points)==0;}
            finally {if(para!=IntPtr.Zero)Marshal.Release(para);Marshal.Release(range);}
        }
    }
}
