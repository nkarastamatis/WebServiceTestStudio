using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WebServiceTestStudio.Controls
{
    /// <summary>
    /// Represents a generic context menu generator.
    /// </summary>
    public interface IContextMenuGenerator
    {
        void OnRightClick(Control control, Point location);
    }
}
