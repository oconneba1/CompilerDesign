using System;
using System.Collections;
 
namespace Tastier { 

public class Obj { // properties of declared symbol
   public bool constAssigned; // # Used to ensure assignment to a constant can only occur once.
   public string name; // its name
   public int kind;    // var, proc or scope
   public int sizeArray;    // # If an array is declared, this attribute is used to determine how much memory to allocate. 
   public int type;    // its type if var (undef for proc)
   public int sort;    // # sort of var - scalar or array
   public int level;   // lexic level: 0 = global; >= 1 local
   public int adr;     // address (displacement) in scope 
   public Obj next;    // ptr to next object in scope
   // for scopes
   public Obj outer;   // ptr to enclosing scope
   public Obj locals;  // ptr to locally declared objects
   public int nextAdr; // next free address in scope
}

public class SymbolTable {

   const int // object kinds
      var = 0, proc = 1, scope = 2, constant = 3; 
  
   const int // # Variable sort
      scalar = 1, array = 2;
   
   const int // types
      undef = 0, integer = 1, boolean = 2;

   public Obj topScope; // topmost procedure scope
   public int curLevel; // nesting level of current scope
   public Obj undefObj; // object node for erroneous symbols

   public bool mainPresent;
   
   Parser parser;
   
   public SymbolTable(Parser parser) {
      curLevel = -1; 
      topScope = null;
      undefObj = new Obj();
      undefObj.name = "undef";
      undefObj.kind = var;
      undefObj.type = undef;
      undefObj.level = 0;
      undefObj.adr = 0;
      undefObj.next = null;
      this.parser = parser;
      mainPresent = false;
   }

// open new scope and make it the current scope (topScope)
   public void OpenScope() {
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope; 
      scop.outer = topScope; 
      scop.locals = null;
      scop.nextAdr = 0;
      topScope = scop; 
      curLevel++;
   }

// close current scope
   public void CloseScope() {

      Obj p = topScope.locals;         // #Access the locally declared objects and iterate through them. 
      
      while(p!=null)
      {
         // #Use the kind attribute to determine if identifier refers to a procedure, variable or constant. 
         // #If it's a variable, determine whether local or global using level attribute.

         if(p.kind == 1)
         {
            // #Printing out the information about the identifier.
            Console.WriteLine(";" + p.name + ": Procedure, address:" + p.adr);
         }
         else if(p.kind == 0)
         {
            String theType = null;
            if(p.type == 0)
               theType = "undef";
            else if(p.type == 1)
               theType = "integer";
            else
               theType = "boolean";


            if(p.level == 0)
            {
               Console.WriteLine(";" + p.name + ": Global var, type: " + theType + ", address:" + p.adr);
            }
            else if(p.level >= 1)
            {
               Console.WriteLine(";" + p.name + ": Local var, type: " + theType + ", address:" + p.adr);
            }
            
         }
         else if(p.kind == 3)
         {
            Console.WriteLine(";" + p.name + ": Constant, address:" + p.adr);
         }
   
         p=p.next;
      }

      
      topScope = topScope.outer;
      curLevel--;
   }

// open new sub-scope and make it the current scope (topScope)
   public void OpenSubScope() {
   // lexic level remains unchanged
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope;
      scop.outer = topScope;
      scop.locals = null;
   // next available address in stack frame remains unchanged
      scop.nextAdr = topScope.nextAdr;
      topScope = scop;
   }

// close current sub-scope
   public void CloseSubScope() {
     
      Obj p = topScope.locals;      // # Similar to the CloseScope() method, I iterate through the identifiers in scope and print out the relevant information.
      
      while(p!=null)
      {
         if(p.kind == 0)
         {
            String theType = null;
            if(p.type == 0)
               theType = "undef";
            else if(p.type == 1)
               theType = "integer";
            else
               theType = "boolean";


            if(p.level >= 1)
            {
               Console.WriteLine(";" + p.name + ": Local var, type: " + theType + ", address:" + p.adr);
            }
            
         }
         p = p.next;
      }
     

   // update next available address in enclosing scope
      topScope.outer.nextAdr = topScope.nextAdr;
   // lexic level remains unchanged
      topScope = topScope.outer;
   }

   /* #I added sort*/
// create new object node in current scope
   public Obj NewObj(string name, int kind, int type, int sort, int sizeArray) {
      Obj p, last; 
      Obj obj = new Obj();
      obj.name = name; obj.kind = kind;
      obj.type = type; obj.level = curLevel;
      obj.sort = sort;
      obj.sizeArray = sizeArray;
      obj.next = null; 
      p = topScope.locals; last = null;
      while (p != null) { 
         if (p.name == name)
            parser.SemErr("name declared twice");
         last = p; p = p.next;
      }
      if (last == null)
         topScope.locals = obj; else last.next = obj;

      if ( (kind == var && sort == scalar) || kind == constant)      //# If scalar var or constant, allocating space for 1 variable.
         obj.adr = topScope.nextAdr++;

      if( kind == var && sort == array )                             //# If array var, allocate space corresponding to size of array.
      {
         obj.adr = topScope.nextAdr;
         topScope.nextAdr += sizeArray;
      }     
      //# If this is a constant, initialising constAssigned field 
      if(kind == constant)
         obj.constAssigned = false;
      return obj;
   }

// search for name in open scopes and return its object node
   public Obj Find(string name) {
      Obj obj, scope;
      scope = topScope;
      while (scope != null) { // for all open scopes
         obj = scope.locals;
         while (obj != null) { // for all objects in this scope
            if (obj.name == name) return obj;
            obj = obj.next;
         }
         scope = scope.outer;
      }
      parser.SemErr(name + " is undeclared");
      return undefObj;
   }

} // end SymbolTable

} // end namespace
