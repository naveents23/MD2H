using System.Xml.Linq;

namespace Md2h.Html {
   public class Aside (Node root) {
      #region Methods -----------------------------------------------
      public string Build () {
         StringBuilder sb = new ();
         foreach (var node in mroot.Child!) {
            if (node!.IsFolder) {
               sb.AppendLine ($"""
                    <details>
                       <summary>{node.DName}</summary>
                  """);

            }
            sb.AppendLine ("<ul>");
            node.Child!.ForEach (fn => OutAsideFile (fn!));
            sb.AppendLine ("  </ul>  </details>\r\n");
         }
         return Structure (sb.ToString ());

         // Helper Methods
         void OutAsideFile (Node fileNode) {
            if (Html.IsIndex) {
               // first arg display text,Second arg is relative path w.r.t index.
               var s = new WebLink (fileNode.DName, string.Concat ("ZOut/Content/", fileNode.DName, ".html")).Build ();
               sb.AppendLine ($"    <li>{s}</li>");
            } else {
               var s = new WebLink (fileNode.DName, string.Concat (fileNode.DName, ".html")).Build ();
               sb.AppendLine ($"    <li>{s}</li>");
            }
         }
      }
      #endregion

      #region Implementation ----------------------------------------
      string Structure (string tree) {
         return $"""
              <aside>
                <nav aria-label="Contents">
                      {tree}
                   </nav>
            </aside>
            """;
      }
      #endregion

      #region Field -------------------------------------------------
      // private field
      Node mroot = root;
      #endregion

   }
}
