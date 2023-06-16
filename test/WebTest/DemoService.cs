using WebTest.Models;

namespace WebTest;

public class DemoService
{

    static IEnumerable<User> getUsers()
    {
        foreach (var user in Enumerable.Range(1,2000))
        {
            yield return new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = $"user_{user}",
                Email = $"user@{user}.com"
            };
        }
    }


    public Result<User, string> FindByEmail(string email) 
    {
        User user = getUsers().FirstOrDefault(x=>x.Email==email);
        if(user is null) {
            return "No user found";
        }
        return user;
    }

    public Result<List<User>, string> GetAll()
    {
        return getUsers().ToList();
    }
}