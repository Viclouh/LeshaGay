namespace API.Models
{
    public class UserAuthData
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
