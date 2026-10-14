using System.Windows.Forms;

namespace Xtranslator
{
	internal class ToolStripEx : ToolStrip
	{
		protected override void WndProc(ref Message m)
		{
			if (((global::_96163720)_63AF42A1._71AC5D16[706])(ref m) == 33 && ((global::_7B3D17BA)_63AF42A1._71AC5D16[786])(this) && !((global::BE00278A)_63AF42A1._71AC5D16[481])(this))
			{
				((global::_44023A3B)_63AF42A1._71AC5D16[593])(this);
			}
			C500F296.E7046E8A(this, ref m);
		}

		public ToolStripEx()
		{
			((global::_74A7C001)_63AF42A1._71AC5D16[20])(this);
		}
	}
}
