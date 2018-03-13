using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DT1.Watchdog.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void NotifytViewModelChanged()
        {
            PropertyChanged(this, new PropertyChangedEventArgs( null ) );
        }

        public void NotifytPropertyChanged<TProperty>(Expression<Func<TProperty>> projection)
        {
            var memberExpression = (MemberExpression)projection.Body;
            ForwardNotifytPropertyChanged(memberExpression.Member.Name);
        }

        protected void ForwardNotifytPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
