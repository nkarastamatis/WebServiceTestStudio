using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.UnityExtensions;
using WebServiceTestStudio.Events;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using System.Data;

namespace WebServiceTestStudio.Views
{
    public class DataGridWindowViewModel : BindableBase
    {
        public Array Array { get; set; }
        public DataTable DataTable { get; set; }

        public DataGridWindowViewModel(Array array)
        {
            Array = array;

            CreateDataTable();
        }

        public void CreateDataTable()
        {
            var dt = new DataTable();
           

            Type elementType = Array.GetType().GetElementType();
            AddColumn(dt, elementType, String.Empty);

            foreach (var element in Array)
            {
                var row = dt.NewRow();

                if (element == null)
                {
                    dt.Rows.Add(row);
                    continue;
                }

                foreach (DataColumn col in dt.Columns)
                {
                    var levels = col.ColumnName.Split(',');
                    object rowValue = element;
                    foreach (var level in levels)
                    {
                        if (rowValue == null) break;
                        FieldInfo field = rowValue.GetType().GetField(level);
                        rowValue = field.GetValue(rowValue);
                    }

                    if (rowValue == null)
                        row[col] = null;
                    else
                        row[col] = rowValue.ToString();
                }
                
                dt.Rows.Add(row);
            }

            DataTable = dt;
        }

        private void AddColumn(DataTable dt, Type type, string prefix)
        {
            foreach (var field in type.GetFields())
            {
                var colName = field.Name;
                if (!String.IsNullOrEmpty(prefix))
                    colName = String.Join(",", prefix, colName);

                if (field.FieldType.Namespace != "System" && field.FieldType.BaseType.Name != "Enum")
                {
                    AddColumn(dt, field.FieldType, colName);
                }
                else
                {
                    var col = new DataColumn();
                    col.ColumnName = colName;
                    dt.Columns.Add(col);
                }

            }
        }
    }
}
