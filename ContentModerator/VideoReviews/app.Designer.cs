﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VideoReviews {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.5.0.0")]
    internal sealed partial class app : global::System.Configuration.ApplicationSettingsBase {
        
        private static app defaultInstance = ((app)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new app())));
        
        public static app Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("###")]
        public string ContentModeratorTeamName {
            get {
                return ((string)(this["ContentModeratorTeamName"]));
            }
            set {
                this["ContentModeratorTeamName"] = value;
            }
        }
    }
}
