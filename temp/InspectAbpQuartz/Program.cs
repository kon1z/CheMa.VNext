using System.Reflection;

var assemblyPath = @"C:\Users\Administrator\.nuget\packages\volo.abp.backgroundworkers.quartz\10.3.0\lib\net10.0\Volo.Abp.BackgroundWorkers.Quartz.dll";
var assembly = Assembly.LoadFrom(assemblyPath);
foreach (var type in assembly.GetTypes().OrderBy(t => t.FullName))
{
    if (type.FullName?.Contains("Quartz") != true)
    {
        continue;
    }

    Console.WriteLine(type.FullName);
    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        Console.WriteLine($"  FIELD {field.FieldType.FullName} {field.Name}");
    }
    foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
    {
        Console.WriteLine($"  PROP {property.PropertyType.FullName} {property.Name}");
    }
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
    {
        Console.WriteLine($"  METHOD {method.ReturnType.FullName} {method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName + " " + p.Name))})");
    }
}
