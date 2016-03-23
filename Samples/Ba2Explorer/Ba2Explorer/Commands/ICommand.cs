using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Commands
{
    public interface ICommand
    {
        bool CanInvoke();

        void Invoke();
    }
}
