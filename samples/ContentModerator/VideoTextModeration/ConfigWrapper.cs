/*
 * Copyright (c) 2019
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

using System;
using Microsoft.Extensions.Configuration;


namespace Microsoft.ContentModerator.VideoContentModerator
{
    public class ConfigWrapper
    {
        private readonly IConfiguration _config;
        public TextModeratorConfig textModerator;

        public ConfigWrapper(IConfiguration config)
        {
            this._config = config;
            this.textModerator = new TextModeratorConfig(config);
        }
    }

    public class TextModeratorConfig
    {
        private readonly IConfiguration _config;
        // private static string prefix = "VisualModerator:";

        public TextModeratorConfig(IConfiguration config)
        {
            this._config = config;
        }

        // private string prop(string propertyName) { return prefix + propertyName; }
        private string prop(string propertyName) { return propertyName; }

        public string AadClientId { get { return this._config[prop("AadClientId")]; } }
        public Uri AadEndpoint { get { return new Uri(this._config[prop("AadEndpoint")]); } }
        public string AadClientSecret { get { return this._config[prop("AadSecret")]; } }
        public string AadTenantId { get { return this._config[prop("AadTenantId")]; } }
        public string AccountName { get { return this._config[prop("AccountName")]; } }
        public Uri ArmAadAudience { get { return new Uri(this._config[prop("ArmAadAudience")]); } }
        public Uri ArmEndpoint { get { return new Uri(this._config[prop("ArmEndpoint")]); } }
        public string ResourceGroup { get { return this._config[prop("ResourceGroup")]; } }
        public string SubscriptionId { get { return this._config[prop("SubscriptionId")]; } }
        public bool EnableStreamingVideo { get { return Convert.ToBoolean(this._config[prop("EnableStreamingVideo")]); } }
        public string ApiEndpoint { get { return this._config[prop("ContentModeratorModerateApiEndpoint")]; } }
        public string ApiSubscriptionKey { get { return this._config[prop("ContentModeratorModerateApiSubscriptionKey")]; } }
    }
}
