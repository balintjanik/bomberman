using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Bomberman.ViewModel
{
    public class MessageType : ViewModelBase
    {
        #region Fields
        private string _sender;
        private string _message;
        private SolidColorBrush _color;
        #endregion

        #region Properties
        public string Sender {
            get { return _sender; }
            set
            {
                if (value != _sender) {
                    _sender = value;
                    OnPropertyChanged(nameof(Sender));
                }
                    
            }
        }
        public string Message {
            get { return _message; }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }

            }
        }
        public SolidColorBrush Color {
            get { return _color; }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    OnPropertyChanged(nameof(Color));
                }

            }
        }
        #endregion

        #region Constructor
        public MessageType(string sender, string message) { 
            _sender = "[" + sender + "]";
            _message = message;
            switch (sender)
            {
                case "WALL":
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#e9bd02");
                    break;
                case "PLAYER1":
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#19c3e7");
                    break;
                case "PLAYER2":
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#d44141");
                    break;
                case "PLAYER3":
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#a13cb7");
                    break;
                case "INFO":
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#027148");
                    break;
                default:
                    _color = (SolidColorBrush)new BrushConverter().ConvertFrom("#000000");
                    break;
            }
        }
        #endregion
    }
}
