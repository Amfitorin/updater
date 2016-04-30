using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBUpdater.ModelViev
{
    public class ModelBase : INotifyPropertyChanged
    {

        PropertyChangedEventHandler PropertyChangedInvokations;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    var invokations = PropertyChangedInvokations;
                    // проверка на повторное подписывание
                    if (invokations == null || !invokations.GetInvocationList().Contains(value))
                        PropertyChangedInvokations += value;
                }
            }
            remove { PropertyChangedInvokations -= value; }
        }
        protected void FirePropertyChanged(string name)
        {
            var invokations = PropertyChangedInvokations;
            try
            {
                if (invokations != null)
                    invokations(this, new PropertyChangedEventArgs(name));
            }
            catch
            {
                throw;
            }
        }
        protected virtual void FirePropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> property)
        {
            var expression = property.Body as System.Linq.Expressions.MemberExpression;
            if (expression == null)
                throw new NotSupportedException("Invalid expression passed. Only property member should be selected.");

            FirePropertyChanged(expression.Member.Name);
        }
    }
}
