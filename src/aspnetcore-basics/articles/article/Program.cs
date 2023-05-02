// See https://aka.ms/new-console-template for more information
using Article.SequenceEqual;

Console.WriteLine("Learn from an Article!");

var goat = new Goat() {Name = "Ariefs", Age = 33};
Console.WriteLine(goat.ToString());
var goatsFromJakarta = new List<Goat> {goat};
var goatsFromAmbon = new List<Goat> {goat};
bool result = goatsFromJakarta.SequenceEqual(goatsFromAmbon);
Console.WriteLine(result); // Return: True

goatsFromJakarta = new List<Goat> 
{
    new() {Name = "Juita", Age = 33 },
    new() {Name = "Ephra", Age = 3 },
    new() {Name = "Esther", Age = 1 },
};
goatsFromAmbon = new List<Goat> 
{
    new() {Name = "Juita", Age = 33 },
    new() {Name = "Ephra", Age = 3 },
    new() {Name = "Esther", Age = 1 },
};

goatsFromJakarta.ForEach(x => Console.WriteLine(x.Name));
/* Console.WriteLine(goatsFromJakarta.ForEach(x => {x.Name})); 

The code you provided has a syntax error.
The ForEach method expects a delegate or lambda expression 
that defines an action to perform on each element of the list. 
The lambda expression you provided returns a string (x.Name) but doesn't perform any action on it.

To fix the code, you can modify the lambda expression to perform an action, 
such as printing the name to the console:

This will print the names of all the goats in the goatsFromJakarta list to the console. You can do the same for the goatsFromAmbon list
goatsFromJakarta.ForEach(x => Console.WriteLine(x.Name));
*/ 

Console.WriteLine(goatsFromAmbon.Count); 


/// return : False,
/// Because SequenceEqual will by default use the internal comparison which uses the reference to the object. 
/// In the second case, even though the Terry Goat has the same properties in both lists, 
/// the two objects have different references since they are not the same object.
result = goatsFromJakarta.SequenceEqual(goatsFromAmbon);
Console.WriteLine(result); 

/// return : True, The best way to solve this is to provide our own equality comparer. 
/// We can do this by implementing the IEqualityComparer interface and passing an instance of our comparer 
/// to the SequenceEqual method.
result = goatsFromJakarta.SequenceEqual(goatsFromAmbon, new GoatSkillEqualityComparer());
Console.WriteLine(result);

