using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApplication3.Components;

namespace WpfApplication3
{
    public class ViewModel : ViewNotifier
    {
        private readonly IStudentsAdapter _model = StudentsAdapter.GetInstance();

        private ObservableCollection<StudentWrapper> _students;
        public ObservableCollection<StudentWrapper> Students
        {
            get { return _students; }
            set
            {
                _students = value;
                OnPropertyChanged("Students");
            }
        }

        private Student _addableStudent = new Student() { Age = 16 };
        public Student AddableStudent
        {
            get { return _addableStudent; }
            set
            {
                _addableStudent = value;
                OnPropertyChanged("AddableStudent");
            }
        }

        private Student _editableStudent;
        public Student EditableStudent
        {
            get { return _editableStudent; }
            set
            {
                _editableStudent = value;
                OnPropertyChanged("EditableStudent");
            }
        }

        public ICommand AddCmd { get; set; }
        public ICommand EditSubmitCmd { get; set; }

        public ViewModel()
        {
            LoadStudents();

            AddCmd = new BaseCommand(AddExecute, AddCanExecute);
            EditSubmitCmd = new BaseCommand(EditSubmitCmdExecute, EditSubmitCmdCanExecute);
        }

        private void LoadStudents()
        {
            Task.Run(() => {
                try
                {
                    var students = _model.GetStudents();
                    if (students == null) return;

                    List<StudentWrapper> wrappers = new List<StudentWrapper>();
                    foreach (var item in students)
                    {
                        StudentWrapper wrapper = new StudentWrapper();
                        wrapper.Student = item;
                        wrapper.RemoveCmd = new BaseCommand(() => { RemoveCmdExecute(wrapper); });
                        wrapper.EditCmd = new BaseCommand(() => { EditCmdExecute(wrapper); });
                        wrappers.Add(CreateStudentWraper(item));
                    }

                    App.Current.Dispatcher.Invoke(() => {
                        Students = new ObservableCollection<StudentWrapper>(wrappers);
                        Students.CollectionChanged += Students_CollectionChanged;
                    });
                }
                catch { }
            });
        }

        void Students_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private bool AddCanExecute()
        {
            try
            {
                if (Students == null) return false;
                if (AddableStudent.Age < 16 && AddableStudent.Age > 100) return false;
                if (AddableStudent.FirstName.Trim() == string.Empty) return false;
                if (AddableStudent.Last.Trim() == string.Empty) return false;
                if (Students.SingleOrDefault(i => (i.Student.FirstName.ToUpper() == AddableStudent.FirstName.Trim().ToUpper() && i.Student.Last.ToUpper() == AddableStudent.Last.Trim().ToUpper())) != null) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void AddExecute()
        {
            try
            {
                uint max = Students.Max((StudentWrapper wrapper) =>
                {
                    return wrapper.Student.Id;
                });
                AddableStudent.Id = max + 1;
                var all = GetStudentsFromWrappers().ToList();
                all.Add(AddableStudent);
                _model.SaveChanges(all);
                var _new = CreateStudentWraper(AddableStudent);
                App.Current.Dispatcher.Invoke(() => {
                    Students.Add(_new);
                    AddableStudent = new Student();
                });
            }
            catch { }
        }
        
        private void RemoveCmdExecute(StudentWrapper item)
        {
            try
            {
                if (MessageBox.Show("Вы точно хотите удалить пользователя?", "Сообщение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var stds = GetStudentsFromWrappers().Where(i => i != item.Student);
                    _model.SaveChanges(stds);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Students.Remove(item);
                        if (EditableStudent != null && EditableStudent.Id == item.Student.Id)
                            EditableStudent = null;
                    });
                }                
            }
            catch { }

        }

        private void EditCmdExecute(StudentWrapper item)
        {
            Student _edit = new Student();
            _edit.Id = item.Student.Id;
            _edit.Age = item.Student.Age;
            _edit.FirstName = item.Student.FirstName;
            _edit.Gender = item.Student.Gender;
            _edit.Last = item.Student.Last;
            App.Current.Dispatcher.Invoke(() => {
                EditableStudent = _edit;
            });
        }

        private bool EditSubmitCmdCanExecute()
        {
            try
            {
                if (EditableStudent == null) return false;
                if (EditableStudent.FirstName.Trim() == string.Empty) return false;
                if (EditableStudent.Last.Trim() == string.Empty) return false;
                if (EditableStudent.Age < 16 && EditableStudent.Age > 100) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EditSubmitCmdExecute()
        {
            Task.Run(() => {
                try
                {
                    EditableStudent.FirstName = EditableStudent.FirstName.Trim();
                    EditableStudent.Last = EditableStudent.Last.Trim();

                    var stds = GetStudentsFromWrappers().ToList();
                    var my = stds.SingleOrDefault(i => i.Id == EditableStudent.Id);
                    var myWrapper = Students.SingleOrDefault(i => i.Student.Id == EditableStudent.Id);
                    int index = Students.IndexOf(myWrapper);
                    App.Current.Dispatcher.Invoke(() => {
                        Students.Remove(myWrapper);
                    });
                    stds.Remove(my);
                    _model.SaveChanges(stds);
                    stds.Add(EditableStudent);
                    _model.SaveChanges(stds);
                    App.Current.Dispatcher.Invoke(() => {
                        var wrp = CreateStudentWraper(EditableStudent);
                        Students.Insert(index, wrp);
                    });
                }
                catch { }
            });
        }

        private IEnumerable<Student> GetStudentsFromWrappers()
        {
            List<Student> result = new List<Student>();
            foreach (var wrapper in Students)
                result.Add(wrapper.Student);
            return result;
        }

        private StudentWrapper CreateStudentWraper(Student student)
        {
            StudentWrapper wrapper = new StudentWrapper();
            wrapper.Student = student;
            wrapper.RemoveCmd = new BaseCommand(() => { RemoveCmdExecute(wrapper); });
            wrapper.EditCmd = new BaseCommand(() => { EditCmdExecute(wrapper); });
            return wrapper;
        }
    }
}
