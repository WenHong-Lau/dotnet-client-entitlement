using System;
using System.Collections.Specialized;
using System.Web;
using System.Windows.Forms;
using Tenduke.Client.Authorization;
using Tenduke.Client.Config;

namespace Tenduke.EntitlementClient
{
    /// <summary>
    /// Base class for authorization implementations that use an embedded web browser
    /// for displaying user interface as necessary.
    /// </summary>
    /// <typeparam name="T">Type of the implementing class.</typeparam>
    /// <typeparam name="O">OAuth 2.0 configuration object type.</typeparam>
    /// <typeparam name="A">Authorization process argument type.</typeparam>
    [Serializable]
    public abstract class BrowserBasedAuthorization<T, O, A> : Authorization<T, O, A>
            where T : BrowserBasedAuthorization<T, O, A>
            where O : IBrowserBasedAuthorizationConfig
            where A : BrowserBasedAuthorizationArgs
    {
        #region Private fields

        /// <summary>
        /// Internal field holding WebBrowserForm used for displaying the embedded web browser.
        /// </summary>
        [NonSerialized]
        private WebBrowserForm webBrowserForm;

        #endregion

        #region Properties

        /// <summary>
        /// Form used for displaying the embedded web browser.
        /// </summary>
        protected WebBrowserForm WebBrowserForm
        {
            get
            {
                return webBrowserForm;
            }

            set
            {
                webBrowserForm = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the authorization process and waits for the process to complete before returning.
        /// </summary>
        /// <param name="args">Authorization operation arguments.</param>
        /// <returns>Returns OAuth state as received back from the server, or <c>null</c> if OAuth state
        /// is not used with the authorization flow, or if no OAuth state specified in the <paramref name="args"/>.</returns>
        public override string AuthorizeSync(A args)
        {
            OnStarted();

            var authzUri = BuildAuthorizationUri(args);
            WebBrowserForm = new WebBrowserForm()
            {
                Address = authzUri.ToString(),
                RedirectUri = OAuthConfig.RedirectUri
            };
            OnBeforeAuthorization(args, authzUri);

            var result = WebBrowserForm.ShowDialog();
            var cancelled = result != DialogResult.OK;

            string retValue;
            if (cancelled)
            {
                OnCancelled(args, authzUri);
                retValue = null;
            }
            else
            {
                var responseParams = ParseResponseParameters(WebBrowserForm.ResponseUri);
                OnAfterAuthorization(args, authzUri, responseParams);

                retValue = ReadAuthorizationResponse(args, responseParams);
            }

            return retValue;
        }

        /// <summary>
        /// Parses response parameters from the request used by the server for sending the
        /// OAuth response.
        /// </summary>
        /// <param name="responseUri">Request Uri used by the server for redirecting back to the client
        /// for sending the response.</param>
        /// <returns>NameValueCollection containing the parsed response parameters.</returns>
        protected abstract NameValueCollection ParseResponseParameters(string responseUri);

        /// <summary>
        /// Gets the OAuth 2.0 <c>response_type</c> value to use.
        /// </summary>
        /// <returns>The response type value.</returns>
        protected abstract string GetResponseType();

        /// <summary>
        /// Builds the full Uri for starting the authorization process on the server.
        /// </summary>
        /// <param name="args">Authorization operation arguments.</param>
        /// <returns>Uri to use as the initial Uri where the embedded browser is opened.</returns>
        protected Uri BuildAuthorizationUri(A args)
        {
            if (OAuthConfig == null)
            {
                throw new InvalidOperationException("OAuthConfig must be specified");
            }

            if (OAuthConfig.ClientID == null)
            {
                throw new InvalidOperationException("OAuthConfig.ClientID must be specified");
            }

            if (OAuthConfig.AuthzUri == null)
            {
                throw new InvalidOperationException("OAuthConfig.AuthzUri must be specified");
            }

            var uriBuilder = new UriBuilder(OAuthConfig.AuthzUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["response_type"] = GetResponseType();
            query["client_id"] = OAuthConfig.ClientID;
            // Always ask the server to hide "Remember me" checkbox, authorization persistence shall be
            // controlled by the application.
            query["showRememberMe"] = "false";

            if (OAuthConfig.RedirectUri != null)
            {
                query["redirect_uri"] = OAuthConfig.RedirectUri;
            }

            if (OAuthConfig.Scope != null)
            {
                query["scope"] = OAuthConfig.Scope;
            }

            if (OAuthConfig.Scope != null)
            {
                query["scope"] = OAuthConfig.Scope;
            }

            if (args != null && args.State != null)
            {
                query["state"] = args.State;
            }

            if (args != null && args.Nonce != null)
            {
                query["nonce"] = args.Nonce;
            }

            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Frees any resources consumed by the <see cref="Authorization"/> object. Override
        /// this method in derived classes if there are resources that need bo be disposed.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from the zero-argument <see cref="Dispose"/> method,
        /// <c>false</c> if called from finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (WebBrowserForm != null)
                    {
                        WebBrowserForm.Dispose();
                        WebBrowserForm = null;
                    }
                }

                // No unmanaged resources to free

                disposedValue = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
