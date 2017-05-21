using System;

namespace DapperDemo.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Address { set; get; }
    }
}