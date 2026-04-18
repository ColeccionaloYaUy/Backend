namespace API.Domain.Auth;

public class User {
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public int RoleId { get; set; }
	public string RoleName { get; set; } = string.Empty;
	public bool Active { get; set; }
	public DateTime CreatedAt { get; set; }
}
