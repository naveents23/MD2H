// JavaScript Syntax Highlighter ? HTML spans with CSS classes per token
// - Tokenizes: comments (single-line //, multi-line /* */), strings (single/double quotes),
//   template literals (backticks), regular expressions, numbers, identifiers, punctuation/operators.
// - Classifies identifiers as: keyword (kw), built-in type (type), or plain (pln).
// - Preserves original whitespace and layout.
// - Safe HTML output via HtmlEncode.
// Note: This is a pragmatic tokenizer, not a full JS parser; it covers common cases well.

using System.Text.RegularExpressions;

namespace Md2h {
   public class JavaScriptHighLight {
      #region Public API: Highlighter -----------------------------------------------------------
      /// <summary>
      /// Converts a JavaScript source string to HTML with per-token <span class="{EToken}"> wrappers.
      /// </summary>
      /// <param name="text">JavaScript source code to highlight. May be multi-line.</param>
      /// <returns>HTML string with tokens wrapped and all text HTML-encoded.</returns>
      /// <remarks>
      /// Algorithm (linear scan over regex matches):
      /// 1) Run the tokenizer regex to find tokens in order.
      /// 2) Append untouched (encoded) text between tokens to preserve whitespace/layout.
      /// 3) For each token:
      ///    - Determine its category (comment, string, template literal, regex, number, punctuation, identifier).
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
               sb.Append (UtilsSynHL.HtmlEncode (text.Substring (last, m.Index - last)));

            string token = m.Value;
            string cls;

            // Classification by named capture groups:
            // cm: single-line comments //...
            // mlcm: multi-line comments /* ... */
            // tmpl: template literals `...`
            // str: string literals "..." or '...'
            // regex: regular expressions /.../ with flags
            // num: numeric literals (integers, decimals, hex, octal, binary, exponents)
            // pun: punctuation/operators
            // id: identifiers ? further classified into kw/type/pln
            if (m.Groups["cm"].Success) cls = nameof (EToken.cm);
            else if (m.Groups["mlcm"].Success) cls = nameof (EToken.cm);
            else if (m.Groups["tmpl"].Success) cls = nameof (EToken.tmpl);
            else if (m.Groups["str"].Success) cls = nameof (EToken.str);
            else if (m.Groups["regex"].Success) cls = nameof (EToken.pun);
            else if (m.Groups["num"].Success) cls = nameof (EToken.num);
            else if (m.Groups["pun"].Success) cls = nameof (EToken.pun);
            else if (m.Groups["id"].Success) {
               if (Keywords.Contains (token)) cls = nameof (EToken.kw);
               else if (Types.Contains (token)) cls = nameof (EToken.type);
               else cls = nameof (EToken.pln);
            } else cls = nameof (EToken.pln);

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
      // - cm: single-line comments //... up to newline
      // - mlcm: multi-line comments /* ... */
      // - tmpl: template literals `...` with escape handling
      // - str: string literals "..." or '...' with escape handling
      // - regex: regular expressions /.../ with optional flags (g, i, m, s, u, y)
      // - num: numeric literals (integers, decimals, hex 0x, octal 0o, binary 0b, scientific notation)
      // - id: identifiers (alphanumeric + underscore + $, starting with letter, underscore, or $)
      // - pun: punctuation and operators (balanced set for typical JavaScript code)
      //
      // Notes:
      // - RegexOptions.Compiled improves performance for repeated use.
      // - [\s\S]*? is used for non-greedy multi-line comment capture.
      // - Template literals handle escaped backticks: \`
      // - Regex detection is pragmatic (handles common patterns; full regex parsing is complex).
      private static readonly Regex Tokenizer = new Regex (
          // Single-line comment
          @"(?<cm>//[^\r\n]*)" +
          // Multi-line comment
          @"|(?<mlcm>/\*[\s\S]*?\*/)" +
          // Template literal: `...` with escape handling (\`)
          @"|(?<tmpl>`(?:\\.|[^\\`])*`)" +
          // String literal: "..." or '...' with escape handling
          @"|(?<str>""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')" +
          // Regular expression: /.../ with optional flags; basic heuristic to avoid division confusion
          @"|(?<regex>/(?:[^/\\\r\n]|\\.)+/[gimsuvy]*)" +
          // Number: integers, decimals, hex (0x), octal (0o), binary (0b), scientific notation
          @"|(?<num>\b(?:0[xX][0-9a-fA-F]+|0[oO][0-7]+|0[bB][01]+|\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)\b)" +
          // Identifier: $ and _ allowed anywhere, letter or $ or _ to start
          @"|(?<id>\b[_$A-Za-z][_$A-Za-z0-9]*\b)" +
          // Punctuation/operators: common JS operators and delimiters
          @"|(?<pun>[{}()\[\];:,.=<>!&|^~?+\-*/%])",
          RegexOptions.Compiled);
      #endregion

      #region Classification Data: Keywords and built-in types ---------------------------------
      /// <summary>
      /// JavaScript keywords (case-sensitive). Includes ES6+ keywords.
      /// </summary>
      private static readonly HashSet<string> Keywords = new HashSet<string> (StringComparer.Ordinal)
      {
            // Declaration and scope
            "var", "let", "const",
            // Control flow
            "if", "else", "switch", "case", "default",
            "for", "while", "do", "break", "continue",
            "try", "catch", "finally", "throw",
            // Functions and classes
            "function", "return", "class", "extends", "super", "this",
            "new", "delete", "typeof", "instanceof",
            // Async/await
            "async", "await", "yield",
            // Module
            "import", "export", "from", "as",
            // Logical/boolean
            "true", "false", "null", "undefined",
            "in", "of",
            // Other
            "void", "static", "get", "set", "constructor",
            // ES2020 and beyond
            "globalThis", "BigInt"
        };

      /// <summary>
      /// JavaScript built-in objects and global identifiers (case-sensitive).
      /// </summary>
      private static readonly HashSet<string> Types = new HashSet<string> (StringComparer.Ordinal)
      {
            // Global objects
            "Object", "Array", "String", "Number", "Boolean", "Symbol",
            "Function", "Date", "RegExp", "Error",
            // Error types
            "TypeError", "ReferenceError", "SyntaxError", "RangeError", "EvalError", "URIError",
            // Global functions
            "console", "window", "document", "navigator", "location",
            // Common constructors
            "Promise", "Map", "Set", "WeakMap", "WeakSet",
            "ArrayBuffer", "DataView", "Int8Array", "Uint8Array",
            "Int16Array", "Uint16Array", "Int32Array", "Uint32Array",
            "Float32Array", "Float64Array",
            "BigInt64Array", "BigUint64Array",
            // Built-in methods
            "JSON", "Math", "Reflect", "Proxy"
        };
      #endregion
   }
}
