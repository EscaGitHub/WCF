using System;
using System.ServiceModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;


namespace Swr.Capital1C.Service
{
    public class CustomUserNameValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Данные для проверки пустые");

            if (userName != "SWR" || password != "SWRPassword")
                throw new FaultException("Неверное имя пользователя или пароль");
        }
    }
}