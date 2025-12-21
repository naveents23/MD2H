using Md2h.Html;
using System.Text.RegularExpressions;

namespace Md2h {
   #region Class MdnToHtml ------------------------------------------------------------------------
   public class MdnToHtml {
      #region Constuctor --------------------------------------------
      /// <summary>
      /// This constructor method is used  From the command prompt its Enumerate all .md files including sub folder and convert into html file.
      /// Excluding Readme file.
      /// </summary>
      /// <param name="filePath"></param>
      public MdnToHtml (string currentDir) {
         mCurDir = currentDir;
         GetDirTreeView (currentDir);
         OutDir (currentDir);
         var files = GetFileList (currentDir);
         foreach (var file in files) {
            var outPath = Path.Combine (mDestContentDir!, Path.GetFileNameWithoutExtension (file));
            MdParse (file).Save (outPath);
            Console.WriteLine (file + "..................................... Processed");
         }
         Html.Html.IsIndex = true;
         mHtml!.Save (Path.Combine (mCurDir, "Index"));
      }

      // Returns all markdown file path its enumerate all subfolder and ignore Readme.md file 
      private IEnumerable<string> GetFileList (string currentDir) {
         var fileList = Directory.EnumerateFileSystemEntries (currentDir, "*.md", SearchOption.AllDirectories);
         var files = fileList.Where (f => !f.Contains ("Readme.md"));
         // This is text to display in the aside section.
         mAsideText = [.. files.Select (a => Path.GetFileNameWithoutExtension (a))];
         return fileList;
      }
      #endregion

      #region Methods -----------------------------------------------
      Html.Html MdParse (string mdPath) {
         string title = Path.GetFileNameWithoutExtension (mdPath);
         mHtml = new Html.Html () { Aside = new Aside (mAsideRoot), Title = title };
         mLines = File.ReadAllLines (mdPath);
         var enumerator = mLines.GetEnumerator ();
         while (enumerator.MoveNext ()) {
            Line = enumerator.Current.ToString ()!;
            if (!IsBreakLine && IsUnOrderedListItem (Line)) {
               while (IsUnOrderedListItem (Line)) {
                  sb.AppendLine (mL[1..]);
                  if (!enumerator.MoveNext ()) break;
                  Line = enumerator.Current.ToString ()!;
               }
               ProcessUnOrderedListItem (sb.ToString ());
            }
            if (IsBreakLine) {
               mHtml.AddBody (new HorizontalLine ());
               IsBreakLine = false;
               continue;
            }
            if (Line.StartsWith ('#')) {
               ProcessHeading (); continue;
            } else if (IsCodeBlock (Line)) {
               while (enumerator.MoveNext ()) {
                  var current = enumerator.Current.ToString ()!;
                  if (IsCodeBlock (current)) break;
                  sb.AppendLine (current);
               }
               ProcessCodeBlock (sb.ToString ()); continue;
            } else if (IsBlockQuote (Line)) {
               ProcessBlockQuote (); continue;
            } else if (Line.StartsWith ('|')) {
               var table = Line.Split ('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
               mTD = [.. table];
               while (enumerator.MoveNext ()) {
                  Line = enumerator.Current.ToString ()!;
                  if (!Line.SW ('|')) break;
                  if (Regex.Match (Line, @"^\|(-+)(\|(-+))*\|$").Success) { mTD.Clear (); mTH = table; continue; }
                  var data = Line.Split ('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                  mdata.Add ([.. data]);
               }
               mHtml.AddBody (new Table (mTH, mdata));
            } else ProcessParagraph ();
         }
         return mHtml;
      }
      #endregion

      #region Implementation ----------------------------------------
      void ProcessParagraph () {
         if (Regex.IsMatch (Line, mImagePattern)) {
            ProcessImage ();
            return;
         } else if (Regex.Match (Line, mLinkPattern).Success) {
            Line = Regex.Replace (Line, mLinkPattern, m => Util.Link (m.Groups[1].Value, m.Groups[2].Value));
         }
         mHtml.AddBody (new P (Line));
      }

      private void ProcessImage () {
         Line = Regex.Replace (Line, mImagePattern, m => Util.Image (m.Groups[1].Value, m.Groups[2].Value));
         mHtml.AddBody (new Div (Line));
      }

      private void ProcessCodeBlock (string v) {
         string highLightedStr = new CSharpHighLight ().Highlight (v);
         mHtml.AddBody (new CODE (highLightedStr));
         sb.Clear ();
      }

      private void ProcessBlockQuote () => mHtml.AddBody (new BlockQuote (Line[1..]));

      private void ProcessUnOrderedListItem (string str) {
         var ul = new UnOrderedList () { };
         foreach (var lit in str.Split ("\r\n")[..^1]) {
            ul.AddListItem (lit);
         }
         sb.Clear ();
         mHtml.AddBody (ul);
      }

      private void ProcessHeading () {
         string? headType = Line.Split ()[0];
         IBody? obj = headType switch {
            "###" => new H3 (Line[3..]),
            "##" => new H2 (Line[2..]),
            "#" => new H1 (Line[1..]),
            _ => null
         };
         mHtml.AddBody (obj!);
      }

      public static IEnumerable<string> GetFileName (IEnumerable<string> fileList) => fileList.Select (file => Path.GetFullPath (file));
      static bool IsCodeBlock (string line) => line.SW ("```") || line.SW ("~~~");
      static bool IsUnOrderedListItem (string line) => line.StartsWith ('*') || line.StartsWith ('-');
      static bool IsHorizontalRule (string line) => line.SW ("___") || line.SW ("***") || line.SW ("---");
      static bool IsBlockQuote (string line) => line.SW (">");


      // Prepare Output Directory and add resource to it like image,css,js files.
      void OutDir (string currentDir) {
         var imgDir = Directory.CreateDirectory (Path.Combine (currentDir, "ZOut", "Img"));
         var contentDir = Directory.CreateDirectory (Path.Combine (currentDir, "ZOut", "Content"));
         var resDir = Directory.CreateDirectory (Path.Combine (currentDir, "ZOut", "Res"));

         File.WriteAllText (Path.Combine (resDir.FullName, "Style.css"), Css.Build ());
         File.WriteAllText (Path.Combine (resDir.FullName, "JS.js"), JS.Build ());
         mDestContentDir = contentDir.FullName;

         var imgList = Directory.EnumerateFileSystemEntries (currentDir, "*.png", SearchOption.AllDirectories);
         foreach (var img in imgList) {
            var name = Path.GetFileName (img);
            var destPath = Path.Combine (imgDir.FullName, name);
            if (!File.Exists (destPath)) {
               File.Copy (Path.GetFullPath (img), destPath);
            }
         }
      }

      // Construct a tree view with given folder its enumerate all folder and files form a tree view later use we display it in aside only one level of tree
      void GetDirTreeView (string currentDir) {
         DirectoryInfo dirInfo = new (currentDir);
         var directories = dirInfo.EnumerateDirectories ();
         var root = new Node () { DName = "root", Child = [] };
         HashSet<string> outDir = [".git", "Content", "Res", "Img", "ZOut"];
         foreach (var dir in directories) {
            if (outDir.Contains (dir.Name)) continue;
            var node = new Node { DName = dir.Name, IsFolder = true, Child = [] };
            root.Child.Add (node);
            var dirFiles = dir.GetFiles ();
            foreach (var file in dirFiles) {
               if (file is null) continue;
               node.Child!.Add (new Node { DName = Path.GetFileNameWithoutExtension (file.FullName), Child = [] });
            }
         }
         mAsideRoot = root;
      }
      #endregion

      #region Properties --------------------------------------------
      public string Line {
         get => mL;
         set {
            var rawTxt = value.Trim ();
            if (Regex.Match (rawTxt, mBreak).Success) {
               mL = "";
               IsBreakLine = true;
               return;
            }
            if (Regex.Match (rawTxt, mBoldPattern).Success) {
               rawTxt = Regex.Replace (rawTxt, mBoldPattern, m => Util.Bold (m.Groups[1].Value ?? m.Groups[2].Value));
            }
            if (Regex.Match (rawTxt, mItalicPattern).Success) {
               rawTxt = Regex.Replace (rawTxt, mItalicPattern, m => Util.Italic (m.Groups[1].Value ?? m.Groups[2].Value));
            }
            mL = rawTxt;
         }
      }
      readonly string mBreak = @"^(_+|-+|\*+)$";
      readonly string mBoldPattern = @"\*\*(.*?)\*\*|__(.*?)__";
      readonly string mItalicPattern = @"\*(.*?)\*|_(.*?)_";
      readonly string mLinkPattern = @"\[(.*?)\]\((.*?)\)";
      readonly string mImagePattern = @"!\[(.*?)\]\((.*?)\)";
      #endregion

      #region Private Variables -------------------------------------
      string[] mLines = [];
      string mL = "";
      bool IsBreakLine = false;
      readonly StringBuilder sb = new ();
      Html.Html mHtml;
      List<string> mTD;
      List<List<string>> mdata = [];
      string[] mTH;
      string mDestContentDir; // Destination Content Directory
      IEnumerable<string> mAsideText; // Aside Links
      string mCurDir; // Current Directory
      Node mAsideRoot;
      #endregion
   }

   #region Class Node -----------------------------------------------------------------------------
   public class Node {
      public required string DName { get; set; } // Display name
      public bool IsFolder;
      public List<Node?>? Child;
   }
   #endregion

   #endregion
}
