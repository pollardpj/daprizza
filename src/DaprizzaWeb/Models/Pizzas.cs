using DaprizzaModels;
using DaprizzaShared;

namespace DaprizzaWeb.Models;

public class Pizza
{
    public PizzaSize Size { get; set; } = PizzaSize.Small;
    public string[] Toppings { get; set; } = [];
}
