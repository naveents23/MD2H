// It is command line tool to convert .md(markdown ) to html file)

using Md2h.Html;
using System.Text.Json;
using System.Threading.Channels;

namespace Md2h;

public class Program {
   static void Main (string[] args) {
      if (args.Length is 1) {
         _ = new MdnToHtml (args[0]);
      } else ShowHelp ();
   }

   private static void ShowHelp () {
      string helpNotes = "Markdown to html converter \n Add exe location into Path section\n" +
         "To run: From the Markdown file location open via cmd and Enter like => md2h . ";
      Console.WriteLine (helpNotes);
   }

   public static void Test () {
      _ = new MdnToHtml (@"C:\etc\New folder\test.md");
   }
}