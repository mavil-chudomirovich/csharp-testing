using Application.Constants;
using Application.Dtos.User.Request;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IAuthService
    {
        Task<string?> Login(UserLoginReq user);
    }
}
