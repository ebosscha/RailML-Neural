﻿/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:RailMLNeural.UI.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using RailMLNeural.UI.Dialog.ViewModel;
using RailMLNeural.UI.Model;
using RailMLNeural.UI.Neural.OutputVisualization.ViewModel;
using RailMLNeural.UI.Neural.ViewModel;
using RailMLNeural.UI.RailML.ViewModel;
using RailMLNeural.UI.Statistics.ViewModel;

namespace RailMLNeural.UI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MainRailMLViewModel>();
            SimpleIoc.Default.Register<MainNeuralViewModel>();
            SimpleIoc.Default.Register<TrackTableViewModel>();
            SimpleIoc.Default.Register<OCPTableViewModel>();
            SimpleIoc.Default.Register<PropertiesViewModel>();
            SimpleIoc.Default.Register<VisualizationViewModel>();
            SimpleIoc.Default.Register<NetworkRenderViewModel>();
            SimpleIoc.Default.Register<ProjectOptionsViewModel>();
            SimpleIoc.Default.Register<NeuralCollectionViewModel>();
            SimpleIoc.Default.Register<NeuralPropertiesViewModel>();
            SimpleIoc.Default.Register<NeuralResultsViewModel>();
            SimpleIoc.Default.Register<NeuralRunManagerViewModel>();
            SimpleIoc.Default.Register<CreateFeedForwardViewModel>();
            SimpleIoc.Default.Register<NetworkStatisticsViewModel>();
            SimpleIoc.Default.Register<DebugViewModel>();
            SimpleIoc.Default.Register<NeuralOutputViewModel>();
            SimpleIoc.Default.Register<CreateRecurrentNetworkViewModel>();
            SimpleIoc.Default.Register<DelayStatisticsViewModel>();
            SimpleIoc.Default.Register<MainStatisticsViewModel>();
            SimpleIoc.Default.Register<CreateRecursiveNetworkViewModel>();
            SimpleIoc.Default.Register<GraphVisualizationViewModel>();
            SimpleIoc.Default.Register<GraphPropertiesViewModel>();
            SimpleIoc.Default.Register<EdgeStatisticsViewModel>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public MainRailMLViewModel MainRailML
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainRailMLViewModel>();
            }
        }

        public MainNeuralViewModel MainNeural
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainNeuralViewModel>();
            }
        }

        public TrackTableViewModel TrackTableVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TrackTableViewModel>();
            }
        }

        public OCPTableViewModel OCPTableVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OCPTableViewModel>();
            }
        }

        public PropertiesViewModel PropertiesVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PropertiesViewModel>();
            }
        }

        public VisualizationViewModel VisualizationVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VisualizationViewModel>();
            }
        }

        public NetworkRenderViewModel NetworkRenderVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NetworkRenderViewModel>();
            }
        }

        public ProjectOptionsViewModel ProjectSettingsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProjectOptionsViewModel>();
            }
        }

        public NeuralCollectionViewModel NeuralCollectionVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NeuralCollectionViewModel>();
            }
        }
        public NeuralPropertiesViewModel NeuralPropertiesVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NeuralPropertiesViewModel>();
            }
        }
        public NeuralResultsViewModel NeuralResultsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NeuralResultsViewModel>();
            }
        }
        public NeuralRunManagerViewModel NeuralRunManagerVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NeuralRunManagerViewModel>();
            }
        }
        public CreateFeedForwardViewModel CreateFeedForwardVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CreateFeedForwardViewModel>();
            }
        }
        public NetworkStatisticsViewModel NetworkStatisticsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NetworkStatisticsViewModel>();
            }
        }
        public DebugViewModel DebugVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DebugViewModel>();
            }
        }
        public NeuralOutputViewModel NeuralOutputVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NeuralOutputViewModel>();
            }
        }

        public CreateRecurrentNetworkViewModel CreateRecurrentNetworkVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CreateRecurrentNetworkViewModel>();
            }
        }

        public DelayStatisticsViewModel DelayStatisticsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DelayStatisticsViewModel>();
            }
        }

        public MainStatisticsViewModel MainStatisticsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainStatisticsViewModel>();
            }
        }

        public CreateRecursiveNetworkViewModel CreateRecursiveNetworkVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CreateRecursiveNetworkViewModel>();
            }
        }

        public GraphVisualizationViewModel GraphVisualizationVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GraphVisualizationViewModel>();
            }
        }

        public GraphPropertiesViewModel GraphPropertiesVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GraphPropertiesViewModel>();
            }
        }

        public EdgeStatisticsViewModel EdgeStatisticsVM
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EdgeStatisticsViewModel>();
            }
        }
           

    

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}