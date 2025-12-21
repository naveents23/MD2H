namespace Md2h;

public static class Extensions {

   // Extensions block for sting 
   extension(string s) {
      public bool SW (string prefix) => s.StartsWith (prefix);
      public bool SW (char prefix) => s.StartsWith (prefix);
      public bool EQ (string prefix) => s == prefix;
   }


}

