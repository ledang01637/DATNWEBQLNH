namespace DATN.Shared
{
    public class CartDTO
    {
        public int ProductId { get; set; }
        public int TableNumber {  get; set; }
        public int UnitId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
