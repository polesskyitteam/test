using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication3
{
    public interface IStudentsAdapter
    {
        IEnumerable<Student> GetStudents();
        void SaveChanges(IEnumerable<Student> students);
    }
}
