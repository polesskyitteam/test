using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApplication3.Components;

namespace WpfApplication3
{
    public class StudentWrapper
    {
        public Student Student { get; set; }

        public ICommand RemoveCmd { get; set; }

        public ICommand EditCmd { get; set; }
    }
}
