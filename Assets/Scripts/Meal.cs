using UnityEngine.UI;

public class Meal
{
    public MealType Type;
    public Image Image;

    public Meal(MealType type, Image image)
    {
        Type = type;
        Image = image;
    }
}
