using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniversalRingApi.Entities;

namespace UniversalRingApi.NUnitTests
{
    /// <summary>
    /// Demonstration to show that NUnit and NSubstitute work just as well as MSTest and MOQ.
    /// </summary>
    [TestFixture]
    public class RingApiNUnitTests
    {
        #region Public Properties
        public static string Username => "Someone@gmail.com";

        public static string Password => "Blah";

        public static string AuthenticateResponseFilename => "TestData\\AuthenticateResponse.json";

        public static string DevicesResponseFilename => "TestData\\DevicesResponse.json";

        public static string DoorbotHistoryResponseFilename => "TestData\\DoorbotHistoryResponse.json";

        public static string DoorbotDownloadFileResponseFilename => "TestData\\TestFile.txt";

        public static byte[] ExpectedAuthenticationResponseBytes { get; set; }

        public static Session ExpectedAuthenticationSession { get; set; }

        public static byte[] ExpectedDevicesResponseBytes { get; set; }

        public static Devices ExpectedDevices { get; set; }

        public static byte[] ExpectedDoorbotsHistoryResponseBytes { get; set; }

        public static List<DoorbotHistoryEvent> ExpectedDoorbotsHistoryList { get; set; }

        public static byte[] ExpectedDoorbotsDownloadFileResponseBytes { get; set; }

        #endregion

        public RingApiNUnitTests()
        {
            // Read-in the AuthenticateResponse.json test data - as a byte array, to be utilized by the mocked HttpWebResponse
            ExpectedAuthenticationResponseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(AuthenticateResponseFilename));

            // Convert the AuthenticationResponse json to a Session object - to be utilized for comparison against returned AuthenticationResponse
            ExpectedAuthenticationSession = JsonConvert.DeserializeObject<Session>(Encoding.UTF8.GetString(ExpectedAuthenticationResponseBytes));

            // Read-in the DevicesResponse.json test data - as a byte array, to be utilized by the mocked HttpWebResponse
            ExpectedDevicesResponseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(DevicesResponseFilename));

            // Convert the DevicesResponse.json to a Session object - to be utilized for comparison against returned DevicesResponse
            ExpectedDevices = JsonConvert.DeserializeObject<Devices>(Encoding.UTF8.GetString(ExpectedDevicesResponseBytes));

            // Read-in the DoorbotHistoryResponse.json test data - as a byte array, to be utilized by the mocked HttpWebResponse
            ExpectedDoorbotsHistoryResponseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(DoorbotHistoryResponseFilename));

            // Convert the DoorbotHistoryResponse.json to a DoorbotHistoryEvent object - to be utilized for comparison against returned DoorbotHistoryEvent
            ExpectedDoorbotsHistoryList = JsonConvert.DeserializeObject<List<DoorbotHistoryEvent>>(Encoding.UTF8.GetString(ExpectedDoorbotsHistoryResponseBytes));

            // Read-in the testFile.txt file - to be utilized as a mock'd 'downloaded' file
            ExpectedDoorbotsDownloadFileResponseBytes = Encoding.UTF8.GetBytes(File.ReadAllText(DoorbotDownloadFileResponseFilename));
        }

        [Test]
        public async Task Authenticate_ExpectSuccess()
        {
            // ARRANGE

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)
            var mockHttpWebRequest = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password) { AuthRequest = mockHttpWebRequest };
            var actualAuthenticationSession = await comm.Authenticate();

            //ASSERT
            ObjectCompare(ExpectedAuthenticationSession, actualAuthenticationSession);
        }

        [Test]
        public async Task Authenticate_VerifyToken()
        {
            // ARRANGE

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)
            var mockHttpWebRequest = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password) { AuthRequest = mockHttpWebRequest };
            var actualAuthenticationSession = await comm.Authenticate();

            // ASSERT
            Assert.IsTrue(!string.IsNullOrEmpty(actualAuthenticationSession.Profile.AuthenticationToken), "Failed to authenticate");
        }

        [Test]
        public async Task Authenticate_VerifyCredentialsEncoded()
        {
            // ARRANGE

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)
            var mockHttpWebRequest = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password) { AuthRequest = mockHttpWebRequest };
            var actualSessionObject = await comm.Authenticate();

            var base64DecodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(comm.CredentialsEncoded));
            Assert.AreEqual(base64DecodedCredentials, $"{Username}:{Password}", "Base64 Credential Decoding failed");
        }

        [Test]
        public async Task GetRingDevices_Verify()
        {
            // ARRANGE

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- AUTH
            var mockHttpWebRequestAuth = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- Devices
            var mockHttpWebRequestDevices = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedDevicesResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password)
            {
                AuthRequest = mockHttpWebRequestAuth,
                DevicesRequest = mockHttpWebRequestDevices
            };

            // Authenticate
            var actualSessionAuthObject = await comm.Authenticate();

            var actualDevices = await comm.GetRingDevices();
            Assert.IsTrue(actualDevices.Chimes.Count > 0 && actualDevices.Doorbots.Count > 0, "No doorbots and/or chimes returned");
        }

        [Test]
        public async Task GetDoorbotsHistory_Verify()
        {
            // ARRANGE

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- AUTH
            var mockHttpWebRequestAuth = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- Devices
            var mockHttpWebRequestDoorbotHistory = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedDoorbotsHistoryResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password)
            {
                AuthRequest = mockHttpWebRequestAuth,
                DoorbotHistoryRequest = mockHttpWebRequestDoorbotHistory
            };

            // Authenticate
            var actualSessionAuthObject = await comm.Authenticate();

            // Acquire Doorbot History
            var actualDoorbotsHistoryList = await comm.GetDoorbotsHistory();

            Assert.AreEqual(ExpectedDoorbotsHistoryList.Count, actualDoorbotsHistoryList.Count, "The DoorbotsHistoryList doesn't contain the same number of items as expected");

            // Compare all itesm within the ExpectedList and the actual
            var cnt = 0;
            foreach (var expected in ExpectedDoorbotsHistoryList)
            {
                ObjectCompare(expected, actualDoorbotsHistoryList[cnt]);
                cnt++;
            }
        }

        [Test]
        public async Task GetDoorbotHistoryRecording_Verify()
        {
            // ARRANGE

            // Set the path to the acquired 'downlaoaded' file (no file is actually downloaded, thanks to MOQ :) )
            var tempFilePath = Path.GetTempFileName();

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- AUTH
            var mockHttpWebRequestAuth = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedAuthenticationResponseBytes);

            // Mock the HttpWebRequest and HttpWebResponse (which is within the request)- Devices
            var mockHttpWebRequestDoorbotDownloadFile = CreateMockHttpWebRequest(HttpStatusCode.NotModified, "A-OK", ExpectedDoorbotsDownloadFileResponseBytes);

            // ACT
            var comm = new RingCommunications(Username, Password)
            {
                AuthRequest = mockHttpWebRequestAuth,
                DoorbotFileRequest = mockHttpWebRequestDoorbotDownloadFile
            };

            // Authenticate
            var actualSessionAuthObject = await comm.Authenticate();

            // Acquire a Doorbot file
            await comm.GetDoorbotHistoryRecordingAndCreateFile("1", tempFilePath);

            // ASSERT

            // Read-in the 'expected' download-file bytes
            var acquiredFileBytes = File.ReadAllBytes(tempFilePath);

            // Let's cleanup the temp file, regardless of pass-or-fail
            File.Delete(tempFilePath);

            CollectionAssert.AreEqual(ExpectedDoorbotsDownloadFileResponseBytes, acquiredFileBytes, "Expected file bytes are not matching with the acquired");
        }

        /// <summary>
        /// Compare Objects and all fields within.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        private static void ObjectCompare(object expected, object actual)
        {
            PropertyInfo[] properties = expected.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object expectedValue = property.GetValue(expected, null);
                object actualValue = property.GetValue(actual, null);

                // if the following types exist, let's get recursive, because they contain one or more of their own fields that we must verify.
                if (expectedValue is SessionFeatures || expectedValue is Profile
                    || expectedValue is DoorbotHistoryEventRecording || expectedValue is Doorbot)
                {
                    ObjectCompare(expectedValue, actualValue);
                    break;
                }

                if (expectedValue is IList)
                {
                    CollectionAssert.AreEqual(expectedValue as IList, actualValue as IList);
                }
                else
                {
                    Assert.AreEqual(expectedValue, actualValue, "Property {0}.{1} does not match. Expected: {2} but was: {3}",
                        property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                }
            }
        }

        /// <summary>
        /// Create a full, Mock object for the HttpWebRequest
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="statusDescription"></param>
        /// <param name="responseBytes"></param>
        /// <returns></returns>
        private static HttpWebRequest CreateMockHttpWebRequest(HttpStatusCode httpStatusCode, string statusDescription, byte[] responseBytes)
        {
            var requestBytes = Encoding.ASCII.GetBytes("Blah Blah Blah");
            Stream requestStream = new MemoryStream();
            Stream responseStream = new MemoryStream();

            using (var memStream = new MemoryStream(requestBytes))
            {
                memStream.CopyTo(requestStream);
                requestStream.Position = 0;
            }

            using (var responseMemStream = new MemoryStream(responseBytes))
            {
                responseMemStream.CopyTo(responseStream);
                responseStream.Position = 0;
            }

            var response = Substitute.For<HttpWebResponse>();
            response.StatusCode.Returns(httpStatusCode);
            response.GetResponseStream().Returns(responseStream);
            response.StatusDescription.Returns(statusDescription);

            var request = Substitute.For<HttpWebRequest>();
            request.GetRequestStreamAsync().Returns(requestStream);
            request.RequestUri.Returns(new Uri("https://www.blah.com"));
            request.GetResponseAsync().Returns(response);

            return request;
        }
    }
}
