using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Univ.Service.Dtos.UserDtos;

namespace Univ.Service.Services.Interfaces
{
    public interface IAuthService
    {
        string Login(UserLoginDto loginDto);
    }
}
