using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Models
{
    public class ObservableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets field value of property and notifies any observer of any changes
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="field">Field reference</param>
        /// <param name="value">New field value</param>
        /// <param name="propertyName">Name of the property that has been changed</param>
        protected void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (!(field?.Equals(value)) ?? true)
            {
                field = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
