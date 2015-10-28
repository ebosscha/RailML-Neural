using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RailML___WPF.Data;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Reflection;

namespace RailML___WPF.NeuralNetwork.ViewModels
{
    class ParameterViewModel : BaseViewModel
    {
        public Data.Settings settings { get; set; }
        public CompositeCollection coll { get; set; }

        public ParameterViewModel()
        {
            coll = new CompositeCollection();
            settings = DataContainer.NeuralNetwork.Settings;
            PropertyInfo[] props = settings.GetType().GetProperties();
            foreach(PropertyInfo prop in props)
            {
                ObservableCollection<dynamic> tempcoll = new ObservableCollection<dynamic>();
                tempcoll.Add(prop.GetValue(settings));
                coll.Add(new CollectionContainer() { Collection = tempcoll });

            }
        }


    }


}
