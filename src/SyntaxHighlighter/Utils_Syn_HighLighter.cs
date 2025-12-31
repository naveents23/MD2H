
namespace Md2h;
public static class UtilsSynHL {
   /// <summary>
   /// Minimal HTML encoder to avoid breaking the HTML output.
   /// Encodes: &lt; &gt; &amp; &quot; and single quote.
   /// </summary>
   public static string HtmlEncode (string s) {
      if (string.IsNullOrEmpty (s)) return string.Empty;
      var sb = new StringBuilder (s.Length);
      foreach (var ch in s) {
         switch (ch) {
            case '&': sb.Append ("&amp;"); break;
            case '<': sb.Append ("&lt;"); break;
            case '>': sb.Append ("&gt;"); break;
            case '"': sb.Append ("&quot;"); break;
            case '\'': sb.Append ("&#39;"); break;
            default: sb.Append (ch); break;
         }
      }
      return sb.ToString ();
   }

   /// <summary>
   /// Highlight tokens based on type.
   /// </summary>
   /// <param name="token"></param>
   /// <param name="cls"></param>
   /// <returns>wrapped text  for css highlight </returns>
   public static string Wrap (string token, EToken cls)
          => $"<span class=\"{nameof (cls)}\">{HtmlEncode (token)}</span>";
}

// keyword ,type,number,string,template,decorator,comment,punctuation,Plain
public enum EToken { kw, type, num, str, tmpl, dec, cm, pun, pln }