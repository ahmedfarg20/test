using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace projectUsers.Data.Models
{
    public class User:IdentityUser
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public int Age { get; set; }
        public string Phone { get; set; }
        public string Nationality { get; set; }
    }
}
