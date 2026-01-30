using RengaBri4kaKernel.AuxFunctions;
using RengaBri4kaKernel.RengaInternalResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RengaBri4kaKernel.UI.Windows
{
    /// <summary>
    /// Interaction logic for Bri4ka_PinCategories.xaml
    /// </summary>
    public partial class Bri4ka_PinCategories : Window
    {
        public Bri4ka_PinCategories()
        {
            InitializeComponent();

            Categories = new Guid[] { };
            pRengaTypes = RengaObjectTypes.GetRengaObjectTypesInfo(true);
            initObjectTypes();

            this.CheckBox_SelecttAll.Checked += CheckBox_SelecttAll_Checked;
            this.Button_SetSelection.Click += Button_SetSelection_Click;

            this.SizeToContent = SizeToContent.Width;
        }

        private void CheckBox_SelecttAll_Checked(object sender, RoutedEventArgs e)
        {
            bool status = this.CheckBox_SelecttAll.IsChecked ?? false;
            foreach (ListViewItem item in this.ListBox_Categories.Items)
            {
                item.IsSelected = status;
            }
        }

        private void Button_SetSelection_Click(object sender, RoutedEventArgs e)
        {
            List<Guid> tmpCategories = new List<Guid>();
            foreach (ListBoxItem item in this.ListBox_Categories.Items)
            {
                if (item.IsSelected)
                {
                    RengaTypeInfo? itemAsPropType = item.Tag as RengaTypeInfo;
                    if (itemAsPropType == null) continue;

                    tmpCategories.Add(itemAsPropType.Id);
                }
            }

            Categories = tmpCategories.ToArray();
            this.DialogResult = true;
            this.Close();
        }

        private void initObjectTypes()
        {

            this.ListBox_Categories.SelectionMode = SelectionMode.Multiple;
            this.ListBox_Categories.Items.Clear();
            foreach (var type in pRengaTypes)
            {
                ListViewItem item = new ListViewItem();
                item.Tag = type;
                item.Content = type.Name;
                this.ListBox_Categories.Items.Add(item);
            }
        }


        public Guid[]? Categories;
        private RengaTypeInfo[] pRengaTypes;
    }
}
