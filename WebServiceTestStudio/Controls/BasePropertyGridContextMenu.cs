using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Events;
using System.Windows.Forms;

namespace WebServiceTestStudio.Controls
{
    /// <summary>
    /// BasePropertyGridContextMenu creates a basic context menu
    /// for the WinForms property grid. The Base features are Copy
    /// and Paste (if an object is in the clipboard).
    /// </summary>
    public class BasePropertyGridContextMenu : ContextMenu
    {
        public BasePropertyGridContextMenu(MenuItem[] menuItems) :
            base(menuItems)
        {
            MenuItems.Add("-");
            MenuItems.Add("Copy", OnCopy);
            if (EventService.ClipboardObject != null)
            {
                MenuItems.Add("Paste", OnPaste);
            }
        }

        private void OnPaste(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var gridControl = contextMenu.SourceControl as Control;
                var propertyGrid = gridControl.Parent.Parent as PropertyGrid;
                var selectedItem = propertyGrid.SelectedGridItem.Value;

                var copyToObj = selectedItem;
                var copyFromObj = EventService.ClipboardObject;
                if (copyFromObj == null)
                    return;

                if (copyFromObj.GetType() == copyToObj.GetType())
                {
                    copyFromObj.Copy(ref copyToObj);
                    propertyGrid.Refresh();
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        String.Format(
                        "Can not copy {0} to {1}",
                        copyFromObj.GetType(),
                        copyToObj.GetType()));
                }
            }
        }

        private void OnCopy(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var gridControl = contextMenu.SourceControl as Control;
                var propertyGrid = gridControl.Parent.Parent as PropertyGrid;
                var selectedItem = propertyGrid.SelectedGridItem.Value;

                EventService.ClipboardObject = selectedItem;
            }
        }


    }
}
