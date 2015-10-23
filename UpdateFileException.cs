using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    public class UpdateFileException : Exception
    {

        public UpdateFileException() : base()
        {

        }

        public UpdateFileException(string message): base(message)
        {

        }
    }
}
