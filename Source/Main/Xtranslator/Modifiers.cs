using System;

namespace Xtranslator
{
	[Flags]
	public enum Modifiers
	{
		NoModifier = 0,
		Alt = 1,
		Ctrl = 2,
		Shift = 4,
		Win = 8
	}
}
