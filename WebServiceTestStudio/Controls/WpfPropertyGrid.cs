using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Collections.Generic;
using Control = System.Windows.Controls.Control;

namespace WebServiceTestStudio.Controls
{
    public class WpfPropertyGrid : Control
    {
        public static readonly DependencyProperty SelectedObjectProperty =
             DependencyProperty.Register("SelectedObject", typeof(object), typeof(WpfPropertyGrid),
             new PropertyMetadata(false, new PropertyChangedCallback(OnSelectedObject)));

        public static readonly DependencyProperty ContextMenuGeneratorProperty =
             DependencyProperty.Register("ContextMenuGenerator", typeof(object), typeof(WpfPropertyGrid),
             new PropertyMetadata(false, new PropertyChangedCallback(OnContextMenuGenerator)));

        private static void OnContextMenuGenerator(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Assign the IContextMenuGenerator to the property grid's Tag.
            var container = (WpfPropertyGrid)d;
            container.PropertyGrid.Tag = e.NewValue;
        }

        private PropertyGrid _propertyGrid;
        private static void OnSelectedObject(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = (WpfPropertyGrid)d;
            container.PropertyGrid.SelectedObject = container.SelectedObject;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var x = GetTemplateChild("host") as WindowsFormsHost;
            if (x != null)
            {
                _propertyGrid = x.Child as PropertyGrid;
                InitializePropertyGrid();
            }
        }

        public PropertyGrid PropertyGrid
        {
            get { return _propertyGrid; }
        }
        public object SelectedObject
        {
            get
            {
                return GetValue(SelectedObjectProperty);
            }
            set
            {
                SetValue(SelectedObjectProperty, _propertyGrid.SelectedObject);
            }
        }

        public object ContextMenuGenerator { get; set; }

        static WpfPropertyGrid()
        {
            // Gets the Control Template from Themes\Generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WpfPropertyGrid), new FrameworkPropertyMetadata(typeof(WpfPropertyGrid)));
        }

        #region WebServiceTestStudio Initialization Methods
        private bool previewKeyAdded = false;

        private void InitializePropertyGrid()
        {
            _propertyGrid.PropertySort = PropertySort.NoSort;
            _propertyGrid.HelpVisible = false;
            _propertyGrid.SelectedObjectsChanged += new System.EventHandler(this.propertyGrid_SelectedObjectsChanged);

            // Add the context menu handler to the property grid.
            PropetyGridContextMenuAssociator.Add(_propertyGrid);
        }

        private void propertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
        {
            if (!previewKeyAdded)
            {
                // Adds the key handler so user can hit Enter and go to the next griditem.
                foreach (System.Windows.Forms.Control control in _propertyGrid.Controls[2].Controls)
                    control.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.propertyGrid_PreviewKeyDown);

                previewKeyAdded = true;
            }
        }

        private void propertyGrid_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                // The following logic submits the enters value for the current grid
                // and moves the cursor to the next grid item for fast editing.
                var gridItem = _propertyGrid.SelectedGridItem;
                if (gridItem.Expanded)
                {
                    if (gridItem.GridItems.Count > 0)
                        gridItem.GridItems[0].Select();
                }
                else
                {
                    bool found = false;
                    GridItem nextFocus = gridItem;
                    GridItem parent = gridItem.Parent;
                    while (!found && parent != null)
                    {
                        var enumerator = parent.GridItems.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (gridItem == enumerator.Current)
                            {
                                if (enumerator.MoveNext())
                                {
                                    nextFocus = (GridItem)enumerator.Current;
                                    found = true;
                                    break;
                                }
                            }

                        }

                        gridItem = parent;
                        parent = parent.Parent;
                    }

                    nextFocus.Select();
                    if (nextFocus.Expandable && !nextFocus.Expanded)
                        nextFocus.Expanded = true;
                }

                // After the next grid item is found and focused
                // we have to send a tab message to get the cursor
                // from the property name field to the property value
                // field. 
                // Use timer to allow property grid to select the 
                // property name and then we will make the value editable
                // by immediately sending a tab message.
                timer = new Timer();
                timer.Interval = 1;
                timer.Tick += new EventHandler(timer_Tick);
                timer.Start();

            }
        }

        Timer timer;

        void timer_Tick(object sender, EventArgs e)
        {
            SendKeys.SendWait("{TAB}");
            timer.Stop();
        }

        #endregion
    }
}
