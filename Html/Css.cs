using System.Dynamic;
using System.Reflection;

namespace Md2h.Html;
public static class Css {
   public static string Build () {
      //// if the Style.css file exists in the same directory as the executable, use it otherwise use the embedded resource
      //if (Path.Exists (Path.Combine (Path.GetDirectoryName (Environment.ProcessPath)!, "Style.css"))) {
      //   return File.ReadAllText (Path.Combine (Path.GetDirectoryName (Environment.ProcessPath)!, "Style.css"));
      //}
      using Stream? stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Md2h.Style.css"); // Embedded resource name
         using StreamReader reader = new (stream!);
         string content = reader.ReadToEnd ();
      return content;
   }
}
public static class JS {
   public static string Build () {
      using Stream? stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("Md2h.JS.js"); // Embedded resource name
         using StreamReader reader = new (stream!);
         string content = reader.ReadToEnd ();
      return content;
   }
}

