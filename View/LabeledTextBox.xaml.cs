using Autodesk.Internal.InfoCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WellCalculations2010.View
{
    /// <summary>
    /// Логика взаимодействия для LabeledTextBox.xaml
    /// </summary>
    public partial class LabeledTextBox : UserControl
    {

        public static readonly DependencyProperty labelContentProperty = 
            DependencyProperty.Register("LabelContent", typeof(string), typeof(LabeledTextBox),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLabelContentChanged));

        public static readonly DependencyProperty labelFontSizeProperty =
            DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(LabeledTextBox), 
                new FrameworkPropertyMetadata(default(double),FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLabelFontSizeChanged));

        public static readonly DependencyProperty textBoxTextProperty =
            DependencyProperty.Register(nameof(TextBoxText), typeof(string), typeof(LabeledTextBox), new FrameworkPropertyMetadata(
            null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextBoxTextChanged));

        private static void OnLabelContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabeledTextBox control = (LabeledTextBox)d;
            control.label.Content = e.NewValue;
        }
        private static void OnLabelFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabeledTextBox control = (LabeledTextBox)d;
            control.label.FontSize = (double)e.NewValue;
        }
        private static void OnTextBoxTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LabeledTextBox control = (LabeledTextBox)d;
            control.textBox.Text = (string)e.NewValue;
        }

        public string LabelContent
        {
            get => (string)GetValue(labelContentProperty);
            set => SetValue(labelContentProperty, value);
        }
        public double LabelFontSize
        {
            get => (double)GetValue(labelFontSizeProperty);
            set => SetValue(labelFontSizeProperty, value);
        }
        public string TextBoxText
        {
            get => (string)GetValue(textBoxTextProperty);
            set => SetValue(textBoxTextProperty, value);
        }

        public LabeledTextBox()
        {
            InitializeComponent();
        }
    }
}
