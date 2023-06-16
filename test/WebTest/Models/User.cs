namespace WebTest.Models;

public class User
{
    public string Id { get; set; }

    public string Name { get; set; }    

    public string Email { get; set; }   
}


public interface IRes<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
}


public class ResultUser : IRes<User>
{
    /// <inheritdoc />
    public bool IsSuccess { get; set; }

    /// <inheritdoc />
    public User Data { get; set; }
}