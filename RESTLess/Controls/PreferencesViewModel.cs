using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;

namespace RESTLess.Controls
{
    [Export(typeof(PreferencesViewModel))]
    public class PreferencesViewModel : PropertyChangedBase
    {
        private string timeoutTextBox;

        private readonly IEventAggregator eventAggregator;

        public string TimeoutTextBox
        {
            get { return timeoutTextBox; }
            set
            {
                timeoutTextBox = value;
                NotifyOfPropertyChange(() => TimeoutTextBox);
            }
        }


        public PreferencesViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            eventAggregator.Subscribe(this);
        }

        public void SaveButton()
        {
            // TODO : save settings?
        }
    }
}
