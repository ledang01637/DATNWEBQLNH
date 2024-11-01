namespace DATN.Shared
{
    public class Cart
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public byte[] ProductImage { get; set; }
        public int Quantity {  get; set; }
    }
}
