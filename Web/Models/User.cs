using System.ComponentModel.DataAnnotations;

namespace Web_for_IotProject.Models
{
    public class User
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public User()
        {

        }

    }
}
