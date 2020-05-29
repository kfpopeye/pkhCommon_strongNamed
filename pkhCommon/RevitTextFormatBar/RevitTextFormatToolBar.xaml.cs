using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System;
using System.Diagnostics;

namespace pkhCommon.WPF
{
    /// <summary>
    /// Interaction logic for RichTextBoxFormatBar.xaml
    /// </summary>
    public partial class RevitTextFormatToolBar : UserControl
    {
        #region Properties
        public int maximumIndentLevel { get; set; }
        public int maximumStartLevel { get; set; }
        public int minimumStartLevel { get; set; }

        #region RichTextBox

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target",
            typeof(global::System.Windows.Controls.RichTextBox),
            typeof(RevitTextFormatToolBar),
            new PropertyMetadata(null, OnRichTextBoxPropertyChanged));
        public global::System.Windows.Controls.RichTextBox Target
        {
            get
            {
                return (global::System.Windows.Controls.RichTextBox)GetValue(TargetProperty);
            }
            set
            {
                SetValue(TargetProperty, value);
            }
        }

        private void OnRichTextBoxSelectionChanged(Object sender, RoutedEventArgs e)
        {
            Update();
        }

        private static void OnRichTextBoxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RevitTextFormatToolBar formatBar = d as RevitTextFormatToolBar;
        }

        #endregion //RichTextBox

        #endregion

        #region Constructors

        public RevitTextFormatToolBar()
        {
            InitializeComponent();
            Loaded += FormatToolbar_Loaded;
        }

        #endregion //Constructors

        #region Methods

        public void Update()
        {
            UpdateToggleButtonState();
            UpdateSelectionListType();
            UpdateIndentButtons();
        }

        private void UpdateIndentButtons()
        {
            _btnIncreaseIndent.IsEnabled = false;
            _btnDecreaseIndent.IsEnabled = false;

            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                                   ? Target.Selection.Start.Paragraph
                                                   : null;

            if (startParagraph == null)
                return;

            if (startParagraph.TextIndent < maximumIndentLevel)
                _btnIncreaseIndent.IsEnabled = true;
            if (startParagraph.TextIndent > 0)
                _btnDecreaseIndent.IsEnabled = true;
        }

        private void UpdateToggleButtonState()
        {
            UpdateItemCheckedState(_btnBold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateItemCheckedState(_btnItalic, TextElement.FontStyleProperty, FontStyles.Italic);

            var currentValue = Target.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            TextDecorationCollection collection = null;
            if (currentValue is TextDecorationCollection && currentValue != DependencyProperty.UnsetValue)
                collection = currentValue as TextDecorationCollection;
            _btnUnderline.IsChecked = collection != null && collection.Count > 0;

            UpdateItemCheckedState(_btnSubscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
            UpdateItemCheckedState(_btnSuperscript, Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
        }

        void UpdateItemCheckedState(ToggleButton button, DependencyProperty formattingProperty, object expectedValue)
        {
            object currentValue = DependencyProperty.UnsetValue;
            if ((Target != null) && (Target.Selection != null))
            {
                currentValue = Target.Selection.GetPropertyValue(formattingProperty);
            }
            button.IsChecked = ((currentValue == null) || (currentValue == DependencyProperty.UnsetValue))
                                ? false
                                : currentValue != null && currentValue.Equals(expectedValue);
        }

        /// <summary>
        /// Updates the visual state of the List styles, such as Numbers and Bullets.
        /// </summary>
        private void UpdateSelectionListType()
        {
            _btnBullets.IsChecked = false;
            _btnNumbers.IsChecked = false;
            _btnUppercase.IsChecked = false;
            _btnLowercase.IsChecked = false;
            _btnNone.IsChecked = false;
            _btnIncreaseStartnumber.IsEnabled = false;
            _btnDecreaseStartnumber.IsEnabled = false;

            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                        ? Target.Selection.Start.Paragraph
                                        : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;
            if (startParagraph != null &&
                endParagraph != null &&
                (startParagraph.Parent is ListItem) &&
                (endParagraph.Parent is ListItem) &&
                object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
            {
                if (((ListItem)startParagraph.Parent).List.Parent is ListItem)
                {
                    _btnIncreaseStartnumber.IsEnabled = false;
                    _btnDecreaseStartnumber.IsEnabled = false;
                }
                else
                {
                    int startIndex = ((ListItem)startParagraph.Parent).List.StartIndex;
                    if (startIndex > minimumStartLevel)
                        _btnDecreaseStartnumber.IsEnabled = true;
                    if (startIndex < maximumStartLevel)
                        _btnIncreaseStartnumber.IsEnabled = true;
                }

                TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                if (markerStyle == TextMarkerStyle.Disc) //bullets
                {
                    _btnBullets.IsChecked = true;
                    _btnIncreaseStartnumber.IsEnabled = false;
                    _btnDecreaseStartnumber.IsEnabled = false;
                }
                else if (markerStyle == TextMarkerStyle.Decimal) //numbers
                {
                    _btnNumbers.IsChecked = true;
                }
                else if (markerStyle == TextMarkerStyle.UpperLatin) //uppercase
                {
                    _btnUppercase.IsChecked = true;
                }
                else if (markerStyle == TextMarkerStyle.LowerLatin) //lowercase
                {
                    _btnLowercase.IsChecked = true;
                }
            }
            else if (startParagraph != null &&
               endParagraph != null &&
               !(startParagraph.Parent is ListItem) &&
               !(endParagraph.Parent is ListItem))
            {
                _btnNone.IsChecked = true;
                _btnIncreaseStartnumber.IsEnabled = false;
                _btnDecreaseStartnumber.IsEnabled = false;
            }
        }
        #endregion //Methods

        #region Event Hanlders

        void FormatToolbar_Loaded(object sender, RoutedEventArgs e)
        {
            Target.SelectionChanged += new RoutedEventHandler(OnRichTextBoxSelectionChanged);
            Update();
        }

        private void _btnBullets_Click(object sender, RoutedEventArgs e)
        {
            ApplyListStyle(TextMarkerStyle.Disc);
        }

        private void UpperLatin_Clicked(object sender, RoutedEventArgs e)
        {
            ApplyListStyle(TextMarkerStyle.UpperLatin);
        }

        private void LowerLatin_Clicked(object sender, RoutedEventArgs e)
        {
            ApplyListStyle(TextMarkerStyle.LowerLatin);
        }

        private void _btnNumbers_Click(object sender, RoutedEventArgs e)
        {
            ApplyListStyle(TextMarkerStyle.Decimal);
        }

        private void ApplyListStyle(TextMarkerStyle _listStyle)
        {
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                                   ? Target.Selection.Start.Paragraph
                                                   : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;
            if (startParagraph == null || endParagraph == null)
                return;

            int startOffset = Target.Document.ContentStart.GetOffsetToPosition(Target.Selection.Start);
            int endOffset = Target.Document.ContentStart.GetOffsetToPosition(Target.Selection.End);
            TextPointer startPntr = Target.Selection.Start;
            TextPointer movingPntr = Target.Document.ContentStart.GetPositionAtOffset(startOffset);
            TextPointer endPntr = Target.Selection.End;
            Paragraph p = null;
            int x = 0;
            List existingList = null;
            Block nextBlock = endParagraph.NextBlock;

            //check to see if block before selection is a 1st level list
            Block prevBlock = startParagraph.PreviousBlock;
            if (prevBlock is List)
                existingList = prevBlock as List;
            else if (prevBlock is Section)
            {
                Section s = prevBlock as Section;
                prevBlock = s.Blocks.FirstBlock;
                if (prevBlock is List)
                    existingList = prevBlock as List;
            }

            //set marker style of each listitem between start and end paragraphs
            do
            {
                p = movingPntr.Paragraph;
                if (p != null)
                {
                    Target.Selection.Select(p.ElementStart, p.ElementEnd);
                    if (p.Parent is ListItem)   //if this is already a listitem
                    {
                        if (existingList == null)
                        {
                            existingList = ((ListItem)p.Parent).List;
                        }
                    }
                    else   //else this is a paragraph. Turn it into a listitem
                    {
                        EditingCommands.ToggleNumbering.Execute(null, Target);
                        if (existingList != null)
                            existingList.ListItems.Add(p.Parent as ListItem);
                        else
                            existingList = ((ListItem)p.Parent).List;
                    }
                    existingList.MarkerStyle = _listStyle;
                    endOffset = Target.Document.ContentStart.GetOffsetToPosition(endPntr);  //end text pointer will move as text is changed
                }
                do   //set moving pointer to next elementstart context
                {
                    movingPntr = movingPntr.GetNextContextPosition(LogicalDirection.Forward);
                    x = Target.Document.ContentStart.GetOffsetToPosition(movingPntr);
                } while (x < endOffset &&
                         movingPntr.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart);
            }
            while (x < endOffset);

            //check next paragraph. If list merge with previous list
            //check if list style is the same????? No, Revit doesn't.
            if (nextBlock is Section)   //section indicates new list not a sublist
            {
                Section s = nextBlock as Section;
                nextBlock = s.Blocks.FirstBlock;
                if (nextBlock is List)
                {
                    ListItem[] lia = new ListItem[(nextBlock as List).ListItems.Count];
                    (nextBlock as List).ListItems.CopyTo(lia, 0);
                    for (int i = 0; i < lia.Length; ++i)
                    {
                        existingList.ListItems.Add(lia[i]);
                    }
                }
            }

            Target.Selection.Select(startPntr, endPntr);
            Update();
            Target.Focus();
        }

        private void _btnNone_Click(object sender, RoutedEventArgs e)
        {
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                       ? Target.Selection.Start.Paragraph
                                       : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;
            if (startParagraph == null || endParagraph == null)
                return;

            if (!(startParagraph.Parent is ListItem))
                return;

            //set marker style of each listitem between start and end paragraphs
            int startOffset = Target.Document.ContentStart.GetOffsetToPosition(Target.Selection.Start);
            int endOffset = Target.Document.ContentStart.GetOffsetToPosition(Target.Selection.End);
            TextPointer startPntr = Target.Selection.Start;
            TextPointer movingPntr = Target.Document.ContentStart.GetPositionAtOffset(startOffset);
            TextPointer endPntr = Target.Selection.End;
            Paragraph p = null;
            int x = 0;
            TextPointer insertionPoint = null;
            TextRange tr = null;

            //copy paragraphs to end of list
            insertionPoint = pkhCommon.WPF.FlowDocumentHelpers.getTopLevelList(movingPntr.Paragraph).ElementEnd;
            insertionPoint = insertionPoint.GetNextInsertionPosition(LogicalDirection.Forward);
            if (insertionPoint == null || insertionPoint.CompareTo(Target.Document.ContentEnd) > 0)
                insertionPoint = Target.Document.ContentEnd;
            do
            {
                p = movingPntr.Paragraph;
                if (p != null)
                {
                    if (p.Parent is ListItem)
                    {
                        tr = new TextRange(p.ContentStart, p.ContentEnd);
                        Debug.WriteLine("_btnNone_Click() copy paragraphs: " + tr.Text);
                        Target.Selection.Select(p.ElementStart, p.ElementEnd);
                        Target.Copy();
                        Target.Selection.Select(insertionPoint, insertionPoint);
                        Target.Paste();
                        insertionPoint.InsertParagraphBreak();
                        insertionPoint = insertionPoint.GetNextInsertionPosition(LogicalDirection.Forward);
                        if (insertionPoint == null || insertionPoint.CompareTo(Target.Document.ContentEnd) > 0)
                            insertionPoint = Target.Document.ContentEnd;
                    }
                }
                do
                {
                    movingPntr = movingPntr.GetNextContextPosition(LogicalDirection.Forward);
                    if (movingPntr == null || movingPntr.CompareTo(Target.Document.ContentEnd) > 0)
                        movingPntr = Target.Document.ContentEnd;
                    x = Target.Document.ContentStart.GetOffsetToPosition(movingPntr);
                } while (x < endOffset &&
                         movingPntr.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart);
            }
            while (x < endOffset);

            //copy remaining listitems to new list after paragraphs
            do
            {
                movingPntr = movingPntr.GetNextContextPosition(LogicalDirection.Forward);
            } while (movingPntr.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart);
            List topList = FlowDocumentHelpers.getTopLevelList(p);
            TextPointer endOfList = topList.ElementEnd;
            endOffset = Target.Document.ContentStart.GetOffsetToPosition(endOfList);
            List newList = new List();
            newList.MarkerStyle = topList.MarkerStyle;
            insertionPoint = insertionPoint.GetNextInsertionPosition(LogicalDirection.Backward);
            if (insertionPoint == null || insertionPoint.CompareTo(Target.Document.ContentEnd) > 0)
                insertionPoint = Target.Document.ContentEnd;
            do
            {
                p = movingPntr.Paragraph;
                if (p != null)
                    if (p.Parent is ListItem)
                    {
                        tr = new TextRange(p.ContentStart, p.ContentEnd);
                        Debug.WriteLine("_btnNone_Click() copy listitems: " + tr.Text);
                        newList.ListItems.Add(p.Parent as ListItem);
                    }
                do
                {
                    movingPntr = movingPntr.GetNextContextPosition(LogicalDirection.Forward);
                    if (movingPntr == null || movingPntr.CompareTo(Target.Document.ContentEnd) > 0)
                        movingPntr = Target.Document.ContentEnd;
                    x = Target.Document.ContentStart.GetOffsetToPosition(movingPntr);
                } while (x < endOffset &&
                         movingPntr.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.ElementStart);
            }
            while (x < endOffset);
            if (newList.ListItems.Count != 0)
                Target.Document.Blocks.InsertAfter(insertionPoint.Paragraph, newList);

            //remove listitems from existing list
            TextPointer tp = (startPntr.Paragraph.ElementStart.GetNextInsertionPosition(LogicalDirection.Backward) != null) 
                ? startPntr.Paragraph.ElementStart.GetNextInsertionPosition(LogicalDirection.Backward) 
                : startPntr.Paragraph.ElementStart;
            Target.Selection.Select(tp, endPntr.Paragraph.ElementEnd);
            tr = new TextRange(tp, endPntr.Paragraph.ElementEnd);
            Debug.WriteLine("_btnNone_Click() cut listitems: " + tr.Text);
            Target.Cut();

            if (topList.ListItems.Count == 0)
            {
                Target.Selection.Select(topList.ElementStart, topList.ElementEnd);
                Target.Cut();
            }

            Update();
            Target.Focus();
        }

        private void _btnIncreaseStartnumber_Click(object sender, RoutedEventArgs e)
        {
            //can only be used on top level lists is handles by UpdateIndentButtons()
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.Start.Paragraph
                                      : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;

            if (startParagraph == null || endParagraph == null)
                return;

            if (!(startParagraph.Parent is ListItem))
                return;

            int x = ((ListItem)startParagraph.Parent).List.StartIndex;
            if (x < maximumStartLevel)
                ((ListItem)startParagraph.Parent).List.StartIndex = ++x;
            Update();
            Target.Focus();
        }

        private void _btnDecreaseStartnumber_Click(object sender, RoutedEventArgs e)
        {
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.Start.Paragraph
                                      : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;

            if (startParagraph == null || endParagraph == null)
                return;

            if (!(startParagraph.Parent is ListItem))
                return;

            int x = ((ListItem)startParagraph.Parent).List.StartIndex;
            if (x > minimumStartLevel)
                ((ListItem)startParagraph.Parent).List.StartIndex = --x;
            Update();
            Target.Focus();
        }

        private void _btnSubscript_Click(object sender, RoutedEventArgs e)
        {
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                       ? Target.Selection.Start.Paragraph
                                       : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;
            if (startParagraph != null &&
                endParagraph != null)
            {
                Inline i = Target.Selection.Start.Parent as Inline;
                if (i.BaselineAlignment != BaselineAlignment.Subscript)
                {
                    Target.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
                    Target.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 12d);
                }
                else
                {
                    i.SetCurrentValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                    i.SetCurrentValue(Inline.FontSizeProperty, DependencyProperty.UnsetValue);
                }
            }
        }

        private void _btnSuperscript_Click(object sender, RoutedEventArgs e)
        {
            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                       ? Target.Selection.Start.Paragraph
                                       : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;
            if (startParagraph != null &&
                endParagraph != null)
            {
                Inline i = Target.Selection.Start.Parent as Inline;
                if (i.BaselineAlignment != BaselineAlignment.Superscript)
                {
                    Target.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
                    Target.Selection.ApplyPropertyValue(Inline.FontSizeProperty, 12d);
                }
                else
                {
                    i.SetCurrentValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
                    i.SetCurrentValue(Inline.FontSizeProperty, DependencyProperty.UnsetValue);
                }
            }
        }

        private void _btnIncreaseIndent_Click(object sender, RoutedEventArgs e)
        {

            Paragraph startParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.Start.Paragraph
                                      : null;
            Paragraph endParagraph = ((Target != null) && (Target.Selection != null))
                                      ? Target.Selection.End.Paragraph
                                      : null;

            if (startParagraph == null || endParagraph == null)
                return;

            if (startParagraph.TextIndent < maximumIndentLevel)
                startParagraph.TextIndent++;
                //EditingCommands.IncreaseIndentation.Execute(null, Target);

            Update();
            Target.Focus();
        }
        #endregion //Event Hanlders
    }
}