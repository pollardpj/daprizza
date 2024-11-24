namespace DaprizzaModels;

public class Driver
{
    public string Name { get; init; }
}

public class RegisterDriversRequest
{
    public IEnumerable<Driver> Drivers { get; init; }
}