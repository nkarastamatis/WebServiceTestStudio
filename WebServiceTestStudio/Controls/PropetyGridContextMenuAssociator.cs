using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebServiceTestStudio;
using System.Reflection;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Events;

namespace WebServiceTestStudio.Controls
{
    /// <summary>
    /// Associates a property grid with it's Tagged IContextMenuGenerator.
    /// </summary>
    public class PropetyGridContextMenuAssociator
    {
        /// <summary>
        /// We don't have a public constructor this class. 
        /// Add is used to do the setup work needed to have a property grid create 
        /// a custom context menu when a grid cell is clicked.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static PropetyGridContextMenuAssociator Add(PropertyGrid control)
        {
            var contextMenu = new PropetyGridContextMenuAssociator();
            // We can't add the mouse clicked event yet, because the property grid
            // may not have it's internal controls constructed. The controls we need
            // to add event handlers to get created when a selected object is changed for
            // the first time.
            control.SelectedObjectsChanged += new EventHandler(contextMenu.propertyGrid_SelectedObjectsChanged);
            return contextMenu;
        }

        private void propertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
        {
            var propertyGrid = sender as PropertyGrid;
            foreach (System.Windows.Forms.Control control in propertyGrid.Controls[2].Controls)
            {
                // Remove the handler before you add it, because we don't want it to be called twice.
                // It may get called multiple times if the selected object is changed for some reason.
                // The framework is OK with removing a handler if it doesn't already exist.
                control.MouseDown -= new System.Windows.Forms.MouseEventHandler(propertyGrid_MouseClick);
                control.MouseDown += new System.Windows.Forms.MouseEventHandler(propertyGrid_MouseClick);
            }
        }

        private void propertyGrid_MouseClick(object sender, MouseEventArgs e)
        {
            // We only need to handle a right click. 
            // Use the Tagged IContextMenuGenerator to do the work.
            if (e.Button == MouseButtons.Right)
            {
                var control = sender as Control;
                var pg = control.Parent.Parent as PropertyGrid;
                var contextMenuGenerator = pg.Tag as IContextMenuGenerator;
                if (contextMenuGenerator != null)
                {
                    contextMenuGenerator.OnRightClick(sender as Control, e.Location);
                }
                
            }
        }


       
    }
}
