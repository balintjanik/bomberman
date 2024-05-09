using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bomberman.ViewModel
{
    public class BindButton : ViewModelBase
    {
        #region Fields
        string _content;
        #endregion

        #region Properties
        public int Row { get; set; }
        public int Col { get; set; }
        public string Content {
            get { return _content; }
            set
            {
                if (value != _content) {
                    _content = value;
                }
                OnPropertyChanged();
            }
        }
        public DelegateCommand Command { get; set; }
        public string Parameter { get; set; }
        #endregion
    }
}
