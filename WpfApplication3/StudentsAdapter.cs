using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WpfApplication3
{
    public class StudentsAdapter : IStudentsAdapter
    {
        private static readonly object syncRoot = new object();
        private static IStudentsAdapter _instance;

        private readonly string STUDENTS_FILE_NAME = Environment.CurrentDirectory + @"\studentsList.xml";
        private readonly string OLD_STUDENTS_FILE_NAME = Environment.CurrentDirectory + @"\Students.xml";

        private StudentsAdapter() { }

        public static IStudentsAdapter GetInstance()
        {
            lock (syncRoot)
                if (_instance == null) _instance = new StudentsAdapter();
            return _instance;
        }

        public IEnumerable<Student> GetStudents()
        {
            List<Student> result = new List<Student>();

            try
            {
                IEnumerable<Student> preResult = ReadStudentsFromNewSource();
                if (preResult != null) return preResult;

                XmlDocument doc = new XmlDocument();
                lock (syncRoot)
                {
                    FileStream stream = new FileStream(OLD_STUDENTS_FILE_NAME, FileMode.Open, FileAccess.Read);
                    doc.Load(stream);
                    stream.Close();
                }

                var nav = doc.CreateNavigator();
                var students = nav.Select("//Students/Student");
                students.MoveNext();

                for (int i = 0; i < students.Count; i++)
                {
                    uint id = uint.Parse(students.Current.SelectSingleNode("./@Id").Value);
                    string firstName = students.Current.SelectSingleNode("./FirstName").Value;
                    string lastName = students.Current.SelectSingleNode("./Last").Value;
                    int gender = int.Parse(students.Current.SelectSingleNode("./Gender").Value);
                    byte age = byte.Parse(students.Current.SelectSingleNode("./Age").Value);

                    students.MoveNext();

                    result.Add(new Student() { Id = id, Age = age, FirstName = firstName, Gender = gender, Last = lastName });
                }

                Task.Run(() => {
                    SaveChanges(result);
                });

                return result;
            }
            catch
            {
                return null;
            }
        }

        public void SaveChanges(IEnumerable<Student> students)
        {
            Task.Run(() => {
                try
                {
                    lock (syncRoot)
                    {
                        FileStream stream = new FileStream(STUDENTS_FILE_NAME, FileMode.Create, FileAccess.Write);
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Student>));
                        serializer.Serialize(stream, students.ToList());
                        stream.Close();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private IEnumerable<Student> ReadStudentsFromNewSource()
        {
            IEnumerable<Student> result = null;
            try
            {
                lock (syncRoot)
                {
                    FileStream stream = new FileStream(STUDENTS_FILE_NAME, FileMode.Open, FileAccess.Read);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Student>));
                    result = serializer.Deserialize(stream) as IEnumerable<Student>;
                    stream.Close();
                }
                return result;
            }
            catch
            {
                return result;
            }
        }

    }
}
