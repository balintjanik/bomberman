using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Bomberman.Persistence
{
    public class MessageEventArgs : EventArgs
    {
        #region Fields
        private string _sender;
        private string _message;
        #endregion

        #region Properties
        public string Sender { get { return _sender; } }
        public string Message { get { return _message; } }
        #endregion

        public MessageEventArgs(string sender, string message) { 
            _sender = sender;
            _message = message;
        }
    }
}
