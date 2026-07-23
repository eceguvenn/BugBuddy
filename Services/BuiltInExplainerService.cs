using BugBuddy.Models;

namespace BugBuddy.Services;

/// <summary>
/// En yaygın C# hata kodları için yerleşik (offline) dostça açıklamalar.
/// API key girilmemişse veya API erişilemezse fallback olarak çalışır.
/// </summary>
public static class BuiltInExplainerService
{
    private static readonly Dictionary<string, ErrorExplanation> Explanations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CS0003"] = new(
            "Looks like the system ran out of memory during compilation. That's rare!",
            "Try closing other apps, or break your project into smaller parts.",
            null
        ),
        ["CS1002"] = new(
            "Hey! Looks like you forgot a semicolon (;) at the end of a statement.",
            "Add a ';' at the end of the line — easy fix!",
            "int x = 5;  // Don't forget this little guy → ;"
        ),
        ["CS1003"] = new(
            "The compiler expected a specific syntax element here but found something else.",
            "Check the line for missing commas, brackets, or other syntax elements.",
            null
        ),
        ["CS0246"] = new(
            "The compiler can't find the type you're using. Usually it's a missing 'using' statement or NuGet package.",
            "1. Add the right 'using' directive at the top of your file.\n2. Or install the missing NuGet package: dotnet add package <PackageName>",
            "using System.Collections.Generic;  // Example: adds List<T>, Dictionary<T,T> etc."
        ),
        ["CS0103"] = new(
            "You're trying to use a variable or method that doesn't exist in the current scope.",
            "Check for typos in the name, or make sure you declared it before using it.",
            "string name = \"BugBuddy\";\nConsole.WriteLine(name);  // 'name' must be declared first!"
        ),
        ["CS1061"] = new(
            "You're calling a method or property that doesn't exist on this type.",
            "Check the type's documentation, or look for a typo in the method/property name.",
            "// Maybe you meant .Count instead of .Length for a List?\nvar list = new List<int>();\nint count = list.Count;  // ✅ not .Length"
        ),
        ["CS0029"] = new(
            "You're trying to assign a value of one type to a variable of another type, and they're not compatible.",
            "Use explicit casting, or convert the value to the correct type.",
            "int number = int.Parse(\"42\");  // Convert string to int"
        ),
        ["CS0019"] = new(
            "You're using an operator (like +, -, ==) with types that don't support it.",
            "Make sure both sides of the operator are compatible types.",
            null
        ),
        ["CS0118"] = new(
            "You're using a name as if it were one thing (like a variable), but it's actually something else (like a type or namespace).",
            "Check if you're accidentally using a class name where a variable should be, or vice versa.",
            null
        ),
        ["CS0120"] = new(
            "You're trying to access a non-static member without an instance of the class.",
            "Either create an instance of the class, or make the member static.",
            "var service = new MyService();\nservice.DoWork();  // Need an instance for non-static methods!"
        ),
        ["CS0128"] = new(
            "You already declared a variable with this name in the same scope.",
            "Rename one of the variables or remove the duplicate declaration.",
            null
        ),
        ["CS0161"] = new(
            "Your method says it returns something, but not all code paths actually return a value.",
            "Make sure every possible path through your method ends with a 'return' statement.",
            "public int GetValue(bool flag)\n{\n    if (flag) return 1;\n    return 0;  // Don't forget the else case!\n}"
        ),
        ["CS0168"] = new(
            "You declared a variable but never used it. The compiler is just giving you a heads-up!",
            "Either use the variable, or remove the declaration to keep your code clean.",
            null
        ),
        ["CS0169"] = new(
            "You have a private field that's never used anywhere in your class.",
            "Use it, or remove it to keep things tidy.",
            null
        ),
        ["CS0200"] = new(
            "You're trying to assign a value to a read-only property (it only has a getter).",
            "Add a setter to the property, or use a different approach to set the value.",
            "public string Name { get; set; }  // Add 'set;' to allow assignment"
        ),
        ["CS0234"] = new(
            "The type or namespace you're trying to use doesn't exist in this namespace.",
            "Check for typos, missing 'using' directives, or a missing NuGet package reference.",
            null
        ),
        ["CS0266"] = new(
            "You need an explicit cast here — the compiler won't do this conversion automatically because you might lose data.",
            "Add an explicit cast if you're sure it's safe.",
            "double pi = 3.14;\nint rounded = (int)pi;  // Explicit cast needed"
        ),
        ["CS0428"] = new(
            "You're referencing a method without calling it. You might have forgotten the parentheses ().",
            "Add () to actually call the method.",
            "var result = MyMethod();  // Don't forget the ()!"
        ),
        ["CS0433"] = new(
            "The same type exists in multiple assemblies, and the compiler doesn't know which one you mean.",
            "Use a fully qualified name or add an alias to resolve the ambiguity.",
            null
        ),
        ["CS0535"] = new(
            "Your class implements an interface but doesn't implement all of its members.",
            "Add the missing method/property implementations to your class.",
            null
        ),
        ["CS0612"] = new(
            "You're using something that's been marked as obsolete/deprecated.",
            "Check the documentation for the recommended replacement.",
            null
        ),
        ["CS0649"] = new(
            "A field is never assigned a value, so it will always be its default (null, 0, false, etc.).",
            "Initialize the field, or check if you forgot to assign it somewhere.",
            null
        ),
        ["CS1501"] = new(
            "You're calling a method with the wrong number of arguments.",
            "Check the method signature and make sure you're passing the right number of parameters.",
            null
        ),
        ["CS1503"] = new(
            "The argument you're passing doesn't match the expected parameter type.",
            "Check the method signature and convert the argument to the correct type.",
            null
        ),
        ["CS1519"] = new(
            "There's an unexpected token in your class, struct, or interface declaration.",
            "You might have a stray character, missing brace, or code outside of a method.",
            null
        ),
        ["CS1529"] = new(
            "A 'using' directive must come before any other elements in the namespace.",
            "Move your 'using' statements to the top of the file.",
            null
        ),
        ["CS7036"] = new(
            "You're not providing all required arguments when calling a method or constructor.",
            "Check what parameters are required and pass them all.",
            null
        ),
        ["CS8600"] = new(
            "You're converting a possibly null value to a non-nullable type. Null safety alert!",
            "Add a null check, use the null-conditional operator (?.), or use the null-forgiving operator (!).",
            "string? input = GetInput();\nstring safe = input ?? \"default\";  // Provide a fallback"
        ),
        ["CS8602"] = new(
            "You might be dereferencing a null value here. The compiler is protecting you!",
            "Add a null check before using this value.",
            "if (myObject is not null)\n{\n    myObject.DoSomething();  // Safe!\n}"
        ),
        ["CS8604"] = new(
            "You're passing a possibly null argument to a parameter that doesn't accept null.",
            "Make sure the value isn't null before passing it, or use the '!' operator if you're sure.",
            null
        ),
        ["CS8618"] = new(
            "A non-nullable property isn't initialized. The compiler wants you to guarantee it won't be null.",
            "Initialize it in the constructor, use 'required' keyword, or make it nullable with '?'.",
            "public required string Name { get; set; }  // 'required' ensures it's set"
        ),
        ["CS8625"] = new(
            "You're trying to pass 'null' to a parameter that doesn't accept it.",
            "Pass a non-null value, or change the parameter to accept null by adding '?'.",
            null
        ),
    };

    /// <summary>
    /// Yerleşik sözlükten hata açıklaması döner.
    /// Sözlükte yoksa genel bir açıklama üretir.
    /// </summary>
    public static ErrorExplanation Explain(BuildError error)
    {
        if (Explanations.TryGetValue(error.ErrorCode, out var explanation))
            return explanation;

        // Sözlükte olmayan hatalar için genel açıklama
        return new ErrorExplanation(
            $"The compiler says: \"{error.Message}\"",
            "Check the official docs for this error code, or try searching for it online:\n"
            + $"https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/{error.ErrorCode.ToLowerInvariant()}",
            null
        );
    }
}
