using Microsoft.Win32;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Tenduke.Client.Config;
using Tenduke.Client.EntApi;
using Tenduke.Client.EntApi.Authz;
using Tenduke.Client.Util;
using Tenduke.EntitlementClient;

namespace SampleApp
{
    /// <summary>
    /// Form that uses <see cref="Tenduke.EntitlementClient.EntClient"/> for the purpose of giving basic usage examples.
    /// </summary>
    public partial class SampleAppForm : Form
    {
        /// <summary>
        /// Registry key for stored authorization.
        /// </summary>
        private readonly string REGISTRY_KEY_STORED_AUTHORIZATION = "Software\\10Duke\\Tenduke.EntitlementClient\\SampleApp";

        /// <summary>
        /// Public key to use for verifying 10Duke Entitlement Service signatures.
        /// </summary>
        public static readonly RSA EntServerPublicKey = CryptoUtil.ReadRsaPublicKey(Properties.Settings.Default.SignerKey);

        /// <summary>
        /// OAuth 2.0 configuration for connecting this sample application to the 10Duke Entitlement service.
        /// </summary>
        public readonly AuthorizationCodeGrantConfig OAuthConfig = new AuthorizationCodeGrantConfig()
        {
            AuthzUri = Properties.Settings.Default.AuthzUri,
            TokenUri = Properties.Settings.Default.TokenUri,
            UserInfoUri = Properties.Settings.Default.UserInfoUri,
            ClientID = Properties.Settings.Default.ClientID,
            ClientSecret = Properties.Settings.Default.ClientSecret,
            RedirectUri = Properties.Settings.Default.RedirectUri,
            Scope = Properties.Settings.Default.Scope,
            SignerKey = EntServerPublicKey
        };

        /// <summary>
        /// <para>The <see cref="Tenduke.EntitlementClient.EntClient"/> instance used by this sample application.</para>
        /// <para>Please note that </para>
        /// </summary>
        protected EntClient EntClient { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleAppForm"/> class.
        /// </summary>
        public SampleAppForm()
        {
            InitializeComponent();
            comboBoxConsumeMode.SelectedIndex = 0;
            comboBoxResponseFormat.SelectedIndex = 0;
            listViewAuthorizationDecisions.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            EntClient = new EntClient() { OAuthConfig = OAuthConfig };
        }

        /// <summary>
        /// Called when the form is first displayed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void SampleAppForm_Shown(object sender, EventArgs e)
        {
            // This sample application always requires sign-on / authorization against the 10Duke entitlement service.
            EnsureAuthorization();
            if (EntClient.IsAuthorized())
            {
                ShowWelcomeMessage();
                ShowComputerId();
                StoreAuthorization();
            }
            else
            {
                // If the authorization process was cancelled, close this form. This will cause the whole application
                // to be closed.
                Close();
            }
        }

        /// <summary>
        /// Checks that either a previously stored valid authorization against the 10Duke Entitlement Service exists,
        /// or launches embedded browser for signing on and getting the authorization.
        /// </summary>
        private void EnsureAuthorization()
        {
            if (!UseStoredAuthorization())
            {
                EntClient.AuthorizeSync();
            }
        }

        /// <summary>
        /// Checks value of the <c>StoreAuthorization</c> setting to determine if earlier authorization stored in the registry
        /// should be read and used. If stored authorization is used and a stored authorization value is found in the registry,
        /// initializes <see cref="EntClient"/> to use the stored authorization.
        /// </summary>
        /// <returns><c>true</c> if stored authorization is used and a stored authorization info is found in the registry,
        /// <c>false</c> otherwise.</returns>
        private bool UseStoredAuthorization()
        {
            bool retValue = false;
            if (Properties.Settings.Default.StoreAuthorization)
            {
                var storedAuthorization = ReadAuthorizationInfoFromRegistry();
                if (storedAuthorization != null)
                {
                    EntClient.AuthorizationSerializer.WriteAuthorization(storedAuthorization);
                    retValue = true;
                }
            }

            return retValue;
        }

        /// <summary>
        /// Reads stored authorization info from registry.
        /// </summary>
        /// <returns>Authorization info serialized as a byte array, or <c>null</c> if no stored authorization information found.</returns>
        private byte[] ReadAuthorizationInfoFromRegistry()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_STORED_AUTHORIZATION))
            {
                return key == null ? null : key.GetValue("StoredAuthorization") as byte[];
            }
        }

        /// <summary>
        /// Checks value of the <c>StoreAuthorization</c> setting to determine if authorization from the 10Duke Entitlement
        /// Service should be stored in the registry. If value of the setting is <c>true</c>, stored current <see cref="EntClient.Authorization"/>
        /// to the registry.
        /// </summary>
        /// <returns><c>true</c> if authorization was stored, <c>false</c> otherwise.</returns>
        private bool StoreAuthorization()
        {
            bool retValue = false;
            if (Properties.Settings.Default.StoreAuthorization)
            {
                StoreAuthorizationInfoToRegistry(EntClient.AuthorizationSerializer.ReadAuthorization());
                retValue = true;
            }

            return retValue;
        }

        /// <summary>
        /// Stores authorization info to registry.
        /// </summary>
        /// <param name="authorizationInfo">Serialized authorization info.</param>
        private void StoreAuthorizationInfoToRegistry(byte[] authorizationInfo)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_STORED_AUTHORIZATION))
            {
                key.SetValue("StoredAuthorization", authorizationInfo);
            }
        }

        /// <summary>
        /// Populates welcome message shown by this form using user attributes received from the 10Duke entitlement
        /// service in the received OpenID Connect ID token.
        /// </summary>
        private void ShowWelcomeMessage()
        {
            var name = (string)EntClient.Authorization.AccessTokenResponse.IDToken["name"];
            if (string.IsNullOrEmpty(name))
            {
                var givenName = (string)EntClient.Authorization.AccessTokenResponse.IDToken["given_name"];
                var familyName = (string)EntClient.Authorization.AccessTokenResponse.IDToken["family_name"];

                var builder = new StringBuilder();
                if (!string.IsNullOrEmpty(givenName))
                {
                    builder.Append(givenName);
                }
                if (!string.IsNullOrEmpty(familyName))
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(familyName);
                }

                name = builder.Length == 0 ? null : builder.ToString();
            }

            name = name ?? "anonymous";
            labelWelcome.Text = string.Format("Welcome {0}", name);
        }

        /// <summary>
        /// Computes a computer id (identifier for this system) and displays it on the form.
        /// </summary>
        private void ShowComputerId()
        {
            textBoxComputerId.Text = EntClient.ComputerId;
        }

        /// <summary>
        /// Called when button for requesting an authorization decision is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void buttonRequestAuthorizationDecision_Click(object sender, EventArgs e)
        {
            var authorizedItems = textBoxAuthorizedItemName.Text.Split(',').Select(item => item.Trim()).ToArray();
            var responseType = ResponseType.FromExtension(comboBoxResponseFormat.Text);
            var consumeMode = comboBoxConsumeMode.Text;
            var consume = consumeMode == "consume";
            var authorizationDecisions = await GetAuthzApi().CheckOrConsumeAsync(authorizedItems, consume, responseType);
            for (int i = 0; i < authorizedItems.Length; i++)
            {
                ShowAuthorizationDecision(authorizedItems[i], authorizationDecisions[i]);
            }
        }

        /// <summary>
        /// Adds an item for the received <see cref="AuthorizationDecision"/> in the list view.
        /// </summary>
        /// <param name="authorizedItem">Name of the authorized item.</param>
        /// <param name="authorizationDecision"><see cref="AuthorizationDecision"/> object describing authorization decision
        /// response received for the authorized item.</param>
        private void ShowAuthorizationDecision(string authorizedItem, AuthorizationDecision authorizationDecision)
        {
            var listViewItem = BuildListViewItemForAuthorizationDecision(authorizedItem, authorizationDecision);
            listViewAuthorizationDecisions.Items.Add(listViewItem);
        }

        /// <summary>
        /// Builds a <see cref="ListViewItem"/> for displaying an <see cref="AuthorizationDecision"/> in the list view.
        /// </summary>
        /// <param name="authorizedItem">Name of the authorized item.</param>
        /// <param name="authorizationDecision"><see cref="AuthorizationDecision"/> object describing authorization decision
        /// response received for the authorized item.</param>
        /// <returns>The <see cref="ListViewItem"/>.</returns>
        private ListViewItem BuildListViewItemForAuthorizationDecision(string authorizedItem, AuthorizationDecision authorizationDecision)
        {
            return new AuthorizationDecisionListViewItem(authorizedItem, authorizationDecision);
        }

        /// <summary>
        /// Called when selection is changed in the list view.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void listViewAuthorizationDecisions_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        /// <summary>
        /// Enables / disables buttons on the form.
        /// </summary>
        private void EnableButtons()
        {
            var selectedItem = (AuthorizationDecisionListViewItem)
                (listViewAuthorizationDecisions.SelectedItems.Count == 1
                ? listViewAuthorizationDecisions.SelectedItems[0]
                : null);

            bool releaseLicenseButtonEnabled = false;
            bool showDataButtonEnabled = false;

            if (selectedItem != null)
            {
                showDataButtonEnabled = true;
                if (selectedItem.Granted && selectedItem.AuthorizationDecision["jti"] != null)
                {
                    releaseLicenseButtonEnabled = true;
                }
            }

            buttonReleaseLicense.Enabled = releaseLicenseButtonEnabled;
            buttonShowData.Enabled = showDataButtonEnabled;
        }

        /// <summary>
        /// Called when the show authorization decision details button is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void buttonShowData_Click(object sender, EventArgs e)
        {
            var selectedItem = (AuthorizationDecisionListViewItem)listViewAuthorizationDecisions.SelectedItems[0];
            var dataForm = new AuthorizationDecisionDetailsForm() { AuthorizationDecision = selectedItem.AuthorizationDecision };
            dataForm.ShowDialog();
        }

        /// <summary>
        /// Called when the release license button is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void buttonReleaseLicense_Click(object sender, EventArgs e)
        {
            var selectedItem = (AuthorizationDecisionListViewItem)listViewAuthorizationDecisions.SelectedItems[0];
            var tokenId = (string)selectedItem.AuthorizationDecision["jti"];
            var response = await GetAuthzApi().ReleaseLicenseAsync(tokenId, ResponseType.JWT);
            bool successfullyReleased = response[tokenId] != null && (bool)response[tokenId] == true;
            bool noConsumptionFound = "noConsumptionFoundById" == (string)response[tokenId + "_errorCode"];
            if (successfullyReleased || noConsumptionFound)
            {
                listViewAuthorizationDecisions.Items.Remove(selectedItem);
            }
            else
            {
                MessageBox.Show(response.ToString(), "Error");
            }
        }

        /// <summary>
        /// Gets <see cref="AuthzApi"/> for accessing the authorization api.
        /// </summary>
        /// <returns>The <see cref="AuthzApi"/>.</returns>
        private AuthzApi GetAuthzApi()
        {
            var retValue = EntClient.AuthzApi;
            retValue.ComputerId = textBoxComputerId.Text;
            return retValue;
        }

        /// <summary>
        /// Item for displaying an <see cref="AuthorizationDecision"/> in the list view and for storing data of the authorization decision.
        /// </summary>
        private class AuthorizationDecisionListViewItem : ListViewItem
        {
            /// <summary>
            /// Name of the authorized item.
            /// </summary>
            public string AuthorizedItem { get; set; }

            /// <summary>
            /// Flag indicating if authorization was granted.
            /// </summary>
            public bool Granted { get; set; }

            /// <summary>
            /// The authorization decision response from the server.
            /// </summary>
            public AuthorizationDecision AuthorizationDecision { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AuthorizationDecisionListViewItem"/> class.
            /// </summary>
            /// <param name="authorizedItem">Name of the authorized item.</param>
            /// <param name="authorizationDecision"><see cref="AuthorizationDecision"/> object describing authorization decision
            /// response received for the authorized item.</param>
            public AuthorizationDecisionListViewItem(string authorizedItem, AuthorizationDecision authorizationDecision)
                : base(new string[] { authorizedItem, authorizationDecision[authorizedItem]?.ToString(), authorizationDecision.ToString() })
            {
                AuthorizedItem = authorizedItem;
                Granted = authorizationDecision[authorizedItem] != null && (bool)authorizationDecision[authorizedItem];
                AuthorizationDecision = authorizationDecision;
            }
        }
    }
}
