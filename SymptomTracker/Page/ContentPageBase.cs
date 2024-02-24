using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.Page
{
    public class ContentPageBase : ContentPage
    {
        protected override void OnAppearing()
        {
            if(BindingContext is ViewModelBase viewModel)            
                viewModel.OnAppearing();       
            
            base.OnAppearing();
        }
    }
}
