using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Md2h.Html {
   /// <summary>Class for Syntax highlighter</summary>
   public class SynHighLighter {
      /// <summary>CSharp Syntax highlighter</summary>
      public static string CSharpSyntaxHighlighter (string txt) {
        // var sb = new StringBuilder ();
         return txt;
      }
      string HighLight (string text, EToken tok) => Convert (text, tok.ToString ());
      string Convert (string text, string className) => $"<span class={className} >{text}</span>";

   }
   public enum EToken { kw, type, num, str, cm, pun, pln }
}
