using System.ComponentModel.DataAnnotations;

namespace TestModels;

public class TestModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(1, 100)]
    public int Age { get; set; }

    public string Description { get; set; }

    public static TestModel New(string name, int age = 1, string description = null) => new()
    {
        Name = name,
        Age = age,
        Description = description
    };
}
