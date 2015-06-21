// ***********************************************************************
// Assembly         : MusicPickerService
// Author           : Pierre
// Created          : 06-12-2015
//
// Last Modified By : Pierre
// Last Modified On : 06-21-2015
// ***********************************************************************
// <copyright file="ChallengeResult.cs" company="Hutopi">
//     Copyright ©  2015 Hugo Caille, Pierre Defache & Thomas Fossati.
//     Music Picker is released upon the terms of the Apache 2.0 License.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

/// <summary>
/// The Results namespace.
/// </summary>
namespace MusicPickerService.Results
{
    /// <summary>
    /// Class ChallengeResult.
    /// </summary>
    public class ChallengeResult : IHttpActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChallengeResult"/> class.
        /// </summary>
        /// <param name="loginProvider">The login provider.</param>
        /// <param name="controller">The controller.</param>
        public ChallengeResult(string loginProvider, ApiController controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }

        /// <summary>
        /// Gets or sets the login provider.
        /// </summary>
        /// <value>The login provider.</value>
        public string LoginProvider { get; set; }
        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>The request.</value>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Creates an <see cref="T:System.Net.Http.HttpResponseMessage" /> asynchronously.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that, when completed, contains the <see cref="T:System.Net.Http.HttpResponseMessage" />.</returns>
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            Request.GetOwinContext().Authentication.Challenge(LoginProvider);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            return Task.FromResult(response);
        }
    }
}
