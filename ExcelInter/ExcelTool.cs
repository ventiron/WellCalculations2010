using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;


namespace WellCalculations2010.ExcelInter
{
    public static class ExcelTool
    {

        //------------------------ written by kaefer ------------------------------//
        // Acquire and release Application objects
        static object GetInstance(string appName)
        {
            return Marshal.GetActiveObject(appName);
        }
        static object CreateInstance(string appName)
        {
            return Activator.CreateInstance(Type.GetTypeFromProgID(appName));
        }
        static object GetOrCreateInstance(string appName)
        {
            try { return GetInstance(appName); }
            catch { return CreateInstance(appName); }
        }
        // Type extensions on System.Object
        static void ReleaseInstance(this object o)
        {
            Marshal.ReleaseComObject(o);
        }
        // Get, set and invoke for all objects
        static object Get(this object o, string name, params object[] args)
        {
            return o.GetType().InvokeMember(name, BindingFlags.GetProperty, null, o, args);
        }
        static void Set(this object o, string name, params object[] args)
        {
            o.GetType().InvokeMember(name, BindingFlags.SetProperty, null, o, args);
        }
        static object Invoke(this object o, string name, params object[] args)
        {
            return o.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, o, args);
        }
        // Operates on Excel's Range object only
        static object XlRangef(this object o, int r0, int c0, int r1, int c1)
        {
            return o.Get("Range", o.Get("Cells", r0, c0), o.Get("Cells", r1, c1));
        }
        //--------------------------------------------------------------------------//
        public static void FillRange(string fname, string sheetname, object[] header, Array arr)
        {
            object xlApp = GetOrCreateInstance("Excel.Application");

            object xlBooks = xlApp.Get("Workbooks");
            object xlBook = xlBooks.Invoke("Open", fname);
            object xlSheets = xlBook.Get("Worksheets");
            object xlSheet = xlSheets.Get("Item", sheetname);

            // Fill in header in row 1 and make it bold
            object xlRange = xlSheet.XlRangef(1, 1, 1, header.Length);
            xlRange.Set("NumberFormat", "@");
            xlRange.Get("Font").Set("Bold", true);
            xlRange.Set("Value2", new object[] { header });

            // Transfer data
            xlRange = xlSheet.XlRangef(2, 1, arr.GetLength(0) + 1, arr.GetLength(1));
            xlRange.Set("NumberFormat", "@");
            xlRange.Set("Value2", new object[] { arr });

            // This column has numeric format
            xlRange = xlSheet.XlRangef(2, 2, arr.GetLength(0) + 1, 2);
            xlRange.Set("NumberFormat", "0");

            // Optimal column width
            xlSheet.Get("Columns").Invoke("AutoFit");

            //Return control of Excel to the user.
            xlApp.Set("Visible", true);
            xlApp.Set("UserControl", true);
            xlApp.ReleaseInstance();
        }
        public static void FillExistingBook(string fname, object[] header, Array arr2)
        {
            object xlApp = GetOrCreateInstance("Excel.Application");

            object xlBooks = xlApp.Get("Workbooks");
            object xlBook = xlBooks.Invoke("Add");
            object xlSheets = xlBook.Get("Worksheets");
            object xlSheet = xlSheets.Get("Item", 1);

            // Fill in header in row 1 and make it bold
            var xlRange = xlSheet.XlRangef(1, 1, 1, header.Length);
            xlRange.Set("NumberFormat", "@");
            xlRange.Get("Font").Set("Bold", true);
            xlRange.Set("Value2", new object[] { header });

            // Transfer data
            xlRange = xlSheet.XlRangef(2, 1, arr2.GetLength(0) + 1, arr2.GetLength(1));
            xlRange.Set("NumberFormat", "@");
            xlRange.Set("Value2", new object[] { arr2 });

            // This column has numeric format
            xlRange = xlSheet.XlRangef(2, 2, arr2.GetLength(0) + 1, 2);
            xlRange.Set("NumberFormat", "0");

            // Optimal column width
            xlSheet.Get("Columns").Invoke("AutoFit");

            //Return control of Excel to the user.
            xlApp.Set("Visible", true);
            xlApp.Set("UserControl", true);
            xlApp.ReleaseInstance();
        }
    }
}