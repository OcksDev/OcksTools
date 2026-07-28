public class GISContainerSimple : GISContainer
{
    public override bool IsAbstract => true;

    public override void StartCode()
    {
        slots.Clear();
    }
}
