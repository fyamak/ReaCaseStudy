using Business.Services.Security.Auth.Jwt.Interface;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters;

public class AuthorizeAttribute : TypeFilterAttribute
{
    public AuthorizeAttribute(params UserType[] roles) : base(typeof(AuthorizeFilter))
    {
        Arguments = [roles];
    }

    private class AuthorizeFilter : IAuthorizationFilter
    {
        private readonly UserType[] _roles;

        private readonly IUserContext _userContext;

        public AuthorizeFilter(IUserContext userContext, params UserType[] roles)
        {
            _roles = roles;

            _userContext = userContext;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            bool canAccess;

            try
            {
                canAccess = _userContext.IsAuthenticated && (_roles.Length == 0 || _roles.Contains(_userContext.GetUserType()));
            }
            catch
            {
                canAccess = false;
            }

            if (!canAccess)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
