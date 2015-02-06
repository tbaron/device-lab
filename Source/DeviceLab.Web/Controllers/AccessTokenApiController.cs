using System.Net;
using System.Net.Http;
using System.Web.Http;
using InfoSpace.DeviceLab.Web.Models;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    public class AccessTokenApiController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage PostToken([FromBody]AccessTokenRequest request)
        {
            var token = KeyManager.Instance.CreateAccessToken(request.client_id, request.client_secret);

            if (token == null)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            return Request.CreateResponse(new
            {
                access_token = token,
                token_type = "bearer"
            });
        }
    }
}
