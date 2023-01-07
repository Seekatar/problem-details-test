using System.Runtime.Serialization;

namespace ProblemDetailsTest.Models
{
    /// <summary>
    ///
    /// </summary>
    [DataContract]
    public partial class Widget
    {
        public string MyString { get; set; } = $"I was create at {DateTime.Now}";
        public int MyInt { get; set; }
        public DateTime CreatedAt { get; } = DateTime.Now;
    }
}

