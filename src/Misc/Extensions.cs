namespace Md2h;

public static class Extensions {

   // Extensions block for sting 
   extension(string src) {
      public bool SW (string prefix) => src.StartsWith (prefix);

      public bool SW (char prefix) => src.StartsWith (prefix);

      public bool EQ (string prefix) => src == prefix;

      /// <summary>For given string test with list of search strigs if any match returns true.</summary>
      public bool ContainsIc (string[] searchStrings) {
         bool IsContain = false;
         foreach (string curStr in searchStrings) {
            if (src.Contains (curStr, StringComparison.OrdinalIgnoreCase)) IsContain = true;
         }
         return IsContain;
      }
   }
}

