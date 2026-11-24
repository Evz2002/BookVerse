namespace BookVerse.Domain.Entities;
using BookVerse.Domain.Common;
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}
public static class UserErrors
{
    public static readonly Error DuplicateEmail = new("User.DuplicateEmail", "این ایمیل قبلاً در سیستم ثبت شده است.");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "ایمیل یا رمز عبور اشتباه است.");
}

public sealed class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = UserRoles.Customer;

    private User() { } 

    private User(string fullName, string email, string passwordHash, string role)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }
    public static Result<User> Create(string fullName, string email, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 3)
            return Result.Failure<User>(new Error("User.InvalidName", "نام و نام خانوادگی باید حداقل ۳ کاراکتر باشد."));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Result.Failure<User>(new Error("User.InvalidEmail", "فرمت ایمیل معتبر نیست."));

        var validRole = role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase)
            ? UserRoles.Admin
            : UserRoles.Customer;

        return Result.Success(new User(fullName.Trim(), email.Trim().ToLowerInvariant(), passwordHash, validRole));
    }
}