using Md2h.Html;

namespace Md2h;

public static class Util {
   public static string Style (string txt) => $"<style>{txt}</style> ";

   public static string Link (string title, string link) => $"<a href=\"{link}\">{title}</a>";

   public static string Body (string text) => $"<body>{text}</body>";

   public static string Escape (string text) => text.Replace ("&", "&amp;").Replace ("<", "&lt;").Replace (">", "&gt;");

   public static string Bold (string text) {
      return $"<b>{text}</b>";
   }
   public static string Italic (string text) {
      return $"<i>{text}</i>";
   }
   public static string Underline (string text) {
      return $"<u>{text}</u>";
   }
   public static string Paragraph (string text) {
      return $"<p>{text}</p>";
   }
   public static string H1 (string text) {
      return $"<h1>{text}</h1>";
   }
   public static string H2 (string text) {
      return $"<h2>{text}</h2>";
   }
   public static string H3 (string text) {
      return $"<h3>{text}</h3>";
   }
   public static string ListItem (string text) {
      return $"     <li>{text}</li>";
   }
   public static string HorizontalLine () {
      return "<hr>";
   }
   public static string CodeBlock (string code) {
      return @$" <div class=""code-container"">
                    <button class=""copy-btn"" type=""button"" aria-label=""Copy code"">Copy</button>
                      <pre class=""code-block"">
                   {code}
                </code>
            </pre>
         </div>";
   }
   public static string Image (string altName, string location) {
      return @$"<img src= ""{"../Img/" + location}"" alt =""{altName}"" />";
   }

   public static string BlockQuote (string text) {
      return $"<blockquote>{text}</blockquote>";
   }

   public static string TableHead (IEnumerable<string> head) {
      if (head is null) return "";
      StringBuilder sb = new ();
      sb.AppendLine ("<tr>");
      foreach (var item in head) {
         sb.Append ($"\t<th>{item}</th>\n");
      }
      sb.AppendLine (" </tr>"); return sb.ToString ();
   }

   public static string TableData (IEnumerable<IEnumerable<string>> datas) {
      if (datas is null) return "";
      StringBuilder sb = new ();
      foreach (var data in datas) {
         sb.AppendLine ("<tr>");
         foreach (var item in data) {
            sb.Append ($"\t<td>{item}</td>\n");
         }
         sb.AppendLine (" </tr>");
      }
      return sb.ToString ();
   }
}

