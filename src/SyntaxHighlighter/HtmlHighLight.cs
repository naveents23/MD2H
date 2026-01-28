// HTML Syntax Highlighter ? HTML spans with CSS classes per token
// - Tokenizes: comments, tags, attributes, strings (single/double quotes),
//   entity references, text content, and special characters.
// - Classifies tokens as: comment (cm), tag (pun), attribute name (kw), string (str),
//   entity (num), or plain text (pln).
// - Preserves original whitespace and layout.
// - Safe HTML output via HtmlEncode.
// Note: This is a pragmatic tokenizer, not a full HTML parser; it covers common cases well.

using System.Text.RegularExpressions;

namespace Md2h {
   public class HtmlHighLight {
      #region Public API: Highlighter -----------------------------------------------------------
      /// <summary>
      /// Converts an HTML source string to HTML with per-token <span class="{EToken}"> wrappers.
      /// </summary>
      /// <param name="text">HTML source code to highlight. May be multi-line.</param>
      /// <returns>HTML string with tokens wrapped and all text HTML-encoded.</returns>
      /// <remarks>
      /// Algorithm (linear scan over regex matches):
      /// 1) Run the tokenizer regex to find tokens in order.
      /// 2) Append untouched (encoded) text between tokens to preserve whitespace/layout.
      /// 3) For each token:
      ///    - Determine its category (comment, tag, attribute, string, entity, text).
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
               sb.Append (UtilsSynHL.HtmlEncode (text.Substring (last, m.Index - last)));

            string token = m.Value;
            string cls;

            // Classification by named capture groups:
            // cm: HTML comments <!-- ... -->
            // tag: opening/closing/self-closing tags <...>
            // attr: attribute names
            // str: string literals "..." or '...'
            // ent: entity references &...;
            // num: numeric references &#...;
            if (m.Groups["cm"].Success) cls = nameof (EToken.cm);
            else if (m.Groups["tag"].Success) cls = nameof (EToken.pun);
            else if (m.Groups["attr"].Success) cls = nameof (EToken.kw);
            else if (m.Groups["str"].Success) cls = nameof (EToken.str);
            else if (m.Groups["ent"].Success) cls = nameof (EToken.num);
            else if (m.Groups["num"].Success) cls = nameof (EToken.num);
            else cls = nameof (EToken.pln);

            // Wrap and append the token.
            sb.Append (UtilsSynHL.Wrap (token, Enum.Parse<EToken> (cls)));

            // Update the cursor to the end of the current token.
            last = m.Index + m.Length;
         }

         // Append any trailing text after the final token.
         if (last < text.Length)
            sb.Append (UtilsSynHL.HtmlEncode (text.Substring (last)));

         return sb.ToString ();
      }
      #endregion

      #region Tokenization: Regex and groups ----------------------------------------------------
      // Tokenizer definition:
      // - cm: HTML comments <!-- ... -->
      // - tag: opening/closing/self-closing tags <tagname ...>
      // - attr: attribute names (word characters before = or space/> in tag context)
      // - str: string literals "..." or '...' (typically attribute values)
      // - ent: HTML entity references &name;
      // - num: numeric entity references &#123; or &#xABC;
      //
      // Notes:
      // - RegexOptions.Compiled improves performance for repeated use.
      // - [\s\S]*? is used for non-greedy comment capture.
      // - Tag matching is pragmatic to handle various HTML structures.
      private static readonly Regex Tokenizer = new Regex (
          // HTML comment
          @"(?<cm><!--[\s\S]*?-->)" +
          // Tag: <...> (opening, closing, or self-closing)
          @"|(?<tag></?[a-zA-Z][a-zA-Z0-9\-]*(?:\s+[^>]*)?>)" +
          // Attribute name (word followed by = within a tag context)
          @"|(?<attr>\b[a-zA-Z\-][a-zA-Z0-9\-]*(?=\s*=))" +
          // String: "..." or '...'
          @"|(?<str>""[^""]*""|'[^']*')" +
          // HTML entity: &name;
          @"|(?<ent>&[a-zA-Z][a-zA-Z0-9]*;)" +
          // Numeric entity: &#123; or &#xABC;
          @"|(?<num>&#[0-9]+;|&#[xX][0-9a-fA-F]+;)",
          RegexOptions.Compiled);
      #endregion
   }
}
