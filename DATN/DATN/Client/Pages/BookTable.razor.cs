using DATN.Shared;
using System;

namespace DATN.Client.Pages
{
    public partial class BookTable
    {
        private Reservation reservationModel = new();
        private int numberTable;

        public void HandleBookTable()
        {
            reservationModel.TableId = 1;
            reservationModel.CreatedDate = DateTime.Now;
            reservationModel.IsDeleted = false;

        }
    }
}
