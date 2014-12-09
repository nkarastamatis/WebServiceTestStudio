using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Events;

namespace WebServiceTestStudio.Controls
{
    /// <summary>
    /// Context menu generator for property grids in LookupWindow.
    /// </summary>
    public class LookupContextMenuGenerator : IContextMenuGenerator
    {
        EventAggregator eventAggregator = EventService.EventAggregator;
        
        #region IContextMenuGenerator Members

        /// <summary>
        /// Adds the Copy & Paste action to the base context menu.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="location"></param>
        public void OnRightClick(Control control, Point location)
        {
            var propertyGrid = control.Parent.Parent as PropertyGrid;
            var selectedItem = propertyGrid.SelectedGridItem.Value;

            if (selectedItem != null)
            {
                var selectedType = selectedItem.GetType();

                List<MenuItem> menuItems = new List<MenuItem>();

                var copyAndPasteMenuItem = new MenuItem("Copy & Paste");
                copyAndPasteMenuItem.Click += copyAndPasteMenuItem_Click;
                menuItems.Add(copyAndPasteMenuItem);

                var contextMenu = new BasePropertyGridContextMenu(menuItems.ToArray());
                contextMenu.Show(control, location);
            }
        }

        /// <summary>
        /// Puts the selected item on the clipboard and publishes the
        /// CopyAndPasteEvent for subscribers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyAndPasteMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var gridControl = contextMenu.SourceControl as Control;
                var propertyGrid = gridControl.Parent.Parent as PropertyGrid;
                var selectedItem = propertyGrid.SelectedGridItem.Value;

                EventService.ClipboardObject = selectedItem;

                var evt = eventAggregator.GetEvent<CopyAndPasteEvent>();
                evt.Publish(new CopyAndPasteEventArgs());
            }
        }

        #endregion
    }
}
