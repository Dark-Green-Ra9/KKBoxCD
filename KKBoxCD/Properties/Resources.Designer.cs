﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KKBoxCD.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KKBoxCD.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var lookup = &apos;ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_&apos;;
        ///(function (exports) {
        ///    &apos;use strict&apos;
        ///    var Arr = (typeof Uint8Array !== &apos;undefined&apos;) ? Uint8Array : Array
        ///    var PLUS = &apos;+&apos;.charCodeAt(0)
        ///    var SLASH = &apos;/&apos;.charCodeAt(0)
        ///    var NUMBER = &apos;0&apos;.charCodeAt(0)
        ///    var LOWER = &apos;a&apos;.charCodeAt(0)
        ///    var UPPER = &apos;A&apos;.charCodeAt(0)
        ///    var PLUS_URL_SAFE = &apos;-&apos;.charCodeAt(0)
        ///    var SLASH_URL_SAFE = &apos;_&apos;.charCodeAt(0)
        ///
        ///    function decode(elt) {
        ///        var code = elt.charCodeAt(0)
        ///  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string base64 {
            get {
                return ResourceManager.GetString("base64", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to const ListSRP = {};
        ///
        ///const CountryData = {
        ///    &quot;af&quot;: &quot;93&quot;,
        ///    &quot;al&quot;: &quot;355&quot;,
        ///    &quot;dz&quot;: &quot;213&quot;,
        ///    &quot;as&quot;: &quot;1&quot;,
        ///    &quot;ad&quot;: &quot;376&quot;,
        ///    &quot;ao&quot;: &quot;244&quot;,
        ///    &quot;ai&quot;: &quot;1&quot;,
        ///    &quot;ag&quot;: &quot;1&quot;,
        ///    &quot;ar&quot;: &quot;54&quot;,
        ///    &quot;am&quot;: &quot;374&quot;,
        ///    &quot;aw&quot;: &quot;297&quot;,
        ///    &quot;au&quot;: &quot;61&quot;,
        ///    &quot;at&quot;: &quot;43&quot;,
        ///    &quot;az&quot;: &quot;994&quot;,
        ///    &quot;bs&quot;: &quot;1&quot;,
        ///    &quot;bh&quot;: &quot;973&quot;,
        ///    &quot;bd&quot;: &quot;880&quot;,
        ///    &quot;bb&quot;: &quot;1&quot;,
        ///    &quot;by&quot;: &quot;375&quot;,
        ///    &quot;be&quot;: &quot;32&quot;,
        ///    &quot;bz&quot;: &quot;501&quot;,
        ///    &quot;bj&quot;: &quot;229&quot;,
        ///    &quot;bm&quot;: &quot;1&quot;,
        ///    &quot;bt&quot;: &quot;975&quot;,
        ///    &quot;bo&quot;: &quot;591&quot;,
        ///    &quot;ba&quot;: &quot;387&quot;,
        ///    ....
        /// </summary>
        internal static string chrome_client {
            get {
                return ResourceManager.GetString("chrome_client", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (function () {
        ///    function u(b) {
        ///        var c, m, d = &quot;&quot;,
        ///            a = -1,
        ///            l;
        ///        if (b &amp;&amp; b.length)
        ///            for (l = b.length;
        ///                (a += 1) &lt; l;) c = b.charCodeAt(a), m = a + 1 &lt; l ? b.charCodeAt(a + 1) : 0, 55296 &lt;= c &amp;&amp; (56319 &gt;= c &amp;&amp; 56320 &lt;= m &amp;&amp; 57343 &gt;= m) &amp;&amp; (c = 65536 + ((c &amp; 1023) &lt;&lt; 10) + (m &amp; 1023), a += 1), 127 &gt;= c ? d += String.fromCharCode(c) : 2047 &gt;= c ? d += String.fromCharCode(192 | c &gt;&gt;&gt; 6 &amp; 31, 128 | c &amp; 63) : 65535 &gt;= c ? d += String.fromCh [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string hashes_min {
            get {
                return ResourceManager.GetString("hashes_min", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var dbits;
        ///var canary = 0xdeadbeefcafe;
        ///var j_lm = ((canary &amp; 0xffffff) == 0xefcafe);
        ///
        ///function BigInteger(a, b, c) {
        ///    if (a != null)
        ///        if (&quot;number&quot; == typeof a) this.fromNumber(a, b, c);
        ///        else if (b == null &amp;&amp; &quot;string&quot; != typeof a) this.fromString(a, 256);
        ///        else this.fromString(a, b);
        ///}
        ///
        ///function nbi() {
        ///    return new BigInteger(null);
        ///}
        ///
        ///function am1(i, x, w, j, c, n) {
        ///    while (--n &gt;= 0) {
        ///        var v = x * this[i++] + w[j] + c;
        ///        c = Math.floor(v / 0x4000000);
        ///        w [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string jsbn {
            get {
                return ResourceManager.GetString("jsbn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function bnClone() {
        ///    var r = nbi();
        ///    this.copyTo(r);
        ///    return r;
        ///}
        ///
        ///function bnIntValue() {
        ///    if (this.s &lt; 0) {
        ///        if (this.t == 1) return this[0] - this.DV;
        ///        else if (this.t == 0) return -1;
        ///    } else if (this.t == 1) return this[0];
        ///    else if (this.t == 0) return 0;
        ///    return ((this[1] &amp; ((1 &lt;&lt; (32 - this.DB)) - 1)) &lt;&lt; this.DB) | this[0];
        ///}
        ///
        ///function bnByteValue() {
        ///    return (this.t == 0) ? this.s : (this[0] &lt;&lt; 24) &gt;&gt; 24;
        ///}
        ///
        ///function bnShortValue() {
        ///    return (this.t == 0) ? [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string jsbn2 {
            get {
                return ResourceManager.GetString("jsbn2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to challenge = function () {
        ///    const country_code = &quot;65&quot;;
        ///    const territory_code = &quot;SG&quot;;
        ///
        ///    srp = new SRP();
        ///    srp.init();
        ///    srp.computeA();
        ///
        ///    $(&quot;#username&quot;).val(window.__username);
        ///    $(&quot;#ori_username&quot;).val(window.__username);
        ///    $(&quot;#secret&quot;).val(window.__password);
        ///    $(&quot;#remember&quot;).val(&quot;&quot;);
        ///    $(&quot;#phone_country_code&quot;).val(country_code);
        ///    $(&quot;#phone_territory_code&quot;).val(territory_code);
        ///
        ///    $.ajax({
        ///        method: &quot;POST&quot;,
        ///        url: &quot;/challenge&quot;,
        ///        data: {
        ///     [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string kkbox_inject {
            get {
                return ResourceManager.GetString("kkbox_inject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///
        ///&lt;!DOCTYPE html&gt;
        ///&lt;html&gt;
        ///&lt;head&gt;
        ///
        ///    &lt;meta charset=&quot;utf-8&quot;&gt;
        ///    &lt;title&gt;Log In&lt;/title&gt;
        ///    &lt;meta name=&quot;robots&quot; content=&quot;noindex&quot;&gt;
        ///    &lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1.0, user-scalable=no&quot;&gt;
        ///
        ///
        ///    &lt;script src=&quot;https://code.jquery.com/jquery-3.4.1.min.js&quot;&gt;&lt;/script&gt;
        ///    &lt;script src=&quot;https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.19.1/jquery.validate.min.js&quot;&gt;&lt;/script&gt;
        ///    &lt;script src=&quot;https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js&quot;
        ///  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string login {
            get {
                return ResourceManager.GetString("login", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var scrypt_module_factory = (function (requested_total_memory) {
        ///    var Module = {
        ///        TOTAL_MEMORY: (requested_total_memory || 33554432)
        ///    };
        ///    var scrypt_raw = Module;
        ///
        ///    function g(a) {
        ///        throw a;
        ///    }
        ///    var k = void 0,
        ///        l = !0,
        ///        m = null,
        ///        p = !1;
        ///
        ///    function aa() {
        ///        return function () { }
        ///    }
        ///    var q, s;
        ///    s || (s = eval(&quot;(function() { try { return Module || {} } catch(e) { return {} } })()&quot;));
        ///    var ba = {},
        ///        t;
        ///    for (t in s) {
        ///       [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string scrypt {
            get {
                return ResourceManager.GetString("scrypt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to if (typeof module !== &apos;undefined&apos;) {
        ///    module.exports = SRP;
        ///}
        ///
        ///function SRP() {
        ///    this.N = null;
        ///    this.g = null;
        ///    this.a = null;
        ///    this.b = null;
        ///    this.A = null;
        ///    this.B = null;
        ///    this.clientK = null;
        ///    this.serverK = null;
        ///    this.M1 = null;
        ///    this.M2 = null;
        ///    this.k = null;
        ///    this.I = null;
        ///    this.p = null;
        ///    this.x = null;
        ///    this.s = null;
        ///    this.v = null;
        ///}
        ///SRP.prototype.init = function (g) {
        ///    switch (g) {
        ///        case &quot;G3072&quot;:
        ///            this.N = new BigIntege [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string srp {
            get {
                return ResourceManager.GetString("srp", resourceCulture);
            }
        }
    }
}
