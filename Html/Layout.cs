using System.Runtime.Intrinsics.Arm;

namespace Md2h.Html;

public class Html {
   public Html () { }
   #region Properities ----------------------------------------------
   public string? Title { get; set; }

   public Aside Aside { get => mAside; set => mAside = value; }

   #endregion

   #region Methods --------------------------------------------------
   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine (
        $"""
             <!DOCTYPE html>
               <html lang="en">
                 <head>
                   <meta charset="UTF-8">
                   <meta name="viewport" content="width=device-width, initial-scale=1">
            <div>
               {OutStyle ()}
            </div>
        """
         );

      sb.AppendLine ($"<title>{Title}</title>");
      sb.AppendLine (CodeHelperhead ());
      sb.AppendLine ($"</head>");
      sb.AppendLine ("<body>");
      sb.AppendLine (Aside.Build ());
      sb.AppendLine ("<main>");
      foreach (var body in Bodies) {
         if (body != null) {
            sb.AppendLine (body.Build ());
         }
      }
      sb.AppendLine ("</main>");
      sb.AppendLine (JavaScriptLink ());
      sb.AppendLine ("</body>");
      sb.AppendLine ("</html>");
      return sb.ToString ();
   }

    string OutStyle () {
      return IsIndex
         ? " <link href=\"ZOut/Res/Style.css\" rel=\"stylesheet\" />"
         : " <link href=\"../Res/Style.css\" rel=\"stylesheet\" />";
   }
   public void AddBody (IBody body) {
      Bodies.Add (body);
   }

   public void AddBody (params IBody[] body) {
      foreach (var b in body) {
         AddBody (b);
      }
   }

   public void Save (string path) {
      File.WriteAllText (path + ".html", Build ());
   }
   #endregion

   #region Implementations ----------------------------------------
   string JavaScriptLink () {
      string codeLink = "https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/prism.min.js";
      string csharpLink = "https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/prism-csharp.min.js";

      string script = "";
      // script += new Script (codeLink).Build () + "\n";
      // script += new Script (csharpLink).Build ();
      if (IsIndex) script += new Script ("ZOut/Res/JS.js").Build ();
      else script += new Script ("../Res/JS.js").Build ();
      return new Div (script).Build ();
   }

   static string CodeHelperhead () {
      return " <div>\r\n        <link href=\" https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism.min.css\" rel=\"stylesheet\" />\r\n    </div>";
   }
   #endregion

   #region Private fields -------------------------------------------
   List<IBody> Bodies { get; } = [];
   Aside mAside = new (null!);
   public static bool IsIndex;
   #endregion
}

#region Interface ---------------------------------------------------------------------------------
public interface IBody {
   string Build ();

}
#endregion
