
namespace Md2h.Html;
public class UnOrderedList : IBody {
   #region Methods --------------------------------------------------
   public void AddListItem (string str) => listItems.Add (Util.ListItem (str));
   #endregion

   #region Overridable ----------------------------------------------
   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine ($"<ul>");
      foreach (var item in listItems) {
         sb.AppendLine (item);
      }
      sb.AppendLine ("</ul>");
      return sb.ToString ();
   }
   #endregion

   #region Private fields -------------------------------------------

   readonly List<string?> listItems = [];

   #endregion
}

public class OrderedList : IBody {
   public required string Title { get; set; }
   public void AddListItem (string str) {
      listItems.Add (Util.ListItem (str));
   }

   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine ($"<ol>{Title}");
      foreach (var item in listItems) {
         sb.AppendLine (item);
      }
      sb.AppendLine ("</ol>");
      return sb.ToString ();
   }
   readonly List<string?> listItems = [];
}

public class DefinitionList : IBody {
   public required string Title { get; set; }
   public void AddDefinition (string term, string definition) {
      definitions.Add ($"<dt>{term}</dt><dd>{definition}</dd>");
   }
   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine ($"<dl>{Title}");
      foreach (var item in definitions) {
         sb.AppendLine (item);
      }
      sb.AppendLine ("</dl>");
      return sb.ToString ();
   }
   readonly List<string?> definitions = [];
}

public class Article : IBody {
   public required string Title { get; set; }
   public void AddParagraph (string paragraph) {
      paragraphs.Add (Util.Paragraph (paragraph));
   }
   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine ($"<article><h1>{Title}</h1>");
      foreach (var item in paragraphs) {
         sb.AppendLine (item);
      }
      sb.AppendLine ("</article>");
      return sb.ToString ();
   }
   readonly List<string?> paragraphs = [];
}

public class Section : IBody {
   public required string Title { get; set; }
   public void AddParagraph (string paragraph) {
      paragraphs.Add (Util.Paragraph (paragraph));
   }
   public string Build () {
      StringBuilder sb = new ();
      sb.AppendLine ($"<section><h2>{Title}</h2>");
      foreach (var item in paragraphs) {
         sb.AppendLine (item);
      }
      sb.AppendLine ("</section>");
      return sb.ToString ();
   }
   readonly List<string?> paragraphs = [];
}

public class H1 (String h1) : IBody {
   public string Build () {
      return Util.H1 (h1);
   }
}
public class H2 (String h2) : IBody {
   public string Build () {
      return Util.H2 (h2);
   }
}
public class H3 (String h3) : IBody {
   public string Build () {
      return Util.H3 (h3);
   }
}
public class P (String p) : IBody {
   public string Build () {
      return Util.Paragraph (p);
   }
}

public class CODE (string code) : IBody {
   public string Build () {
      return Util.CodeBlock (code);
   }
}

public class HorizontalLine : IBody {
   public string Build () {
      return Util.HorizontalLine ();
   }
}

/// <summary>
/// Web link or nav link
/// </summary>
/// <param name="text">Display text</param>
/// <param name="link">link to the page </param>
public class WebLink (string text, string link) : IBody {
   public string Build () {
      return Util.Link (text, link);
   }
}

public class Img (string altName, string location) : IBody {
   public string Build () {
      return Util.Image (altName, location);
   }
}

public class BlockQuote (string quote) : IBody {
   public string Build () {
      return Util.BlockQuote (quote);
   }
}
public class Div (string content) : IBody {
   public string Build () {
      return $"<div>{content}</div>";
   }
}

public class Span (string content) : IBody {
   public string Build () {
      return $"<span>{content}</span>";
   }
}

public class Script (string src) : IBody {
   public string Build () {
      return $"<script src=\"{src}\"></script>";
   }
}

public class Table (IEnumerable<string> head, IEnumerable<IEnumerable<string>> data) : IBody {
   public string Build () {
      var h = Util.TableHead (head);
      var d = Util.TableData (data);
      string tb = $"""
         <table>
         <thead>
            {h}
            </thead>
            <tbody>
            {d}
            </tbody>
         </table>
         """;
      return tb;
   }
}

