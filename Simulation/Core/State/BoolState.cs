using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class BoolState : IBoolState, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Properties

        public bool? previousState;
        public bool? PreviousState
        {
            get { return previousState; }
            set
            {
                if (value != previousState)
                {
                    previousState = value;
                    Notify("PreviousState");
                }
            }
        }

        public bool? state;
        public bool? State
        {
            get { return state; }
            set
            {
                if (value != state)
                {
                    state = value;
                    Notify("State");
                }
            }
        }

        #endregion
    }
}
