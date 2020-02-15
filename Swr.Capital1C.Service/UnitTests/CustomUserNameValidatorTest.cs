using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service;
using Xunit;

namespace UnitTests
{
    public class CustomUserNameValidatorTest
    {
        [Fact]
        public void Validate_ShouldReturnOk()
        {
            var userName = "SWR";
            var password = "SWRPassword";

            var nameValidator = new CustomUserNameValidator();
            nameValidator.Validate(userName, password);
        }

        [Fact]
        public void EmptyFields_Validate_ShouldReturnThrowArgumentException()
        {
            var userName = string.Empty;
            var password = string.Empty;

            var nameValidator = new CustomUserNameValidator();
            Assert.Throws<ArgumentException>(() => nameValidator.Validate(userName, password));
        }

        [Fact]
        public void Validate_ShouldReturnFaultException()
        {
            var userName = "SWR1";
            var password = "SWRPassword";

            var nameValidator = new CustomUserNameValidator();
            Assert.Throws<FaultException>(() => nameValidator.Validate(userName, password));
        }
    }
}
