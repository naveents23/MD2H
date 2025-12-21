// C# Syntax Highlighter → HTML spans with CSS classes per token
// - Tokenizes: comments, strings (including verbatim), char literals, numbers,
//   identifiers, punctuation/operators.
// - Classifies identifiers as: keyword (kw), built-in type (type), or plain (pln).
// - Preserves original whitespace and layout.
// - Safe HTML output via HtmlEncode.
// Note: This is a pragmatic tokenizer, not a full C# parser; it covers common cases well.

using System.Text.RegularExpressions;

namespace Md2h {
   #region Enums: Token categories --------------------------------------------------------------
   /// <summary>
   /// Token categories used as CSS class names in the generated HTML.
   /// </summary>
   public enum EToken { kw, type, num, str, cm, pun, pln }
   #endregion

   public class CSharpHighLight {
      #region Public API: Highlighter -----------------------------------------------------------
      /// <summary>
      /// Converts a C# source string to HTML with per-token <span class="{EToken}"> wrappers.
      /// </summary>
      /// <param name="text">C# source code to highlight. May be multi-line.</param>
      /// <returns>HTML string with tokens wrapped and all text HTML-encoded.</returns>
      /// <remarks>
      /// Algorithm (linear scan over regex matches):
      /// 1) Run the tokenizer regex to find tokens in order.
      /// 2) Append untouched (encoded) text between tokens to preserve whitespace/layout.
      /// 3) For each token:
      ///    - Determine its category (comment, string, number, punctuation, identifier).
      ///    - If identifier: classify as keyword, built-in type, or plain.
      /// 4) Wrap token text in a span with the category as CSS class and append.
      /// 5) Append any trailing text after the last token.
      /// </remarks>
      public string Highlight (string text) {
         if (text == null) return string.Empty;

         var sb = new StringBuilder (text.Length * 2);
         int last = 0;

         // Iterate over all tokens found by the tokenizer.
         foreach (Match m in Tokenizer.Matches (text)) {
            // Preserve any text between the previous token and this one (spaces, newlines).
            if (m.Index > last)
               sb.Append (HtmlEncode (text.Substring (last, m.Index - last)));

            string token = m.Value;
            string cls;

            // Classification by named capture groups:
            // cm: line/multi-line comments
            // str: string literals (including verbatim strings) and chr: char literals
            // num: numeric literals (basic form)
            // pun: punctuation/operators
            // id: identifiers → further classified into kw/type/pln
            if (m.Groups["cm"].Success || m.Groups["mlcm"].Success) cls = nameof (EToken.cm);
            else if (m.Groups["str"].Success || m.Groups["chr"].Success) cls = nameof (EToken.str);
            else if (m.Groups["num"].Success) cls = nameof (EToken.num);
            else if (m.Groups["pun"].Success) cls = nameof (EToken.pun);
            else if (m.Groups["id"].Success) {
               if (Keywords.Contains (token)) cls = nameof (EToken.kw);
               else if (Types.Contains (token)) cls = nameof (EToken.type);
               else cls = nameof (EToken.pln);
            } else cls = nameof (EToken.pln);

            // Wrap and append the token.
            sb.Append (Convert (token, cls));

            // Update the cursor to the end of the current token.
            last = m.Index + m.Length;
         }

         // Append any trailing text after the final token.
         if (last < text.Length)
            sb.Append (HtmlEncode (text.Substring (last)));

         return sb.ToString ();
      }
      #endregion

      #region Tokenization: Regex and groups ----------------------------------------------------
      // Tokenizer definition:
      // - cm: single-line comments //... up to newline
      // - mlcm: multi-line comments /* ... */
      // - str: string literals "..." and verbatim strings @"..." (handles doubled quotes)
      // - chr: char literals 'a', escaped forms '\n', etc.
      // - num: simple numbers (integers or decimals) — extend as needed for suffixes
      // - id: identifiers (alphanumeric + underscore, starting with letter or underscore)
      // - pun: punctuation and operators (balanced set for typical C# code)
      //
      // Notes:
      // - RegexOptions.Compiled improves performance for repeated use.
      // - [\s\S]*? is used for non-greedy multi-line comment capture.
      private static readonly Regex Tokenizer = new Regex (
          // Single-line comment
          @"(?<cm>//[^\r\n]*)" +
          // Multi-line comment
          @"|(?<mlcm>/\*[\s\S]*?\*/)" +
          // String literal and verbatim string literal (@"..."), supports "" escaping in verbatim strings
          @"|(?<str>@?""(?:[^""]|"""")*"")" +
          // Char literal: single character or escape sequence
          @"|(?<chr>'(?:\\.|[^'\\])')" +
          // Number: integer or decimal (basic)
          @"|(?<num>\b\d+(?:\.\d+)?\b)" +
          // Identifier
          @"|(?<id>\b[_A-Za-z][_A-Za-z0-9]*\b)" +
          // Punctuation/operators
          @"|(?<pun>[{}()\[\];.,:+\-*/%&|^!~?<>=])",
          RegexOptions.Compiled);
      #endregion

      #region Classification Data: Keywords and built-in types ---------------------------------
      /// <summary>
      /// C# keywords (case-sensitive). Add or remove entries as needed.
      /// </summary>
      private static readonly HashSet<string> Keywords = new HashSet<string> (StringComparer.Ordinal)
      {
            "namespace","using",
            "class","struct","enum","interface",
            "public","private","protected","internal",
            "static","readonly","const","new","return","void","var",
            "this","base",
            "if","else","switch","case","default",
            "for","foreach","while","do","break","continue",
            "try","catch","finally","throw",
            "true","false","null",
            "in","out","ref","params","yield",
            "async","await",
            // Add more language keywords as desired:
            "operator","implicit","explicit","partial","record"
        };

      /// <summary>
      /// Built-in primitive/system types (case-sensitive).
      /// </summary>
      private static readonly HashSet<string> Types = new HashSet<string> (StringComparer.Ordinal)
      {
            "bool","byte","sbyte","short","ushort","int","uint","long","ulong",
            "float","double","decimal",
            "char","string","object"
        };
      #endregion

      #region HTML helpers: wrapping and encoding ----------------------------------------------
      /// <summary>
      /// Wraps the token in a span with the given CSS class and ensures safe HTML.
      /// </summary>
      private static string Convert (string text, string className) =>
          $"<span class=\"{className}\">{HtmlEncode (text)}</span>";

      /// <summary>
      /// Minimal HTML encoder to avoid breaking the HTML output.
      /// Encodes: &lt; &gt; &amp; &quot; and single quote.
      /// </summary>
      private static string HtmlEncode (string s) {
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
      #endregion
   }
}
