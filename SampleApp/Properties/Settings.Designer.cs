//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SampleApp.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://test-account.10duke.com/oauth2/authz/")]
        public string AuthzUri {
            get {
                return ((string)(this["AuthzUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://test-account.10duke.com/oauth2/access/")]
        public string TokenUri {
            get {
                return ((string)(this["TokenUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://test-account.10duke.com/userinfo/")]
        public string UserInfoUri {
            get {
                return ((string)(this["UserInfoUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SampleApp")]
        public string ClientID {
            get {
                return ((string)(this["ClientID"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("YohSi-u3")]
        public string ClientSecret {
            get {
                return ((string)(this["ClientSecret"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("oob:SampleApp")]
        public string RedirectUri {
            get {
                return ((string)(this["RedirectUri"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("openid profile email")]
        public string Scope {
            get {
                return ((string)(this["Scope"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1wRc5dsWBbIJfxay3SYP
MYp/BaLEt0b26/QtwQbrKq6hgVH+euMWsSk6gf0GZiwHMFF+t8/WcsNOfcYMBEHV
mGWSFeYb63IcFN+v3h2580kANzuKuqYnBeBOCN56lJf8q5FOUYQKFVuX/bvEKp+L
1KkMErmIm9e5fkw70zCngxoXGK6qyWX01SPTVfd3UZdPv1H+VOoEpbDsI2yhg5xR
jFAAsqyTYvHQaixiJqqw/T8+2/ond8AlxpzCa1UK9x2l1lMezlwHTHXyPh2ZMpwe
lDBIosKLPHbaZyNwpU0iGOvrDJo8xlw4qGm/fClbaEWM8BCdbn/aKjWMN/t7FEaQ
TQIDAQAB
-----END PUBLIC KEY-----")]
        public string SignerKey {
            get {
                return ((string)(this["SignerKey"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool StoreAuthorization {
            get {
                return ((bool)(this["StoreAuthorization"]));
            }
            set {
                this["StoreAuthorization"] = value;
            }
        }
    }
}
