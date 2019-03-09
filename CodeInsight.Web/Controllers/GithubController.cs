using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeInsight.Github;
using CodeInsight.Github.Import;
using CodeInsight.Library.Extensions;
using CodeInsight.Library.Types;
using CodeInsight.Web.Common;
using CodeInsight.Web.Common.Security;
using CodeInsight.Web.Models.Github;
using FuncSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using Octokit.GraphQL;
using Controller = Microsoft.AspNetCore.Mvc.Controller;
using static CodeInsight.Library.Prelude;
using Connection = Octokit.GraphQL.Connection;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace CodeInsight.Web.Controllers
{
    public class GithubController : Controller
    {
        private static readonly string CsrfSessionKey = "GithubCsrf";

        private readonly ApplicationConfiguration configuration;
        private readonly Importer importer;
        private readonly GitHubClient client;

        public GithubController(ApplicationConfiguration configuration, Importer importer)
        {
            this.configuration = configuration;
            this.importer = importer;
            this.client = new GitHubClient(new ProductHeaderValue(configuration.ApplicationName));
        }

        #region SignIn

        public IActionResult SignIn() =>
            CreateOAuthLoginUrl()
                .ToString()
                .Pipe(Redirect);
        
        private Uri CreateOAuthLoginUrl()
        {
            var csrfToken = Guid.NewGuid().ToString();
            HttpContext.Session.Set(CsrfSessionKey, csrfToken);
            
            return client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(configuration.ClientId)
            {
                Scopes = { "read:org", "repo" },
                State = csrfToken
            });
        }
        
        #endregion
        
        #region Authorize

        public Task<IActionResult> Authorize(string code, string state)
        {
            var session = HttpContext.Session;
            var verifiedCode = GetVerifiedCode(code, state, session);
            var oAuthToken = verifiedCode.Map(vc => GetOAuthAccessToken(vc));
            return oAuthToken.Match(
                tokenTask => tokenTask.Map(token =>
                {
                    session.Remove(CsrfSessionKey);
                    session.Set(ClientAuthenticator.GithubTokenSessionKey, token);
                    return (IActionResult)RedirectToAction("ChooseRepository");
                }),
                _ => RedirectToAction("Index", "Home").Async<RedirectToActionResult, IActionResult>()
            );
        }
        
        private static IOption<NonEmptyString> GetVerifiedCode(string code, string state, ISession session)
        {
            var expectedState = session.Get<string>(CsrfSessionKey).GetOrElse("");
            return NonEmptyString.Create(code).Where(_ => state == expectedState);
        }
        
        private Task<string> GetOAuthAccessToken(NonEmptyString code)
        {
            var request = new OauthTokenRequest(configuration.ClientId, configuration.ClientSecret, code);
            return client.Oauth.CreateAccessToken(request).Map(t => t.AccessToken);
        }
        
        #endregion

        #region ChooseRepository

        [HttpGet]
        public Task<IActionResult> ChooseRepository() => ConnectionAction(connection =>
            connection
                .Run(GetAllRepositoriesQuery())
                .Map(items => new ChooseRepositoryViewModel(items.SelectMany(i => i)))
                .Map(vm => (IActionResult) View(vm)));

        [HttpPost]
        public Task<IActionResult> ChooseRepository(string nameWithOwner) => ConnectionAction(async conn =>
        {
            try
            {
                var parts = nameWithOwner.Split('/');
                var owner = parts[0];
                var name = parts[1];
                var query = new Query().Repository(name, owner).Select(r => new { Name = r.Name, Owner = r.Owner.Login }).Compile();
                var repository = await conn.Run(query);

                var importedRepository = await importer.ImportRepository(
                    conn,
                    NonEmptyString.Create(repository.Owner).Get(),
                    NonEmptyString.Create(repository.Name).Get()
                );
                
                HttpContext.Session.Set(ClientAuthenticator.GithubRepositoryIdSessionKey, importedRepository.Id.Value.Value);
                
                return RedirectToAction("Index", "PullRequest");
            }
            catch (NotFoundException)
            {
                return BadRequest();
            }
        });
        
        private static ICompiledQuery<IEnumerable<List<RepositoryInputDto>>> GetAllRepositoriesQuery() =>
            new Query()
                .Viewer
                .Organizations()
                .AllPages()
                .Select(n => n
                    .Repositories(null, null, null, null, null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new RepositoryInputDto(r.Name, r.Owner.Login))
                    .ToList()
                )
                .Compile();
        
        #endregion
        
        private Task<IActionResult> ConnectionAction(Func<Connection, Task<IActionResult>> action)
        {
            var token = HttpContext.Session.Get<string>(ClientAuthenticator.GithubTokenSessionKey);
            return token.Match(
                t => action(new Connection(new Octokit.GraphQL.ProductHeaderValue(configuration.ApplicationName), t)),
                _ => NotFound().Async<NotFoundResult, IActionResult>()
            );
        }
    }
}