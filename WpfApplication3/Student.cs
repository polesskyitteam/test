using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication3
{
    [Serializable]
    public class Student
    {
        public string GenderView
        {
            get { return Gender == 0 ? "Мужской" : "Женский"; }
        }
        public string NameView
        {
            get { return FirstName + " " + Last; }
        }
        public string AgeView
        {
            get { return Age.ToString() + "лет"; }
        }

        public uint Id { get; set; }
        public string FirstName { get; set; }
        public string Last { get; set; }
        public byte Age { get; set; }
        public int Gender { get; set; }
    }
}
