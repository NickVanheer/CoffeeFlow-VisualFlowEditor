using System;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using CoffeeFlow.Base;
using CoffeeFlow.Nodes;

namespace CoffeeFlow.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NetworkViewModel>();

            PopulateWithTestData();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public NetworkViewModel Network
        {
            get
            {
                
                return ServiceLocator.Current.GetInstance<NetworkViewModel>();
            }
        }
        
        public static void Cleanup()
        {
        }

        public void PopulateWithTestData()
        {
        }

       
    }
}