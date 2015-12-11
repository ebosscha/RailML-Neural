using RailMLNeural.RailML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RailMLNeural.UI.RailML.Views
{
    /// <summary>
    /// Interaction logic for PropertiesPresenter.xaml
    /// </summary>
    public partial class PropertiesPresenter : UserControl
    {
        private dynamic _selectedElement;
        public PropertiesPresenter(dynamic elem, string name, bool expanded)
        {
            if (elem == null) { return; }
            InitializeComponent();
            _selectedElement = elem;
            this.DataContext = elem;
            this.HeaderLabel.Content = name ?? String.Empty;
            this.PropExpander.Expanded += new RoutedEventHandler(PropExpander_Expanded);
            this.PropExpander.IsExpanded = expanded;
            if (elem.GetType().Namespace == "System.Collections.Generic" && elem.GetType().GetGenericArguments()[0].GetConstructor(Type.EmptyTypes) != null)
            {
                this.AddButton.IsEnabled = true;
            }
            else { this.AddButton.IsEnabled = false; }
            this.HeaderLabel.ContextMenu.MouseLeave += new MouseEventHandler(Expander_MouseLeave);
            this.PropExpander.Collapsed += new RoutedEventHandler(PropExpander_Collapsed);
        }

        private void PropExpander_Expanded(object sender, RoutedEventArgs e)
        {
            dynamic elem = this.DataContext;
            if (elem.GetType().Namespace == "System.Collections.Generic")
            {
                for (int i = 0; i < elem.Count; i++)
                {
                    dynamic listelem = elem[i];

                    if (listelem.GetType().Namespace == "RailMLNeural.RailML" && !listelem.GetType().IsEnum)
                    {
                        PropStack.Children.Add(new PropertiesPresenter(listelem, listelem.id ?? null, false));
                    }
                    else if (listelem.GetType() == typeof(double) || listelem.GetType() == typeof(string) || listelem.GetType() == typeof(decimal))
                    {
                        TextBox textbox = new TextBox() { DataContext = elem, HorizontalAlignment = HorizontalAlignment.Right, MinWidth = 150 };
                        Grid.SetColumn(textbox, 1);
                        textbox.SetBinding(TextBox.TextProperty, new Binding("Item[" + i.ToString() + "]"));
                        PropStack.Children.Add(textbox);

                    }


                }
            }
            else
            {
                foreach (PropertyInfo prop in elem.GetType().GetProperties())
                {

                    Grid property = new Grid();
                    property.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    property.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    Label label = new Label() { DataContext = prop, HorizontalAlignment = HorizontalAlignment.Left };
                    Grid.SetColumn(label, 0);
                    label.Content = prop.Name;
                    property.Children.Add(label);

                    if (prop.PropertyType.Namespace == "RailMLNeural.RailML" && !prop.PropertyType.IsEnum)
                    {
                        PropStack.Children.Add(new PropertiesPresenter(prop.GetValue(elem), prop.Name, false));
                    }
                    else if (prop.PropertyType.Namespace == "System.Collections.Generic")
                    {
                        PropStack.Children.Add(new PropertiesPresenter(prop.GetValue(elem), prop.Name, false));
                    }
                    else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(decimal))
                    {
                        TextBox textbox = new TextBox() { DataContext = elem, HorizontalAlignment = HorizontalAlignment.Right, MinWidth = 150 };
                        Grid.SetColumn(textbox, 1);
                        textbox.SetBinding(TextBox.TextProperty, new Binding((string)prop.Name));
                        property.Children.Add(textbox);
                        PropStack.Children.Add(property);
                    }
                    else if (prop.PropertyType.IsEnum)
                    {
                        ComboBox box = new ComboBox() { DataContext = elem, HorizontalAlignment = HorizontalAlignment.Right, MinWidth = 150 };
                        Grid.SetColumn(box, 1);
                        box.ItemsSource = Enum.GetValues(prop.PropertyType);
                        box.SetBinding(ComboBox.SelectedValueProperty, new Binding((string)prop.Name));
                        property.Children.Add(box);
                        PropStack.Children.Add(property);
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        CheckBox box = new CheckBox() { DataContext = elem, HorizontalAlignment = HorizontalAlignment.Right };


                        Grid.SetColumn(box, 1);
                        box.SetBinding(CheckBox.IsCheckedProperty, new Binding((string)prop.Name));
                        property.Children.Add(box);
                        PropStack.Children.Add(property);
                    }
                    else if (prop.PropertyType == typeof(object))
                    {
                        try { PropStack.Children.Add(new PropertiesPresenter((tConnectionData)prop.GetValue(elem), "Connection", false)); continue; }
                        catch { }
                        try { PropStack.Children.Add(new PropertiesPresenter((tMacroscopicNode)prop.GetValue(elem), "MacroscopicNode", false)); continue; }
                        catch { }
                        try { PropStack.Children.Add(new PropertiesPresenter((tBufferStop)prop.GetValue(elem), "BufferStop", false)); continue; }
                        catch { }
                        try { PropStack.Children.Add(new PropertiesPresenter((tOpenEnd)prop.GetValue(elem), "OpenEnd", false)); continue; }
                        catch { }

                    }

                }
            }

            e.Handled = true;


        }

        public void Add_Click(object sender, EventArgs e)
        {
            Type t = DataContext.GetType().GetGenericArguments()[0];
            if (t != typeof(string) && t != typeof(double) && t != typeof(decimal))
            {
                dynamic newitem = Activator.CreateInstance(t);
                _selectedElement.Add(newitem);
                this.PropStack.Children.Add(new PropertiesPresenter(newitem, null, false));
            }

        }

        public void Expander_MouseLeave(object sender, MouseEventArgs e)
        {
            this.HeaderLabel.ContextMenu.IsOpen = false;
        }

        public void PropExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            PropStack.Children.Clear();
            e.Handled = true;
        }
    }
}
