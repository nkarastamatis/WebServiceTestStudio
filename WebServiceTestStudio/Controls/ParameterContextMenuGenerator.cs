using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Microsoft.Practices.Prism.PubSubEvents;
using WebServiceTestStudio.Events;

namespace WebServiceTestStudio.Controls
{
    /// <summary>
    /// Context menu generator for MainWindow property grids.
    /// </summary>
    public class ParameterContextMenuGenerator : IContextMenuGenerator
    {
        /// <summary>
        /// Delegate to get methods by type. Must be assign by the 
        /// appropriate model.
        /// </summary>
        public static WebServiceTestStudio.Model.GetMethods GetMethodsByType;

        /// <summary>
        /// Local reference to EventAggregator instance.
        /// </summary>
        EventAggregator EventAggregator = EventService.EventAggregator;

        #region IContextMenuGenerator Members
        /// <summary>
        /// Adds the 'Send to' and 'Look up' menu items with sub items
        /// to the base context menu.
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
                // Invoke the GetMethodsByType delegate
                var methods = GetMethodsByType(selectedType);

                // Probably not the best for performance, but not worried abou that 
                // right now.
                // First get all the methods that return the type we are looking for 
                // and call them the lookupMethods.
                // Then remove all them from the main list for the 'Send to' list.
                var lookupMethods = methods
                    .Where(method =>
                        method.ReturnType.FullName == selectedType.FullName ||
                        method.ReturnType.FullName == selectedType.FullName + "[]")
                    .ToList();
                methods.RemoveAll(method => lookupMethods.Contains(method));

                List<MenuItem> menuItems = new List<MenuItem>();

                if (methods.Any())
                {
                    // Now add the "Send to new" menu items.
                    MenuItem[] sendToObjectSubMenuItems = GetMethodMenuItems(methods, sendToMenuItem_MouseClick);

                    var sendToObjectMenuItem = new MenuItem("Send to new...", sendToObjectSubMenuItems);
                    menuItems.Add(sendToObjectMenuItem);
                }

                if (lookupMethods.Any())
                {
                    // Now add the "Look up" menu items.
                    MenuItem[] lookUpObjectSubMenuItems = GetMethodMenuItems(lookupMethods, lookupMenuItem_MouseClick);

                    var lookUpObjectMenuItem = new MenuItem("Look up...", lookUpObjectSubMenuItems);
                    menuItems.Add(lookUpObjectMenuItem);
                }

                if (selectedItem is Array)
                {
                    var displayInDataGrid = new MenuItem("Data Grid");
                    displayInDataGrid.Click += new EventHandler(displayInDataGridMenuItem_MouseClick);
                    displayInDataGrid.Tag = selectedItem;
                    menuItems.Add(displayInDataGrid);
                }

                var contextMenu = new BasePropertyGridContextMenu(menuItems.ToArray());
                contextMenu.Show(control, location);
            }
        }
        #endregion

        private void displayInDataGridMenuItem_MouseClick(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var array = menuItem.Tag as Array;
            if (array == null)
                return;

            var window = new Views.DataGridWindow(array);
            window.Title = array.GetType().Name;
            window.Show();
        }

        /// <summary>
        /// Creates the list of submenu items.
        /// </summary>
        /// <param name="methods">Menu items will be created for each method.</param>
        /// <param name="method_MouseClick">The action to call when the menu item is clicked.</param>
        /// <returns></returns>
        private MenuItem[] GetMethodMenuItems(List<MethodInfo> methods, Action<object, EventArgs> method_MouseClick)
        {
            var items = new List<MenuItem>();

            foreach (var method in methods)
            {
                var menuItem = new MenuItem(method.Name);
                menuItem.Click += new EventHandler(method_MouseClick);
                menuItem.Tag = method;
                items.Add(menuItem);
            }

            return items.ToArray();
        }

        private void sendToMenuItem_MouseClick(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            // The menu item tag should be set when the item is created.
            // If not something is wrong and we can not proceed.
            if (menuItem.Tag != null)
            {
                // First, do what's needed to get the objects you're looking for.
                var subMenu = menuItem.Parent as MenuItem;
                var contextMenu = subMenu.Parent as ContextMenu;
                var gridControl = contextMenu.SourceControl as Control;
                var propertyGrid = gridControl.Parent.Parent as PropertyGrid;
                var selectedItem = propertyGrid.SelectedGridItem.Value;

                // Then publish the event. 
                if (EventAggregator != null)
                {
                    var evt = EventAggregator.GetEvent<SendParameterToNewMethodEvent>();
                    evt.Publish(new SendParameterToNewMethodEventArgs(
                        selectedItem, 
                        menuItem.Tag as MethodInfo));
                }
            }
        }

        private void lookupMenuItem_MouseClick(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            // The menu item tag should be set when the item is created.
            // If not something is wrong and we can not proceed.
            if (menuItem.Tag != null)
            {
                // First, do what's needed to get the objects you're looking for.
                var subMenu = menuItem.Parent as MenuItem;
                var contextMenu = subMenu.Parent as ContextMenu;
                var gridControl = contextMenu.SourceControl as Control;
                var propertyGrid = gridControl.Parent.Parent as PropertyGrid;
                var selectedItem = propertyGrid.SelectedGridItem.Value;
                
                // Then publish the event.
                if (EventAggregator != null)
                {
                    var evt = EventAggregator.GetEvent<LookupEvent>();
                    evt.Publish(new LookupEventArgs(
                        ref selectedItem, 
                        menuItem.Tag as MethodInfo,
                        propertyGrid.Refresh));
                }
            }
        }
    }
}
