using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.model.agreements;
using IO.Swagger.model.transientDocuments;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Http;
using IO.Swagger.model.megaSigns;
using System.Drawing.Printing;

namespace echoSign_example.Controllers
{
    public class EchoSignController : ApiController
    {
        /*// GET: echoSign
        public string Index()
        {
            return "test";
        }

        // GET: oauthDemo
        public string GetCodeAndState(string code, string state)
        {
            return "teste";
        }*/

        private static HttpClient _httpClient = new HttpClient();
        private static string clientId = "CBJCHBCAABAAO5iL7Ahz7onKLwPDXx6Prv0ZD07tz3Fo";
        private static string token = "";

        [HttpGet]
        [Route("oauthDemo")]
        public string GetCodeAndState(string code, string state) {
            return code;
        }

        [HttpGet]
        [Route("oauthToken")]
        public async Task<string> GetToken(string code)
        {
            var values = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", clientId},
                {"client_secret", "-RnP6Fc6igPX1gVOs8dj2znpdqAp75B-"},
                {"redirect_uri", "https://localhost:44386/oauthDemo"},
                {"grant_type", "authorization_code"}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://api.na3.adobesign.com/oauth/v2/token", content);
            var responseTxt = await response.Content.ReadAsStringAsync();
            token = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt)["access_token"];
            return responseTxt;
        }


        [HttpGet]
        [Route("sign")]
        public SigningUrlResponse SigningUrl(string filname, string agreementName, string signerEmail)
        {
            string docId = GetDocumentId(filname).Result;
            string agreementId = GetAgreementCreation(agreementName, signerEmail, docId).Id;
            return GetSigningUrl(agreementId);
        }

        private async Task<string> GetDocumentId(string fileName)
        {
            // string filepath = HttpContext.Current.Server.MapPath("~/Props/test.pdf");
            string filepath = HttpContext.Current.Server.MapPath("~/Props/test.pdf");
            FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

            var apiInstance = new TransientDocumentsApi(new Configuration
            {
                BasePath = "https://api.na3.adobesign.com/api/rest/v6"
            });
            var authorization = $"Bearer {token}";

            try
            {
                // Uploads a document and obtains the document's ID.
                TransientDocumentResponse result = apiInstance.CreateTransientDocument(authorization, fileStream, null, null, fileName, null);
                Debug.WriteLine(result);
                return result.TransientDocumentId;
                // return result;
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling TransientDocumentsApi.CreateTransientDocument: " + e.Message);
            }
            // Corrigir isso aqui
            return null;
        }

        private AgreementCreationResponse GetAgreementCreation(string name, string signerEmail, string docId) 
        {
            var apiInstance = new AgreementsApi(new Configuration
            {
                BasePath = "https://api.na3.adobesign.com/api/rest/v6"
            });
            var authorization = $"Bearer {token}";
            
            // List of emails
            AgreementCcInfo observer = new AgreementCcInfo(signerEmail, null, null);
            List<AgreementCcInfo> ccs = new List<AgreementCcInfo>() {observer};
            
            // List of participants
            ParticipantSetMemberInfo participant = new ParticipantSetMemberInfo() { Email = signerEmail};
            ParticipantSetInfo particapents = new ParticipantSetInfo()
            {
                MemberInfos = new List<ParticipantSetMemberInfo>(){participant},
                Name = name,
                Order = 1,
                Role = ParticipantSetInfo.RoleEnum.SIGNER,
            };

            IO.Swagger.model.agreements.FileInfo transientDocument = new IO.Swagger.model.agreements.FileInfo(null, null, null, docId, null);

            var agreementInfo = new AgreementCreationInfo()
            {
                Ccs = ccs,
                Name = name,
                FileInfos = new List<IO.Swagger.model.agreements.FileInfo>() {transientDocument},
                SignatureType = AgreementCreationInfo.SignatureTypeEnum.ESIGN,
                State = AgreementCreationInfo.StateEnum.INPROCESS,
                ParticipantSetsInfo = new List<ParticipantSetInfo>() {particapents}
            };

            try
            {
                // Creates an agreement. Sends it out for signatures, and returns the agreementID in the response to the client.
                AgreementCreationResponse result = apiInstance.CreateAgreement(authorization, agreementInfo, null, null);
                Debug.WriteLine(result);
                return result;
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AgreementsApi.CreateAgreement: " + e.Message);
            }
            return null;
        }

        private SigningUrlResponse GetSigningUrl(string agreementId)
        {
            var apiInstance = new AgreementsApi(new Configuration
            {
                BasePath = "https://api.na3.adobesign.com/api/rest/v6"
            });
            var authorization = $"Bearer {token}";

            try
            {
                // Retrieves the URL for the e-sign page for the current signer(s) of an agreement.
                SigningUrlResponse result = apiInstance.GetSigningUrl(authorization, agreementId, null, null);
                Debug.WriteLine(result);
                return result;
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AgreementsApi.GetSigningUrl: " + e.Message);
            }
            return null;
        }

        [HttpGet]
        [Route("myAgreements")]
        public UserAgreements GetUserAgreements()
        {
            var apiInstance = new AgreementsApi(new Configuration
            {
                BasePath = "https://api.na3.adobesign.com/api/rest/v6"
            });
            var authorization = $"Bearer {token}";
            
            try
            {
                // Retrieves agreements for the user.
                UserAgreements result = apiInstance.GetAgreements(authorization);
                Debug.WriteLine(result);
                return result;
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AgreementsApi.GetAgreements: " + e.Message);
            }
            return null;
        }

        [HttpGet]
        [Route("getAgreement")]
        public AgreementInfo GetAgreement(string agreementId)
        {
            var apiInstance = new AgreementsApi(new Configuration
            {
                BasePath = "https://api.na3.adobesign.com/api/rest/v6"
            });
            var authorization = $"Bearer {token}";

            try
            {
                // Retrieves agreements for the user.
                AgreementInfo result = apiInstance.GetAgreementInfo(authorization, agreementId);
                Debug.WriteLine(result);
                return result;
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling AgreementsApi.GetAgreements: " + e.Message);
            }
            return null;
        }
    }
}