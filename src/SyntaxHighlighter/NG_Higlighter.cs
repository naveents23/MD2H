using System.Text.RegularExpressions;

namespace Md2h {

   public class AngularTypeScriptHighlighter {
      // TypeScript/JS keywords (case-sensitive)
      private static readonly HashSet<string> Keywords = new (StringComparer.Ordinal)
      {
            // modules / exports
            "import","export","from","as","default",
            // declarations and types
            "class","interface","enum","type","extends","implements","abstract","declare",
            // visibility / modifiers
            "public","private","protected","readonly","static","override",
            // functions / flow
            "function","constructor","return","if","else","switch","case","default",
            "for","while","do","break","continue","throw","try","catch","finally",
            // variables
            "let","const","var",
            // values / operators
            "new","this","super","true","false","null","undefined",
            "typeof","instanceof","void","delete","yield","await","async","in","of",
            // namespaces (TS legacy)
            "namespace","module"
        };

      // Built-in TS types (case-sensitive)
      private static readonly HashSet<string> Types = new (StringComparer.Ordinal)
      {
            "string","number","boolean","bigint","symbol","object","any","console",
            "unknown","never","void","null","undefined"
        };

      // Tokenizer (named capture groups)
      // Notes:
      // - tmpl: template literal (basic, non-nesting-aware). Handles escaped backticks.
      // - str: "..." or '...' (handles escapes).
      // - dec: @Decorator (captures the whole token, e.g., @Component, @Input).
      // - num: decimal/hex/bin/oct/BigInt with underscores and exponent (pragmatic).
      // - id: identifiers ($ and _ allowed).
      // - pun: punctuation/operators (single char; multi-char operators tokenize as multiple).
      private static readonly Regex Tokenizer = new Regex (
          // single-line comment
          @"(?<cm>//[^\r\n]*)" +
          // multi-line comment
          @"|(?<mlcm>/\*[\s\S]*?\*/)" +
          // template literal `...` (non-greedy, handles \`)
          @"|(?<tmpl>`(?:\\.|[^\\`])*`)" +
          // strings: "..." or '...'
          @"|(?<str>""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*')" +
          // decorator: @Name
          @"|(?<dec>@[$A-Za-z_][$A-Za-z0-9_]*)" +
          // numbers: decimal, hex, bin, oct, BigInt (n), underscores, optional exponent
          @"|(?<num>\b0[xX][0-9A-Fa-f]+n?\b|\b0[bB][01]+n?\b|\b0[oO][0-7]+n?\b|\b\d+(?:_\d+)*(?:\.\d+(?:_\d+)*)?(?:[eE][+-]?\d+)?n?\b)" +
          // identifier
          @"|(?<id>\b[$A-Za-z_][$A-Za-z0-9_]*\b)" +
          // punctuation/operators
          @"|(?<pun>[{}()\[\];.,:?~!%^&|+\-*/=<>@])",
          RegexOptions.Compiled);

      // Public API: convert TypeScript/Angular source to highlighted HTML
      public string Highlight (string source) {
         if (source == null) return string.Empty;
         var sb = new StringBuilder (source.Length * 2);
         int last = 0;

         foreach (Match m in Tokenizer.Matches (source)) {
            // Preserve untouched text between tokens
            if (m.Index > last) {
               sb.Append (UtilsSynHL.HtmlEncode (source.Substring (last, m.Index - last)));
            }

            var token = m.Value.Trim ();
            sb.Append (UtilsSynHL.Wrap (token, DetermineClass (m, token)));

            last = m.Index + m.Length;
         }

         // Trailing text
         if (last < source.Length)
            sb.Append (UtilsSynHL.HtmlEncode (source.Substring (last)));

         return sb.ToString ();
      }

      private static EToken DetermineClass (Match m, string token) {
         if (m.Groups["cm"].Success || m.Groups["mlcm"].Success) return EToken.cm;
         if (m.Groups["tmpl"].Success) return EToken.tmpl;
         if (m.Groups["str"].Success) return EToken.str;
         if (m.Groups["dec"].Success) return EToken.dec;
         if (m.Groups["num"].Success) return EToken.num;
         if (m.Groups["pun"].Success) return EToken.pun;

         if (m.Groups["id"].Success) {
            if (Keywords.Contains (token)) return EToken.kw;
            if (Types.Contains (token)) return EToken.type;
            return EToken.pln;
         }

         return EToken.pln;
      }

   }
}
