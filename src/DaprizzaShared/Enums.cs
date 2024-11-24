namespace DaprizzaShared;

public enum PizzaSize
{
    Small = 10,
    Medium = 20,
    Large = 30
}

public enum OrderStatus
{
    Created = 10,
    CookingInProgress = 20,
    ReadyForDelivery = 30,
    InTransit = 40,
    Delivered = 50,
    InError = 999
}