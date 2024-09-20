using JWT_Token_Implementation.Models;

namespace JWT_Token_Implementation.Dto;

public class DataModels
{
    public List<LoginModel> UserInformation()
    {
        var Information = new List<LoginModel>()
        {
         new LoginModel{ Username="Rokon", Password="Rokon"},
         new LoginModel{ Username="Ahsan", Password="Ahsan"},
         new LoginModel{ Username="Haibib", Password="Habib"},
         new LoginModel{ Username="Nahid", Password="Nahid"},
         new LoginModel{ Username="demo", Password="password"},
        };
        return Information;
    }
}
