using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;

namespace pkhCommon.WPF
{
    /// <summary>
    /// Attached behavior that keeps the window on the screen
    /// Just add pkhCommon.WPF:WindowService.EscapeClosesWindow="True" to XAML
    /// </summary>
    public static class WindowService
    {
        /// <summary>
        /// KeepOnScreen Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty EscapeClosesWindowProperty = DependencyProperty.RegisterAttached(
           "EscapeClosesWindow",
           typeof(bool),
           typeof(WindowService),
           new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnEscapeClosesWindowChanged)));

        /// <summary>
        /// Gets the EscapeClosesWindow property.  This dependency property 
        /// indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to get the property from</param>
        /// <returns>The value of the EscapeClosesWindow property</returns>
        public static bool GetEscapeClosesWindow(DependencyObject d)
        {
            return (bool)d.GetValue(EscapeClosesWindowProperty);
        }

        /// <summary>
        /// Sets the EscapeClosesWindow property.  This dependency property 
        /// indicates whether or not the escape key closes the window.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> to set the property on</param>
        /// <param name="value">value of the property</param>
        public static void SetEscapeClosesWindow(DependencyObject d, bool value)
        {
            d.SetValue(EscapeClosesWindowProperty, value);
        }

        /// <summary>
        /// Handles changes to the EscapeClosesWindow property.
        /// </summary>
        /// <param name="d"><see cref="DependencyObject"/> that fired the event</param>
        /// <param name="e">A <see cref="DependencyPropertyChangedEventArgs"/> that contains the event data.</param>
        private static void OnEscapeClosesWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window target = (Window)d;
            if (target != null)
            {
                target.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(Window_PreviewKeyDown);
            }
        }

        /// <summary>
        /// Handle the PreviewKeyDown event on the window
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Window target = (Window)sender;

            // If this is the escape key, close the window
            if (e.Key == Key.Escape)
                target.Close();
        }
    }

    public static class FlowDocumentHelpers
    {
        public static List getTopLevelList(Paragraph para)
        {
            List l = (para.Parent as ListItem).List;
            while((l.Parent as ListItem) != null)
            {
                l = (l.Parent as ListItem).List;
            }

           return l;
        }

        /// <summary>
        /// Adds one flowdocument to another.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public static void AddDocument(FlowDocument from, FlowDocument to)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                TextRange range = new TextRange(from.ContentStart, from.ContentEnd);
                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(to.ContentEnd, to.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);
            }
        }

        /// <summary>
        /// Adds a block to a flowdocument.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public static void AddBlock(Block from, FlowDocument to)
        {
            if (from != null)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    TextRange range = new TextRange(from.ContentStart, from.ContentEnd);
                    range.Save(stream, DataFormats.Xaml);
                    stream.Position = 0;
                    Block b = System.Windows.Markup.XamlReader.Load(stream) as Block;
                    to.Blocks.Add(b);
                }
            }
        }

        public static void AddInline(Inline from, FlowDocument to)
        {
            if (from != null)
            {
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                {
                    TextRange range = new TextRange(from.ContentStart, from.ContentEnd);
                    range.Save(stream, DataFormats.Xaml);
                    stream.Position = 0;
                    Inline i = System.Windows.Markup.XamlReader.Load(stream) as Inline;
                    (to.Blocks.LastBlock as Paragraph).Inlines.Add(i);
                }
            }
        }
    }

    public static class Helpers
    {
        public static FrameworkElement FindByName(string name, FrameworkElement root)
        {
            Stack<FrameworkElement> tree = new Stack<FrameworkElement>();
            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (current == null || current.Name == name)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }

            return null;
        }

        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null) return null;
            if (VisualTreeHelper.GetChildrenCount(visual) == 0) return null;
            return VisualTreeHelper.GetChild(visual, 0);
        }

        public static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                T objTest = obj as T;
                if (objTest != null)
                    return objTest;
                obj = GetParent(obj);
            }
            return null;
        }

        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        public static DependencyObject GetParent(DependencyObject obj)
        {
            if (obj == null)
                return null;
            ContentElement ce = obj as ContentElement;
            if (ce != null)
            {
                DependencyObject parent = ContentOperations.GetParent(ce);
                if (parent != null)
                    return parent;
                FrameworkContentElement fce = ce as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }
            return VisualTreeHelper.GetParent(obj);
        }

        public static T FindVisualChild<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null) return null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(current, i);
                if (child is T) return (T)child;
                T result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }
    }
}


namespace pkhCommon.WPF.Converters
{
    /// <summary>
    /// Converts zero to false, true otherwise
    /// </summary>
    [ValueConversion(typeof(int), typeof(bool))]
    public class IntegerToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, System.Globalization.CultureInfo culture)
        {
            int theValue = (int)value;
            if (theValue > 0)
                return true;

            //if (parameter != null)
            //{
            //    bool? useCollapse = (bool)parameter;
            //    if (useCollapse != null && (bool)useCollapse)
            //        return Visibility.Collapsed;
            //}

            return false;
        }

        public object ConvertBack(object o, Type type, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts true to Visible and false to Hidden unless the parameter
    /// is set to true then false converts to Collapsed.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type type, object parameter, System.Globalization.CultureInfo culture)
        {
            bool? theValue = (bool)value;
            if (theValue != null && (bool)theValue)
                return Visibility.Visible;

            if (parameter != null)
            {
                bool? useCollapse = (bool)parameter;
                if (useCollapse != null && (bool)useCollapse)
                    return Visibility.Collapsed;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object o, Type type, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Converts inches to pixels at 1 inch = 96 pixels
    /// </summary>
    [ValueConversion(typeof(float), typeof(double))]
    public class InchToPixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float f = (float)value;
            return (double)(f * 96);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Compares two integers and returns true if they are equal, false otherwise
    /// </summary>
    public class IntegerComparer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(values[0] is int) || !(values[1] is int))
                return false;

            bool result = (int)values[0] == (int)values[1];
            return result;
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("IntergerComparer cannot convert back");
        }
    }

    public static class BitmapToBitmapSource
    {
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            using (var handle = new SafeHBitmapHandle(source))
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle.DangerousGetHandle(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private sealed class SafeHBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            public SafeHBitmapHandle(Bitmap bitmap)
                : base(true)
            {
                SetHandle(bitmap.GetHbitmap());
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                return DeleteObject(handle) > 0;
            }
        }
    }
}
