using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MSScriptControl
{
	[ComImport]
	[TypeIdentifier]
	[Guid("0E59F1D3-1FBE-11D0-8FF2-00A0D10038BC")]
	[CompilerGenerated]
	public interface IScriptControl
	{
		[DispId(1500)]
		string Language
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[DispId(1500)]
			[return: MarshalAs(UnmanagedType.BStr)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[DispId(1500)]
			[param: In]
			[param: MarshalAs(UnmanagedType.BStr)]
			set;
		}

		void _VtblGap1_8();

		[DispId(1505)]
		bool UseSafeSubset
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[DispId(1505)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[DispId(1505)]
			[param: In]
			set;
		}

		void _VtblGap2_7();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2000)]
		void AddCode([In][MarshalAs(UnmanagedType.BStr)] string Code);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2001)]
		[return: MarshalAs(UnmanagedType.Struct)]
		object Eval([In][MarshalAs(UnmanagedType.BStr)] string Expression);
	}
}
