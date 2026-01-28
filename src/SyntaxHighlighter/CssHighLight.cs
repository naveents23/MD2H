// CSS Syntax Highlighter ? HTML spans with CSS classes per token
// - Tokenizes: comments, selectors, properties, values, colors (hex/rgb), units,
//   strings, numbers, and special characters.
// - Classifies tokens as: comment (cm), keyword/property (kw), string (str),
//   number (num), or plain text (pln).
// - Preserves original whitespace and layout.
// - Safe HTML output via HtmlEncode.
// Note: This is a pragmatic tokenizer, not a full CSS parser; it covers common cases well.

using System.Text.RegularExpressions;

namespace Md2h {
   public class CssHighLight {
      #region Public API: Highlighter -----------------------------------------------------------
      /// <summary>
      /// Converts a CSS source string to HTML with per-token <span class="{EToken}"> wrappers.
      /// </summary>
      /// <param name="text">CSS source code to highlight. May be multi-line.</param>
      /// <returns>HTML string with tokens wrapped and all text HTML-encoded.</returns>
      /// <remarks>
      /// Algorithm (linear scan over regex matches):
      /// 1) Run the tokenizer regex to find tokens in order.
      /// 2) Append untouched (encoded) text between tokens to preserve whitespace/layout.
      /// 3) For each token:
      ///    - Determine its category (comment, property, color, number with unit, string, etc.).
      ///    - If identifier: check if it's a known CSS property.
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
            // cm: multi-line comments /* ... */
            // hex: hex color codes #RGB or #RRGGBB
            // rgb: rgb/rgba colors rgb(...) or rgba(...)
            // prop: CSS property names (known properties)
            // str: string literals "..." or '...'
            // num: numeric values with units (12px, 1.5em, etc.)
            // val: numeric values without units
            if (m.Groups["cm"].Success) cls = nameof (EToken.cm);
            else if (m.Groups["hex"].Success) cls = nameof (EToken.num);
            else if (m.Groups["rgb"].Success) cls = nameof (EToken.num);
            else if (m.Groups["prop"].Success) {
               if (CssProperties.Contains (token)) cls = nameof (EToken.kw);
               else cls = nameof (EToken.pln);
            } else if (m.Groups["str"].Success) cls = nameof (EToken.str);
            else if (m.Groups["num"].Success) cls = nameof (EToken.num);
            else if (m.Groups["val"].Success) cls = nameof (EToken.num);
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
      // - cm: multi-line comments /* ... */
      // - hex: hex colors #RGB or #RRGGBB or #RRGGBBAA
      // - rgb: rgb/rgba functions rgb(...) or rgba(...)
      // - prop: CSS property names (identifiers followed by :)
      // - str: string literals "..." or '...'
      // - num: numeric values with units (px, em, rem, %, etc.)
      // - val: numeric values without units
      //
      // Notes:
      // - RegexOptions.Compiled improves performance for repeated use.
      // - [\s\S]*? is used for non-greedy comment capture.
      // - Selectors (class, id, element) are tokenized as plain text.
      private static readonly Regex Tokenizer = new Regex (
          // Multi-line comment
          @"(?<cm>/\*[\s\S]*?\*/)" +
          // Hex color: #RGB, #RRGGBB, or #RRGGBBAA
          @"|(?<hex>#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})\b)" +
          // RGB/RGBA color function
          @"|(?<rgb>rgba?\s*\([^)]*\))" +
          // CSS property name: identifier followed by colon
          @"|(?<prop>\b[a-z\-]+(?=\s*:))" +
          // String: "..." or '...'
          @"|(?<str>""[^""]*""|'[^']*')" +
          // Number with unit: digits, optional decimal, optional unit (px, em, rem, %, etc.)
          @"|(?<num>\b\d+(?:\.\d+)?(?:px|em|rem|%|ch|cm|mm|in|pt|pc|vh|vw|vmin|vmax|deg|rad|turn|s|ms|hz|khz)?(?![a-z\-]))" +
          // Plain number
          @"|(?<val>\b\d+(?:\.\d+)?\b)",
          RegexOptions.Compiled);
      #endregion

      #region Classification Data: CSS Properties -----------------------------------------------
      /// <summary>
      /// Common CSS properties (case-sensitive, lowercase). Extend as needed.
      /// </summary>
      private static readonly HashSet<string> CssProperties = new HashSet<string> (StringComparer.OrdinalIgnoreCase)
      {
            // Layout
            "display", "position", "float", "clear", "width", "height",
            "top", "right", "bottom", "left", "z-index",
            // Margin and padding
            "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
            "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
            // Border
            "border", "border-top", "border-right", "border-bottom", "border-left",
            "border-width", "border-style", "border-color", "border-radius",
            // Background
            "background", "background-color", "background-image", "background-position",
            "background-size", "background-repeat", "background-attachment",
            // Text
            "color", "font", "font-family", "font-size", "font-weight", "font-style",
            "text-align", "text-decoration", "text-transform", "text-shadow",
            "line-height", "letter-spacing", "word-spacing",
            // Box model
            "box-sizing", "box-shadow", "overflow", "overflow-x", "overflow-y",
            // Flexbox
            "flex", "flex-direction", "flex-wrap", "justify-content", "align-items",
            "align-content", "flex-grow", "flex-shrink", "flex-basis",
            // Grid
            "grid", "grid-template-columns", "grid-template-rows", "grid-gap",
            "grid-column", "grid-row", "grid-area",
            // Transform and animation
            "transform", "transition", "animation", "opacity", "visibility",
            // Pseudo-elements and pseudo-classes
            "content", "cursor", "list-style", "outline", "appearance",
            // Other
            "min-width", "max-width", "min-height", "max-height",
            "vertical-align", "white-space", "word-wrap", "word-break"
        };
      #endregion
   }
}
