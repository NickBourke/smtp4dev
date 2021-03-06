﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rnwood.SmtpServer.Extensions.Auth;

namespace Rnwood.SmtpServer.Tests.Extensions.Auth
{
    [TestClass]
    public class LoginMechanismProcessorTests : AuthMechanismTest
    {
        [TestMethod]
        public void ProcessRepsonse_NoUsername_GetUsernameChallenge()
        {
            Mocks mocks = new Mocks();

            LoginMechanismProcessor processor = Setup(mocks);
            AuthMechanismProcessorStatus result = processor.ProcessResponse(null);

            Assert.AreEqual(AuthMechanismProcessorStatus.Continue, result);
            mocks.Connection.Verify(c =>
                c.WriteResponse(
                        It.Is<SmtpResponse>(r =>
                           r.Code == (int)StandardSmtpResponseCode.AuthenticationContinue &&
                           VerifyBase64Response(r.Message, "Username:")
                        )
                )
            );
        }

        [TestMethod]
        public void ProcessRepsonse_Username_GetPasswordChallenge()
        {
            Mocks mocks = new Mocks();

            LoginMechanismProcessor processor = Setup(mocks);
            AuthMechanismProcessorStatus result = processor.ProcessResponse(EncodeBase64("rob"));

            Assert.AreEqual(AuthMechanismProcessorStatus.Continue, result);

            mocks.Connection.Verify(c =>
                c.WriteResponse(
                    It.Is<SmtpResponse>(r =>
                        VerifyBase64Response(r.Message, "Password:")
                        && r.Code == (int)StandardSmtpResponseCode.AuthenticationContinue
                    )
                )
            );
        }

        [TestMethod]
        [ExpectedException(typeof(BadBase64Exception))]
        public void ProcessResponse_Response_BadBase64()
        {
            Mocks mocks = new Mocks();

            LoginMechanismProcessor processor = Setup(mocks);
            processor.ProcessResponse(null);
            processor.ProcessResponse("rob blah");
        }

        private LoginMechanismProcessor Setup(Mocks mocks)
        {
            return new LoginMechanismProcessor(mocks.Connection.Object);
        }
    }
}