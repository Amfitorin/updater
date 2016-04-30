using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SBUpdater.Helpers
{
    class Command : ICommand
    {
       public Command(Action execute = null, Func<object, bool> canExecuteHandler = null)
		{
			ExecuteHandler = execute  != null ? new Action<object>(_ => execute ()) : (_ => { });
			CanExecuteHandler = canExecuteHandler;
		}
		public Command(Action<object> execute, Func<object, bool> canExecuteHandler = null)
		{
			ExecuteHandler = execute ?? (_ => { });
			CanExecuteHandler = canExecuteHandler;
		}

		public event EventHandler CanExecuteChanged;
		public Action<object> ExecuteHandler { get; private set; }
		public Func<object, bool> CanExecuteHandler { get; private set; }

		public void FireCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, EventArgs.Empty);
		}

		public bool CanExecute(object parameter)
		{
			return CanExecuteHandler == null ? true : CanExecuteHandler(parameter);
		}

        //string _ToolTipText;
        //public string ToolTipText
        //{
        //    get { return _ToolTipText; }
        //    set
        //    {
        //        _ToolTipText = value;
        //        FirePropertyChanged(() => ToolTipText);
        //    }
        //}
        //string _Name;
        //public string Name
        //{
        //    get { return _Name; }
        //    set
        //    {
        //        _Name = value;
        //        FirePropertyChanged(() => Name);
        //    }
        //}
        //object _ImageSource;
        //public object ImageSource
        //{
        //    get { return _ImageSource; }
        //    set
        //    {
        //        _ImageSource = value;
        //        FirePropertyChanged(() => ImageSource);
        //    }
        //}
        //bool _Avaliable = true;
        //public bool Avaliable
        //{
        //    get {return _Avaliable;}
        //    set
        //    {
        //        _Avaliable = value;
        //        FirePropertyChanged(() => Avaliable);
        //    }
        //}
        //bool _Accessable = true;
        //public bool Accessable
        //{
        //    get { return _Accessable; }
        //    set
        //    {
        //        _Accessable = value;
        //        FirePropertyChanged(() => Accessable);
        //    }
        //}

        //public override string ToString()
        //{
        //    return Name ?? base.ToString();
        //}
		public void Execute(object parameter)
		{
			ExecuteHandler(parameter);

			// не работает при показе сплеш-окна через бекграунд-воркер (проблема с потоками)
			//if (CanExecuteChanged != null)
			//    CanExecuteChanged(this, EventArgs.Empty);
		}
    }
}
