namespace Jaison.Containers
{
    public class UnsealedOrderedDictionary : SealedOrderedDictionary
    {
        public new void Seal()
        {
            // just don't
        }
    }
}
